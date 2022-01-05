using Excel.FinancialFunctions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebScrapper
{
    public class MutualFundService
    {

        public MutualFundRow? GetRow(IWebDriver driver, IWebElement webRow)
        {
            var mfRow = new MutualFundRow();
            var cells = webRow.FindElements(By.TagName("td"));

            if (cells.Count == 0)
            {
                return null;
            }

            mfRow.SchemeName = cells[0].Text;
            mfRow.InvestmentAmount = Math.Round(Convert.ToDouble(cells[3].Text), 2);
            mfRow.InvestmentDate = Convert.ToDateTime(cells[1].Text);
            mfRow.PresentValue = Math.Round(Convert.ToDouble(cells[8].Text), 2);
            mfRow.GainLoss = Math.Round(Convert.ToDouble(cells[11].Text), 2);
            mfRow.AnnualizedReturn = Math.Round(Convert.ToDouble(cells[12].Text.Replace("%", "")), 2);
            mfRow.AbsoluteReturn = Math.Round(Convert.ToDouble(cells[13].Text.Replace("%", "")), 2);

            var transactions = GetTransactions(driver, webRow);
            if (transactions != null)
            {
                mfRow.Transactions = transactions;
                switch (transactions.FirstOrDefault().TransactionType)
                {
                    case "Purchase":
                    case "Remaining Purchase":
                        mfRow.InvestmentStrategy = "One Time";
                        break;
                    case "Switch In":
                        mfRow.InvestmentStrategy = "STP";
                        break;
                    case "STP In":
                        mfRow.InvestmentStrategy = "STP";
                        break;
                    case "SIP Purchase":
                        mfRow.InvestmentStrategy = "SIP";
                        break;
                }
            }

            var amountList = mfRow.Transactions.Select(x => x.Amount).ToList();
            amountList.Add(mfRow.PresentValue.GetValueOrDefault() * -1);
            var dateList = mfRow.Transactions.Select(x => x.TransactionDate).ToList();
            dateList.Add(DateTime.Now);

            if (amountList.Count > 1)
            {
                try
                {
                    mfRow.CalculatedXirr = Math.Round(Financial.XIrr(amountList, dateList, .3) * 100, 2);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return mfRow;
        }
        public MutualFundRow GetSummaryRow(List<MutualFundRow> mutualFunds)
        {
            var mfRow = new MutualFundRow
            {
                SchemeName = "Total: ",
                InvestmentAmount = mutualFunds.Sum(x => x.InvestmentAmount),
                PresentValue = mutualFunds.Sum(x => x.PresentValue),
                GainLoss = mutualFunds.Sum(x => x.GainLoss)
            };
            mfRow.AbsoluteReturn = (mfRow.PresentValue - mfRow.InvestmentAmount) / mfRow.InvestmentAmount * 100;

            var amountList = new List<double>();
            var dateList = new List<DateTime>();

            foreach (var mf in mutualFunds)
            {
                amountList.AddRange(mf.Transactions.Select(x => x.Amount).ToList());
                dateList.AddRange(mf.Transactions.Select(x => x.TransactionDate).ToList());
            }
            amountList.Add(mutualFunds.Sum(x => x.PresentValue).Value * -1);
            dateList.Add(DateTime.Now);
            if (amountList.Count > 1)
            {
                try
                {
                    mfRow.CalculatedXirr = Math.Round(Financial.XIrr(amountList, dateList, .3) * 100, 2);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return mfRow;
        }
        private List<MFTransaction>? GetTransactions(IWebDriver driver, IWebElement mfRow)
        {
            var mfTransactions = new List<MFTransaction>();
            var clickableCell = mfRow.FindElement(By.ClassName("w-172"));
            clickableCell.Click();

            var webRowId = mfRow.GetAttribute("id");
            if (webRowId == null)
            {
                return null;
            }

            var transactionTableBodyId = "innerTransaction_" + string.Join('_', webRowId.Split('_').Skip(1).ToArray());

            //var transactionTableRows = FindElements(driver, By.TagName("tr"), 5);
            var transactionTableRows = FindElements(driver, By.XPath($"//*[@id='{transactionTableBodyId}']/tr"), 5);
            //var transactionTableRows = transactionTableBody.FindElements(By.TagName("tr"));

            foreach (var row in transactionTableRows)
            {
                AddTransaction(mfTransactions, row);
            }

            return mfTransactions;
        }
        private void AddTransaction(List<MFTransaction> mFTransactions, IWebElement transactionRow)
        {
            List<string> ValidTranTypes = new List<string>()
            {
                "Remaining Purchase",
                "Switch In",
                "STP In",
                "SIP Purchase",
                "Purchase"
            };

            var cells = transactionRow.FindElements(By.TagName("td"));
            if (cells.Count == 0)
            {
                return;
            }
            if (ValidTranTypes.Contains(cells[1].Text) && cells[9].Text != "N.A.")
            {
                var transaction = new MFTransaction
                {
                    TransactionType = cells[1].Text,
                    TransactionDate = Convert.ToDateTime(cells[2].Text),
                    Amount = Convert.ToDouble(cells[3].Text)
                };
                mFTransactions.Add(transaction);
            }
        }
        public SelectElement FindSelectElementWhenPopulated(IWebDriver driver, By by, int delayInSeconds)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(delayInSeconds));
            return wait.Until<SelectElement>(drv =>
            {
                SelectElement element = new SelectElement(drv.FindElement(by));
                if (element.Options.Count >= 2)
                {
                    return element;
                }

                return null;
            }
            );
        }
        private IWebElement? FindElement(IWebDriver driver, By by, int delayInSeconds)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(delayInSeconds));
            var element = wait.Until(drv => drv.FindElement(by));

            return element;
        }
        private System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>? FindElements(IWebDriver driver, By by, int delayInSeconds)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(delayInSeconds));
            wait.Until(ExpectedConditions.ElementExists(by));
            var elements = wait.Until(drv => drv.FindElements(by));
            return elements;
        }
    }
}
