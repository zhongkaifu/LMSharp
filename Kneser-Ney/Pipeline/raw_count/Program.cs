using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using AdvUtils;

namespace raw_count
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Generate NGram data from give corpus");
                Console.WriteLine("raw_count.exe [input file] [output file] [ngram-order]");
                Console.WriteLine("  [input file] - the format is text line or text line with weight score");
                return;
            }

            int order = int.Parse(args[2]);
            if (order <= 0)
            {
                Console.WriteLine("[ngram-order] is invalidated");
                return;
            }

            if (File.Exists(args[0]) == false)
            {
                Console.WriteLine("{0} isn't existed.");
                return;
            }

            StreamReader sr = new StreamReader(args[0]);
            StreamWriter sw = new StreamWriter(args[1]);

            //ngram dictionary
            Dictionary<string, long> ngram = new Dictionary<string, long>();
            string strLine = null;
            long lineCnt = 0;

            while ((strLine = sr.ReadLine()) != null)
            {
                //Corpus format: "word1 word2 ... wordN \t freq"
                //if freq is empty, the default freq is 1
                string[] items = strLine.Split('\t');
                long freq = 1;
                if (items.Length == 2)
                {
                    freq = long.Parse(items[1]);
                }

                //Load data from corpus and word break as token list
                items[0] = "BOS " + items[0] + " EOS";
                string[] token = items[0].Split(' ');

                //Generate ngram
                for (int i = 0; i < order; ++i)
                {
                    for (int j = 0; j < token.Length - i; ++j)
                    {
                        //output ngram
                        string strNGram = String.Join(" ", token, j, i + 1);
                        if (ngram.ContainsKey(strNGram) == false)
                        {
                            ngram.Add(strNGram, freq);
                        }
                        else
                        {
                            ngram[strNGram] += freq;
                        }
                    }
                }

                lineCnt++;
                if (lineCnt % 100000 == 0)
                {
                    Console.Write("{0}...", lineCnt);
                }
            }


            //Find the min frequency
            long minFreq = long.MaxValue;
            foreach (KeyValuePair<string, long> pair in ngram)
            {
                if (pair.Value < minFreq)
                {
                    minFreq = pair.Value;
                }
            }

            //Generated format:
            //ngram "word1 word2 ... wordN \t (raw frequency) - (min frequency)"
            minFreq--;
            foreach (KeyValuePair<string, long> pair in ngram)
            {
                sw.WriteLine("{0}\t{1}", pair.Key, pair.Value - minFreq);
            }

            sr.Close();
            sw.Close();
        }
    }
}
