using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.Model
{
    internal class TickerCandle
    {
        public DateTime TimeUtc { get; set; }
        public decimal? Open { get; set; }
        public decimal? Close { get; set; }
        public decimal? Low { get; set; }
        public decimal? High { get; set; }
    }
}
