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
        private static class DailyWorld
        {
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

            private static (StockExchange, string, string[], Func<string, string>)[] s_selectedFromDailyWorld = new (StockExchange, string, string[], Func<string, string>)[]
            {
                (StockExchange.Currency, CurrenciesDailyWorldZipPath, s_selectedCurrenciesFromDailyWorld, BaseCurrency_CurrencyExchange),
            };

            public static IEnumerable<TickerCandleHistory> EnumerateSelectedFromZipArchive(ZipArchive zip)
                => EnumerateSelectedFromZip(zip, s_selectedFromDailyWorld);
        }
    }
}
