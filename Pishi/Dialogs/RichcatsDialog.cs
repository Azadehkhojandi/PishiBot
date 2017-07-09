using System;
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
    public class RichCatsDialog : IDialog<bool>
    {
        private readonly IRichCatsService _richCatsService;
        private readonly ICatReplyService _catReplyService;
        public RichCatsDialog()
        {
            //todo DI
            _catReplyService = new CatReplyService();
            _richCatsService = new RichCatsService();

        }

        public async Task StartAsync(IDialogContext context)
        {
            string detectedLanguage;
            context.UserData.TryGetValue("PreferredLanguage", out detectedLanguage);

            try
            {
                var richCats = await _richCatsService.RichCatsCards(detectedLanguage);
                if (richCats.Any())
                {
                    var reply = context.MakeMessage();

                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    reply.Attachments = richCats;
                    await context.PostAsync(reply);
                    context.Done(true);
                }
                else
                {
                    await context.PostAsync(await _catReplyService.NoRichCatsMessage(detectedLanguage));
                    context.Done(true);
                }
            }
            catch (Exception e)
            {

                await context.PostAsync(await _catReplyService.ErrorMessage(detectedLanguage));
                context.Fail(e);
               

            }
        }
    }
}