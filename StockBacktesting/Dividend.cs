using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting
{
    internal class Dividend
    {
        /// <summary>
        /// Cut-off point (buy before to get dividend)
        /// </summary>
        public DateOnly? ExDate { get; set; }
        public DividendType Type { get; set; }
        public decimal? Amount { get; set; }
        public DateOnly? DeclarationDate { get; set; }
        public DateOnly? RecordDate { get; set; }
        public DateOnly? PaymentDate { get; set; }

        public enum DividendType
        {
            Cash,
            //Stock,
        }
    }
}
