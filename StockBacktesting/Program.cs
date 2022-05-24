using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using StockBacktesting.Strategies;

namespace StockBacktesting
{
    class Program
    {
        static int Main(string[] args) // async Task<int>
        {
            //var divHistory = await NasdaqData.GetDividendHistoryAsync("MSFT");

            //foreach (var div in divHistory.Dividends)
            //{
            //    Console.WriteLine($"Ex: {div.ExDate}, Amt: {div.Amount}, Decl: {div.DeclarationDate}, Rec: {div.RecordDate}, Pmt: {div.PaymentDate}");
            //}

            //string dailyUsZipPath = @"D:\src\StockBacktesting\StockBacktesting\data\stooq\daily_us_txt.zip";
            //var tickers = StooqDataSets.GetNyseFromDailyUs(File.Open(dailyUsZipPath, FileMode.Open));
            string dailyWorldZipPath = @"D:\src\StockBacktesting\StockBacktesting\data\stooq\daily_world_txt.zip";
            var currencies = StooqDataSets.GetCurrenciesFromDailyWorld(File.Open(dailyWorldZipPath, FileMode.Open));

            //int idx = 0;
            //foreach (TickerCandle candle in ticker.Candles)
            //{
            //    Console.WriteLine($"{candle.TimeUtc} O: {candle.Open} C: {candle.Close} L: {candle.Low} H: {candle.High}");

            //    idx++;
            //    if (idx % 100 == 0)
            //        Console.ReadLine();
            //}


            var tickersTv = TradingViewDataSets.TradingViewMax1MSelected();
            tickersTv.AddUsdToUsd();
            foreach (var kv in currencies)
            {
                var hist = kv.Value;
                var last = hist.LastCandle;
                if (!hist.TickerName.Contains('_')) continue;
                Console.WriteLine($"{hist.TickerName} [{hist.Exchange}] [{hist.BaseCurrency}] [{last.TimeUtc}] => Open: {last.Open}, Close: {last.Close}, Low: {last.Low}, High: {last.High}");
            }

            //tickers.TestStrategyIncomeEveryMonth(StrategyIncomeEveryMonth.GetYearlyIncomeIncreaseFunc(1000, 2012, 5), "PLN");
            //InteractiveQueryPrice(tickers["PLATINUM"]);

            return 0;
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