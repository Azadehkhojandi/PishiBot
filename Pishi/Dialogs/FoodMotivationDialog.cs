﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using PishiBot.Services;

namespace PishiBot.Dialogs
{
    [Serializable]
    public class FoodMotivationDialog : IDialog<bool>
    {
        private readonly ICatReplyService _catReplyService;
        public FoodMotivationDialog()
        {
            //todo DI
            _catReplyService = new CatReplyService();
        }
        public async Task StartAsync(IDialogContext context)
        {

            string detectedLanguage;
            context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);
            var activity = context.Activity;
            if (activity.Type == ActivityTypes.Message)
            {
                var message = ((Activity)activity).Text;
                var replyText = await _catReplyService.CatFoodReply(message, detectedLanguage);
                await context.PostAsync(replyText);
                context.Done(true);

            }
            else
            {
                await context.PostAsync("huh?");
                context.Done(false);
            }

        }
    }
}