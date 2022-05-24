using StockBacktesting.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.Utils
{
    internal static class CandlesHelpers
    {
        public static TickerCandle FindClosestToTime(this List<TickerCandle> candles, DateTime dateTimeUtc, TimeSpan maxDist)
        {
            int start = 0;
            int end = candles.Count - 1;
            while (start < end)
            {
                int mid = (start + end) / 2;
                if (dateTimeUtc.CompareTo(candles[mid].TimeUtc) <= 0)
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

            if (start == candles.Count - 1)
            {
                return GetIfWithinMaxDist(candles[start], dateTimeUtc, maxDist);
            }
            else
            {
                var diffA = AbsTimeDiff(candles[start].TimeUtc, dateTimeUtc);
                var diffB = AbsTimeDiff(candles[start + 1].TimeUtc, dateTimeUtc);

                return diffA < diffB ?
                    GetIfWithinMaxDist(candles[start], dateTimeUtc, maxDist) :
                    GetIfWithinMaxDist(candles[start + 1], dateTimeUtc, maxDist);
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

            return diff <= maxDist ? candle : null;
        }
    }
}
