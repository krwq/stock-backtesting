using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.Model
{
    internal enum StockExchange
    {
        Error, // so that default is not NYSE
        NYSE,
        WSE,
        Commodity,
        Currency, // also: gold, silver, platinum, palladium, SDRs
    }
}
