using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Pishi.Models;

namespace Pishi.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
               
                // return our reply to the user
                var replyText = MakeReplyText(await MakeSentimentRequest(activity.Text));
                var reply = activity.CreateReply(await replyText);
                
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        async Task<HttpResponseMessage> MakeSentimentRequest(string message)
        {
            var client = new HttpClient();

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ConfigurationManager.AppSettings["TextAnalyticsKey"]);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var uri = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment" ;

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

            return sentimentPost;

        }

        async Task<string> MakeReplyText(HttpResponseMessage sentimentPost)
        {
            var sentimentRawResponse = await sentimentPost.Content.ReadAsStringAsync();
            var sentimentJsonResponse = JsonConvert.DeserializeObject<BatchResult>(sentimentRawResponse);
            var sentimentScore = sentimentJsonResponse?.Documents?.FirstOrDefault()?.Score ?? 0;

            var result = sentimentScore > 0.5 ? "Meow xoxo" : "Hiss, Maybe you can say something nicer";
            
            return result;
        } 
    }
}