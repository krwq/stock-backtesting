using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBacktesting.Strategies
{
    internal class StrategyIncomeEveryMonth
    {
        public static Func<DateTime, decimal> GetConstIncomeFunc(decimal monthlyIncome) => (time) => monthlyIncome;
        public static Func<DateTime, decimal> GetYearlyIncomeIncreaseFunc(decimal monthlyStartIncome, int startYear, double yearlyPercentageRaise)
        {
            double raiseRatio = 1.0 + yearlyPercentageRaise / 100.0;
            return (timeUtc) =>
            {
                int yearsPassed = timeUtc.Year - startYear;
                return monthlyStartIncome * (decimal)Math.Pow(raiseRatio, yearsPassed);
            };
        }
    }
}
