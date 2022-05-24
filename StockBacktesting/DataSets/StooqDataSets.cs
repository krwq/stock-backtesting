using StockBacktesting.DataParsers;
using StockBacktesting.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace StockBacktesting.DataSets
{
    internal static partial class StooqDataSets
    {
        public static Dictionary<string, TickerCandleHistory> GetSelectedFromZips(string dailyUsZipPath, string dailyWorldZipPath, string dailyPlZipPath)
        {
            return EnumerateSelectedFromZips(dailyUsZipPath, dailyWorldZipPath, dailyPlZipPath)
                .ToDictionary((hist) => hist.TickerName);
        }

        public static IEnumerable<TickerCandleHistory> EnumerateSelectedFromZips(string dailyUsZipPath, string dailyWorldZipPath, string dailyPlZipPath)
        {
            using ZipArchive dailyUsZip = OpenZip(dailyUsZipPath);
            using ZipArchive dailyWorldZip = OpenZip(dailyWorldZipPath);
            using ZipArchive dailyPlZip = OpenZip(dailyPlZipPath);

            return EnumerateSelectedFromZipArchives(dailyUsZip, dailyWorldZip, dailyPlZip).ToArray();

            static ZipArchive OpenZip(string path)
            {
                return new ZipArchive(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
            }
        }

        public static IEnumerable<TickerCandleHistory> EnumerateSelectedFromZipArchives(ZipArchive dailyUsZip, ZipArchive dailyWorldZip, ZipArchive dailyPlZip)
        {
            return DailyUs.EnumerateSelectedFromZipArchive(dailyUsZip)
                .Concat(DailyWorld.EnumerateSelectedFromZipArchive(dailyWorldZip))
                .Concat(DailyPl.EnumerateSelectedFromZipArchive(dailyPlZip));
        }

        private static IEnumerable<TickerCandleHistory> EnumerateSelectedFromZip(ZipArchive zip, (StockExchange, string, string[], Func<string, string>)[] selected)
        {
            foreach (var entry in zip.Entries)
            {
                string fullName = entry.FullName.Replace('\\', '/');
                foreach ((StockExchange exchange, string prefix, string[] tickers, Func<string, string> baseCurrency) in selected)
                {
                    if (fullName.StartsWith(prefix) && !fullName.EndsWith('/') && entry.Length != 0)
                    {
                        string name = entry.Name.ToUpperInvariant().Split('.')[0];
                        if (tickers.Contains(name))
                        {
                            yield return StooqData.LoadFromZipEntry(entry, exchange, baseCurrency);
                            break;
                        }
                    }
                }
            }
        }

        //        // currencies
        //             //D:\src\StockBacktesting\StockBacktesting\data\stooq\daily_world_txt.zip\data\daily\world\currencies\other\plnusd.txt
    }
}
