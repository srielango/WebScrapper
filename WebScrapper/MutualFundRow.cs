using System;
using System.Collections.Generic;

namespace WebScrapper
{
    public class MutualFundRow
    {
        public string InvestmentStrategy { get; set; }
        public string SchemeName { get; set; }
        public DateTime? InvestmentDate { get; set; }
        public double? InvestmentAmount { get; set; }
        public double? PresentValue { get; set; }
        public double? GainLoss { get; set; }
        public double? AnnualizedReturn { get; set; }
        public double CalculatedXirr { get; set; }
        public double? AbsoluteReturn { get; set; }
        public List<MFTransaction> Transactions { get; set; } = new List<MFTransaction>();
    }

    public class MFTransaction
    {
        public string TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public double Amount { get; set; }
    }
}
