using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace kn_stat
{
    class Program
    {
        static void Main(string[] args)
        {
            const int LM_ORDER = 9;
            const int KN_STAT_ORDER = 100;    ///< kn stat order

            if (args.Length != 3)
            {
                Console.WriteLine("kn_stat.exe [input file] [output file] [max-order]");
                return;
            }

            int order = int.Parse(args[2]);
            StreamReader sr = new StreamReader(args[0]);
            StreamWriter sw = new StreamWriter(args[1]);

            long[,] counts = new long[LM_ORDER, KN_STAT_ORDER];

            for (int i = 0; i < LM_ORDER; i++)
            {
                for (int j = 0; j < KN_STAT_ORDER; j++)
                {
                    counts[i, j] = 0;
                }
            }

            long total = 0;	//total 1-gram count
            long min1 = 0;	//number of 1-gram with count >= 1
            long min2 = 0;	//number of 1-gram with count >= 2
            long min3 = 0;	//number of 1-gram with count >= 3

            string strLine = null;
            while ((strLine = sr.ReadLine()) != null)
            {
                string[] words = strLine.Split('\t');

                if (words[0] == "BOS")
                {
                    //If the ngram is just a "BOS", skip it
                    continue;
                }

                string[] grams = words[0].Split();
                int cur_order = grams.Length;
                long cur_occur = long.Parse(words[1]);

                if (cur_order == 1)
                {
                    total += cur_occur;
                    min1++;
                    if (cur_occur >= 2)
                    {
                        min2++;
                    }
                    if (cur_occur >= 3)
                    {
                        min3++;
                    }
                }
                if (cur_occur >= 1 && cur_occur <= KN_STAT_ORDER)
                {
                    counts[cur_order - 1, cur_occur - 1]++;
                }

            }


            sw.WriteLine("0-gram\t{0} {1} {2} {3}", total, min1, min2, min3);
            for (int i = 0; i < order; i++)
            {

                for (int j = KN_STAT_ORDER - 1; j > 0; j--)
                {
                    if (counts[i, j - 1] == 0)
                    {
                        counts[i, j - 1] = counts[i, j];
                    }
                }

                double n1 = (double)counts[i, 0];
                double n2 = (double)counts[i, 1];
                double n3 = (double)counts[i, 2];
                double n4 = (double)counts[i, 3];
                double y = n1 / (n1 + 2 * n2);
                double d1 = 1 - 2 * y * n2 / n1;
                double d2 = 2 - 3 * y * n3 / n2;
                double d3plus = 3 - 4 * y * n4 / n3;
                if (d1 <= 0.0 || d1 >= 1.0 || d2 <= 0.0 || d2 >= 2.0 || d3plus <= 0 || d3plus >= 3.0)
                {
                    Console.WriteLine("error: illegal discounting constants");
                    Console.WriteLine("{0}-gram\t{1} {2} {3}", i + 1, d1, d2, d3plus);
                }
                sw.WriteLine("{0}-gram\t{1} {2} {3}", i + 1, d1, d2, d3plus);
            }

            sr.Close();
            sw.Close();
        }
    }
}
