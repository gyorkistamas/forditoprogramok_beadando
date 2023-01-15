using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace forditoprog_beadano
{
    public static class Automaton
    {
        /// <summary>
        /// A nyelvtan szabályait tartalmazó táblázat
        /// </summary>
        public static DataTable Rules;

        /// <summary>
        /// Az automata verme
        /// </summary>
        private static Stack<string> StackCheck;

        /// <summary>
        /// Az automata állapota a számítás végén (Elfogad, Hiba ...)
        /// </summary>
        public static string State;
        
        /// <summary>
        /// A bemeneti szöveg
        /// </summary>
        public static string Input { get; private set; }

        /// <summary>
        /// Logikai változó, annak tárolására, hogy lett-e táblázat beolvasva
        /// </summary>
        public static bool IsTableRead { get; private set; }

        /// <summary>
        /// Lista, amiben az automata egyes lépései vannak tárolva (input szalagon maradt szöveg, verem tartalma, alkalmazott szabályok sorszáma
        /// </summary>
        public static List<string> Transitions { get; private set; }

        static Automaton()
        {
            IsTableRead = false;
        }

        /// <summary>
        /// Szabály kikeresése a táblázatból
        /// </summary>
        /// <param name="col">Input szalagról olvasott karakter</param>
        /// <param name="line">Veremből olvasott szabály</param>
        /// <returns>A megtalált szabály, vagy üres string, ha nem sikerült megtalálni</returns>
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

        /// <summary>
        /// Táblázat beolvasása csv fájlból
        /// </summary>
        /// <param name="file">A fájl elérési útja</param>
        /// <param name="grid">A táblázat refernciája, amibe kerül a beolvasott adat</param>
        public static void ReadTable(string file, DataGrid grid)
        {
            IsTableRead = false;
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
                    string line = reader.ReadLine();
                    if (line == "")
                        break;

                    string[] cells = line.Split(';');
                    
                    Rules.Rows.Add(cells);
                }

                IsTableRead = true;
                grid.ItemsSource = Rules.DefaultView;


            }
            catch (IndexOutOfRangeException e)
            {
                MessageBox.Show($"A fájl formátuma nem megfelelő:\n{e.Message}", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Hiba a fájl beolvasása közben: {e.Message}", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (reader is not null)
                    reader.Close();
            }

        }

        /// <summary>
        /// Táblázat mentése vissza fájlba
        /// </summary>
        /// <param name="file">A fájl elérési útja, ahove mentése kerül.</param>
        public static void WriteTable(string file)
        {

            if (!IsTableRead)
            {
                MessageBox.Show("Nincsen betöltve táblázat!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            StreamWriter sw = new StreamWriter(file, false);

            for (int i = 0; i < Rules.Rows.Count; i++)
            {
                string line = "";
                for (int j = 0; j < Rules.Columns.Count; j++)
                {
                    line += $"{Rules.Rows[i][j]};";
                }

                line = line.TrimEnd(';');

                sw.WriteLine(line);
            }

            sw.Close();
        }


        /// <summary>
        /// A vizsgálat elindítása
        /// </summary>
        /// <param name="input">A bemeneti szöveg</param>
        /// <param name="transformNum">Az ablakban megjelenő gomb referenciája</param>
        /// <returns>Igaz vagy hamis érték, annak függvényében, hogy elfogadta-e a szót, vagy sem</returns>
        public static bool Start(string input, CheckBox transformNum)
        {
            if (input == "" || input is null)
            {
                MessageBox.Show("Adja meg az elemezendő szöveget!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!IsTableRead)
            {
                MessageBox.Show("Töltsön be egy szimbólumtáblázatot!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Input = input;
            TransformInput(transformNum.IsChecked);


            StackCheck = new Stack<string>();
            StackCheck.Push("#");
            StackCheck.Push((string)Rules.Rows[1][0]);
            State = "Working";
            Transitions = new List<string>();
            Analyze();

            return State == "Accept";
        }

        /// <summary>
        /// Szöveg átalakítása (Ha nincs a végén #, akkor odarakjuk, illetve ha kell, akkor a számokat kicseréljük "i"-re)
        /// </summary>
        /// <param name="transformNums"></param>
        private static void TransformInput(bool? transformNums)
        {
            if ((bool)transformNums)
            {
                Input = Regex.Replace(Input, @"[0-9]+", "i");
            }

            if (Input[Input.Length -1] != '#')
                Input += "#";
        }

        /// <summary>
        /// Az elemzést végző metódus
        /// </summary>
        private static void Analyze()
        {
            string input = Input;

            Transitions.Add($"{Input},{StackCheck.Peek()}#,");

            while (State != "Deny" && State != "Error" && input.Length != 0)
            {
                string stackItem = StackCheck.Pop();
                string inputItem = Convert.ToString(input[0]);

                string rule = GetCell(inputItem, stackItem);

                switch (rule)
                {
                    case "accept":
                        State = "Accept";
                        Transitions.Add("accept");
                        return;

                    case "pop":
                        input = input.Remove(0, 1);
                        UpdateTransitions(input);
                        break;

                    case "":
                        State = "error";
                        Transitions.Add("error");
                        return;

                    default:
                        ProcessRule(rule, input);
                        break;
                } 
            }

        }

        /// <summary>
        /// Szabály feldolgozása, ellenőrzése
        /// </summary>
        /// <param name="rule">A feldolgozandó sazbály</param>
        /// <param name="input">A bemenit szöveg, ahol jelenleg járunk a feldolgozásban</param>
        private static void ProcessRule(string rule, string input)
        {
            string[] toPush = rule.Split(',');

            if (toPush.Length != 2 || toPush[0] == "")
            {
                MessageBox.Show($"A szabály formátuma nem megfelelő: {rule}", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                State = "Error";
                Transitions.Add("error");
                return;
            }


            for (int i = toPush[0].Length - 1; i >= 0; i--)
            {
                if (toPush[0][i] != 'ε')
                {
                    if (toPush[0][i] == '\'')
                    {
                        string ruleToPush = Convert.ToString(toPush[0][i - 1]) + Convert.ToString(toPush[0][i]);
                        StackCheck.Push(ruleToPush);
                        i--;
                    }
                    else
                    {
                        StackCheck.Push(Convert.ToString(toPush[0][i]));
                    }
                }
            }

            string[] prevMessage = Transitions.Last().Split(',');

            prevMessage[0] = input;

            prevMessage[1] = prevMessage[1].Remove(0, 1);
            if (prevMessage[1][0] == '\'')
            {
                prevMessage[1] = prevMessage[1].Remove(0, 1);
            }

            if (toPush[0][0] != 'ε')
            {
                prevMessage[1] = prevMessage[1].Insert(0, toPush[0]);
            }
            prevMessage[2] = prevMessage[2] + toPush[1];

            string newMessage = prevMessage[0] + ',' + prevMessage[1] + ',' + prevMessage[2];

            Transitions.Add(newMessage);
        }

        /// <summary>
        /// Az átmenetek lista frissítése
        /// </summary>
        /// <param name="input">Az eddig feldolgozott bementi szöveg</param>
        private static void UpdateTransitions(string input)
        {
            string[] prevMessage = Transitions.Last().Split(',');

            prevMessage[0] = input;

            prevMessage[1] = prevMessage[1].Remove(0, 1);

            string newMessage = prevMessage[0] + ',' + prevMessage[1] + ',' + prevMessage[2];

            Transitions.Add(newMessage);
        }

    }
}
