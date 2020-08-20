using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class Translator
    {

        public Translator() 
        { 
        }

        public  string translateWord(string word, string fromlang, string tolang)//翻訳
        {
            try
            {
                string route = "dictionary/lookup?api-version=3.0";
                string params_ = "&from=" + fromlang + "&to=" + tolang;
                string uri = Key.tlEndpoint + route + params_;
                System.Object[] body = new System.Object[] { new { Text = word } };
                var requestBody = JsonConvert.SerializeObject(body);
                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(uri);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", Key.tlSubscriptionKey);
                    var response = client.SendAsync(request).Result;
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    dynamic jarray = JsonConvert.DeserializeObject(jsonResponse);
                    double confidence = jarray[0]["translations"][0]["confidence"];
                    String translatedword = jarray[0]["translations"][0]["displayTarget"];
                    Debug.WriteLine(confidence);
                    if (confidence < 0.9)
                    {
                        Debug.WriteLine(translatedword);
                        return translatedword;
                    }
                    return "Cant trance";
                }
            }
            catch
            {
                return "Cant trance";
            }
        }

    }
}
