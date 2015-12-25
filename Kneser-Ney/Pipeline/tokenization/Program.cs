using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WordSeg;

namespace tokenization
{
    class Program
    {
        /// <summary>
        /// Tokenizate given corpus. The output corpus format is "word1 word2 ... wordN \t frequency"
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length < 3 || args.Length > 4)
            {
                Console.WriteLine("tokenization.exe [lexical dictionary] [input file] [output file] <min-frequency>");
                Console.WriteLine("  min-frequency: default value is 1");
                return;
            }

            int minFreq = 1;
            if (args.Length == 4)
            {
                minFreq = int.Parse(args[3]);
            }

            WordSeg.WordSeg wordseg = new WordSeg.WordSeg();
            WordSeg.Tokens tokens = null;

            //Load lexical dictionary with raw text format
            if (File.Exists(args[0]) == false)
            {
                Console.WriteLine("lexical dictionary isn't existed.");
                return;
            }
            wordseg.LoadLexicalDict(args[0], true);
            tokens = wordseg.CreateTokens();

            if (File.Exists(args[1]) == false)
            {
                Console.WriteLine("{0} isn't existed.", args[1]);
                return;
            }

            StreamReader sr = new StreamReader(args[1], Encoding.UTF8);
            StreamWriter sw = new StreamWriter(args[2], false, Encoding.UTF8);
            string strLine = null;
            long lineCnt = 0;

            while ((strLine = sr.ReadLine()) != null)
            {
                string[] items = strLine.Split('\t');
                long freq = 1;
                if (items.Length == 2)
                {
                    //Normalize frequency for smoothing when building LM
                    freq = long.Parse(items[1]) - (minFreq - 1);

                    if (freq <= 0)
                    {
                        continue;
                    }
                }

                lineCnt++;
                if (lineCnt % 100000 == 0)
                {
                    Console.Write("{0}...", lineCnt);
                }

                //Simple normalize text
                string strQuery = items[0].ToLower().Trim();

                //Segment text by lexical dictionary
                wordseg.Segment(strQuery, tokens, false);
                StringBuilder sb = new StringBuilder();
                //Parse each broken token
                for (int i = 0; i < tokens.tokenList.Count; i++)
                {
                    string strTerm = tokens.tokenList[i].strTerm.Trim();
                    if (strTerm.Length > 0)
                    {
                        sb.Append(strTerm);
                        sb.Append(" ");
                    }
                }

                //output token with begin/end flag
                sw.WriteLine("{0}\t{1}", sb.ToString().Trim(), freq);
            }

            sr.Close();
            sw.Close();
        }
    }
}
