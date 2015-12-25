using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace kn_lowern
{
    struct Ngram
    {
        public string word;	///< the last word of ngram
        public double alpha;		///< current order prob
        public double lower;		///< lower order backoff prob
    };


    class Program
    {
        /**
 * @brief interpolate lower ngram probabilty to current order
**/
        static void output(string context, List<Ngram> vec_ngram, bool interpolate, StreamWriter sw)
        {
            int idx = -1;
            for (int i = 0; i < vec_ngram.Count; i++)
            {
                Ngram ngram = vec_ngram[i];
                if (ngram.word == "")
                {
                    //It's context only, and no word
                    idx = i;
                    sw.WriteLine("{0}\t{1} {2}", context, ngram.alpha, ngram.lower);
                    break;
                }
            }

            //if no lower prob, just cut off current n-gram
            if (idx == -1)
            {
                return;
            }

            for (int i = 0; i < vec_ngram.Count; i++)
            {
                Ngram ngram = vec_ngram[i];
                //assert there is only 1 lower ngram
                if (ngram.word == "")
                {
                    //Ignore context only
                    continue;
                }
                double lower_alpha = vec_ngram[idx].alpha;
                if (interpolate)
                {
                    ngram.alpha += ngram.lower * lower_alpha;
                }
                ngram.lower = lower_alpha;

                sw.WriteLine("{0} {1}\t{2} {3}", ngram.word, context, ngram.alpha, ngram.lower);
            }
        }



        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("kn_lowern [input file] [output file] [ngram-order] [-i]");
                return;
            }

            bool interpolate = false;
            foreach (string arg in args)
            {
                if (arg == "-i")
                {
                    interpolate = true;
                }
            }

            int order = int.Parse(args[2]);
            StreamReader sr = new StreamReader(args[0]);
            StreamWriter sw = new StreamWriter(args[1]);

            SortedDictionary<string, List<Ngram>> kv = new SortedDictionary<string, List<Ngram>>();
            string strLine = null;
            while ((strLine = sr.ReadLine()) != null)
            {
                string[] record = strLine.Split('\t');

                //We only process ngram which order is the same as [ngram-order] parameter.
                //If the order of ngram is the same as [ngram-order], we generate its context, otherwise, we just
                //save it
                int cur_order = record[0].Split().Length;
                string strCorpus;
                if (cur_order != order)
                {
                    strCorpus = strLine;
                }
                else
                {
                    //Convert foramt "word1 word2 ... wordN \t alpha gamm" to "word2 ... wordN \t word1 \t alpha gamma"
                    string[] ngrams = record[0].Split();
                    string p = String.Join(" ", ngrams, 1, ngrams.Length - 1);
                    strCorpus = p + "\t" + ngrams[0] + "\t" + record[1];
                }

                string[] items = strCorpus.Split('\t');
                string context = items[0];
                Ngram v = new Ngram();

                if (items.Length == 2)
                {
                    v.word = "";
                    string[] strV = items[1].Split();
                    v.alpha = double.Parse(strV[0]);
                    v.lower = double.Parse(strV[1]);
                }
                else if (items.Length == 3)
                {
                    v.word = items[1];
                    string[] strV = items[2].Split();
                    v.alpha = double.Parse(strV[0]);
                    v.lower = double.Parse(strV[1]);
                }

                if (kv.ContainsKey(context) == false)
                {
                    kv.Add(context, new List<Ngram>());
                }
                kv[context].Add(v);
            }

            sr.Close();


            foreach (KeyValuePair<string, List<Ngram>> pair in kv)
            {
                output(pair.Key, pair.Value, interpolate, sw);

            }
            sw.Close();

        }
    }
}
