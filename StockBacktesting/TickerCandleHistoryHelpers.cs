using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting
{
    internal static class TickerCandleHistoryHelpers
    {
        public static void AddUsdToUsd(this Dictionary<string, TickerCandleHistory> tickers)
        {
            TickerCandleHistory usdToUsd = new TickerCandleHistory("USDUSD");

            foreach (var anyTickerKv in tickers)
            {
                TickerCandleHistory anyTicker = anyTickerKv.Value;
                for (int i = 0; i < anyTicker.Candles.Count; i++)
                {
                    usdToUsd.Candles.Add(new TickerCandle()
                    {
                        TimeUtc = anyTicker.Candles[i].TimeUtc,
                        Open = 1.0m,
                        Close = 1.0m,
                        Low = 1.0m,
                        High = 1.0m,
                    });
                }

                tickers.Add(usdToUsd.TickerName, usdToUsd);
                return;
            }
        }

        public static Func<int, decimal> GetConversionRateFunc(this Dictionary<string, TickerCandleHistory> tickers, string baseCurrency, string destinationCurrency)
        {
            if (baseCurrency == destinationCurrency)
            {
                return (i) => 1.0m;
            }

            TickerCandleHistory conversionTicker;

            // Try getting direct conversion func
            if (tickers.TryGetValue(baseCurrency + destinationCurrency, out conversionTicker))
            {
                return (i) => conversionTicker.Candles[i].Close.Value;
            }

            // Try getting inverse conversion func
            if (tickers.TryGetValue(destinationCurrency + baseCurrency, out conversionTicker))
            {
                return (i) => 1.0m / conversionTicker.Candles[i].Close.Value;
            }

            if (baseCurrency == "USD" || destinationCurrency == "USD")
            {
                // TODO: find path in currency graph
                // For now assume all currencies should have XUSD or USDX ticker
                throw new Exception($"Could not find any ticker to convert '{baseCurrency}' to '{destinationCurrency}'");
            }

            // Try getting conversion through USD
            Func<int, decimal> baseToUsd = GetConversionRateFunc(tickers, baseCurrency, "USD");
            Func<int, decimal> usdToDest = GetConversionRateFunc(tickers, "USD", destinationCurrency);

            return (i) => baseToUsd(i) * usdToDest(i);
        }
    }
}
