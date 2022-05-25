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

            tickers.RemoveDataBefore(new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            foreach (var kv in tickers.TestStrategyIncomeEveryMonth(StrategyIncomeEveryMonth.GetYearlyIncomeIncreaseFunc(200, 1995, 5)))
            {
                DateOnly from = DateOnly.FromDateTime(kv.Value.InvestementStartUtc);
                DateOnly to = DateOnly.FromDateTime(kv.Value.InvestmentEndUtc);
                Console.WriteLine($"{kv.Value} [{from}]-[{to}]");
            }

            //foreach (var kv in tickers)
            //{
            //    var hist = kv.Value;
            //    var last = hist.LastCandle;
            //    DateOnly from = DateOnly.FromDateTime(hist.Candles[0].TimeUtc);
            //    DateOnly to = DateOnly.FromDateTime(last.TimeUtc);
            //    string line = $"[{from}]-[{to}] {hist.TickerName} [{hist.BaseCurrency}] [n={hist.Candles.Count}] => Open: {last.Open}, Close: {last.Close}, Low: {last.Low}, High: {last.High}";
            //    Console.WriteLine(line);
            //}

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
                int closesIdx = hist.Candles.FindClosestToTime(dt, TimeSpan.FromDays(20));
                if (closesIdx == -1)
                {
                    Console.WriteLine("Not found");
                }
                else
                {
                    TickerCandle candle = hist.Candles[closesIdx];
                    Console.WriteLine($"Time: {candle.TimeUtc} Open: {candle.Open}, Close: {candle.Close}, Low: {candle.Low}, High: {candle.High}");
                }
            }
        }
    }
}