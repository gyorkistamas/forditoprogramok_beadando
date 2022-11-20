using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace forditoprog_beadano
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

        private void LoadTable(object sender, RoutedEventArgs e)
        {
            var file = new OpenFileDialog();
            file.ShowDialog();
            labelOpenedTable.Content = "Megnyitott táblázat: " + file.FileName;
            if (file.FileName != "")
                StackAutomaton.ReadTable(file.FileName, dataGridTable);
        }

        private void StartCheck(object sender, RoutedEventArgs e)
        {
            string col = txtBoxInput.Text.Split(',')[0];
            string row = txtBoxInput.Text.Split(',')[1];

            string content = StackAutomaton.GetCell(col, row);

            MessageBox.Show(content);
        }
    }
}
