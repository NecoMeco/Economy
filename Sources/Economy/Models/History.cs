﻿using System;
using System.Collections.Generic;
using Economy.Dtos;

namespace Economy.Models
{
    [Serializable]
    public class History
    {
        readonly Dictionary<long, BelinvestCourseArhiveDto> _priceHistories;

        public CurrencyTypeDto MainCurrency { get; }

        public History(List<BelinvestCourseArhiveDto> dtos, CurrencyTypeDto mainCurrency)
        {
            MainCurrency = mainCurrency;
            _priceHistories = new Dictionary<long, BelinvestCourseArhiveDto>();
            foreach (var dto in dtos)
            {
                _priceHistories.Add(GetKey(dto.RegDate, dto.CurrencyTypeDto), dto);
            }
        }

        private long GetKey(DateTime date, CurrencyTypeDto currencyType)
        {
            return currencyType.Id * 1000000000000 + date.Year * 100000000 + date.Month * 1000000 + date.Day * 10000 + date.Hour * 100 + date.Minute;
        }

        public List<BelinvestCourseArhiveDto> PriceHistories
        {
            get;
            set;
        }

        public BelinvestCourseArhiveDto GetNearest(DateTime date, CurrencyTypeDto currencyType)
        {
            var key = GetKey(date, currencyType);
            if (_priceHistories.ContainsKey(key))
                return _priceHistories[key];

            BelinvestCourseArhiveDto result = null;
            for (int i = 0; i < _priceHistories.Count; i++)
            {
                var itm = _priceHistories[i];
                if (itm.CurrencyTypeDto == currencyType)
                {
                    if (itm.RegDate < date && (result == null || itm.RegDate > result.RegDate))
                        result = itm;
                    else
                        break;
                }
            }
            return result;
        }
    }
}