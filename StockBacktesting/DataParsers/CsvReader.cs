using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.DataParsers
{
    internal static class CsvReader
    {
        public static IEnumerable<string[]> ReadCsv(Stream stream)
        {
            using (StreamReader sr = new StreamReader(stream, leaveOpen: true))
            {
                while (true)
                {
                    string line = sr.ReadLine();
                    if (line == null)
                        break;

                    if (string.IsNullOrEmpty(line))
                    {
                        if (sr.ReadLine() != null)
                        {
                            throw new Exception($"Empty line in CSV found but it was not followed by EOF.");
                        }
                        break;
                    }

                    yield return SplitRow(line).ToArray();
                }
            }
        }

        private static IEnumerable<string> SplitRow(string line)
        {
            bool isQuoteOpen = false;
            bool isEscaping = false;
            bool expectingCommaOrEndOfLine = false;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (expectingCommaOrEndOfLine)
                {
                    if (c != ',')
                    {
                        throw new Exception($"Expected ',' but '{c}' found.");
                    }

                    expectingCommaOrEndOfLine = false;
                }
                else if (isEscaping)
                {
                    sb.Append(c);
                    isEscaping = false;
                }
                else if (c == '"')
                {
                    isQuoteOpen = !isQuoteOpen;
                    if (!isQuoteOpen)
                    {
                        yield return sb.ToString();
                        sb.Clear();
                        expectingCommaOrEndOfLine = true;
                    }
                    else
                    {
                        if (sb.Length != 0)
                        {
                            throw new Exception($"Invalid CSV: quote appeared in the middle of the string. CSV line: {line}");
                        }
                    }
                }
                else if (isQuoteOpen)
                {
                    if (c == '\\')
                    {
                        isEscaping = true;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        yield return sb.ToString();
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }
        }
    }
}
