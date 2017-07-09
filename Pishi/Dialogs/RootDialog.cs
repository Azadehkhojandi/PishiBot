using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
            if (result.Intents.FirstOrDefault()?.Score > 0.1)
            {
                context.Call(new GreetingDialog(), AfterDialogFinishesConditionalEngagement);
            }
            else
            {
                await GenericAnswer(context, message, result);
            }
           

        }


        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {


            if (result.Intents.FirstOrDefault()?.Score > 0.5)
            {
                context.Call(new HelpDialog(), AfterDialogFinishesConditionalEngagement);

            }
            else
            {
                await GenericAnswer(context, message, result);
            }


        }

        [LuisIntent("Show rich cats")]
        public async Task ShowRichCats(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {

            if (result.Intents.FirstOrDefault()?.Score > 0.5)
            {
                context.Call(new RichCatsDialog(), AfterDialogFinishesConditionalEngagement);

            }
            else
            {
                await GenericAnswer(context, message, result);
            }


        }

        [LuisIntent("Show cat photos")]
        public async Task ShowCatPhotos(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {

            if (result.Intents.FirstOrDefault()?.Score > 0.5)
            {
                //call cat photos dialog
                context.Call(new CatPhotosDialog(), AfterDialogFinishes);

            }
            else
            {
                await GenericAnswer(context, message, result);
            }



        }
        [LuisIntent("Rate my cat")]
        public async Task Ratemycat(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            try
            {
                if (result.Intents.FirstOrDefault()?.Score > 0.4)
                {
                    context.Call(new RateMyCatDialog(), AfterDialogFinishes);
                }
                else
                {
                    await GenericAnswer(context, message, result);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await context.PostAsync(e.Message);
            }
           



        }

        [LuisIntent("Play time")]
        public async Task PlayTime(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {

            if (result.Intents.FirstOrDefault()?.Score > 0.5)
            {
                context.Call(new PlayTimeDialog(), AfterDialogFinishesConditionalEngagement);

            }
            else
            {
                await GenericAnswer(context, message, result);
            }


        }

        [LuisIntent("Food motivation")]
        public async Task GetAttention(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {


            if (result.Intents.FirstOrDefault()?.Score > 0.5)
            {
                context.Call(new FoodMotivationDialog(), AfterDialogFinishesConditionalEngagement);

            }
            else
            {
                await GenericAnswer(context, message, result);
            }
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
            }


            context.Wait(MessageReceived);
        }

        private async Task AfterDialogFinishesConditionalEngagement(IDialogContext context, IAwaitable<bool> result)
        {
            try
            {
                var success = await result;
                if (!success)
                {
                    var message = await EngagmentMessage(context);
                    await context.PostAsync(message);
                }

               
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
               
            }
             context.Wait(MessageReceived);

        }

        private async Task AfterDialogFinishes(IDialogContext context, IAwaitable<bool> result)
        {
            var message = await EngagmentMessage(context);
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        private async Task<IMessageActivity> EngagmentMessage(IDialogContext context)
        {
            string detectedLanguage;
            context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);
            var engagingReply = await _catReplyService.CatEngagingReply(detectedLanguage);
            var message = context.MakeMessage();
            message.Attachments.Add(engagingReply);
            return message;
        }
    }
}