using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TeamsFileUpload.Bots;

namespace Microsoft.BotBuilderSamples.Bots
{
    public static class WordBooks
    {
        public static List<WordBook> wordbooks = new List<WordBook>();

        public static WordBook getYourBook(ChannelAccount user)
        {
            foreach(WordBook wb in wordbooks)
            {
                if (user.ToString().Equals(wb.user.ToString()))
                {
                    return wb;
                }
            }
            //存在しないなら新しく作成
            WordBook newWb = new WordBook(user);
            wordbooks.Add(newWb);
            Debug.WriteLine("new book");
            return newWb;
        }
    }


    public class WordBook : BotApp
    {
        public ChannelAccount user;
        public ComputerVison cvClient = new ComputerVison();
        public Translator tlClient = new Translator();
        public Words words = new Words();
        public Word askingWord =null ;
        public Word checkingWord = null;
        private Messenger messenger = Messengers.normal;
        private int successcnt = 0; 
        public WordBook(ChannelAccount user)
        {
            this.user = user;
        }

        //受け取ったメッセージに応じていろいろするクラス word **** **** て感じのめっそっど
        public async Task receiveWordMessage(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, IHttpClientFactory clientFactory)
        {
            updateContextToken(turnContext, cancellationToken, clientFactory);//各情報の更新
            if (getMessageSplit().Length == 1)
            {
                reply("wordの後にスペース入れてそのあとにメッセージを入れてほしいな").Wait();
            }
            else if( getMessageSplit()[1] == "pic"){//画像入力
                addWordFromPic().Wait();//画像受け取ったらtemp単語を登録
            }
            else if( getMessageSplit()[1] == "view" )
            {
                if (getMessageSplit()[2] == "words")//temp一覧を表示
                {
                    foreach(Word w in words.getWords())
                    {
                        reply(w.ToString()).Wait();
                    }
                }
                else if(getMessageSplit()[2] == "temp")
                {
                    foreach(Word w in words.getTempWords())//words一覧を表示
                    {
                        reply(w.ToString()).Wait();
                    }
                }
            }
            else if( getMessageSplit()[1] == "check")//test
            {
                if ( getMessageSplit()[2] == "temp")
                {
                    checkAWordFromTemp().Wait();//tempワードから一つ取り出して単語帳に追加するかユーザに問う
                }
                if (getMessageSplit()[2] == "word")
                {
                    checkWord().Wait();
                }
            }
            
            else if (getMessageSplit()[1] == "ans")
            {
                if ( getState() == "asking")
                {
                    reply(askingWord.eng + "の意味は…" + askingWord.jap + "でした！わかりましたか？").Wait();
                }  
            }
            
            else if (getMessageSplit()[1] == "add")//新規単語追加
            {
                
                String eng = getMessageSplit()[2];
                String jap = getMessageSplit()[3];
                if (words.addNewWord(eng, jap))
                {
                    reply(eng + ":" + jap + "は追加されました").Wait();
                }
                else
                {
                    reply(eng + "という単語は既に存在しています").Wait();
                }
                //word追加
            }
            
            else if (getMessageSplit()[1] == "tocsv")
            {
                //csv出力
            }
            
            else
            {
                reply("ちょっと何言ってるかわからない（サンドウィッチマン）").Wait();
            }

        }




            //受け取ったりアクションに対していろいろする
        public async Task receiveReaction(ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken, IHttpClientFactory clientFactory)
        {
            Debug.WriteLine("receiveReaction");
            try
            {
                String reactType = turnContext.Activity.ReactionsAdded[0].Type;
                if (getState() == "checking")
                {
                    receiveReactionAboutChecking(reactType).Wait();
                }
                else if(getState() == "asking")
                {
                    receiveReactionAboutAsking(reactType).Wait();
                }
                askingWord = null;
                checkingWord = null;
            }
            catch
            {
                reply(messenger.getMessage("error")).Wait();
                reply("リアクションが取得できないです。リアクションを上書きしたりするとおこります。もう一度いちからやり直して見てください").Wait();
            }
        }

        //tempワードから一つ取り出して単語帳に追加するかユーザに問う
        private async Task checkAWordFromTemp()
        {
            Word w = words.getTempWord();
            if (w == null)
            {
                reply("現在登録されているtempwordは存在しません。").Wait();
            }
            else
            {
                checkingWord = w;
                askingWord = null;
                reply("この単語を追加しますか？(英語:" + w.eng + "、日本語:" + w.jap).Wait();
            }
        }

        private async Task receiveReactionAboutAsking(String reaction)
        {
            if (reaction == "laugh")
            {
                successcnt += 1;
                askingWord.success();
                reply(messenger.getMessage(successcnt+"success")).Wait();
            }
            else if(reaction == "sad")
            {
                successcnt = 0;
                askingWord.miss();
                reply(messenger.getMessage("miss")).Wait();
            }
            else if(reaction == "like")
            {
                words.delWord(askingWord);
                reply(messenger.getMessage("delWord")).Wait();
            }
            else
            {
                reply("その入力は無効です").Wait();
            }
        }

        private async Task receiveReactionAboutChecking(String reaction)
        {
            if(reaction == "heart")
            {
                if (words.addWordFromTemp(checkingWord))//temp単語を登録する
                {
                    reply(checkingWord.eng + ":" + checkingWord.jap + "が登録されました").Wait();
                }
                else
                {
                    Debug.WriteLine("ADDWordError");
                }
            }
            else if(reaction == "angry")
            {
                if(words.delTempWord(checkingWord))//temp単語を削除する
                {
                    reply(checkingWord.eng + ":" + checkingWord.jap + "を削除しました").Wait();
                }
                else
                {
                    Debug.WriteLine("DelTempError");
                }
            }
        }

        //問題出題
        private async Task checkWord()
        {
            Word w = words.getMostDifficultWord();
            if (w == null)
            {
                reply("登録されている単語がありません....").Wait();
            }
            else
            {
                askingWord = w;
                reply("問題:" + askingWord.eng + "の意味はなーんだ？？").Wait();
            }
        }

        //状態を返す。asking:質問中, checking:チェック中
        private String getState()
        {
            if (askingWord != null && checkingWord == null)
            {
                return "asking";
            }
            else if(askingWord == null && checkingWord != null)
            {
                return "checking";
            }
            else
            {
                Debug.WriteLine("stateError");
                return "state error";
            }
        }

        // 画像から単語を抽出しwordsにtempで追加
        private async Task addWordFromPic()
        {
            List<String> urls = await getPictureUrls();
            reply("urls").Wait();
            if (urls.Count() == 0)
            {
                reply("画像が読み込めないよ！ごめんなさいm(__)m").Wait();
                return;
            }
            else
            {
                List<String> engs = new List<string>();
                foreach(String url in urls)
                {
                    using(Stream urlstream = File.OpenRead(url))
                    {
                        List<String> engsn = await cvClient.getTextFromPicture(urlstream);
                        engs.AddRange(engsn);
                    }
                }
                if(engs.Count == 0)
                {
                    reply("画像から文字を読み取れなかった！勉強不足(´；ω；`)ｳｩｩ").Wait();
                    return;
                }
                else
                {
                    String replyStr = "";
                    foreach (String eng in engs)
                    {
                        Debug.WriteLine(eng);
                        String jap = tlClient.translateWord(eng, "en", "ja");
                        if(jap != "Cant trance")
                        {
                            bool result = words.addTempWord(eng, jap);
                            Debug.WriteLine("addTempWord" + result);
                            if (result)
                            {
                                replyStr += eng + ":" + jap + ", ";
                            }
                        }
                    }
                    if (replyStr == "")
                    {
                        reply("画像から文字を読み取れなかった！ごめんなさい！").Wait();
                    }
                    else
                    {
                        reply("検出した単語" + replyStr).Wait();
                    }
                }
            }
        }



        /// messenger系 /////////////////////////////////////////////////////////////////////////////////////////////////////////

        //messengerメッセージを受け取ったときの処理
        public async Task receiveMessengerMessage(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, IHttpClientFactory clientFactory)
        {
            updateContextToken(turnContext, cancellationToken, clientFactory);//各情報の更新
            if (getMessageSplit()[1] == "add")
            {
                try
                {
                    addMessenger(getMessageSplit()[2]).Wait();
                }
                catch
                {
                    reply("messsengerの名前を第3因数で指定してね messenger add ***").Wait();
                }
            }
            else if (getMessageSplit()[1] == "list")
            {
                replyMessengerList().Wait();
            }
            else if (getMessageSplit()[1] == "set")
            {
                try
                { 
                    setMessager(getMessageSplit()[2]).Wait();
                }
                catch
                {
                    reply("messengerの名前を第3因数で指定してください messenger set ***").Wait();
                }
            }
        }

        private async Task addMessenger(String name)
        {
            List<String> urls = await getPictureUrls();
            reply("urls").Wait();
            if (urls.Count() == 0)
            {
                reply("ファイルが読み込めないよ！ごめんなさいm(__)m").Wait();
                return;
            }
            else
            {
                Messenger m = Messengers.createMessenger(name, urls[0]);
                if (m == null)
                {
                    reply("名前が重複しています。違う名前で登録してみてください").Wait();
                }
                else
                {
                    messenger = m;
                    reply(name + "messengerが追加されたよ！").Wait();
                    reply("messengerが"+name+"に変更されました！").Wait();
                }
            }
        }

        private async Task replyMessengerList()
        {
            reply("messengerの一覧です").Wait();
            foreach(String s in Messengers.getNameList()){
                reply(s).Wait();
            }
        }

        private async Task setMessager(String name)//nameからmessengerを取得してセットする
        {
            Messenger m = Messengers.getMessenger(name);
            if (m == null)
            {
                reply("その名前のMessengerは存在していません。messenger listで再度確認してみてください。").Wait();
            }
            else
            {
                messenger = m;
                reply(m.getName() + "Messengerがセットされました。").Wait();
            }
        }
    }
}
