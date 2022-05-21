using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting
{
    internal class TickerCandleHistory
    {
        // if used more than once
        private const string FOREX = "FOREX";

        // simplified name mappings (make sure to update methods below as well)
        private const string TVC = "COMMODITY";
        private const string IDC = FOREX;
        private const string FXCM = FOREX;
        private const string CBOE_BZX = "CBOE";
        private const string GPW = "WSE";

        public string TickerName { get; private set; }
        public string Exchange { get; private set; }
        public string BaseCurrency { get; private set; }
        public List<TickerCandle> Candles { get; } = new List<TickerCandle>();
        public TickerCandle FirstCandle => Candles[0];
        public TickerCandle LastCandle => Candles[Candles.Count - 1];

        public TickerCandleHistory(string tickerName)
        {
            string[] parts = tickerName.Split(", ", 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            TickerName = parts[0];
            Exchange = GetSimplifiedExchangeName(parts);
            BaseCurrency = GetCurrencyFromSimplifiedExchangeName(Exchange, TickerName);
        }

        public TickerCandle FindClosestCandleToTime(DateTime dateTimeUtc, TimeSpan maxDist)
        {
            int start = 0;
            int end = Candles.Count - 1;
            while (start < end)
            {
                int mid = (start + end) / 2;
                if (dateTimeUtc.CompareTo(Candles[mid].TimeUtc) <= 0)
                {
                    end = mid;
                }
                else
                {
                    if (start == mid)
                        break;

                    start = mid;
                }
            }

            if (start == Candles.Count - 1)
            {
                return GetIfWithinMaxDist(Candles[start], dateTimeUtc, maxDist);
            }
            else
            {
                var diffA = AbsTimeDiff(Candles[start].TimeUtc, dateTimeUtc);
                var diffB = AbsTimeDiff(Candles[start + 1].TimeUtc, dateTimeUtc);

                return (diffA < diffB) ?
                    GetIfWithinMaxDist(Candles[start], dateTimeUtc, maxDist) :
                    GetIfWithinMaxDist(Candles[start + 1], dateTimeUtc, maxDist);
            }
        }

        private static TimeSpan AbsTimeDiff(DateTime a, DateTime b)
        {
            TimeSpan diff = a - b;
            if (diff < TimeSpan.Zero)
            {
                diff = diff.Negate();
            }

            return diff;
        }

        private static TickerCandle GetIfWithinMaxDist(TickerCandle candle, DateTime dateTimeUtc, TimeSpan maxDist)
        {
            TimeSpan diff = AbsTimeDiff(candle.TimeUtc, dateTimeUtc);

            return (diff <= maxDist) ? candle : null;
        }

        private static string GetSimplifiedExchangeName(string[] parts)
        {
            if (parts.Length == 2)
            {
                return SimplifyExchangeName(parts[1]);
            }
            else
            {
                // most likely forex
                if (parts[0].Length == 6 && parts[0].StartsWith("USD") || parts[0].EndsWith("USD"))
                    return "FOREX";

                throw new Exception($"Unknown exchange for ticker '{parts[0]}'");
            }
        }

        private static string SimplifyExchangeName(string fullName)
        {
            switch (fullName)
            {
                case "TVC": return TVC;
                case "IDC": return IDC;
                case "FXCM": return FXCM;
                case "CBOE BZX": return CBOE_BZX;
                case "GPW": return GPW;
                default: throw new Exception($"TODO: Simplify full exchange name '{fullName}'");
            }
        }

        private static string GetCurrencyFromSimplifiedExchangeName(string exchangeName, string tickerName)
        {
            switch (exchangeName)
            {
                case TVC: return "USD";
                case FOREX:
                    {
                        if (tickerName.Length != 6)
                        {
                            throw new Exception($"Currency exchange ticker '{tickerName}' does not have standard format");
                        }

                        return tickerName.Substring(3);
                    }
                case CBOE_BZX: return "USD";
                case GPW: return "PLN";
                default: throw new Exception($"TODO: Unrecognized exchange name {exchangeName}");
            }
        }
    }
}
