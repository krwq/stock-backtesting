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
        private static class DailyUs
        {
            private const string NyseEtfsDailyUsZipPath = @"data/daily/us/nyse etfs/";
            private static string[] s_selectedNyseEtfsFromDailyUs = new[]
            {
                "SPY",
            };

            private const string NyseStocksDailyUsZipPath = @"data/daily/us/nyse stocks/";
            private static string[] s_selectedNyseStocksFromDailyUs = new[]
            {
                "AAPL",
                "MSFT",
                "GOOGL",
                "USB",
                "TSLA",
                "ADSK",
                "NFLX",
                "AMD",
                "AVGO",
                "AMZN",
                "SPGI",
                "PYPL",
                "SBUX",
                "QCOM",
                "FB",
            };

            private static string BaseCurrency_USD(string tickerName) => "USD";

            private static (StockExchange, string, string[], Func<string, string>)[] s_selectedFromDailyUs = new (StockExchange, string, string[], Func<string, string>)[]
            {
                (StockExchange.NYSE, NyseEtfsDailyUsZipPath, s_selectedNyseEtfsFromDailyUs, BaseCurrency_USD),
                (StockExchange.NYSE, NyseStocksDailyUsZipPath, s_selectedNyseStocksFromDailyUs, BaseCurrency_USD),
            };

            public static IEnumerable<TickerCandleHistory> EnumerateSelectedFromZipArchive(ZipArchive zip)
                => EnumerateSelectedFromZip(zip, s_selectedFromDailyUs);
        }
    }
}
