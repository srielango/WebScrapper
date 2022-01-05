using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;

namespace WebScrapper
{

    /// </summary>
    public partial class MyOffice : Window
    {
        public List<MutualFundRow> MutualFundList { get; set; } = new List<MutualFundRow>();
        public List<MutualFundRow> SelectedFundList { get; set; } = new List<MutualFundRow>();
        public MutualFundRow MutualFundSummary { get; set; } = new MutualFundRow();
        public MutualFundRow SelectedMutualFundSummary { get; set; } = new MutualFundRow();

        private string siteUrl = "https://my-eoffice.com/client/index.php";

        private MutualFundService _mutualFundService = new MutualFundService();

        public MyOffice()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void BtnGetFunds_Click(object sender, RoutedEventArgs e)
        {
            ScrapeWebsite();
        }
        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            foreach (var selectedItem in MfTable.SelectedItems)
            {
                var selectedFund = selectedItem as MutualFundRow;
                SelectedFundList.Add(selectedFund);
            }

            SelectedMutualFundSummary = _mutualFundService.GetSummaryRow(SelectedFundList);
            lblSelectedInvestmentAmount.Text = GetValue(SelectedMutualFundSummary.InvestmentAmount);
            lblSelectedPresentValue.Text = GetValue(SelectedMutualFundSummary.PresentValue);
            lblSelectedGainLoss.Text = GetValue(SelectedMutualFundSummary.GainLoss);
            //lblSelectedAnnualizedReturn.Text = GetValue(SelectedMutualFundSummary.AnnualizedReturn);
            lblSelectedCalculatedXirr.Text = GetValue(SelectedMutualFundSummary.CalculatedXirr);
            lblSelectedAbsoluteReturn.Text = GetValue(SelectedMutualFundSummary.AbsoluteReturn);

            SelectedMF.ItemsSource = SelectedFundList;
            SelectedMF.Items.Refresh();
        }

        private void btnCleanSelection_Click(object sender, RoutedEventArgs e)
        {
            SelectedFundList = new List<MutualFundRow>();
            SelectedMF.ItemsSource = SelectedFundList;

            SelectedMF.Items.Refresh();

            lblSelectedInvestmentAmount.Text = "0.00";
            lblSelectedPresentValue.Text = "0.00";
            lblSelectedGainLoss.Text = "0.00";
            //lblSelectedAnnualizedReturn.Text = "0.00";
            lblSelectedCalculatedXirr.Text = "0.00";
            lblSelectedAbsoluteReturn.Text = "0.00";
        }

        private void MfTable_LayoutUpdated(object sender, EventArgs e)
        {
            Thickness t = lblSummary.Margin;
            t.Left = (MfTable.Columns[0].ActualWidth);
            lblSummary.Margin = t;
            lblSummary.Width = MfTable.Columns[1].ActualWidth;
            lblInvestmentAmount.Width = MfTable.Columns[2].ActualWidth;
            lblPresentValue.Width = MfTable.Columns[3].ActualWidth;
            lblGainLoss.Width = MfTable.Columns[4].ActualWidth;
            //lblAnnualizedReturn.Width = MfTable.Columns[5].ActualWidth;
            lblCalculatedXirr.Width = MfTable.Columns[5].ActualWidth;
            lblAbsoluteReturn.Width = MfTable.Columns[6].ActualWidth;
        }
        private void SelectedMF_LayoutUpdated(object sender, EventArgs e)
        {
            Thickness t = lblSummary.Margin;
            t.Left = (SelectedMF.Columns[0].ActualWidth);
            lblSelectedSummary.Margin = t;
            lblSelectedSummary.Width = SelectedMF.Columns[1].ActualWidth;
            lblSelectedInvestmentAmount.Width = SelectedMF.Columns[2].ActualWidth;
            lblSelectedPresentValue.Width = SelectedMF.Columns[3].ActualWidth;
            lblSelectedGainLoss.Width = SelectedMF.Columns[4].ActualWidth;
            //lblSelectedAnnualizedReturn.Width = SelectedMF.Columns[5].ActualWidth;
            lblSelectedCalculatedXirr.Width = SelectedMF.Columns[5].ActualWidth;
            lblSelectedAbsoluteReturn.Width = SelectedMF.Columns[6].ActualWidth;
        }
        private void ScrapeWebsite()    //Works!
        {
            var appSettings = ConfigurationManager.AppSettings;

            var options = new ChromeOptions()
            {
                BinaryLocation = appSettings["BinaryLocation"]
            };

            options.AddArguments(new List<string>() { "headless", "disable-gpu" });
            var driver = new ChromeDriver(options);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            driver.Navigate().GoToUrl(siteUrl);
            var loginId = driver.FindElementByName("uName");
            loginId.SendKeys(appSettings["MfLogin"]);
            var pwd = driver.FindElementByName("password");
            pwd.SendKeys(appSettings["MfPassword"]);
            var loginButton = driver.FindElementByName("button2");
            loginButton.Click();
            var valueReportUrl = "https://my-eoffice.com/client/valueReport.php";
            driver.Navigate().GoToUrl(valueReportUrl);
            var viewElement = driver.FindElementByName("view");
            new SelectElement(viewElement).SelectByValue("1");

            By elementName = By.CssSelector("#bcode");
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(elementName));

            var result = _mutualFundService.FindSelectElementWhenPopulated(driver, By.Name("bcode"), 5);

            if (result != null)
            {
                result.SelectByIndex(1);
            }

            var all_trnsElements = driver.FindElements(By.Name("all_trns"));
            all_trnsElements[1].Click();

            var showButton = driver.FindElementByName("name_submit");
            var parent = driver.CurrentWindowHandle;

            showButton.Click();

            foreach (var windowHandle in driver.WindowHandles)
            {
                if (parent != windowHandle)
                {
                    driver.SwitchTo().Window(windowHandle);
                    break;
                }
            }
            var mainTable = driver.FindElements(By.ClassName("table")).FirstOrDefault();
            var rows = mainTable.FindElements(By.ClassName("graw-bg")); //mainTable.FindElements(By.XPath("//*[@class='graw-bg']/tr"));

            MfTable.ItemsSource = MutualFundList;

            foreach (var row in rows)
            {
                var mf = _mutualFundService.GetRow(driver, row);
                if (mf != null)
                {
                    MutualFundList.Add(mf);
                    MfTable.Items.Refresh();
                }
            }

            MutualFundSummary = _mutualFundService.GetSummaryRow(MutualFundList);
            lblInvestmentAmount.Text = GetValue(MutualFundSummary.InvestmentAmount);
            lblPresentValue.Text = GetValue(MutualFundSummary.PresentValue);
            lblGainLoss.Text = GetValue(MutualFundSummary.GainLoss);
            //lblAnnualizedReturn.Text = GetValue(MutualFundSummary.AnnualizedReturn);
            lblCalculatedXirr.Text = GetValue(MutualFundSummary.CalculatedXirr);
            lblAbsoluteReturn.Text = GetValue(MutualFundSummary.AbsoluteReturn);

            driver.Quit();
        }
        private string GetValue(double? doubleValue)
        {
            return doubleValue.HasValue ? doubleValue.Value.ToString("#,#.##") : "0.00";
        }
    }
}
