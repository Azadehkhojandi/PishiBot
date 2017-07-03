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
        private readonly IRichCatsService _richCatsService;

        public RootDialog()
        {
            //todo DI
            _catReplyService = new CatReplyService();
            _richCatsService = new RichCatsService();
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

            var newMessage = context.MakeMessage();

            newMessage.Attachments.Add(_catReplyService.Aboutme());

            await context.PostAsync(newMessage);




        }

        [LuisIntent("Show rich cats")]
        public async Task ShowRichCats(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            string detectedLanguage;
            context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);

            try
            {
                var test = await _richCatsService.RichCatsCards();
                if (test.Any())
                {
                    var reply = context.MakeMessage();

                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    reply.Attachments = test;
                    await context.PostAsync(reply);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }




            await context.PostAsync("couldn't find any rich cat around you!");




        }

        [LuisIntent("Show cat photos")]
        public async Task ShowCatPhotos(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {

            if (result.Intents.FirstOrDefault()?.Score > 0.5)
            {
                //call qanA
                context.Call(new CatPhotosDialog(), AfterCatPhotosDialog);

            }
            else
            {
                await GenericAnswer(context, message, result);
            }



        }



        [LuisIntent("Play time")]
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

        private async Task AfterCatPhotosDialog(IDialogContext context, IAwaitable<bool> result)
        {
            context.Wait(MessageReceived);
        }

    }
}