using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace forditoprog_beadano
{
    public static class StackAutomaton
    {
        public static DataTable Rules;

        public static bool IsTableRead { get; private set; }

        static StackAutomaton()
        {
            IsTableRead = false;
        }


        public static string GetCell(string col, string line)
        {
            int colnum = -1;
            int rownum = -1;


            for (int i = 0; i < Rules.Columns.Count; i++)
            {
                if ((string)Rules.Rows[0][i] == col)
                {
                    colnum= i;
                    break;
                }
            }

            for (int i = 0; i < Rules.Rows.Count; i++)
            {
                if ((string)Rules.Rows[i][0] == line)
                {
                    rownum = i;
                    break;
                }
            }

            if (colnum == -1 || rownum == -1)
                return "";

            return (string)Rules.Rows[rownum][colnum];
        }


        public static void ReadTable(string file, DataGrid grid)
        {
            Rules = new DataTable();
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(file);
                Rules.Columns.Add("-");
                string[] firstLine = reader.ReadLine().Split(';');
                for (int i = 1; i < firstLine.Length; i++)
                {
                    Rules.Columns.Add($"Szimbólum {i}", typeof(string));
                }

                Rules.Rows.Add(firstLine);

                while (!reader.EndOfStream)
                {
                    string[] line = reader.ReadLine().Split(';');
                    Rules.Rows.Add(line);
                }

                IsTableRead = true;
                grid.ItemsSource = Rules.DefaultView;


            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("A fájl formátuma nem megfelelő!");
            }
            //catch
            //{
            //    MessageBox.Show("Hiba a fájl beolvasása közben!");
            //}
            finally
            {
                if (reader is not null)
                    reader.Close();
            }

        }
    }
}
