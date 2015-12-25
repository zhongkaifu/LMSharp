using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace logFreq
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("logFreq.exe [input file] [output file]");
                return;
            }

            StreamReader sr = new StreamReader(args[0], Encoding.UTF8);
            StreamWriter sw = new StreamWriter(args[1], false, Encoding.UTF8);
            string strLine = null;

            while ((strLine = sr.ReadLine()) != null)
            {
                string[] cols = strLine.Split('\t');
                int freq = int.Parse(cols[1]);
                freq = (int)(Math.Log10(freq) + 1);

                sw.WriteLine("{0}\t{1}", cols[0], freq);
            }
            sr.Close();
            sw.Close();

        }
    }
}
