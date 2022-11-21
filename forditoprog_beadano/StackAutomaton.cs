﻿using System;
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
    public static class StackAutomaton
    {
        public static DataTable Rules;
        private static Stack<string> StackCheck;
        private static string State;
        

        public static string Input { get; private set; }

        public static bool IsTableRead { get; private set; }

        public static List<string> Transitions { get; private set; }

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
            catch (Exception e)
            {
                MessageBox.Show($"Hiba a fájl beolvasása közben: {e.Message}");
            }
            finally
            {
                if (reader is not null)
                    reader.Close();
            }

        }


        public static void WriteTable(string file)
        {
            StreamWriter sw = new StreamWriter(file, false);

            for (int i = 0; i < Rules.Rows.Count; i++)
            {
                string line = "";
                for (int j = 0; j < Rules.Columns.Count; j++)
                {
                    line += $"{Rules.Rows[i][j]};";
                }

                sw.WriteLine(line);
            }

            sw.Close();
        }


        public static bool Start(string input)
        {
            if (input == "" || input is null)
            {
                MessageBox.Show("Adja meg az elemezendő szöveget!");
                return false;
            }

            if (!IsTableRead)
            {
                MessageBox.Show("Töltsön be egy szimbólumtáblázatot!");
                return false;
            }

            Input = input;
            TransformInput();


            StackCheck = new Stack<string>();
            StackCheck.Push("#");
            StackCheck.Push((string)Rules.Rows[1][0]);
            State = "Working";
            Transitions = new List<string>();
            Analyze();

            if (State == "Accept")
                return true;
            else
                return false;
        }


        private static void TransformInput()
        {
            Input = Regex.Replace(Input, @"[0-9]+", "i" );

            if (Input[Input.Length -1] != '#')
            {
                Input += "#";
            }
        }


        private static void Analyze()
        {
            string input = Input;

            Transitions.Add($"{Input},{StackCheck.Peek()}#,");


            while (State != "Deny" && State != "Error" && input.Length != 0)
            {
                string stackItem = StackCheck.Pop();
                string inputItem = Convert.ToString(input[0]);

                string rule = GetCell(inputItem, stackItem);

                string[] toPush = rule.Split(',');


                if (rule == "accept")
                {
                    State = "Accept";
                    Transitions.Add("accept");
                    return;
                }

                else if (rule == "pop")
                {
                    input = input.Remove(0, 1);
                }
                else if (rule == "")
                {
                    State = "error";
                    Transitions.Add("error");
                    return;
                }
                else
                { 

                    for (int i = toPush[0].Length - 1; i >= 0; i--)
                    {
                        StackCheck.Push(Convert.ToString(toPush[0][i]));
                    }
                }
                string[] prevMessage = Transitions.Last().Split(',');

                prevMessage[0] = input;

                prevMessage[1] = prevMessage[1].Remove(0, 1);

                if (toPush[0] != "pop")
                {
                    prevMessage[1] = prevMessage[1].Insert(0, toPush[0]);
                    prevMessage[2] = prevMessage[2] + toPush[1];
                }

                string newMessage = prevMessage[0] + ',' + prevMessage[1] + ',' + prevMessage[2];

                Transitions.Add(newMessage);
            }

        }
    }
}
