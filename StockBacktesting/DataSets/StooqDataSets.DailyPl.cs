using StockBacktesting.Model;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.DataSets
{
    internal static partial class StooqDataSets
    {
        private static class DailyPl
        {
            private const string WseStocksDailPlZipPath = @"data/daily/pl/wse stocks/";
            private static string[] s_selectedFromWseStocksDailyPl = new string[]
            {
                //"CDR",
                //"KGH",
                //"PKN",
            };

            private static string BaseCurrency_PLN(string tickerName) => "PLN";

            private static (StockExchange, string, string[], Func<string, string>)[] s_selectedFromDailyPl = new (StockExchange, string, string[], Func<string, string>)[]
            {
                (StockExchange.WSE, WseStocksDailPlZipPath, s_selectedFromWseStocksDailyPl, BaseCurrency_PLN),
            };

            public static IEnumerable<TickerCandleHistory> EnumerateSelectedFromZipArchive(ZipArchive zip)
                => EnumerateSelectedFromZip(zip, s_selectedFromDailyPl);
        }
    }
}
