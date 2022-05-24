using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using StockBacktesting.DataSets;
using StockBacktesting.Model;
using StockBacktesting.Strategies;
using StockBacktesting.Utils;

namespace StockBacktesting
{
    class Program
    {
        static int Main(string[] args) // async Task<int>
        {
            // XAUPLN, XAUUSD // gold
            // XAGPLN, XAGUSD // silver

            Console.WriteLine("Loading stock info...");
            string dailyUsZipPath = @"D:\src\StockBacktesting\StockBacktesting\data\stooq\daily_us_txt.zip";
            string dailyWorldZipPath = @"D:\src\StockBacktesting\StockBacktesting\data\stooq\daily_world_txt.zip";
            string dailyPlZipPath = @"D:\src\StockBacktesting\StockBacktesting\data\stooq\daily_pl_txt.zip";

            var tickers = StooqDataSets.GetSelectedFromZips(dailyUsZipPath, dailyWorldZipPath, dailyPlZipPath);

            //Console.WriteLine("Loading quick stock info...");
            //var interestingTickers = TradingViewDataSets.TradingViewMax1MSelected();

            //var divHistory = await NasdaqData.GetDividendHistoryAsync("MSFT");

            //foreach (var div in divHistory.Dividends)
            //{
            //    Console.WriteLine($"Ex: {div.ExDate}, Amt: {div.Amount}, Decl: {div.DeclarationDate}, Rec: {div.RecordDate}, Pmt: {div.PaymentDate}");
            //}

            //fff.AddUsdToUsd();
            foreach (var kv in tickers)
            {
                var hist = kv.Value;
                var last = hist.LastCandle;
                Console.WriteLine($"[n={hist.Candles.Count}] [{hist.Candles[0].TimeUtc}] {hist.TickerName} [{hist.BaseCurrency}] [{last.TimeUtc}] => Open: {last.Open}, Close: {last.Close}, Low: {last.Low}, High: {last.High}");
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
                TickerCandle candle = hist.Candles.FindClosestToTime(dt, TimeSpan.FromDays(20));
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