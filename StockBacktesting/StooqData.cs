using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting
{
    internal static class StooqData
    {
        public static TickerCandleHistory LoadFromZip(ZipArchiveEntry zipEntry, string exchange, string baseCurrency = "USD")
        {
            using Stream s = zipEntry.Open();
            return LoadFromCsv(s, exchange, baseCurrency, debugInfo: zipEntry.FullName);
        }

        public static TickerCandleHistory LoadFromCsv(Stream csvStream, string exchange, string baseCurrency = "USD", string debugInfo = null)
        {
            IEnumerable<string[]> csvData = CsvReader.ReadCsv(csvStream);
            ColumnInfo colInfo = new(csvData.GetEnumerator(), debugInfo);

            TickerCandleHistory ticker = null;

            while (colInfo.MoveNext())
            {
                if (ticker == null)
                {
                    string simplifiedTickerName = colInfo.TickerName.Split('.')[0];
                    ticker = new TickerCandleHistory(colInfo.TickerName, simplifiedTickerName, exchange, baseCurrency);
                }
                else
                {
                    if (ticker.TickerFullName != colInfo.TickerName)
                    {
                        throw new Exception($"Rows contains data about different ticker. First row has '{ticker.TickerFullName}' but other has '{colInfo.TickerName}'");
                    }
                }

                TickerCandle candle = new TickerCandle()
                {
                    TimeUtc = ParseDateAndTime(colInfo.Date, colInfo.Time),
                    Open = ParseDecimal(colInfo.Open),
                    Close = ParseDecimal(colInfo.Close),
                    Low = ParseDecimal(colInfo.Low),
                    High = ParseDecimal(colInfo.High),
                };

                ticker.Candles.Add(candle);
            }

            ticker.Candles.Sort((a, b) => a.TimeUtc.CompareTo(b.TimeUtc));

            return ticker;
        }

        private static DateTime ParseDateAndTime(string date, string time)
        {
            DateOnly dateOnly = DateOnly.ParseExact(date, "yyyyMMdd");
            TimeOnly timeOnly = TimeOnly.ParseExact(time, "HHmmss");
            return dateOnly.ToDateTime(timeOnly, DateTimeKind.Utc);
        }

        private static decimal? ParseDecimal(string val)
        {
            return decimal.Parse(val);
        }

        class ColumnInfo
        {
            private IEnumerator<string[]> _csvEnumerator;
            private int _colCount;
            private int _tickerNameIdx = -1;
            private int _perIdx = -1;
            private int _dateIdx = -1;
            private int _timeIdx = -1;
            private int _openIdx = -1;
            private int _highIdx = -1;
            private int _lowIdx = -1;
            private int _closeIdx = -1;
            private int _volIdx = -1;
            private int _openIntIdx = -1;

            public string TickerName => ReadCell(_tickerNameIdx);
            public string Per => ReadCell(_perIdx);
            public string Date => ReadCell(_dateIdx);
            public string Time => ReadCell(_timeIdx);
            public string Open => ReadCell(_openIdx);
            public string High => ReadCell(_highIdx);
            public string Low => ReadCell(_lowIdx);
            public string Close => ReadCell(_closeIdx);
            public string Vol => ReadCell(_volIdx);
            public string OpenInt => ReadCell(_openIntIdx);

            public ColumnInfo(IEnumerator<string[]> csvEnumerator, string debugInfo = null)
            {
                if (!csvEnumerator.MoveNext())
                    throw new Exception($"CSV does not have any data. Debug info={debugInfo}");

                string[] columns = csvEnumerator.Current;
                _colCount = columns.Length;

                for (int i = 0; i < columns.Length; i++)
                {
                    AssignCol(i, columns[i]);
                }

                _csvEnumerator = csvEnumerator;
            }

            public bool MoveNext()
            {
                bool ret = _csvEnumerator.MoveNext();

                if (ret && _csvEnumerator.Current.Length != _colCount)
                {
                    throw new Exception($"Number of columns is {_colCount} but {_csvEnumerator.Current.Length} rows found.");
                }

                return ret;
            }

            private void AssignCol(int idx, string name)
            {
                switch (name)
                {
                    case "<TICKER>": Assign(ref _tickerNameIdx, idx); break;
                    case "<PER>": Assign(ref _perIdx, idx); break;
                    case "<DATE>": Assign(ref _dateIdx, idx); break;
                    case "<TIME>": Assign(ref _timeIdx, idx); break;
                    case "<OPEN>": Assign(ref _openIdx, idx); break;
                    case "<HIGH>": Assign(ref _highIdx, idx); break;
                    case "<LOW>": Assign(ref _lowIdx, idx); break;
                    case "<CLOSE>": Assign(ref _closeIdx, idx); break;
                    case "<VOL>": Assign(ref _volIdx, idx); break;
                    case "<OPENINT>": Assign(ref _openIntIdx, idx); break;
                    default: throw new Exception($"Column '{name}' not known");
                }
            }

            private static void Assign(ref int x, int val)
            {
                if (x != -1)
                    throw new Exception($"Value is already assigned!");
                x = val;
            }

            private string ReadCell(int idx)
            {
                if (idx == -1)
                    return null;

                return _csvEnumerator.Current[idx];
            }
        }
    }
}
