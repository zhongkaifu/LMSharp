using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace removeOOV
{
    class Program
    {
        //Remove the query which contains the token whose frequency is less than the threshold.
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("removeOOV.exe [input file] [output file] [frequency threshold]");
                return;
            }

            Console.WriteLine("Computing word frequency...");
            int lowFreq = int.Parse(args[2]);
            Dictionary<string, int> dict2freq = new Dictionary<string, int>();
            string strLine = null;
            StreamReader sr = new StreamReader(args[0]);
            while ((strLine = sr.ReadLine()) != null)
            {
                //Format: sentence \t frequency
                string[] items = strLine.Split('\t');
                string[] tokens = items[0].Split();
                int freq = int.Parse(items[1]);
                foreach (string token in tokens)
                {
                    if (dict2freq.ContainsKey(token) == false)
                    {
                        dict2freq.Add(token, 0);
                    }
                    dict2freq[token] += freq;
                }
            }
            sr.Close();


            Console.WriteLine("Filter out sentences which contains words with lower frequency");
            StreamWriter sw = new StreamWriter(args[1]);
            sr = new StreamReader(args[0]);
            while ((strLine = sr.ReadLine()) != null)
            {
                string[] items = strLine.Split('\t');
                string[] tokens = items[0].Split();

                bool bDrop = false;
                foreach (string token in tokens)
                {
                    if (dict2freq[token] < lowFreq)
                    {
                        bDrop = true;
                        break;
                    }
                }

                if (bDrop == false)
                {
                    sw.WriteLine(strLine);
                }
            }
            sr.Close();
            sw.Close();
        }
    }
}
