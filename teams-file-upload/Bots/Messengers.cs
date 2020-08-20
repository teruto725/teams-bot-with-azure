using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TeamsFileUpload.Bots
{
    public static class Messengers
    {
        private static List<Messenger> messengers = new List<Messenger>();
        public static Messenger normal = new Messenger("sample","Files/normal.json"); 
        
        public static Messenger createMessenger(String name, String url)
        {
            if (getNameList().Contains(name))//重複
            {
                return null;
            }
            Messenger m = new Messenger(name, url);
            messengers.Add(m);
            return m;
        }
        public static List<String> getNameList()
        {
            List<String> slist = new List<string>();
            foreach (Messenger m in messengers)
            {
                Debug.WriteLine(m.ToString());
                slist.Add(m.getName());
            }
            return slist;
        }
        public static Messenger getMessenger(String s)
        {
            foreach(Messenger m in messengers)
            {
                if(s == m.getName())
                {
                    return m;
                }
            }
            return null;
        }
    }
    public class Messenger
    {
        private String name;
        public Dictionary<string, string> messagedict;
        
        public Messenger(String name, String filePath)
        {
            this.name = name;
            //json to dict
            StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding("UTF-8"));
            string str = sr.ReadToEnd();
            this.messagedict = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
        }
        public string getMessage(String caseName)
        {
            try
            {
                Debug.WriteLine(caseName);
                return messagedict[caseName];
            }
            catch
            {
                Debug.WriteLine("catch");
                if (caseName.Contains("success"))
                {
                    caseName = "success";
                }
                return Messengers.normal.getMessage(caseName);
            }
           
        }
        public String getName()
        {
            return name;
        }
    }
}
