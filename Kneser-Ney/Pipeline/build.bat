REM build.bat [input file] [output file]

REM tokenization sentences which frequency is not less than 5
tokenization.exe LexicalDict.txt %1 tokenization.txt 5

REM remove the sentence if its term's frequency is less than 10000
removeOOV.exe tokenization.txt removeoov.txt 10000
del tokenization.txt

REM generate 4gram data and counting their frequency
raw_count.exe removeoov.txt raw_count.txt 4
del removeoov.txt

REM counting ngram frequency with different context
kn_count.exe raw_count.txt kn_count.txt 4
del raw_count.txt

REM calculate global information
kn_stat.exe kn_count.txt kn_stat.txt 4

REM calculate probability for unigram
kn_lower1.exe kn_count.txt kn_lower1.txt -i -f:kn_stat.txt -c:1,2,3,3
del kn_count.txt

REM calculate probability for bigram
kn_lowern.exe kn_lower1.txt kn_lower2.txt 2 -i
del kn_lower1.txt

REM calculate probability for trigram
kn_lowern.exe kn_lower2.txt kn_lower3.txt 3 -i
del kn_lower2.txt

REM calculate probability for 4gram
kn_lowern.exe kn_lower3.txt kn_lower4.txt 4 -i
del kn_lower3.txt

REM calculate bow
common_bow.exe kn_lower4.txt  common_bow.txt
del kn_lower4.txt

REM compress the model
signDict.exe common_bow.txt %2
del common_bow.txt