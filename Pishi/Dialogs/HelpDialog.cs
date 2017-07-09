using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using PishiBot.Services;

namespace PishiBot.Dialogs
{
    [Serializable]
    public class HelpDialog:IDialog<bool>
    {
        private readonly ICatReplyService _catReplyService;

        public HelpDialog()
        {
            _catReplyService = new CatReplyService();
        }
        public async Task StartAsync(IDialogContext context)
        {
            string detectedLanguage;
            context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);

            var newMessage = context.MakeMessage();

            newMessage.Attachments.Add(await _catReplyService.Aboutme(detectedLanguage));

            await context.PostAsync(newMessage);

            context.Done(true);
        }
    }
}