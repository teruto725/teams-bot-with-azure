using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.IO;
using System.Diagnostics;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class ComputerVison
    {
        private ComputerVisionClient client =
              new ComputerVisionClient(new ApiKeyServiceClientCredentials(Key.cvSubscriptionKey))
              { Endpoint = Key.cvEndpoint };
        public ComputerVison()
        {
        }

        public async Task<List<String>> getTextFromPicture(object urlImage)
        {
            Debug.WriteLine("getTextFromPicture" + urlImage);
            string operationLocation = null;
            if (urlImage as string != null)
            {
                var textHeaders = await this.client.BatchReadFileAsync((String)urlImage);
                operationLocation = textHeaders.OperationLocation;
            }
            if (urlImage as Stream != null)
            {
                var textHeaders = await this.client.BatchReadFileInStreamAsync((Stream)urlImage);
                operationLocation = textHeaders.OperationLocation;
            }
            Debug.WriteLine("hey" + operationLocation);
            var resultList = new List<String>();
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);
            int i = 0;
            int maxRetries = 10;
            ReadOperationResult results;
            do
            {
                results = await this.client.GetReadOperationResultAsync(operationId);
                Debug.Print("Server status: {0}, waiting {1} seconds...", results.Status, i);
                await Task.Delay(1000);
                if (i == 9)
                {
                    Debug.Print("Server timed out.");
                    resultList.Add("RunTimeError");
                    return resultList;
                }
            }
            while ((results.Status == TextOperationStatusCodes.Running ||
                results.Status == TextOperationStatusCodes.NotStarted) && i++ < maxRetries);
            var textRecognitionLocalFileResults = results.RecognitionResults;
            foreach (TextRecognitionResult recResult in textRecognitionLocalFileResults)
            {
                foreach (Line line in recResult.Lines)
                {
                    resultList.Add(line.Text);
                }
            }
            return resultList;
        }
    }
}