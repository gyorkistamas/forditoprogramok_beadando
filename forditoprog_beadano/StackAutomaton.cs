using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace forditoprog_beadano
{
    public static class StackAutomaton
    {
        public static Dictionary<string, Dictionary<string, string>> Rules;


        public static bool IsTableRead { get; private set; }


        static StackAutomaton()
        {
            IsTableRead = false;
        }

        public static void Readtable(string file)
        {
            Rules = new Dictionary<string, Dictionary<string, string>>();
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(file);

            }
            catch 
            {
                MessageBox.Show("Hiba a fájl beolvasása közben!");
            }
            finally
            {
                if (reader is not null)
                    reader.Close();
            }

        }
    }
}
