using StockBacktesting.DataParsers;
using StockBacktesting.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.DataSets
{
    internal class StooqDataSets
    {
        public static Dictionary<string, TickerCandleHistory> GetSelectedFromZips(string dailyUsZipPath, string dailyWorldZipPath, string dailyPlZipPath)
        {
            return EnumerateSelectedFromZips(dailyUsZipPath, dailyWorldZipPath, dailyPlZipPath)
                .ToDictionary((hist) => hist.TickerName);
        }

        public static IEnumerable<TickerCandleHistory> EnumerateSelectedFromZips(string dailyUsZipPath, string dailyWorldZipPath, string dailyPlZipPath)
        {
            using ZipArchive dailyUsZip = OpenZip(dailyUsZipPath);
            using ZipArchive dailyWorldZip = OpenZip(dailyWorldZipPath);
            using ZipArchive dailyPlZip = OpenZip(dailyPlZipPath);

            return EnumerateSelectedFromZipArchives(dailyUsZip, dailyWorldZip, dailyPlZip).ToArray();

            static ZipArchive OpenZip(string path)
            {
                return new ZipArchive(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
            }
        }

        public static IEnumerable<TickerCandleHistory> EnumerateSelectedFromZipArchives(ZipArchive dailyUsZip, ZipArchive dailyWorldZip, ZipArchive dailyPlZip)
        {
            return EnumerateSelectedFromDailyUsZipArchive(dailyUsZip)
                .Concat(EnumerateSelectedFromDailyWorldZipArchive(dailyWorldZip))
                .Concat(EnumerateSelectedFromDailyPlZipArchive(dailyPlZip));
        }

        private static string BaseCurrency_USD(string tickerName) => "USD";
        private static string BaseCurrency_PLN(string tickerName) => "PLN";
        private static string BaseCurrency_CurrencyExchange(string tickerName)
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
        }

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

        private static (StockExchange, string, string[], Func<string, string>)[] s_selectedFromDailyUs = new (StockExchange, string, string[], Func<string, string>)[]
        {
            (StockExchange.NYSE, NyseEtfsDailyUsZipPath, s_selectedNyseEtfsFromDailyUs, BaseCurrency_USD),
            (StockExchange.NYSE, NyseStocksDailyUsZipPath, s_selectedNyseStocksFromDailyUs, BaseCurrency_USD),
        };

        public static IEnumerable<TickerCandleHistory> EnumerateSelectedFromDailyUsZipArchive(ZipArchive zip)
            => EnumerateSelectedFromZip(zip, s_selectedFromDailyUs);

        private const string CurrenciesDailyWorldZipPath = @"data/daily/world/currencies/";
        private static string[] s_selectedCurrenciesFromDailyWorld = new[]
        {
            "PLNUSD",
            "JPYUSD",
            "EURUSD",
            "GBPUSD",
            "XAUUSD", // GOLD
            "XAGUSD", // SILVER
            "XPTUSD", // PLATINUM
        };

        private static (StockExchange, string, string[], Func<string, string>)[] s_selectedFromDailyWorld = new (StockExchange, string, string[], Func<string, string>)[]
        {
            (StockExchange.Currency, CurrenciesDailyWorldZipPath, s_selectedCurrenciesFromDailyWorld, BaseCurrency_CurrencyExchange),
        };

        public static IEnumerable<TickerCandleHistory> EnumerateSelectedFromDailyWorldZipArchive(ZipArchive zip)
            => EnumerateSelectedFromZip(zip, s_selectedFromDailyWorld);


        private const string WseStocksDailPlZipPath = @"data/daily/pl/wse stocks/";
        private static string[] s_selectedFromWseStocksDailyPl = new[]
        {
            "CDR",
            "KGH",
            "PKN",
        };

        private static (StockExchange, string, string[], Func<string, string>)[] s_selectedFromDailyPl = new (StockExchange, string, string[], Func<string, string>)[]
{
            (StockExchange.WSE, WseStocksDailPlZipPath, s_selectedFromWseStocksDailyPl, BaseCurrency_PLN),
        };

        public static IEnumerable<TickerCandleHistory> EnumerateSelectedFromDailyPlZipArchive(ZipArchive zip)
            => EnumerateSelectedFromZip(zip, s_selectedFromDailyPl);

        private static IEnumerable<TickerCandleHistory> EnumerateSelectedFromZip(ZipArchive zip, (StockExchange, string, string[], Func<string, string>)[] selected)
        {
            foreach (var entry in zip.Entries)
            {
                string fullName = entry.FullName.Replace('\\', '/');
                foreach ((StockExchange exchange, string prefix, string[] tickers, Func<string, string> baseCurrency) in selected)
                {
                    if (fullName.StartsWith(prefix) && !fullName.EndsWith('/') && entry.Length != 0)
                    {
                        string name = entry.Name.ToUpperInvariant().Split('.')[0];
                        if (tickers.Contains(name))
                        {
                            yield return StooqData.LoadFromZipEntry(entry, exchange, baseCurrency);
                            break;
                        }
                    }
                }
            }
        }

        //        // currencies
        //             //D:\src\StockBacktesting\StockBacktesting\data\stooq\daily_world_txt.zip\data\daily\world\currencies\other\plnusd.txt
    }
}
