using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting
{
    internal class StooqDataSets
    {
        public static Dictionary<string, TickerCandleHistory> GetNyseFromDailyUs(Stream zipStream)
        {
            const string exchange = "NYSE";
            Func<string, string> baseCurrency = (tickerName) => "USD";
            using ZipArchive zip = new ZipArchive(zipStream);
            var spy = StooqData.LoadFromZip(zip.GetEntry(@"data/daily/us/nyse etfs/2/spy.us.txt"), exchange, baseCurrency);
            var ret = GetTickers(zip, @"data/daily/us/nyse stocks/", exchange, baseCurrency);
            ret.Add(spy.TickerName, spy);
            return ret;
        }

        public static Dictionary<string, TickerCandleHistory> GetCurrenciesFromDailyWorld(Stream zipStream)
        {
            using ZipArchive zip = new ZipArchive(zipStream);
            return GetTickers(zip, @"data/daily/world/currencies/", "FOREX",
                (tickerName) =>
                {
                    int underscorePos = tickerName.IndexOf('_');
                    if (underscorePos != -1)
                    {
                        // index or basket
                        return tickerName.Substring(underscorePos + 1);
                    }
                    else if (tickerName.Length != 6)
                    {
                        throw new Exception($"Ticker name '{tickerName}' cannot be recognized as currency");
                    }

                    return tickerName.Substring(3);
                });
        }

        private static Dictionary<string, TickerCandleHistory> GetTickers(ZipArchive zip, string zipPath, string exchange, Func<string, string> baseCurrency)
        {
            Dictionary<string, TickerCandleHistory> tickers = new();
            foreach (var entry in zip.Entries)
            {
                // daily world somehow ended up with single backslash in the path
                string fullPath = entry.FullName.Replace('\\', '/');
                if (fullPath.StartsWith(zipPath) && !fullPath.EndsWith('/') && entry.Length != 0)
                {
                    var ticker = StooqData.LoadFromZip(entry, exchange, baseCurrency);
                    tickers.Add(ticker.TickerName, ticker);
                }
            }

            return tickers;
        }

        //        // currencies
        //             //D:\src\StockBacktesting\StockBacktesting\data\stooq\daily_world_txt.zip\data\daily\world\currencies\other\plnusd.txt
    }
}
