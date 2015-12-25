using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using AdvUtils;
using LMDecoder;

namespace lm_score
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("lm_score.exe [LexDict file] [language model file] [ngram-order] <input file> <output file>");
                Console.WriteLine(" if <input file> and <output file> is empty, the input/output will be re-directed to console.");
                return;
            }

            WordSeg.WordSeg wordseg = new WordSeg.WordSeg();
            WordSeg.Tokens tokens = null;
            
            //Load lexical dictionary for word breaking
            wordseg.LoadLexicalDict(args[0], true);
            tokens = wordseg.CreateTokens();

            //Load language model
            LMDecoder.KNDecoder lmDecoder = new LMDecoder.KNDecoder();
            lmDecoder.LoadLM(args[1]);

            StreamReader sr = null;
            if (args.Length >= 4)
            {
                sr = new StreamReader(args[3]);
            }

            StreamWriter sw = null;
            if (args.Length >= 5)
            {
                sw = new StreamWriter(args[4]);
            }

            Console.WriteLine("Ready...");
            if (sw == null)
            {
                Console.WriteLine("Text\tProbability\tOOV\tPerplexity");
            }
            else
            {
                sw.WriteLine("Text\tProbability\tOOV\tPerplexity");
            }

            int order = int.Parse(args[2]);
            while (true)
            {
                string strLine = null;

                if (sr == null)
                {
                    strLine = Console.ReadLine();
                }
                else
                {
                    strLine = sr.ReadLine();
                }
                
                //Empty line, exit
                if (strLine == null || strLine.Length == 0)
                {
                    break;
                }

                //Only use the first column
                string[] items = strLine.Split('\t');
                string strText = items[0];


                //Segment text by lexical dictionary
                wordseg.Segment(strText, tokens, false);
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
                strText = sb.ToString().Trim();

                LMResult LMRst = lmDecoder.GetSentProb(strText, order);

                if (sw == null)
                {
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}", strText, LMRst.logProb, LMRst.oovs, LMRst.perplexity);
                }
                else
                {
                    sw.WriteLine("{0}\t{1}\t{2}\t{3}", strText, LMRst.logProb, LMRst.oovs, LMRst.perplexity);
                }
            }

            if (sr != null)
            {
                sr.Close();
            }
            if (sw != null)
            {
                sw.Close();
            }

        }
    }
}
