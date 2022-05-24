using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.Model
{
    internal class DividendHistory
    {
        public string TickerName { get; }
        public List<Dividend> Dividends { get; } = new List<Dividend>();

        public DividendHistory(string tickerName)
        {
            TickerName = tickerName;
        }
    }
}
