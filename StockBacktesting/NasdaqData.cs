using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting
{
    // https://data.nasdaq.com/tools/api
    // https://api.nasdaq.com/api/quote/MSFT/dividends?assetclass=stocks
    internal class NasdaqData
    {
        public static async Task<DividendHistory> GetDividendHistoryAsync(string tickerName)
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            string requestUri = $"https://api.nasdaq.com/api/quote/{tickerName}/dividends?assetclass=stocks";
            //Console.WriteLine($"GET: {requestUri}");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:100.0) Gecko/20100101 Firefox/100.0");
            request.Version = HttpVersion.Version30; // Hangs without it

            HttpResponseMessage resp = await client.SendAsync(request);
            if (!resp.IsSuccessStatusCode)
                return null;

            string json = await resp.Content.ReadAsStringAsync();
            return GetDividendHistoryFromJsonString(tickerName, json);
        }

        private static DividendHistory GetDividendHistoryFromJsonString(string tickerName, string json)
        {
            DividendHistory ret = new DividendHistory(tickerName);

            JObject o = JObject.Parse(json);
            JArray dividends = (JArray)o["data"]["dividends"]["rows"];
            foreach (var dividend in dividends)
            {
                Dividend div = new();

                div.ExDate = ParseUsFormatDate(dividend["exOrEffDate"].Value<string>());
                div.Type = ParseDividendType(dividend["type"].Value<string>());
                div.Amount = ParseAmount(dividend["amount"].Value<string>());
                div.DeclarationDate = ParseUsFormatDate(dividend["declarationDate"].Value<string>());
                div.RecordDate = ParseUsFormatDate(dividend["recordDate"].Value<string>());
                div.PaymentDate = ParseUsFormatDate(dividend["paymentDate"].Value<string>());

                ret.Dividends.Add(div);
            }

            return ret;
        }

        private static DateOnly? ParseUsFormatDate(string date)
        {
            if (date == "N/A")
                return null;

            return DateOnly.ParseExact(date, "MM/dd/yyyy");
        }

        private static decimal? ParseAmount(string amount)
        {
            if (!amount.StartsWith('$'))
                throw new Exception($"Amount '{amount}' does not start with '$'.");

            return decimal.Parse(amount.Substring(1));
        }

        private static Dividend.DividendType ParseDividendType(string dividendType)
        {
            switch (dividendType)
            {
                case "CASH": return Dividend.DividendType.Cash;
                default: throw new Exception($"Unrecognized dividend type '{dividendType}'");
            }
        }
    }
}
