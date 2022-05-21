using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.Strategies
{
    // Assumes monthly candles on the graphs
    internal static class StrategyIncomeEveryMonth
    {
        public static void TestStrategyIncomeEveryMonth(this Dictionary<string, TickerCandleHistory> tickers, Func<DateTime, decimal> monthlyIncome, string currency = "PLN")
        {
            TickerCandleHistory plnToUsd = tickers["PLNUSD"];

            foreach (var tickerKv in tickers)
            {
                var ticker = tickerKv.Value;

                decimal totalInvestedCash = 0;
                decimal tickerAmt = 0;
                decimal totalCash = 0;

                string tickerCurr = ticker.BaseCurrency;
                int max = ticker.Candles.Count;
                int lastCandleIdx = max - 1;

                Func<int, decimal> baseToTickerRate = tickers.GetConversionRateFunc(currency, tickerCurr);

                for (int i = 0; i < max; i++)
                {
                    TickerCandle tickerCandle = ticker.Candles[i];
                    decimal income = monthlyIncome(tickerCandle.TimeUtc);
                    totalCash += income;
                    totalInvestedCash += income;

                    if (tickerCandle.Close.HasValue)
                    {
                        decimal totalCashInTickerCurrency = totalCash * baseToTickerRate(i);

                        // we invest all cash in the ticker
                        tickerAmt += totalCashInTickerCurrency / tickerCandle.Close.Value;
                        totalCash = 0;
                    }

                    // if no value, we keep cash until next month
                }

                decimal totalReturnInTickerCurrency = tickerAmt * ticker.LastCandle.Close.Value;
                decimal totalReturn = totalCash + totalReturnInTickerCurrency / baseToTickerRate(lastCandleIdx);

                Console.WriteLine($"{ticker.TickerName.PadRight(10)} [{ticker.BaseCurrency}]: ratio: {100.0m * totalReturn / totalInvestedCash - 100.0m,8:+#.##;-#.##;+0.00}% [{totalReturn,10:0.00} / {totalInvestedCash,9:0.00}]");
            }
        }

        public static Func<DateTime, decimal> GetConstIncomeFunc(decimal monthlyIncome) => (time) => monthlyIncome;
        public static Func<DateTime, decimal> GetYearlyIncomeIncreaseFunc(decimal monthlyStartIncome, int startYear, double yearlyPercentageRaise)
        {
            double raiseRatio = 1.0 + yearlyPercentageRaise / 100.0;
            return (timeUtc) =>
            {
                int yearsPassed = timeUtc.Year - startYear;
                return monthlyStartIncome * (decimal)Math.Pow(raiseRatio, yearsPassed);
            };
        }

    }
}
