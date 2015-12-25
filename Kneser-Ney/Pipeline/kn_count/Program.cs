using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using AdvUtils;

namespace kn_count
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("kn_count.exe [input file] [output file] [ngram-order]");
                return;
            }

            int order = int.Parse(args[2]);
            if (order <= 0)
            {
                Console.WriteLine("Invalidated ngram-order.");
                return;
            }

            if (File.Exists(args[0]) == false)
            {
                Console.WriteLine("{0} isn't existed.", args[0]);
                return;
            }

            StreamReader sr = new StreamReader(args[0]);
            StreamWriter sw = new StreamWriter(args[1]);
            Dictionary<string, long> ngram = new Dictionary<string, long>();
            string strLine = null;
            while ((strLine = sr.ReadLine()) != null)
            {
                //Load raw count from corpus
                //Format: "word1 word2 ... wordN /t frequency"
                string[] record = strLine.Split('\t');

                string[] grams = record[0].Split();
                int size = grams.Length;

                //skip n-gram of the highest order or started with BOS
                if (size == order || grams[0] == "BOS")
                {
                    long freq = long.Parse(record[1]);
                    if (ngram.ContainsKey(record[0]) == false)
                    {
                        ngram.Add(record[0], freq);
                    }
                    else
                    {
                        ngram[record[0]] += freq;
                    }
                }

                if (size > 1)
                {
                    string contextNGram = String.Join(" ", grams, 1, grams.Length - 1);
                    if (ngram.ContainsKey(contextNGram) == false)
                    {
                        ngram.Add(contextNGram, 1);
                    }
                    else
                    {
                        ngram[contextNGram]++;
                    }
                }
            }

            //Generated ngram
            //Format: "word1 word2 ... wordN \t frequency"
            foreach (KeyValuePair<string, long> pair in ngram)
            {
                sw.WriteLine("{0}\t{1}", pair.Key, pair.Value);
            }

            sr.Close();
            sw.Close();

        }
    }
}
