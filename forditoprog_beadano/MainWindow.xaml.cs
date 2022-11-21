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
            labelCorrect.Content = "";
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
            bool success = StackAutomaton.Start(txtBoxInput.Text);


            labelTransformedText.Content = $"Az átalakított szöveg: {StackAutomaton.Input}";

            if (success)
            {
                labelCorrect.Foreground = Brushes.Green;
                labelCorrect.Content = "A kifejezés helyes!";
            }
            else
            {
                labelCorrect.Foreground = Brushes.Red;
                labelCorrect.Content = "A kifejezés helytelen!";
            }
        }
    }
}
