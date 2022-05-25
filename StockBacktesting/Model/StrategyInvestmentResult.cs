using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.Model
{
    internal class StrategyInvestmentResult
    {
        public string TickerName { get; set; }
        public string InvestmentCurrency { get; set; }

        public decimal TotalInvested { get; set; }
        public decimal TotalReturn { get; set; }

        public DateTime InvestementStartUtc { get; set; }
        public DateTime InvestmentEndUtc { get; set; }

        public override string ToString()
        {
            return $"{TickerName.PadRight(10)} [{InvestmentCurrency}]: ratio: {100.0m * TotalReturn / TotalInvested - 100.0m,8:+#.##;-#.##;+0.00}% [{TotalReturn,10:0.00} / {TotalInvested,9:0.00}]";
        }
    }
}
