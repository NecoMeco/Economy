﻿using System;
using System.Collections.Generic;
using System.Linq;
using Economy.Common;
using Economy.Models;

namespace Economy.ViewModels
{
    public class AccountViewModel : ViewModelBase
    {
        #region Поля

        private string _accountNumber;
        private string _bancInfo;
        private decimal _startBalance;
        private string _currency;
        private List<TransactionItemViewModel> _transactionItems;
        private List<string> _errorsList;
        private bool _isSelected;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Номер счета
        /// </summary>
        public string AccountNumber
        {
            get { return _accountNumber; }
            set { SetProperty(ref _accountNumber, value); }
        }


        /// <summary>
        /// Информация о банке
        /// </summary>
        public string BancInfo
        {
            get { return _bancInfo; }
            set { SetProperty(ref _bancInfo, value); }
        }

        /// <summary>
        /// Итоговое состояние счета
        /// </summary>
        public decimal Balance
        {
            get
            {
                return StartBalance + TransactionItems.Sum(i => i.AmountByAccount);
            }
        }

        /// <summary>
        /// Первоначальный баланс счета
        /// </summary>
        public decimal StartBalance
        {
            get { return _startBalance; }
            set
            {
                SetProperty(ref _startBalance, value);
                OnNotifyPropertyChanged(() => Balance);
            }
        }
        
        public string Currency
        {
            get { return _currency; }
            set { SetProperty(ref _currency, value); }
        }

        /// <summary>
        /// Переводы
        /// </summary>
        public List<TransactionItemViewModel> TransactionItems
        {
            get { return _transactionItems; }
            set
            {
                SetProperty(ref _transactionItems, value);
                OnNotifyPropertyChanged(() => Balance);
            }
        }

        /// <summary>
        /// Список ошибок валидации
        /// </summary>
        public List<string> ErrorsList
        {
            get { return _errorsList; }
            set
            {
                SetProperty(ref _errorsList, value);
                OnNotifyPropertyChanged(() => IsError);
            }
        }

        /// <summary>
        /// Корректность обработки
        /// </summary>
        public bool IsError
        {
            get
            {
                return (ErrorsList != null && ErrorsList.Count > 0);
            }
        }
        
        #endregion Свойства

        
        public AccountViewModel()
        {
            TransactionItems = new List<TransactionItemViewModel>();

            if (IsDesignTime) //данные для теста в режиме дизайна
            {
                AccountNumber = "AccountNumber";
                BancInfo = "BancInfo";
                StartBalance = 0;
                Currency = "Currency";
                TransactionItems.Add(new TransactionItemViewModel());
            }

            ShowErrorCommand = new RelayCommand(ShowError, CanShowError);
            SelectionChangedCommand = new RelayCommand(SelectionChanged, CanSelectionChanged);
        }

        /// <summary>
        /// Добавляем записи в список транзакций без оповешения
        /// </summary>
        /// <param name="items"></param>
        public void SetTransactionItemRangeSilent(IEnumerable<TransactionItem> items)
        {
            foreach (var transactionItem in items)
            {
                _transactionItems.Add(new TransactionItemViewModel(transactionItem, this));
            }
        }

        public RelayCommand SelectionChangedCommand { get; set; }

        public RelayCommand ShowErrorCommand { get; set; }

        private void ShowError(object o)
        {
            ViewModelLocator.Information.MessageText = string.Join(Environment.NewLine, ErrorsList);
            ViewModelLocator.Information.ShowErrorCommand.Execute(null);
        }

        private bool CanShowError(object o)
        {
            return ErrorsList != null && ErrorsList.Any();
        }


        private void SelectionChanged(object o)
        {
            if (_isSelected)
            {
                if (ViewModelLocator.SelectedTransactions.Contains(_transactionItems[0]))
                {
                    foreach (var transactionItemViewModel in TransactionItems)
                    {
                        ViewModelLocator.SelectedTransactions.Remove(transactionItemViewModel);
                    }
                }
            }
            else
            {
                if (!ViewModelLocator.SelectedTransactions.Contains(_transactionItems[0]))
                {
                    ViewModelLocator.SelectedTransactions.AddRange(TransactionItems);
                }
            }
            _isSelected = !_isSelected;
        }

        private bool CanSelectionChanged(object o)
        {
            return _transactionItems != null && _transactionItems.Any();
        }
    }
}
