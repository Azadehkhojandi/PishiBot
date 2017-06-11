using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using PishiBot.Models;

namespace PishiBot.Services
{
    public interface ITextAnalyticsService
    {
        Task<BatchResult> MakeSentimentRequest(string message);
    }
    public class TextAnalyticsService: ITextAnalyticsService
    {
        public async Task<BatchResult> MakeSentimentRequest(string message)
        {
            try
            {
                var client = new HttpClient();

                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ConfigurationManager.AppSettings["TextAnalytics.Key"]);
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var uri = ConfigurationManager.AppSettings["TextAnalytics.SentimentUrl"];

                var sentimentInput = new BatchInput
                {
                    Documents = new List<DocumentInput> {
                        new DocumentInput {
                            Id = 1,
                            Text = message,
                        }
                    }
                };
                var json = JsonConvert.SerializeObject(sentimentInput);
                var sentimentPost = await client.PostAsync(uri, new StringContent(json, Encoding.UTF8, "application/json"));

                var sentimentRawResponse = await sentimentPost.Content.ReadAsStringAsync();
                var sentimentJsonResponse = JsonConvert.DeserializeObject<BatchResult>(sentimentRawResponse);
                return sentimentJsonResponse;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            

        }
    }
}