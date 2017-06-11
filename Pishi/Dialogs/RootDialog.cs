using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using PishiBot.Services;

namespace PishiBot.Dialogs
{
    [LuisModel("53587986-09e1-493a-afaf-3e87ec9cd084", "3942f36c96064194a5edabbf18909f30")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {



        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {

            if (!string.IsNullOrEmpty(result.Query))
            {
                string detectedLanguage;
                context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);
                var replyText = await CatReply(result.Query, detectedLanguage);
                await context.PostAsync(replyText);
            }
            else
            {
                await context.PostAsync("huh?");
            }
            context.Wait(MessageReceived);
        }

        private static async Task<string> CatReply(string text, string preferredLanguage)
        {
            var service = new TextAnalyticsService();
            var sentiments = await service.MakeSentimentRequest(text);
            var sentimentScore = sentiments?.Documents?.FirstOrDefault()?.Score ?? 0;
            var upsetReply = "Maybe you can say something nicer. I like to hear how cute I'm";
            if (!string.IsNullOrEmpty(preferredLanguage) && preferredLanguage != "en")
            {
                var textTranslatorService = new TextTranslatorService();
                upsetReply = await textTranslatorService.Translate( "en", preferredLanguage, upsetReply);
            }
            
            var replyTextInPreferredLanguage = sentimentScore > 0.6
                ? "Meow xoxo"
                : "Hiss," + upsetReply;
            return replyTextInPreferredLanguage;
        }

        [LuisIntent("Get Pishi\'s attention")]
        public async Task GetAttention(IDialogContext context, LuisResult result)
        {
            var entity = result.Entities.FirstOrDefault(x => x.Type == "Adjective");
            if (entity != null)
            {
                string detectedLanguage;
                context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);
                var replyText = await CatReply(entity.Entity, detectedLanguage);
                await context.PostAsync(replyText);
            }
            else
            {
                await context.PostAsync("Meow xoxo");

            }
            context.Wait(MessageReceived);
        }






    }
}