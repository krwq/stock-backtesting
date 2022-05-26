using StockBacktesting.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.Utils
{
    internal static class TickerCandleHistoryHelpers
    {
        public static void RemoveDataBefore(this Dictionary<string, TickerCandleHistory> tickers, DateTime dateTimeUtc)
        {
            foreach (var kv in tickers)
            {
                TickerCandleHistory ticker = kv.Value;

                int idx = ticker.Candles.FindFirstNotBefore(dateTimeUtc);
                ticker.Candles.RemoveRange(0, idx);
            }
        }

        public static void AddUsdToUsd(this Dictionary<string, TickerCandleHistory> tickers)
        {
            TickerCandleHistory usdToUsd = new TickerCandleHistory("USDUSD", "USDUSD", StockExchange.Currency, "USD");

            TickerCandleHistory anyTicker =
                tickers.GetValueOrDefault("PLNUSD")
                ?? tickers.GetValueOrDefault("USDPLN")
                ?? tickers.GetValueOrDefault("MSFT,NYSE")
                ?? tickers.First().Value;


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
        }

        public static Func<DateTime, decimal> GetConversionRateFunc(this Dictionary<string, TickerCandleHistory> tickers, string baseCurrency, string destinationCurrency)
        {
            if (baseCurrency == destinationCurrency)
            {
                return (i) => 1.0m;
            }

            TickerCandleHistory conversionTicker;

            // Try getting direct conversion func
            if (tickers.TryGetValue(baseCurrency + destinationCurrency, out conversionTicker))
            {
                return (time) =>
                {
                    int idx = conversionTicker.Candles.FindFirstNotBeforeWithin(time, TimeSpan.FromDays(15));
                    if (idx == -1)
                    {
                        throw new Exception($"No conversion rate found for '{conversionTicker.TickerName}' at '{time}'");
                    }

                    return conversionTicker.Candles[idx].Close.Value;
                };
            }

            // Try getting inverse conversion func
            if (tickers.TryGetValue(destinationCurrency + baseCurrency, out conversionTicker))
            {
                return (time) =>
                {
                    int idx = conversionTicker.Candles.FindFirstNotBeforeWithin(time, TimeSpan.FromDays(15));
                    if (idx == -1)
                    {
                        throw new Exception($"No conversion rate found for '{conversionTicker.TickerName}' at '{time}'");
                    }

                    return 1.0m / conversionTicker.Candles[idx].Close.Value;
                };
            }

            if (baseCurrency == "USD" || destinationCurrency == "USD")
            {
                // TODO: find path in currency graph
                // For now assume all currencies should have XUSD or USDX ticker
                throw new Exception($"Could not find any ticker to convert '{baseCurrency}' to '{destinationCurrency}'");
            }

            // Try getting conversion through USD
            Func<DateTime, decimal> baseToUsd = tickers.GetConversionRateFunc(baseCurrency, "USD");
            Func<DateTime, decimal> usdToDest = tickers.GetConversionRateFunc("USD", destinationCurrency);

            return (dateTimeUtc) => baseToUsd(dateTimeUtc) * usdToDest(dateTimeUtc);
        }

        public static Func<int, decimal> GetConversionRateFuncForTimeConsistentTickers(this Dictionary<string, TickerCandleHistory> tickers, string baseCurrency, string destinationCurrency)
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
            Func<int, decimal> baseToUsd = tickers.GetConversionRateFuncForTimeConsistentTickers(baseCurrency, "USD");
            Func<int, decimal> usdToDest = tickers.GetConversionRateFuncForTimeConsistentTickers("USD", destinationCurrency);

            return (i) => baseToUsd(i) * usdToDest(i);
        }

        public static void AddTickersFrom(this Dictionary<string, TickerCandleHistory> tickers, Dictionary<string, TickerCandleHistory> tickersToAdd)
        {
            foreach (var kv in tickersToAdd)
            {
                tickers.Add(kv.Key, kv.Value);
            }
        }
    }
}
