using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using PishiBot.Services;

namespace PishiBot.Controllers
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
                var message = activity.Text;
                var audioAttachment = activity.Attachments?.FirstOrDefault(a => a.ContentType.Equals("audio/wav") || a.ContentType.Equals("application/octet-stream"));
                if (audioAttachment != null)
                {
                    var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    var speechService= new MicrosoftCognitiveSpeechService();
                    var stream = await GetAudioStream(connector, audioAttachment);
                    message = await speechService.GetTextFromAudioAsync(stream);
                }

                var textTranslatorService = new TextTranslatorService();

                var detectedLanguage = await textTranslatorService.Detect(message);

                var stateClient = activity.GetStateClient();

                var userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                var preferredLanguage = userData.GetProperty<string>("PreferredLanguage");


                if (!string.IsNullOrEmpty(detectedLanguage) && (string.IsNullOrEmpty(preferredLanguage) || preferredLanguage != detectedLanguage))
                {

                    userData.SetProperty<string>("PreferredLanguage", detectedLanguage);
                    // save updated user data
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                }
                if (detectedLanguage != "en")
                {
                    var supportedLanguages = await textTranslatorService.GetLanguagesForTranslate();
                    if (supportedLanguages.Contains(detectedLanguage))
                    {
                        var text = await textTranslatorService.Translate(detectedLanguage, "en",
                            message);
                        message = text;

                    }
                    else
                    {
                        //i don't understand the language

                    }
                }


                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        
        private async Task<Activity> HandleSystemMessage(Activity message)
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
                using (var scope = Microsoft.Bot.Builder.Dialogs.Internals.DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    var client = scope.Resolve<IConnectorClient>();
                    if (message.MembersAdded.Any())
                    {
                        var reply = message.CreateReply();
                        foreach (var newMember in message.MembersAdded)
                        {
                            if (newMember.Id != message.Recipient.Id)
                            {
                                var welcomeMessage = "hi, I'm Pishi the cat bot. I like being patted and hear how cute I am. Also, You can ask me cat related questions. type help whenever you need help!";

                                reply.Text = welcomeMessage;
                                await client.Conversations.ReplyToActivityAsync(reply);
                            }
                        }
                    }
                }

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

        //todo refactor
        private static async Task<Stream> GetAudioStream(ConnectorClient connector, Attachment audioAttachment)
        {
            using (var httpClient = new HttpClient())
            {
                // The Skype attachment URLs are secured by JwtToken,
                // you should set the JwtToken of your bot as the authorization header for the GET request your bot initiates to fetch the image.
                // https://github.com/Microsoft/BotBuilder/issues/662
                var uri = new Uri(audioAttachment.ContentUrl);
                if (uri.Host.EndsWith("skype.com") && uri.Scheme == "https")
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync(connector));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                }

                return await httpClient.GetStreamAsync(uri);
            }
        }

        /// <summary>
        /// Gets the JwT token of the bot. 
        /// </summary>
        /// <param name="connector"></param>
        /// <returns>JwT token of the bot</returns>
        private static async Task<string> GetTokenAsync(ConnectorClient connector)
        {
            var credentials = connector.Credentials as MicrosoftAppCredentials;
            if (credentials != null)
            {
                return await credentials.GetTokenAsync();
            }

            return null;
        }


    }
}