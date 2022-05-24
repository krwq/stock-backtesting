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
            // XAUPLN, XAUUSD // gold
            // XAGPLN, XAGUSD // silver
            Console.WriteLine("Loading currencies data...");
            string dailyWorldZipPath = @"D:\src\StockBacktesting\StockBacktesting\data\stooq\daily_world_txt.zip";
            var currencies = StooqDataSets.GetCurrenciesFromDailyWorld(File.Open(dailyWorldZipPath, FileMode.Open, FileAccess.Read, FileShare.Read));

            Console.WriteLine("Loading quick stock info...");
            var interestingTickers = TradingViewDataSets.TradingViewMax1MSelected();

            Console.WriteLine("Loading full stock info...");
            string dailyUsZipPath = @"D:\src\StockBacktesting\StockBacktesting\data\stooq\daily_us_txt.zip";
            var nyseTickers = StooqDataSets.GetNyseFromDailyUs(File.Open(dailyUsZipPath, FileMode.Open, FileAccess.Read, FileShare.Read));

            var tickers = new Dictionary<string, TickerCandleHistory>();
            foreach (var kv in interestingTickers)
            {
                TickerCandleHistory tickerCandleHistory;
                if (!nyseTickers.TryGetValue(kv.Key, out tickerCandleHistory) && !currencies.TryGetValue(kv.Key, out tickerCandleHistory))
                {
                    Console.WriteLine($"Could not find full stock info about '{kv.Key}'");
                    tickerCandleHistory = kv.Value;
                }
                else // ? we could continue but probably better to skip
                {
                    tickers.Add(kv.Key, tickerCandleHistory);
                }
            }


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
                Console.WriteLine($"[n={hist.Candles.Count}] [{hist.Candles[0].TimeUtc}] {hist.TickerName} [{hist.Exchange}] [{hist.BaseCurrency}] [{last.TimeUtc}] => Open: {last.Open}, Close: {last.Close}, Low: {last.Low}, High: {last.High}");
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