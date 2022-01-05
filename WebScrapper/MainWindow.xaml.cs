using System.Windows;

namespace WebScrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnFunds_Click(object sender, RoutedEventArgs e)
        {
            MyOffice myOffice = new MyOffice();
            myOffice.Show();
        }

        private void BtnDGTest_Click(object sender, RoutedEventArgs e)
        {
            DataGridTest dataGridTest = new DataGridTest();
            dataGridTest.Show();
        }
    }
}
