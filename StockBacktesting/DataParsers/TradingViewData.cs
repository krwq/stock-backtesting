using StockBacktesting.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StockBacktesting.DataParsers
{
    internal static class TradingViewData
    {
        public static TickerCandleHistory[] LoadFromCsvFile(string path, string mainTickerName, TimeFormat timeFormat = TimeFormat.ISO)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return LoadFromCsv(fs, mainTickerName, timeFormat);
            }
        }

        public static TickerCandleHistory[] LoadFromCsv(Stream csvStream, string mainTickerName, TimeFormat timeFormat = TimeFormat.ISO)
        {
            Dictionary<string, TickerCandleHistory> ret = new();
            IEnumerable<string[]> csvData = CsvReader.ReadCsv(csvStream);
            IEnumerator<string[]> csvEnumerator = csvData.GetEnumerator();
            Action<TickerCandle, string> timeSetter = ValueTypeToColumnSetter(ValueType.Time, timeFormat);
            if (!csvEnumerator.MoveNext())
                throw new Exception($"CSV does not have any data");

            string[] columns = csvEnumerator.Current;
            var columnInfo = new (TickerCandleHistory, ValueType, Action<TickerCandle, string>)[columns.Length];
            int colIdx = 0;
            foreach (var column in columns)
            {
                (string tickerName, ValueType valueType) = GetPriceTypeAndTickerName(column, mainTickerName);

                TickerCandleHistory hist;
                if (!ret.TryGetValue(tickerName, out hist))
                {
                    hist = CreateCandleHistory(tickerName);
                    ret[tickerName] = hist;
                }

                columnInfo[colIdx] = (hist, valueType, ValueTypeToColumnSetter(valueType, timeFormat));
                colIdx++;
            }

            // TODO: validate columnInfo is complete (each ticker has all types of prices)

            while (csvEnumerator.MoveNext())
            {
                foreach (var kv in ret)
                {
                    kv.Value.Candles.Add(new TickerCandle());
                }

                string[] row = csvEnumerator.Current;
                if (row.Length != columns.Length)
                {
                    throw new Exception($"Number of columns is {columns.Length} but {row.Length} rows found.");
                }

                string dateTimeValue = null;
                for (int i = 0; i < row.Length; i++)
                {
                    string val = row[i];
                    (TickerCandleHistory hist, ValueType valueType, Action<TickerCandle, string> setter) = columnInfo[i];
                    if (valueType == ValueType.Time)
                    {
                        dateTimeValue = val;
                        continue;
                    }

                    setter(hist.LastCandle, val);
                }

                if (dateTimeValue != null)
                {
                    foreach (var kv in ret)
                    {
                        timeSetter(kv.Value.LastCandle, dateTimeValue);
                    }
                }
                else
                {
                    throw new Exception($"No time column");
                }
            }

            foreach (var kv in ret)
            {
                kv.Value.Candles.Sort((a, b) => a.TimeUtc.CompareTo(b.TimeUtc));
            }

            return ret.Values.ToArray();
        }

        private static (string, ValueType) GetPriceTypeAndTickerName(string columnName, string mainTickerName)
        {
            string[] parts = columnName.Split(": ", 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
                throw new Exception($"Cannot parse column name: '{columnName}'");

            if (parts.Length == 1)
            {
                return (mainTickerName, GetPriceType(parts[0]));
            }
            else
            {
                return (parts[0], GetPriceType(parts[1]));
            }
        }

        private static ValueType GetPriceType(string priceTypeString)
        {
            switch (priceTypeString.ToLowerInvariant())
            {
                case "time": return ValueType.Time;
                case "open": return ValueType.Open;
                case "close": return ValueType.Close;
                case "low": return ValueType.Low;
                case "high": return ValueType.High;
                case "volume":
                case "volume MA":
                    return ValueType.Ignored;
                default: throw new Exception($"Unexpected price type '{priceTypeString}'");
            }
        }

        private static Action<TickerCandle, string> ValueTypeToColumnSetter(ValueType priceType, TimeFormat timeFormat)
        {
            if (timeFormat != TimeFormat.ISO)
                throw new Exception($"TODO: Time format {timeFormat} not supported.");

            switch (priceType)
            {
                case ValueType.Time: return (candle, value) => { candle.TimeUtc = DateTime.Parse(value, null, System.Globalization.DateTimeStyles.RoundtripKind); };
                case ValueType.Open: return (candle, value) => { candle.Open = ParseDecimal(value); };
                case ValueType.Close: return (candle, value) => { candle.Close = ParseDecimal(value); };
                case ValueType.Low: return (candle, value) => { candle.Low = ParseDecimal(value); };
                case ValueType.High: return (candle, value) => { candle.High = ParseDecimal(value); };
                case ValueType.Ignored: return (candle, value) => { };
                default: throw new Exception($"TODO: PriceTypeToColumnSetter did not handle '{priceType}'");
            }
        }

        private static decimal? ParseDecimal(string val)
        {
            if (val == "NaN")
            {
                return null;
            }
            else
            {
                return decimal.Parse(val);
            }
        }

        private static TickerCandleHistory CreateCandleHistory(string originalTickerName)
        {
            string[] parts = originalTickerName.Split(", ", 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string simplifiedTickerName = parts[0];
            StockExchange exchange = GetSimplifiedExchangeName(parts);
            string baseCurrency = GetCurrencyFromSimplifiedExchangeName(exchange, simplifiedTickerName);
            return new TickerCandleHistory(originalTickerName, simplifiedTickerName, exchange, baseCurrency);
        }

        private static StockExchange GetSimplifiedExchangeName(string[] parts)
        {
            if (parts.Length == 2)
            {
                // special handling for GOLD, PLATINUM, PALLADIUM, SILVER?
                return GetExchangeFromName(parts[1]);
            }
            else
            {
                // most likely forex
                if (parts[0].Length == 6 && parts[0].StartsWith("USD") || parts[0].EndsWith("USD"))
                    return StockExchange.Currency;

                throw new Exception($"Unknown exchange for ticker '{parts[0]}'");
            }
        }

        private static StockExchange GetExchangeFromName(string name)
        {
            switch (name)
            {
                case "TVC": return StockExchange.Commodity;
                case "FOREX": return StockExchange.Currency;
                case "IDC": return StockExchange.Currency;
                case "FXCM": return StockExchange.Currency;
                case "CBOE BZX": return StockExchange.NYSE;
                case "GPW": return StockExchange.WSE;
                default: throw new Exception($"TODO: Simplify full exchange name '{name}'");
            }
        }

        private static string GetCurrencyFromSimplifiedExchangeName(StockExchange exchange, string tickerName)
        {
            switch (exchange)
            {
                case StockExchange.Commodity: return "USD"; // ?
                case StockExchange.Currency:
                    {
                        if (tickerName.Length != 6)
                        {
                            throw new Exception($"Currency exchange ticker '{tickerName}' does not have standard format");
                        }

                        return tickerName.Substring(3);
                    }
                case StockExchange.NYSE: return "USD";
                case StockExchange.WSE: return "PLN";
                default: throw new Exception($"TODO: Unrecognized exchange name {exchange}");
            }
        }

        private enum ValueType
        {
            Time,
            Open,
            Close,
            Low,
            High,
            Ignored,
        }

        public enum TimeFormat
        {
            ISO
        }
    }
}
