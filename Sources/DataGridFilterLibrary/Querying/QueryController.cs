﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Threading;
using DataGridFilterLibrary.Support;

namespace DataGridFilterLibrary.Querying
{
    public class QueryController
    {
        public FilterData ColumnFilterData { get; set; }
        public IEnumerable ItemsSource { get; set; }

        private readonly Dictionary<string, FilterData> filtersForColumns;

        Query query;

        public Dispatcher CallingThreadDispatcher { get; set; }
        public bool UseBackgroundWorker { get; set; }
        private readonly object lockObject;

        public QueryController()
        {
            lockObject = new object();

            filtersForColumns = new Dictionary<string, FilterData>();
            query = new Query();
        }

        public void DoQuery()
        {
            DoQuery(false);
        }

        public void DoQuery(bool force)
        {
            ColumnFilterData.IsSearchPerformed = false;

            if (!filtersForColumns.ContainsKey(ColumnFilterData.ValuePropertyBindingPath))
            {
                filtersForColumns.Add(ColumnFilterData.ValuePropertyBindingPath, ColumnFilterData);
            }
            else
            {
                filtersForColumns[ColumnFilterData.ValuePropertyBindingPath] = ColumnFilterData;
            }

            if (isRefresh)
            {
                if (filtersForColumns.ElementAt(filtersForColumns.Count - 1).Value.ValuePropertyBindingPath
                    == ColumnFilterData.ValuePropertyBindingPath)
                {
                    runFiltering(force);
                }
            }
            else if (filteringNeeded)
            {
                runFiltering(force);
            }

            ColumnFilterData.IsSearchPerformed = true;
            ColumnFilterData.IsRefresh = false;
        }

        public bool IsCurentControlFirstControl
        {
            get
            {
                return filtersForColumns.Count > 0
                    ? filtersForColumns.ElementAt(0).Value.ValuePropertyBindingPath == ColumnFilterData.ValuePropertyBindingPath : false;
            }
        }

        public void ClearFilter()
        {
            var count = filtersForColumns.Count;
            for (var i = 0; i < count; i++)
            {
                var data = filtersForColumns.ElementAt(i).Value;

                data.ClearData();
            }

            DoQuery();
        }

        #region Internal

        private bool isRefresh
        {
            get { return (from f in filtersForColumns where f.Value.IsRefresh == true select f).Count() > 0; }
        }

        private bool filteringNeeded
        {
            get { return (from f in filtersForColumns where f.Value.IsSearchPerformed == false select f).Count() == 1; }
        }

        private void runFiltering(bool force)
        {
            bool filterChanged;

            createFilterExpressionsAndFilteredCollection(out filterChanged, force);

            if (filterChanged || force)
            {
                OnFilteringStarted(this, EventArgs.Empty);

                applayFilter();
            }
        }

        private void createFilterExpressionsAndFilteredCollection(out bool filterChanged, bool force)
        {
            var queryCreator = new QueryCreator(filtersForColumns);

            queryCreator.CreateFilter(ref query);

            filterChanged = (query.IsQueryChanged || (query.FilterString != String.Empty && isRefresh));

            if ((force && query.FilterString != String.Empty) || (query.FilterString != String.Empty && filterChanged))
            {
                var collection = ItemsSource as IEnumerable;

                if (ItemsSource is ListCollectionView)
                {
                    collection = (ItemsSource as ListCollectionView).SourceCollection as IEnumerable;
                }

                var observable = ItemsSource as INotifyCollectionChanged;
                if (observable != null)
                {
                    observable.CollectionChanged -= observable_CollectionChanged;
                    observable.CollectionChanged += observable_CollectionChanged;

                }

                #region Debug
#if DEBUG
                Debug.WriteLine("QUERY STATEMENT: " + query.FilterString);

                var debugParameters = String.Empty;
                query.QueryParameters.ForEach(p =>
                {
                    if (debugParameters.Length > 0) debugParameters += ",";
                    debugParameters += p.ToString();
                });

                Debug.WriteLine("QUERY PARAMETRS: " + debugParameters);
#endif
                #endregion

                if (query.FilterString != String.Empty)
                {
                    var result = collection.AsQueryable().Where(query.FilterString, query.QueryParameters.ToArray<object>());

                    filteredCollection = result.Cast<object>().ToList();
                }
            }
            else
            {
                filteredCollection = null;
            }

            query.StoreLastUsedValues();
        }

        private void observable_CollectionChanged(
            object sender, NotifyCollectionChangedEventArgs e)
        {
            DoQuery(true);
        }

        #region Internal Filtering

        private IList filteredCollection;
        HashSet<object> filteredCollectionHashSet;

        void applayFilter()
        {
            var view = CollectionViewSource.GetDefaultView(ItemsSource);

            if (filteredCollection != null)
            {
                executeFilterAction(
                    () =>
                    {
                        filteredCollectionHashSet = initLookupDictionary(filteredCollection);

                        view.Filter = itemPassesFilter;

                        OnFilteringFinished(this, EventArgs.Empty);
                    }
                );
            }
            else
            {
                executeFilterAction(
                    () =>
                    {
                        if (view.Filter != null)
                        {
                            view.Filter = null;
                        }

                        OnFilteringFinished(this, EventArgs.Empty);
                    }
                );
            }
        }

        private void executeFilterAction(Action action)
        {
            if (UseBackgroundWorker)
            {
                var worker = new BackgroundWorker();

                worker.DoWork += delegate(object sender, DoWorkEventArgs e)
                {
                    lock (lockObject)
                    {
                        executeActionUsingDispatcher(action);
                    }
                };

                worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
                {
                    if (e.Error != null)
                    {
                        OnFilteringError(this, new FilteringEventArgs(e.Error));
                    }
                };

                worker.RunWorkerAsync();
            }
            else
            {
                try
                {
                    executeActionUsingDispatcher(action);
                }
                catch (Exception e)
                {
                    OnFilteringError(this, new FilteringEventArgs(e));
                }
            }
        }

        private void executeActionUsingDispatcher(Action action)
        {
            if (CallingThreadDispatcher != null && !CallingThreadDispatcher.CheckAccess())
            {
                CallingThreadDispatcher.Invoke
                    (
                        () =>
                        {
                            invoke(action);
                        }
                    );
            }
            else
            {
                invoke(action);
            }
        }

        private static void invoke(Action action)
        {
            Trace.WriteLine("------------------ START APPLAY FILTER ------------------------------");
            var sw = Stopwatch.StartNew();

            action.Invoke();

            sw.Stop();
            Trace.WriteLine("TIME: " + sw.ElapsedMilliseconds);
            Trace.WriteLine("------------------ STOP APPLAY FILTER ------------------------------");
        }

        private bool itemPassesFilter(object item)
        {
            return filteredCollectionHashSet.Contains(item);
        }

        #region Helpers
        private HashSet<object> initLookupDictionary(IList collection)
        {
            HashSet<object> dictionary;

            if (collection != null)
            {
                dictionary = new HashSet<object>(collection.Cast<object>()/*.ToList()*/);
            }
            else
            {
                dictionary = new HashSet<object>();
            }

            return dictionary;
        }
        #endregion

        #endregion
        #endregion

        #region Progress Notification
        public event EventHandler<EventArgs> FilteringStarted;
        public event EventHandler<EventArgs> FilteringFinished;
        public event EventHandler<FilteringEventArgs> FilteringError;

        private void OnFilteringStarted(object sender, EventArgs e)
        {
            var localEvent = FilteringStarted;

            if (localEvent != null) localEvent(sender, e);
        }

        private void OnFilteringFinished(object sender, EventArgs e)
        {
            var localEvent = FilteringFinished;

            if (localEvent != null) localEvent(sender, e);
        }

        private void OnFilteringError(object sender, FilteringEventArgs e)
        {
            var localEvent = FilteringError;

            if (localEvent != null) localEvent(sender, e);
        }
        #endregion
    }
}