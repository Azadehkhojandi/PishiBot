using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using PishiBot.Services;

namespace PishiBot.Dialogs
{
    [Serializable]
    public class RateMyCatDialog : IDialog<bool>
    {
        private readonly ICatPhotoAnalyzerService _catPhotoAnalyzerService;
        private readonly ICatReplyService _catReplyService;

        public RateMyCatDialog()
        {
            _catPhotoAnalyzerService = new CatPhotoAnalyzerService();
            _catReplyService=new CatReplyService();
        }

        public async Task StartAsync(IDialogContext context)
        {
           
                string detectedLanguage;
                context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);

                var reply = await _catReplyService.UploadYourCatPhoto(detectedLanguage);
                await context.PostAsync(reply);
                context.Wait(MessageReceived);
            
           

        }

        private async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {

            string detectedLanguage;
            context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);

            var message = await result;

            if (message.Attachments != null && message.Attachments.Any())
            {
                var attachment = message.Attachments.First();
                using (HttpClient httpClient = new HttpClient())
                {
                    // Skype & MS Teams attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.
                    if ((message.ChannelId.Equals("skype", StringComparison.InvariantCultureIgnoreCase) || message.ChannelId.Equals("msteams", StringComparison.InvariantCultureIgnoreCase))
                        && new Uri(attachment.ContentUrl).Host.EndsWith("skype.com"))
                    {
                        var token = await new MicrosoftAppCredentials().GetTokenAsync();
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);

                    var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;

                    var reply = await _catReplyService.ReceivedImage(detectedLanguage);


                    await context.PostAsync(reply);

                    var imageByteArray = await responseMessage.Content.ReadAsByteArrayAsync();
                    var catPhotoAnalyzerReply = await _catPhotoAnalyzerService.MakeAnalysisRequest(imageByteArray, detectedLanguage);
                    await context.PostAsync(catPhotoAnalyzerReply);
                    context.Done(true);

                }
            }
            else
            {
               context.Done(false);
            }

           
        }
    }
}