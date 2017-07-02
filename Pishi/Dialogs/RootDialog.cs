using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using PishiBot.Services;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;

namespace PishiBot.Dialogs
{
    [LuisModel("53587986-09e1-493a-afaf-3e87ec9cd084", "3942f36c96064194a5edabbf18909f30")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        
        private async Task AfterQnADialog(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            try
            {
                var messageHandled = await result;
                if (!(messageHandled.Value != null && bool.Parse(messageHandled.Value.ToString())))
                { 
                    await context.PostAsync("Unfortunately, I don't know the answer of your question.");
                }
               
            }
            catch (Exception e)
            {
                await context.PostAsync("something went wrong when I was looking for your answer.");
                Console.WriteLine(e);
                
            }
            

            context.Wait(MessageReceived);
        }



        [LuisIntent("")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {

            if (!string.IsNullOrEmpty(result.Query))
            {
                

                //if question ends with ?
                if (result.Query.EndsWith("?"))
                {
                    //call qanA
                    //if answer return answer 
                    var messageToForward = await message;
                    await context.Forward<IMessageActivity, IMessageActivity>(new QnADialog(), AfterQnADialog, messageToForward, CancellationToken.None);
                    return;
                }

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
            var sentimentScore = await SentimentScore(text);
            var upsetReply = "maybe you can say something nicer. I like to hear how cute I'm";
            var happyReply = "I like you";
            if (!string.IsNullOrEmpty(preferredLanguage) && preferredLanguage != "en")
            {
                var textTranslatorService = new TextTranslatorService();
                upsetReply = await textTranslatorService.Translate("en", preferredLanguage, upsetReply);
                happyReply = await textTranslatorService.Translate("en", preferredLanguage, happyReply);
            }

            var replyTextInPreferredLanguage = sentimentScore >= 0.5
                ? happyReply + " ,Meow xoxo"
                : "Hiss, " + upsetReply;
            return replyTextInPreferredLanguage;
        }

        private static async Task<string> CatFoodReply(string text, string preferredLanguage)
        {
            var sentimentScore = await SentimentScore(text);
            var upsetReply = "food and unpleasant words can not be in same statement!";
            var happyReply = "I heard food! you are the best";
            if (!string.IsNullOrEmpty(preferredLanguage) && preferredLanguage != "en")
            {
                var textTranslatorService = new TextTranslatorService();
                upsetReply = await textTranslatorService.Translate("en", preferredLanguage, upsetReply);
            }

            var replyTextInPreferredLanguage = sentimentScore > 0.1
                ? happyReply + " ,Meow xoxo"
                : "Hiss, " + upsetReply;
            return replyTextInPreferredLanguage;
        }

        private static async Task<double> SentimentScore(string text)
        {
            var service = new TextAnalyticsService();
            var sentiments = await service.MakeSentimentRequest(text);
            var sentimentScore = sentiments?.Documents?.FirstOrDefault()?.Score ?? 0;
            return sentimentScore;
        }

        [LuisIntent("Food motivation")]
        public async Task GetAttention(IDialogContext context, LuisResult result)
        {

            if (!string.IsNullOrEmpty(result.Query))
            {
                string detectedLanguage;
                context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);

                var replyText = await CatFoodReply(result.Query, detectedLanguage);


                await context.PostAsync(replyText);
            }
            else
            {
                await context.PostAsync("huh?");
            }
            context.Wait(MessageReceived);
        }






    }
}