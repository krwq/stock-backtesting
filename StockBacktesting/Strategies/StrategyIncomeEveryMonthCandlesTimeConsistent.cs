using StockBacktesting.Model;
using StockBacktesting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.Strategies
{
    // Assumes monthly candles on the graphs and that all candles at the same indices have same time
    internal static class StrategyIncomeEveryMonthCandlesTimeConsistent
    {
        public static Dictionary<string, StrategyInvestmentResult> TestStrategyIncomeEveryMonthTimeConsistent(this Dictionary<string, TickerCandleHistory> tickers, Func<DateTime, decimal> monthlyIncome, string currency = "PLN")
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
                int lastCandleIdx = max - 1;

                Func<int, decimal> baseToTickerRate = tickers.GetConversionRateFuncForTimeConsistentTickers(currency, tickerCurr);

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

                ret[ticker.TickerName] = new StrategyInvestmentResult()
                {
                    TickerName = ticker.TickerName,
                    InvestmentCurrency = currency,
                    InvestementStartUtc = ticker.Candles[0].TimeUtc,
                    InvestmentEndUtc = ticker.LastCandle.TimeUtc,
                    TotalInvested = totalInvestedCash,
                    TotalReturn = totalReturn,
                };
            }

            return ret;
        }
    }
}
