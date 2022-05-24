using StockBacktesting.DataParsers;
using StockBacktesting.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.DataSets
{
    internal static class TradingViewDataSets
    {
        public static Dictionary<string, TickerCandleHistory> TradingViewMax1MSelected()
        {
            string csvPath = Path.Combine("data", "tradingview", "perf-comp-max-1M.csv");
            var tickerHistories = TradingViewData.LoadFromCsvFile(csvPath, "PLNUSD");

            // First candle is bogus
            foreach (var hist in tickerHistories)
            {
                hist.Candles.RemoveAt(0);
            }

            return tickerHistories.ToDictionary((h) => h.TickerName);
        }
    }
}
