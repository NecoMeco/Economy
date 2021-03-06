﻿using System;
using System.Windows.Media;
using Economy.Models;

namespace Economy.ViewModels
{
    public class TransactionItemViewModel : ViewModelBase
    {
        #region Поля

        private DateTime _registrationDate;
        private DateTime _transactionDate;
        private string _transactionCode;
        private string _description;
        private string _currency;
        private decimal _amountByCurrency;
        private decimal _amountByAccount;
        private string _accountNumber;
        private AccountViewModel _account;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Дата регистрации
        /// </summary>
        public DateTime RegistrationDate
        {
            get { return _registrationDate; }
            set { SetProperty(ref _registrationDate, value); }
        }

        /// <summary>
        /// Дата транзакции
        /// </summary>
        public DateTime TransactionDate
        {
            get { return _transactionDate; }
            set { SetProperty(ref _transactionDate, value); }
        }

        /// <summary>
        /// Номер транзакции/ документа
        /// </summary>
        public string TransactionCode
        {
            get { return _transactionCode; }
            set { SetProperty(ref _transactionCode, value); }
        }

        /// <summary>
        /// Код транзакции Описание операции Другая сторона, участвующая в операции
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        /// <summary>
        /// Валюта транзакции
        /// </summary>
        public string Currency
        {
            get { return _currency; }
            set { SetProperty(ref _currency, value); _currency = value; }
        }

        /// <summary>
        /// Сумма в валюте транзакции
        /// </summary>
        public decimal AmountByCurrency
        {
            get { return _amountByCurrency; }
            set { SetProperty(ref _amountByCurrency, value); }
        }

        /// <summary>
        /// Сумма в валюте счета
        /// </summary>
        public decimal AmountByAccount
        {
            get { return _amountByAccount; }
            set { SetProperty(ref _amountByAccount, value); }
        }

        /// <summary>
        /// Номер счета
        /// </summary>
        public string AccountNumber
        {
            get { return Account.AccountNumber; }
        }

        /// <summary>
        /// Ссылка на аккаунт
        /// </summary>
        public AccountViewModel Account
        {
            get { return _account; }
            set { SetProperty(ref _account, value); }
        }

        public bool IsIncome
        {
            get
            {
                return (AmountByCurrency > 0 || AmountByAccount > 0) ||
                    (AmountByCurrency == 0 && AmountByAccount == 0);
            }
        }

        public Brush DataRowColor
        {
            get
            {
                var cl = IsIncome ? Colors.Green : Colors.Red;
                return new SolidColorBrush(cl);
            }
        }

        /// <summary>
        /// Является ли транцакция внутри счетов (по своим карточкам)
        /// </summary>
        public bool IsLocalTransaction { get; set; }

        #endregion Свойства

        public TransactionItemViewModel()
        {
            if (IsDesignTime)
            {
                _registrationDate = DateTime.Today;
                _transactionDate = DateTime.Today;
                _transactionCode = "TransactionCode";
                _description = "Description";
                _currency = "Currency";
                _amountByCurrency = 42;
                _amountByAccount = 42;
                _accountNumber = "AccountNumber";
            }
        }

        public TransactionItemViewModel(TransactionItem item, AccountViewModel account)
        {
            SetSilent(item, account);
        }

        public void SetSilent(TransactionItem item, AccountViewModel account)
        {
            _registrationDate = item.RegistrationDate;
            _transactionDate = item.TransactionDate;
            _transactionCode = item.TransactionCode;
            _description = item.Description;
            _currency = item.Currency;
            _amountByCurrency = item.AmountByCurrency;
            _amountByAccount = item.AmountByAccount;
            Account = account;
        }

        public int CompareTo(TransactionItem other)
        {
            return RegistrationDate.CompareTo(other.RegistrationDate);
        }
    }
}
