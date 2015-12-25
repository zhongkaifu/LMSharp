using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AdvUtils;
using WordSeg;

namespace LMDecoder
{
    struct NGram
    {
        public float prob;		///< ngram prob
        public float bow;      ///< ngram bow
    }

    public class KNDecoder
    {
        //Store language model's prob and backoff values
        VarBigArrayNoCMP<NGram> lm_prob;
        //Store language model's string and offset
        DoubleArrayTrieSearch daSearch = new DoubleArrayTrieSearch();

        //Word breaker
        WordSeg.WordSeg wordseg = null;
        WordSeg.Tokens wbTokens = null;

        double M_LN10 = 2.30258509299404568402;      ///< log_2(10)
        ///
        private double log2prob(double logv)
        {
            return Math.Exp(logv * M_LN10);
        }

        private string GenerateNGram(string strLine, int pos)
        {
            string[] items = strLine.Split();
            string strRst = String.Join(" ", items, pos, items.Length - pos);
            return strRst;
        }

        private int lm_ngram_prob(string strText, int start, int end, ref double probability)
        {

            NGram lm_ngram = new NGram();

            // get the longest ngram conditional prob in LM
            int j;
            for (j = start; j <= end; j++)
            {
                string words = GenerateNGram(strText, j);

                int offset = daSearch.SearchByPerfectMatch(words);
                if (offset >= 0)
                {
                    lm_ngram = lm_prob[offset];
                    break;
                }
            }

            if (j > end)
            {
                return 1;// OOV
            }
            else if (j == start)
            {
                probability = lm_ngram.prob;
                return 0;			// exact ngram in LM
            }

            double prob = lm_ngram.prob;
            double bow = 0;

            // get bows starting from the longest ngram prob to the original ngram
            // exclude the last word, set temp buffer end
            string[] ngrams = strText.Split();
            strText = String.Join(" ", ngrams, 0, ngrams.Length - 1);


            for (j--; j >= start; j--)
            {
                string words = GenerateNGram(strText, j);
                int offset = daSearch.SearchByPerfectMatch(words);
                if (offset < 0)
                {
                    break;
                }

                bow += lm_prob[offset].bow;

            }
            probability = prob + bow;
            return 0;
        }


        //Load language model from specific file
        public void LoadLM(string strFileName)
        {
            //Load prob & back off values
            StreamReader srLM = new StreamReader(strFileName + ".prob");
            BinaryReader br = new BinaryReader(srLM.BaseStream);

            lm_prob = new VarBigArrayNoCMP<NGram>(1024000);
            long index = 0;
            try
            {
                while (true)
                {
                    NGram ngram = new NGram();
                    ngram.prob = br.ReadSingle();
                    ngram.bow = br.ReadSingle();
                    lm_prob[index] = ngram;
                    index++;
                }
            }
            catch (EndOfStreamException err)
            {
                br.Close();
            }


            daSearch.Load(strFileName + ".da");
        }

        private void InitializeWordSeg(string strLexicalFileName)
        {
            wordseg = new WordSeg.WordSeg();
            //Load lexical dictionary
            wordseg.LoadLexicalDict(strLexicalFileName, true);
            //Initialize word breaker's token instance
            wbTokens = wordseg.CreateTokens();
        }

        public void LoadLM(string strLMFileName, string strLexicalDictFileName)
        {
            LoadLM(strLMFileName);
            InitializeWordSeg(strLexicalDictFileName);
        }

        //Calculate the probability of raw text
        //The raw text will be wordbroken before predict its probability
        public LMResult GetRawSentProb(string strText, int order)
        {
            if (wordseg == null)
            {
                return null;
            }

            //Segment text by lexical dictionary
            wordseg.Segment(strText, wbTokens, false);
            StringBuilder sb = new StringBuilder();
            //Parse each broken token
            for (int i = 0; i < wbTokens.tokenList.Count; i++)
            {
                string strTerm = wbTokens.tokenList[i].strTerm.Trim();
                if (strTerm.Length > 0)
                {
                    sb.Append(strTerm);
                    sb.Append(" ");
                }
            }

            return GetSentProb(sb.ToString().Trim(), order);
        }

        //Calculate the probability of given text
        public LMResult GetSentProb(string strText, int order)
        {
            LMResult LMRst = new LMResult();
            int calcWordNum = 0;

            //Append EOS into the sentence
            strText = strText + " EOS";

            string[] items = strText.Split();
            int wordNum = items.Length;

            for (int i = 0; i < wordNum; i++)
            {
                string words = String.Join(" ", items, 0, i + 1);
                // calc prob of ngram[j,i]
                int start = (i > order - 1) ? (i - order + 1) : 0;
                double prob = 0.0;

                if (lm_ngram_prob(words, start, i, ref prob) == 0)
                {
                    LMRst.logProb += prob;
                }
                else
                {
                    LMRst.oovs++;
                }
                calcWordNum++;
            }

            int denom = calcWordNum - LMRst.oovs;
            LMRst.perplexity = log2prob(-LMRst.logProb / denom);
            return LMRst;
        }
    }
}
