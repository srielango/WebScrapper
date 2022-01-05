using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WebScrapper
{
    /// <summary>
    /// Interaction logic for DataGridTest.xaml
    /// </summary>
    public partial class DataGridTest : Window
    {
        public List<Book> DG1TestData { get; set; } = new List<Book>();
        public List<Book> DG2TestData { get; set; } = new List<Book>();

        public DataGridTest()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DG1TestData.Add(new Book() { Id = 1, Price = 300.00, Title = "Sapiens" });
            DG1TestData.Add(new Book() { Id = 2, Price = 250, Title = "You can Heal Your Life" });
            DG1TestData.Add(new Book() { Id = 3, Price = 500, Title = "Happy for no reason" });

            DG2TestData.Add(new Book() { Id = 5, Price = 1000, Title = "Living with joy" });

            AllBooks.ItemsSource = DG1TestData;
            SelectedBooks.ItemsSource = DG2TestData;
        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            foreach (var selectedItem in AllBooks.SelectedItems)
            {
                var selectedBook = selectedItem as Book;
                DG2TestData.Add(new Book()
                {
                    Id = selectedBook.Id,
                    Title = selectedBook.Title,
                    Price = selectedBook.Price
                });
            }
            SelectedBooks.Items.Refresh();
        }
    }

    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double Price { get; set; }
    }
}
