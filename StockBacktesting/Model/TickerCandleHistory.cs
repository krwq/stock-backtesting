using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.Model
{
    internal class TickerCandleHistory
    {
        public string OriginalTickerName { get; private set; }
        public string SimplifiedTickerName { get; private set; }
        public string TickerName => $"{SimplifiedTickerName},{Exchange}";
        public StockExchange Exchange { get; private set; }
        public string BaseCurrency { get; private set; }
        public List<TickerCandle> Candles { get; } = new List<TickerCandle>();
        public TickerCandle FirstCandle => Candles[0];
        public TickerCandle LastCandle => Candles[Candles.Count - 1];

        public TickerCandleHistory(string originalTickerName, string simplifiedTickerName, StockExchange exchange, string baseCurrency)
        {
            OriginalTickerName = originalTickerName;
            SimplifiedTickerName = simplifiedTickerName;
            Exchange = exchange;
            BaseCurrency = baseCurrency;
        }
    }
}
