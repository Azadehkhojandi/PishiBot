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
            _catReplyService = new CatReplyService();
        }

        public async Task StartAsync(IDialogContext context)
        {
            try
            {
                await AskForCatImage(context);
            }
            catch (Exception e)
            {
                context.Fail(e);
                //Console.WriteLine(e);
                //await context.PostAsync("error");
                //await context.PostAsync("e.Message " + e.Message);
                //await context.PostAsync("e.Source " + e.Source);
                //await context.PostAsync("e.StackTrace " + e.StackTrace);
            }

           



        }

        private async Task AskForCatImage(IDialogContext context)
        {
            string detectedLanguage;
            context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);

            var reply = await _catReplyService.UploadYourCatPhoto(detectedLanguage);
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        private async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {

            try
            {
                string detectedLanguage;
                context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);

                var message = await result;

                if (message.Attachments != null && message.Attachments.Any())
                {
                    var attachment = message.Attachments.First();
                    //await context.PostAsync(message.ChannelId);

                    using (HttpClient httpClient = new HttpClient())
                    {
                        // Skype & MS Teams attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.
                        if (
                            (message.ChannelId.Equals("skype", StringComparison.InvariantCultureIgnoreCase) || message.ChannelId.Equals("msteams", StringComparison.InvariantCultureIgnoreCase))
                            || new Uri(attachment.ContentUrl).Host.EndsWith("skype.com"))
                        {
                            var token = await new MicrosoftAppCredentials().GetTokenAsync();
                            await context.PostAsync($"token: {token}");
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        }
                        await context.PostAsync($"attachment.ContentUrl: {attachment.ContentUrl}");


                        var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);

                        //await context.PostAsync($"responseMessage: {responseMessage}");

                        var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;

                        //await context.PostAsync($"responseMessage.Content.Headers.ContentType.MediaType: {responseMessage.Content.Headers.ContentType.MediaType}");
                        //await context.PostAsync($"contentLenghtBytes: {contentLenghtBytes}");

                        var reply = await _catReplyService.ReceivedImage(responseMessage.Content.Headers.ContentType.MediaType, contentLenghtBytes, detectedLanguage);
                        await context.PostAsync(reply);

                        var imageByteArray = await responseMessage.Content.ReadAsByteArrayAsync();
                        //await context.PostAsync("call MakeAnalysisRequest");
                        //await context.PostAsync($"detectedLanguage: {detectedLanguage} ");
                        //await context.PostAsync($"imageByteArray: {imageByteArray} ");
                        //await context.PostAsync($"imageByteArray.Length: {imageByteArray.Length} ");
                        //await context.PostAsync($"_catPhotoAnalyzerService: {_catPhotoAnalyzerService != null} ");

                        var catPhotoAnalyzerReply = await _catPhotoAnalyzerService.MakeAnalysisRequest(imageByteArray, detectedLanguage);
                        //await context.PostAsync($"catPhotoAnalyzerReply: {catPhotoAnalyzerReply} ");

                        await context.PostAsync(catPhotoAnalyzerReply);
                        PromptDialog.Choice(
                            context: context,
                            resume: ResumeAfterPromptDialog,
                            descriptions: new[] { "Yes please", "No Thanks" },
                            options: new[] { "Yes please", "No Thanks" },
                            prompt: "Would you like to check another photo?"

                        );

                    }
                }
                else
                {
                    context.Done(false);
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
                //await context.PostAsync("error");
                //await context.PostAsync("e.Message " + e.Message);
                //await context.PostAsync("e.Source " + e.Source);
                //await context.PostAsync("e.StackTrace " + e.StackTrace);
                context.Fail(e);
            }

            

        }

        private async Task ResumeAfterPromptDialog(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var choice = await result;
                if (choice.ToLower().Contains("no"))
                {
                    context.Done(true);
                }
                else 
                {
                    await AskForCatImage(context);

                }

            }
            catch (Exception e)
            {
                context.Fail(e);
            }
        }
    }
}