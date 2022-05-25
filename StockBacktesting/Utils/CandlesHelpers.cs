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
        public static int FindFirstNotBeforeWithin(this List<TickerCandle> candles, DateTime dateTimeUtc, TimeSpan maxTimeAfter)
        {
            int idx = candles.FindFirstNotBefore(dateTimeUtc);
            return idx == -1 ? -1 : GetIfWithinMaxDist(candles, idx, dateTimeUtc, maxTimeAfter);
        }

        public static int FindFirstNotBefore(this List<TickerCandle> candles, DateTime dateTimeUtc)
        {
            TickerCandle first = candles[0];
            if (first.TimeUtc >= dateTimeUtc)
            {
                // first candle is not before
                return 0;
            }

            // Longest tick is usually 1M but let's use 60 days to be sure
            int searchStart = Math.Max(0, candles.FindClosestToTime(dateTimeUtc, TimeSpan.FromDays(60)) - 2);
            for (int i = searchStart; i < candles.Count; i++)
            {
                if (candles[i].TimeUtc >= dateTimeUtc)
                    return i;
            }

            throw new Exception("TODO: this could be a bug we have at least one candle before asked date but none after");
            // return -1;
        }

        public static int FindClosestToTime(this List<TickerCandle> candles, DateTime dateTimeUtc, TimeSpan maxDist)
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
                return GetIfWithinMaxDist(candles, start, dateTimeUtc, maxDist);
            }
            else
            {
                var diffA = AbsTimeDiff(candles[start].TimeUtc, dateTimeUtc);
                var diffB = AbsTimeDiff(candles[start + 1].TimeUtc, dateTimeUtc);

                return diffA < diffB ?
                    GetIfWithinMaxDist(candles, start, dateTimeUtc, maxDist) :
                    GetIfWithinMaxDist(candles, start + 1, dateTimeUtc, maxDist);
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

        private static int GetIfWithinMaxDist(List<TickerCandle> candles, int idx, DateTime dateTimeUtc, TimeSpan maxDist)
        {
            TickerCandle candle = candles[idx];
            TimeSpan diff = AbsTimeDiff(candle.TimeUtc, dateTimeUtc);

            return diff <= maxDist ? idx : -1;
        }
    }
}
