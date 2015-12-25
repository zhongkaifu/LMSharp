using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace common_bow
{
    struct Ngram
    {
        public string word;	///< the last word of ngram
        public double prob;		///< current order prob
        public double lower;		///< lower order backoff prob
    };


    class Program
    {

        const double LM_EPSILON = 3e-6;   ///< float-point epsilon
        const double LM_LOG_ZERO = -3.40232347E+38F;    ///< approximate value for log(0)

        static double prob2log(double prob)
        {
            return Math.Log10(prob);
        }

        //calculate backoff weight         
        static void output(string context, List<Ngram> vec_ngram, StreamWriter sw)
        {
            int idx = -1;
            double numerator = 1.0;
            double denominator = 1.0;
            for (int i = 0; i < vec_ngram.Count; i++)
            {
                Ngram ngram = vec_ngram[i];
                if (ngram.word == "")
                {
                    idx = i;
                }
                else
                {
                    numerator -= ngram.prob;
                    denominator -= ngram.lower;
                }
            }

            // if no prefix context, cut off current n-gram
            if (idx == -1)
            {
                return;
            }

            /**
             * According to SRILM
             * Avoid some predictable anomalies due to rounding errors
             */
            bool valid = true;
            if (numerator < 0.0 && numerator > -LM_EPSILON)
            {
                numerator = 0.0;
            }
            if (denominator < 0.0 && denominator > -LM_EPSILON)
            {
                denominator = 0.0;
            }
            if (denominator == 0.0 && numerator > LM_EPSILON)
            {
                numerator = 0.0;
            }
            else if (numerator < 0.0)
            {
                valid = false;
            }
            else if (denominator <= 0.0)
            {
                if (numerator > LM_EPSILON)
                {
                    valid = false;
                }
                else {
                    numerator = 0.0;
                    denominator = 0.0;  // give bow = 0
                }
            }
            double bow = 0;
            if (valid)
            {
                bow = (numerator == 0.0 && denominator == 0.0) ?
                    0.0 : (prob2log(numerator) - prob2log(denominator));
            }
            else {
                bow = LM_LOG_ZERO;
            }
            sw.WriteLine("{0}\t{1} {2}", context, prob2log(vec_ngram[idx].prob), bow);
        }


        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("common_bow.exe [input file] [output file]");
                return;
            }

            StreamReader sr = new StreamReader(args[0]);
            StreamWriter sw = new StreamWriter(args[1]);

            SortedDictionary<string, List<Ngram>> kv = new SortedDictionary<string, List<Ngram>>();
            string strLine = null;
            while ((strLine = sr.ReadLine()) != null)
            {
                //For unigram, just duplicate input;
                //For others, duplicate input, and split ngram into history\tword
                //e.g. from h w\tvalue to h\tw\tvalue
                string[] items = strLine.Split(new char[] { '\t' }, 2);
                string key = items[0];
                string value = items[1];
                Ngram v = new Ngram();
                v.word = "";
                string[] strV = value.Split(' ');
                v.prob = double.Parse(strV[0]);
                v.lower = double.Parse(strV[1]);

                if (kv.ContainsKey(key) == false)
                {
                    kv.Add(key, new List<Ngram>());
                }
                kv[key].Add(v);


                string[] keys = key.Split(' ');
                if (keys.Length > 1)
                {
                    string strKeys = String.Join(" ", keys, 0, keys.Length - 1).Trim();
                    v = new Ngram();
                    v.word = keys[keys.Length - 1];
                    v.prob = double.Parse(strV[0]);
                    v.lower = double.Parse(strV[1]);

                    if (kv.ContainsKey(strKeys) == false)
                    {
                        kv.Add(strKeys, new List<Ngram>());
                    }
                    kv[strKeys].Add(v);

                }
            }

            sr.Close();


            foreach (KeyValuePair<string, List<Ngram>> pair in kv)
            {
                output(pair.Key, pair.Value, sw);

            }

            sw.Close();
        }
    }
}
