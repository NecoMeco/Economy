using System;
using System.Collections.Generic;
using CQRS.Common;
using CQRS.Dtos;

namespace Economy.Dtos
{
    public class MontlyReportDto : BaseDto
    {
        /// <summary>
        ///идентификатор
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        ///баланс на начало месяца
        /// </summary>
        public virtual decimal StartBalance { get; set; }

        /// <summary>
        ///баланс на конец месяца
        /// </summary>
        public virtual decimal EndBalance { get; set; }

        /// <summary>
        ///Период действия (год месяц)
        /// </summary>
        public virtual DateTime StartDate { get; set; }

        /// <summary>
        ///Идентификатор кошелька
        /// </summary>
        public virtual Guid WalletId { get; set; }

        public virtual WalletDto Wallet { get; set; }

        /// <summary>
        ///Версия для оптимистической блокировки
        /// </summary>
        public virtual int Version { get; set; }

        /// <summary>
        ///
        /// </summary>
        public virtual List<TransactionDto> Transactions { get; set; }

    
    }
}