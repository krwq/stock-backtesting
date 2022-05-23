using System;
using System.Collections.Generic;
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
            Dictionary<string, TickerCandleHistory> tickers = new();
            using ZipArchive zip = new ZipArchive(zipStream);
            string nyseZipPath = @"data/daily/us/nyse stocks/"; // 2/msft.us.txt
            foreach (var entry in zip.Entries)
            {
                if (entry.FullName.StartsWith(nyseZipPath) && !entry.FullName.EndsWith('/') && entry.Length != 0)
                {
                    var ticker = StooqData.LoadFromZip(entry, "NYSE", baseCurrency: "USD");
                    tickers.Add(ticker.TickerName, ticker);
                }
            }

            return tickers;
        }
    }
}
