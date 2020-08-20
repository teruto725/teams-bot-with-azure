using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Bots
{
    public abstract class BotApp
    {
        public System.Threading.CancellationToken cancellationToken;
        public ITurnContext<IMessageActivity> turnContext;
        private IHttpClientFactory _clientFactory;
        private  string message;
        public async Task reply(string m)//reply
        {
            await turnContext.SendActivityAsync(MessageFactory.Text(m, m),cancellationToken);
        }


        //replyようにトークンを更新する
        public void updateContextToken(ITurnContext<IMessageActivity> turnContext, System.Threading.CancellationToken cancellationToken, IHttpClientFactory _clientFactory)
        {
            this.turnContext = turnContext;
            this.cancellationToken = cancellationToken;
            this._clientFactory = _clientFactory;
            this.message = turnContext.Activity.RemoveRecipientMention();
        }


        //azure appにデプロイするときはfilesフォルダをwwwroot直下に作成すること
        public async Task<List<string>> getPictureUrls()//pictureのstreamを取得する
        {
            
            List<string> urls = new List<string>();
            foreach( Attachment attachment in turnContext.Activity.Attachments)
            {
                bool messageWithFileDownloadInfo = attachment.ContentType == FileDownloadInfo.ContentType;
                if (messageWithFileDownloadInfo)
                {
                    var file = turnContext.Activity.Attachments[0];
                    var fileDownload = JObject.FromObject(file.Content).ToObject<FileDownloadInfo>();

                    string filePath = Path.Combine("Files", file.Name);//保存先

                    var client = _clientFactory.CreateClient();
                    var response = await client.GetAsync(fileDownload.DownloadUrl);
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                        urls.Add(filePath);
                    }
                }
            }
            return urls;
        }

        //messageの空白区切りリストを返す
        public string[] getMessageSplit()
        {
            return message.Split(" ");
        }


    
    }

}