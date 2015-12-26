# LMSharp
The project is a language model toolkit based on .NET framework. So far, it's a n-gram language model with Kneser-Ney smoothing. Users are able to train language model by pipeline tool and predict sentence's probability by decoder.

## Training model

Users run build.bat file in pipeline directory to start training model.

**build.bat** [input file] [output file]

 By default, running build.bat will generate a 4-gram language model. If you want to adjust N for gram, please update build.bat.

## Decoding model

The project provides two ways to use the model. One is a console tool and the other is API for developers.

**Console tool**

lm_score.exe [word breaker dictionary] [language model] [ngram-order] [input file] [output file]

[word breaker dictionary] : the lexical dictionary loaded by word breaker

[language model] : language model used by decoder

[ngram-order] : the ngram-order value

[input file] : input file which contains sentences need to be processed by decoder

[output file] : output file with decoder result

Example:

lm_score.exe wordbreak_dict.txt chsLM.txt 4 input.txt output.txt

The format of [output file] as follows:

Text \t Probability \t the number of OOV \t Perplexity

**API for developers**

The language model has provided some APIs for developers to use the model in their projects. The following paragraph introduces how to use APIs.

1. Add LMDecoder.dll as reference into project

2. Create LMDecoder.LMDecoder instance

3. Use LoadLM(string strFileName) to load language model from given file. The strFileName is used to specify the language model path and file name.

4. Use LMResult GetSentProb(string strText, int order) to predict a specific string's score. The strText is the string used to predict score and the order is the max-order. The return value type is LMResult.

LMResult contains predicted result. Its structure as follows:
 public class LMResult
 {
     public double logProb; //the probability score of given string
     public int oovs; //the number of OOV tokens
     public double perplexity; //the perplexity of given string
 }
