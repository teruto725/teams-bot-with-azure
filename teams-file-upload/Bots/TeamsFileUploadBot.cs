// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsFileUploadBot : TeamsActivityHandler
    {
        // You can install this bot at any scope. You can @mention the bot and it will present you with the file prompt. You can accept and 
        // the file will be uploaded, or you can decline and it won't.

        private readonly IHttpClientFactory _clientFactory;

        public TeamsFileUploadBot(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var message = turnContext.Activity.RemoveRecipientMention();//message that mention deleted
            
            string[] mlist = message.Split(" ");
            Debug.WriteLine("Get Message");
            ChannelAccount user = turnContext.Activity.From;//メッセージを送ってきたuser
            if (mlist[0] == "word")
            {
                Debug.WriteLine("if word");
                WordBook yourWb = WordBooks.getYourBook(user);//userのwordbookを取得

                yourWb.receiveWordMessage(turnContext, cancellationToken, _clientFactory).Wait();
            }
            else if (mlist[0] == "messenger")
            {
                WordBook yourWb = WordBooks.getYourBook(user);//userのwordbookを取得
                yourWb.receiveMessengerMessage(turnContext, cancellationToken, _clientFactory).Wait();
            }

            else if (message.Contains("echo"))//echobot
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(message, message), cancellationToken);
            }
            else if (mlist[0] == "help")
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("https://github.com/teruto725/teams-bot-with-azure", "https://github.com/teruto725/teams-bot-with-azure"), cancellationToken);
            }

        }

        // ユーザのリアクション
        protected override async Task OnMessageReactionActivityAsync(ITurnContext<IMessageReactionActivity> turnContext, System.Threading.CancellationToken cancellationToken)
        {
            ChannelAccount user = turnContext.Activity.From;
            
            WordBook yourWb = WordBooks.getYourBook(user);//userのwordbookを取得
            yourWb.receiveReaction(turnContext, cancellationToken, _clientFactory).Wait();
        }
    }
}
