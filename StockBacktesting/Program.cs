using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StockBacktesting.Strategies;

namespace StockBacktesting
{
    class Program
    {
        static void Main()
        {
            var tickers = TradingViewDataSets.TradingViewMax1MSelected();
            tickers.AddUsdToUsd();
            //foreach (var kv in tickers)
            //{
            //    var hist = kv.Value;
            //    var last = hist.LastCandle;
            //    Console.WriteLine($"{hist.TickerName} [{hist.Exchange}] [{hist.BaseCurrency}] [{last.TimeUtc}] => Open: {last.Open}, Close: {last.Close}, Low: {last.Low}, High: {last.High}");
            //}

            tickers.TestStrategyIncomeEveryMonth(StrategyIncomeEveryMonth.GetYearlyIncomeIncreaseFunc(1000, 2012, 5), "PLN");
            InteractiveQueryPrice(tickers["PLATINUM"]);
        }

        static void InteractiveQueryPrice(TickerCandleHistory hist)
        {
            while (true)
            {
                string line = Console.ReadLine();

                if (string.IsNullOrEmpty(line))
                    break;

                DateTime dt = DateTime.Parse(line, null, System.Globalization.DateTimeStyles.RoundtripKind);
                TickerCandle candle = hist.FindClosestCandleToTime(dt, TimeSpan.FromDays(20));
                if (candle == null)
                {
                    Console.WriteLine("Not found");
                }
                else
                {
                    Console.WriteLine($"Time: {candle.TimeUtc} Open: {candle.Open}, Close: {candle.Close}, Low: {candle.Low}, High: {candle.High}");
                }
            }
        }
    }
}