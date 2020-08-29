using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamsFileUpload.Bots
{
    
    public class LevelCounter
    {
        private int level = 1;
        private int exp = 0;
        public LevelCounter()
        {

        }

        //レベルアップしたらtrueでないならfalse
        public bool addExp(int a)
        {
            exp += a;
            if ( (level^2)  <= exp)
            {
                level += 1;
                exp = exp - (level ^ 2);
                return true;
            }
            return false;
        }
        public int getLevel()
        {
            return level;
        }

        public string getInfo()
        {
            return ("現在のレベルは" + level + "lvです！　あと" + ((level ^ 2) - exp) + "expでレベルアップします！");
        }
        public int getExp()
        {
            return exp;
        }
    }
}
