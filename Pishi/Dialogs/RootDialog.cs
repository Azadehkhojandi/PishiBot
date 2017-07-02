using System;
using System.Linq;
using System.Threading;
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
        private readonly ICatReplyService _catReplyService;

        public RootDialog()
        {
            //todo DI
             _catReplyService = new CatReplyService();
        }
        private async Task AfterQnADialog(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            try
            {
                var messageHandled = await result;
                if (!(messageHandled.Value != null && bool.Parse(messageHandled.Value.ToString())))
                {
                    //couldn't find the answer
                   // await GenericCatReply(context, messageHandled.Text);
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
                await GenericAnswer(context, message, result);
            }
            else
            {
                await context.PostAsync("huh?");
                context.Wait(MessageReceived);
            }
            
        }
       
        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            string detectedLanguage;
            context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);
            var replyText = await _catReplyService.GreetingReply(result.Query, detectedLanguage);


            await context.PostAsync(replyText);
        }

        [LuisIntent("Start over")]
        public async Task StartOver(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            string detectedLanguage;
            context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);
            await context.PostAsync("Start over");
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            string detectedLanguage;
            context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);
            await context.PostAsync("Help");
        }

        [LuisIntent("Play Time")]
        public async Task PlayTime(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            string detectedLanguage;
            context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);

            if (result.Intents.FirstOrDefault()?.Score > 0.5)
            {
                var replyText = await _catReplyService.PlayTimeReply(result.Query, detectedLanguage);
                await context.PostAsync(replyText);
            }
            else
            {
                await GenericAnswer(context, message, result);
            }


           
        }

        [LuisIntent("Food motivation")]
        public async Task GetAttention(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {

            if (!string.IsNullOrEmpty(result.Query))
            {
                string detectedLanguage;
                context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);

                
                if (result.Intents.FirstOrDefault()?.Score > 0.5)
                {
                    var replyText = await _catReplyService.CatFoodReply(result.Query, detectedLanguage);
                    await context.PostAsync(replyText);
                }
                else
                {
                    await GenericAnswer(context, message, result);
                }

               
            }
            else
            {
                await context.PostAsync("huh?");
            }
            context.Wait(MessageReceived);
        }


        private async Task GenericAnswer(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            //if question ends with ?
            if (result.Query.EndsWith("?"))
            {
                //call qanA
                await context.Forward(new QnADialog(), AfterQnADialog, await message, CancellationToken.None);
            }
            else
            {
                await GenericCatReply(context, result.Query);
                context.Wait(MessageReceived);
            }
        }

        private async Task GenericCatReply(IDialogContext context, string message)
        {
            string detectedLanguage;
            context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);
            var replyText = await _catReplyService.CatReply(message, detectedLanguage);
            await context.PostAsync(replyText);
        }




    }
}