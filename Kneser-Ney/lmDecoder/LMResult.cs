using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMDecoder
{
    //Language model result
    public class LMResult
    {
        public double logProb; //the probability score of given string
        public int oovs; //the number of OOV tokens
        public double perplexity; //the perplexity of given string
    }
}
