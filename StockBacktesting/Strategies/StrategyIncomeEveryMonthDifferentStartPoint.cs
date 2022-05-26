using StockBacktesting.Model;
using StockBacktesting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.Strategies
{
    // No assumptions on candle time consistency between tickers
    internal static class StrategyIncomeEveryMonthDifferentStartPoint
    {
        // go from begining to end and invest income
        public static Dictionary<string, StrategyInvestmentResult> TestStrategyIncomeEveryMonth(this Dictionary<string, TickerCandleHistory> tickers, Func<DateTime, decimal> monthlyIncome, string currency = "PLN")
        {
            Dictionary<string, StrategyInvestmentResult> ret = new();
            foreach (var tickerKv in tickers)
            {
                var ticker = tickerKv.Value;
                GetStartAndEndMonths(ticker.Candles, out DateOnly start, out DateOnly end);
                ret[ticker.TickerName] = tickers.TestStrategyIncomeEveryMonth(ticker, start, end, monthlyIncome, currency);
            }

            return ret;
        }

        public static Dictionary<string, StrategyInvestmentResult[]> TestStrategyIncomeEveryMonthPerInterval(
            this Dictionary<string, TickerCandleHistory> tickers,
            TimeSpan interval,
            Func<DateTime, decimal> monthlyIncome,
            string currency = "PLN")
        {
            Dictionary<string, StrategyInvestmentResult[]> ret = new();
            foreach (var tickerKv in tickers)
            {
                var ticker = tickerKv.Value;
                GetStartAndEndMonths(ticker.Candles, out DateOnly start, out DateOnly end);
                ret[ticker.TickerName] = tickers.TestStrategyIncomeEveryMonthPerInterval(ticker, interval, monthlyIncome, currency).ToArray();
            }

            return ret;
        }

            // go from beginning to end starting in each month separately for no more than <interval> (TestStrategyIncomeEveryMonth moving window) 
        public static IEnumerable<StrategyInvestmentResult> TestStrategyIncomeEveryMonthPerInterval(
            this Dictionary<string, TickerCandleHistory> tickers,
            TickerCandleHistory tickerToTest,
            TimeSpan interval,
            Func<DateTime, decimal> monthlyIncome,
            string currency = "PLN")
        {
            GetStartAndEndMonths(tickerToTest.Candles, out DateOnly startDate, out DateOnly endDate);

            DateTime start = startDate.ToDateTime(new TimeOnly(0, 0, 0), DateTimeKind.Utc);
            DateOnly intervalEndDate = DateOnly.FromDateTime(start.Add(interval));
            if (intervalEndDate > endDate)
            {
                intervalEndDate = endDate;
            }

            for (; intervalEndDate <= endDate; startDate = startDate.AddMonths(1), intervalEndDate = intervalEndDate.AddMonths(1))
            {
                yield return tickers.TestStrategyIncomeEveryMonth(tickerToTest, startDate, intervalEndDate, monthlyIncome, currency);
            }
        }

        public static StrategyInvestmentResult TestStrategyIncomeEveryMonth(
            this Dictionary<string, TickerCandleHistory> tickers,
            TickerCandleHistory tickerToTest,
            DateOnly startDate,
            DateOnly endDate,
            Func<DateTime, decimal> monthlyIncome,
            string currency = "PLN")
        {
            decimal totalInvestedCash = 0;
            decimal tickerAmt = 0;
            decimal totalCash = 0;

            string tickerCurr = tickerToTest.BaseCurrency;
            int max = tickerToTest.Candles.Count;

            Func<DateTime, decimal> baseToTickerRate = tickers.GetConversionRateFunc(currency, tickerCurr);

            DateTime start = startDate.ToDateTime(new TimeOnly(0, 0, 0), DateTimeKind.Utc);
            DateTime end = endDate.ToDateTime(new TimeOnly(0, 0, 0), DateTimeKind.Utc);
            TickerCandle lastCandle = null;
            for (DateTime date = start; date <= end; date = date.AddMonths(1))
            {
                decimal income = monthlyIncome(date);
                totalCash += income;
                totalInvestedCash += income;

                // ignore candle if it's next month
                int tickerCandleIdx = tickerToTest.Candles.FindFirstNotBeforeWithin(date, TimeSpan.FromDays(31));
                TickerCandle tickerCandle = tickerCandleIdx == -1 ? null : tickerToTest.Candles[tickerCandleIdx];
                if (tickerCandleIdx != -1 && tickerCandle.TimeUtc.Month == date.Month)
                {
                    lastCandle = tickerCandle;

                    if (tickerCandle.Close.HasValue)
                    {
                        // conversion rate from the ticker not date!
                        decimal totalCashInTickerCurrency = totalCash * baseToTickerRate(tickerCandle.TimeUtc);

                        // we invest all cash in the ticker
                        tickerAmt += totalCashInTickerCurrency / tickerCandle.Close.Value;
                        totalCash = 0;
                    }
                }
                else
                {
                    //Console.WriteLine($"Warning: Ticker '{tickerToTest.TickerName}' does not have data for {DateOnly.FromDateTime(date)}");
                }
            }

            decimal totalReturnInTickerCurrency = tickerAmt * lastCandle.Close.Value;
            decimal totalReturn = totalCash + totalReturnInTickerCurrency / baseToTickerRate(lastCandle.TimeUtc);

            return new StrategyInvestmentResult()
            {
                TickerName = tickerToTest.TickerName,
                InvestmentCurrency = currency,
                InvestementStartUtc = start,
                InvestmentEndUtc = end,
                TotalInvested = totalInvestedCash,
                TotalReturn = totalReturn,
            };
        }

        private static void GetStartAndEndMonths(List<TickerCandle> candles, out DateOnly start, out DateOnly end)
        {
            DateTime firstAvailable = candles[0].TimeUtc;
            start = new DateOnly(firstAvailable.Year, firstAvailable.Month, 1);
            DateTime lastAvailable = candles[candles.Count - 1].TimeUtc;
            end = new DateOnly(lastAvailable.Year, lastAvailable.Month, 1);
        }
    }
}
