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
        public static Dictionary<string, StrategyInvestmentResult> TestStrategyIncomeEveryMonth(this Dictionary<string, TickerCandleHistory> tickers, Func<DateTime, decimal> monthlyIncome, string currency = "PLN")
        {
            Dictionary<string, StrategyInvestmentResult> ret = new();
            foreach (var tickerKv in tickers)
            {
                var ticker = tickerKv.Value;

                decimal totalInvestedCash = 0;
                decimal tickerAmt = 0;
                decimal totalCash = 0;

                string tickerCurr = ticker.BaseCurrency;
                int max = ticker.Candles.Count;

                Func<DateTime, decimal> baseToTickerRate = tickers.GetConversionRateFunc(currency, tickerCurr);

                DateTime firstAvailable = ticker.Candles[0].TimeUtc;
                DateTime start = new DateTime(firstAvailable.Year, firstAvailable.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime lastAvailable = ticker.LastCandle.TimeUtc;
                DateTime last = new DateTime(lastAvailable.Year, lastAvailable.Month, 1, 0, 0, 0, DateTimeKind.Utc);

                TickerCandle lastCandle = null;
                for (DateTime date = start; date <= last; date = date.AddMonths(1))
                {
                    decimal income = monthlyIncome(date);
                    totalCash += income;
                    totalInvestedCash += income;

                    // ignore candle if it's next month
                    int tickerCandleIdx = ticker.Candles.FindFirstNotBeforeWithin(date, TimeSpan.FromDays(31));
                    TickerCandle tickerCandle = tickerCandleIdx == -1 ? null : ticker.Candles[tickerCandleIdx];
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
                        Console.WriteLine($"Warning: Ticker '{ticker.TickerName}' does not have data for {DateOnly.FromDateTime(date)}");
                    }
                }

                decimal totalReturnInTickerCurrency = tickerAmt * lastCandle.Close.Value;
                decimal totalReturn = totalCash + totalReturnInTickerCurrency / baseToTickerRate(lastCandle.TimeUtc);

                ret[ticker.TickerName] = new StrategyInvestmentResult()
                {
                    TickerName = ticker.TickerName,
                    InvestmentCurrency = currency,
                    InvestementStartUtc = start,
                    InvestmentEndUtc = last,
                    TotalInvested = totalInvestedCash,
                    TotalReturn = totalReturn,
                };
            }

            return ret;
        }
    }
}
