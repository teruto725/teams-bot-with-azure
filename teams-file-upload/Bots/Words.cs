using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class Words
    {
        private List<Word> words;
        private List<Word> tempwords;
        public Words ()
        {
            words = new List<Word>();
            tempwords = new List<Word>();
        }
        public Boolean addTempWord(String eng,String jap)//add出来たらture
        {
            Word w = new Word(eng, jap);
            if (checkExisting(w)==false)
            {
                tempwords.Add(w);
                return true;
            }
            return false;
        }

        public Boolean addNewWord(String eng, String jap)
        {
            Debug.WriteLine("addNewWord");
            Word w = new Word(eng, jap);
            if (checkExisting(w) == false)
            {
                words.Add(w);
                return true;
            }
            return false;
        }
        
        public Word getMostDifficultWord()
        {
            if (words.Count > 0)
            {

                Word dw = words[0];
                foreach( Word w in words)
                {
                    if( w.getDiff() < dw.getDiff())
                    {
                        dw = w;
                    }
                }
                return dw;
            }
            return null;
        }
        public Word getRandomWord()
        {
            Random r = new System.Random();
            int rnd = r.Next(0, words.Count);
            return words[rnd];
        }
        

        public Boolean addWordFromTemp(Word w)//tempwords内の単語を受け取ってwordsに追加するできなければfalse
        {
            if (tempwords.Contains(w))
            {
                
                words.Add(w);
                tempwords.Remove(w);
                return true;
            }
            else
            {
                return false;
            }
        }

        public Boolean delTempWord(Word w)
        {
            if(tempwords.Contains(w))
            {
                tempwords.Remove(w);
                return true;
            }
            else
            {
                return false;
            }
        }

        public Word getTempWord()//tempwordから一つ取得
        {
            if(tempwords.Count > 0)
            {
                Word w = tempwords[0];
                return w;
            }
            return null;
        }
        
        public Boolean checkExisting(Word word)//trueが存在
        {
            foreach(Word wi in words)
            {
                if (word.Equals(wi))
                {
                    return true;
                }
            }
            return false;
        }

        //csv形式のメッセージを返す
        public string getCsvStr()
        {
            string s = "";
            foreach(Word w in getWords())
            {
                s += w.eng + "," + w.jap + "\n\n";
            }
            return s;
        }


        public List<Word> getWords()
        {
            return words;
        }
        public List<Word> getTempWords()
        {
            return tempwords;
        }

        public void delWord(Word w)
        {
            words.Remove(w);
        }
        public string indexWords()
        {
            Debug.WriteLine("indexWord:" + getWords().Count);

            string s = "";
            if (words.Count == 0)
            {
                return "登録されている単語がありません";
            }
            foreach (Word w in getWords())
            {
                s += w.info() + "\n\n" ;
            }
            
            return s;
        }
    }

    public class Word
    {
        public String eng;
        public String jap;
        private double diff = 0;
        private int misscount = 0;
        private int successcount = 0;
        public Word(String eng, String jap)
        {
            this.eng = eng;
            this.jap = jap;
            this.diff = 1;
        }
        public Boolean Equals(Word word)
        {
            if(this.eng == word.eng && this.jap == word.jap)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override string ToString()
        {
            return (eng + ":" + jap);
        }
        public void miss()//失敗したときのdiff更新
        {
            diff = diff * 0.9;
            misscount += 1;
        }
        public void success()//成功したときのdiff更新
        {
            diff = diff * 1.1;
            successcount += 1;
        }
        public double getDiff()
        {
            return diff;
        }
        public string info()
        {
            return ("{\"スペル\":" + eng + ", \"意味\":" + jap + ", \"正解回数\":" + successcount + ", \"失敗回数\":" + misscount+"}");
        }
    }
}
