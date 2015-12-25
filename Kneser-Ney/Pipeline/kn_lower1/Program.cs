using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace kn_lower1
{
    /** @brief ngram suffix word and occurrence */
    struct Ngram
    {
        public string word;	///< the last word of ngram
        public long occur;		///< ngram occurrence
    };


    /** @brief the ngram occurrence statistics */
    class NgramStat
    {
        public NgramStat()
        {
            reset();
        }

        public void reset()
        {
            m_total = 0;
            m_min1 = 0;
            m_min2 = 0;
            m_min3 = 0;
        }

        public void count(long occur)
        {
            if (occur > 0)
            {
                m_total += occur;
                m_min1++;
                if (occur >= 2)
                {
                    m_min2++;
                }
                if (occur >= 3)
                {
                    m_min3++;
                }
            }
        }

        public long m_total;        ///< total counts
        public long m_min1;         ///< ngrams with counts >= 1
        public long m_min2;         ///< ngrams with counts >= 2
        public long m_min3;         ///< ngrams with counts >= 3
    }

    class Discount
    {

        public double m_d1;		///< discount constant for n-gram occurred one time
        public double m_d2;		///< discount constant for n-gram occurred two times
        public double m_d3plus;	///< discount constant for n-gram occurred more than two times 		
        ///
        public Discount()
        {
            m_d1 = 0.0;
            m_d2 = 0.0;
            m_d3plus = 0.0;
        }

        /**
         * @brief get discount for different count
        **/
        public double get_discount(long count)
        {
            if (count == 1)
            {
                return ((double)count - m_d1);
            }
            else if (count == 2)
            {
                return ((double)count - m_d2);
            }
            else
            {
                return ((double)count - m_d3plus);
            }
        }

        /**
         * @brief calculate interpolate weight
        **/
        public double get_lower_weight(NgramStat stat)
        {
            double total = stat.m_total;
            double min1 = stat.m_min1;
            double min2 = stat.m_min2;
            double min3 = stat.m_min3;

            return (m_d1 * (min1 - min2) + m_d2 * (min2 - min3) + m_d3plus * min3) / total;
        }


    }


    class Program
    {
        const int LM_ORDER = 9;	///< maximum n-gram order
        static long[] g_min_count = new long[LM_ORDER] { 1, 1, 2, 2, 2, 2, 2, 2, 2 };
        public static NgramStat g_uni_stat = new NgramStat();        ///< uni-gram stat
        public static Discount[] g_discounts = new Discount[LM_ORDER];    ///< discounts parameter
        ///

        /**
 * load the unigram statistics and discount constants from file 
 *
 * @param file, file name
 * @param g_uni_stat, unigram statistics
 * @param discounts, n-gram discount constants
 *
 */
        static bool LoadStatFile(string file, NgramStat g_uni_stat, Discount[] discounts)
        {

            StreamReader sr = new StreamReader(file);

            string strLine = sr.ReadLine();
            int pIndex = strLine.IndexOf('\t');
            string[] items = strLine.Substring(pIndex + 1).Split();
            g_uni_stat.m_total = long.Parse(items[0]);
            g_uni_stat.m_min1 = long.Parse(items[1]);
            g_uni_stat.m_min2 = long.Parse(items[2]);
            g_uni_stat.m_min3 = long.Parse(items[3]);


            Console.WriteLine("Total:{0} Min1:{1} Min2:{2} Min3:{3}", g_uni_stat.m_total, g_uni_stat.m_min1, g_uni_stat.m_min2, g_uni_stat.m_min3);

            int order = 0;
            while (sr.EndOfStream == false)
            {
                strLine = sr.ReadLine();
                if (strLine.Length == 0)
                {
                    continue;
                }

                pIndex = strLine.IndexOf('\t');
                items = strLine.Substring(pIndex + 1).Split();

                discounts[order] = new Discount();
                discounts[order].m_d1 = double.Parse(items[0]);
                discounts[order].m_d2 = double.Parse(items[1]);
                discounts[order].m_d3plus = double.Parse(items[2]);
                order++;
            }
            sr.Close();

            return order > 0;
        }

        const string LM_BOS = "BOS";	///< begin of sentence
        const string LM_EOS = "EOS";	///< end of sentence
        const int LM_MINSCORE = -99;    ///< minimal score for unknown word

        //calculate the unigram prob and output it, the prob of BOS is 0
        static void calc_unigram(string word, long occur, NgramStat stat, Discount discount, StreamWriter sw)
        {
            //set the prob=0 for LM_BOS <s>
            if (word == LM_BOS)
            {
                double exponent = (double)LM_MINSCORE * Math.Log(10.0);

                sw.WriteLine("{0}\t{1} 0", word, Math.Exp(exponent));
                return;
            }

            //skip 1-gram with occur = 0
            if (occur <= 0)
            {
                return;
            }

            double prob = discount.get_discount(occur) / (double)stat.m_total;
            // normalize unigram, make sure sum(prob_unigram) = 1
            prob += discount.get_lower_weight(stat) / (double)stat.m_min1;

            sw.WriteLine("{0}\t{1} 0", word, prob);
        }


        /// <summary>
        ///  calculate the ngram prob and lower weight, and output them
        ///  @param context, n-gram prefix context
        ///  @param vec_ngram, all n-gram(of the same order) to calculate
        /// @param stat, current n-gram occurrence statistics
        /// @param discount, current n-gram discount constants
        /// @param interpolate, whether to interpolate
        /// </summary>
        static void calc_ngram(string context, List<Ngram> vec_ngram, Discount discount,
                bool interpolate, StreamWriter sw)
        {
            int order = context.Split().Length;
            NgramStat cur_stat = new NgramStat();
            cur_stat.reset();

            //All ngrams with same context
            for (int i = 0; i < vec_ngram.Count; i++)
            {
                Ngram ngram = vec_ngram[i];
                if (ngram.word == null)
                {
                    //This is unigram
                    if (ngram.occur >= g_min_count[0])
                    {
                        calc_unigram(context, ngram.occur, g_uni_stat, g_discounts[0], sw);
                    }
                }
                else
                {
                    cur_stat.count(ngram.occur);
                }
            }

            double lower_weight = interpolate ? discount.get_lower_weight(cur_stat) : 0.0;
            //All ngrams with same context
            for (int i = 0; i < vec_ngram.Count; i++)
            {
                Ngram ngram = vec_ngram[i];
                if (ngram.word == null)
                {
                    //skip unigram
                    continue;
                }
                if (ngram.occur >= g_min_count[order])
                {
                    double prob = discount.get_discount(ngram.occur) / (double)cur_stat.m_total;
                    sw.WriteLine("{0} {1}\t{2} {3}", context, ngram.word, prob, lower_weight);
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("kn_lower1 [input file] [output file] [option list]");
                return;
            }

            string strLine = null;
            string[] items = null;

            bool interpolate = false;
            for (int i = 2; i < args.Length; i++)
            {
                string[] optPair = args[i].Split(':');
                if (optPair[0] == "-i")
                {
                    interpolate = true;
                }
                else if (optPair[0] == "-f")
                {
                    if (!LoadStatFile(optPair[1], g_uni_stat, g_discounts))
                    {
                        Console.WriteLine("error: failed to load file {0}", optPair[1]);
                        return;
                    }
                }
                else if (optPair[0] == "-c")
                {
                    items = optPair[1].Split(',');
                    for (int j = 0; j < items.Length; j++)
                    {
                        g_min_count[j] = long.Parse(items[j]);
                    }
                }
            }


            StreamReader sr = new StreamReader(args[0]);
            StreamWriter sw = new StreamWriter(args[1]);

            SortedDictionary<string, List<Ngram>> kv = new SortedDictionary<string, List<Ngram>>();
            while ((strLine = sr.ReadLine()) != null)
            {
                 //if input is 1-gram, just keep the same
                //if input is n-gram (such as "a b c\t3"), then output "a b\tc\t3"; 
                if (strLine.Contains(" ") == true)
                {
                    //This is not unigram, so we convert "word1 word2 ... wordN \t frequency" to 
                    //"word1 word2 ... \t wordN \t frequency"
                    items = strLine.Split(' ');
                    strLine = String.Join(" ", items, 0, items.Length - 1);
                    strLine = strLine.Trim() + "\t" + items[items.Length - 1];
                }

                items = strLine.Split('\t');
                string strContext = items[0];

                Ngram v = new Ngram();
                if (items.Length == 2)
                {
                    //This is unigram
                    v.word = null;
                    v.occur = long.Parse(items[1]);
                }
                else if (items.Length == 3)
                {
                    //This is not unigram
                    v.word = items[1];
                    v.occur = long.Parse(items[2]);
                }

                if (kv.ContainsKey(strContext) == false)
                {
                    kv.Add(strContext, new List<Ngram>());
                }
                kv[strContext].Add(v);

            }
            sr.Close();


            foreach (KeyValuePair<string, List<Ngram>> pair in kv)
            {
                int order = pair.Key.Split().Length;
                if (g_discounts[order] == null)
                {
                    Console.WriteLine("Error: Invalidate {0}-gram: {1}", order, pair.Key);
                    continue;
                }

                calc_ngram(pair.Key, pair.Value, g_discounts[order], interpolate, sw);
            }


            sw.Close();
        }
    }
}
