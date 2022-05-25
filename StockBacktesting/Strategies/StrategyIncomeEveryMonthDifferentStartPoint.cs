using StockBacktesting.Model;
using StockBacktesting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.Strategies
{
    // Assumes monthly candles but no assumptions on candle time consistency between tickers
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

                for (int i = 0; i < max; i++)
                {
                    TickerCandle tickerCandle = ticker.Candles[i];
                    decimal income = monthlyIncome(tickerCandle.TimeUtc);
                    totalCash += income;
                    totalInvestedCash += income;

                    if (tickerCandle.Close.HasValue)
                    {
                        decimal totalCashInTickerCurrency = totalCash * baseToTickerRate(tickerCandle.TimeUtc);

                        // we invest all cash in the ticker
                        tickerAmt += totalCashInTickerCurrency / tickerCandle.Close.Value;
                        totalCash = 0;
                    }

                    // if no value, we keep cash until next month
                }

                decimal totalReturnInTickerCurrency = tickerAmt * ticker.LastCandle.Close.Value;
                decimal totalReturn = totalCash + totalReturnInTickerCurrency / baseToTickerRate(ticker.LastCandle.TimeUtc);

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
