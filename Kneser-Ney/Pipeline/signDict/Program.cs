using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AdvUtils;

namespace signDict
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("signDict.exe [raw dictionary] [binary dictionary]");
                return;
            }

            DoubleArrayTrieBuilder daBuilder = new DoubleArrayTrieBuilder(4);


            BTreeDictionary<string, int> dict = new BTreeDictionary<string, int>(StringComparer.Ordinal, 128);
            string strLine = null;
            StreamReader sr = new StreamReader(args[0], Encoding.UTF8);
            StreamWriter sw = new StreamWriter(args[1] + ".prob", false, Encoding.UTF8);
            BinaryWriter bw = new BinaryWriter(sw.BaseStream);

            int index = 0;
            while ((strLine = sr.ReadLine()) != null)
            {
                string[] items = strLine.Split('\t');
                string strNGram = items[0].Trim();

                if (dict.ContainsKey(strNGram) == true)
                {
                    Console.WriteLine("duplicated line: {0}", strLine);
                    continue;
                }

                if (strNGram.Length == 0)
                {
                    continue;
                }

                string[] vals = items[1].Split();
                float prob = float.Parse(vals[0]);
                float backoff = float.Parse(vals[1]);

                //Write item into file
                bw.Write(prob);
                bw.Write(backoff);
                dict.Add(strNGram, index);
                index++;

            }
            sr.Close();

            daBuilder.build(dict);
            daBuilder.save(args[1] + ".da");

            bw.Close();
        }
    }
}
