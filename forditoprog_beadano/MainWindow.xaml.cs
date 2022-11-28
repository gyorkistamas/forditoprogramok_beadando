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

        private OpenFileDialog file = new OpenFileDialog();
        public MainWindow()
        {
            InitializeComponent();
            labelCorrect.Content = "";
        }

        private void LoadTable(object sender, RoutedEventArgs e)
        {
            file.ShowDialog();
            labelOpenedTable.Content = "Megnyitott táblázat: " + file.FileName;
            if (file.FileName != "")
                StackAutomaton.ReadTable(file.FileName, dataGridTable);
        }

        private void StartCheck(object sender, RoutedEventArgs e)
        {
            labelCorrect.Content = "";
            textboxAppliedRules.Text = "";
            bool success = StackAutomaton.Start(txtBoxInput.Text, checkRemoveNums);


            labelTransformedText.Content = $"Az átalakított szöveg: {StackAutomaton.Input}";

            if (success)
            {
                labelCorrect.Foreground = Brushes.Green;
                labelCorrect.Content = "A kifejezés helyes!";
            }
            else if (StackAutomaton.State == "error")
            {
                labelCorrect.Foreground = Brushes.Red;
                if (txtBoxInput.Text != "" && StackAutomaton.Rules is not null)
                {
                    labelCorrect.Content = "A kifejezés helytelen!";
                }
            }

            if (StackAutomaton.Transitions is not null && txtBoxInput.Text != "")
            {
                foreach (var item in StackAutomaton.Transitions)
                {
                    textboxAppliedRules.Text = textboxAppliedRules.Text + $"\n{item}";
                }
            }
        }

        private void Savetable(object sender, RoutedEventArgs e)
        {
            StackAutomaton.WriteTable(file.FileName);
        }
    }
}
