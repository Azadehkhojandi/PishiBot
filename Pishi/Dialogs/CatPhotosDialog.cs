using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using PishiBot.Services;

namespace PishiBot.Dialogs
{
    [Serializable]
    public class CatPhotosDialog : IDialog<bool>
    {
        private readonly ICatPhotosService _catPhotosService;

        public CatPhotosDialog()
        {
            _catPhotosService = new CatPhotosService();
        }
        public async Task StartAsync(IDialogContext context)
        {
            await ShowCatPhotos(context);
        }

        private async Task ShowCatPhotos(IDialogContext context)
        {
            await ShowImages(context);
            PromptDialog.Choice(
                context: context,
                resume: ResumeAfterPromptDialog,
                descriptions: new[] {"Yes please", "No Thanks"},
                options: new[] {"Yes", "No"},
                prompt: "Would you like to see more?"
              
            );
        }


        private async Task ShowImages(IDialogContext context)
        {
            var images = await _catPhotosService.GetPhotos(10);
            var cardsAttachments = new List<Attachment>();

            foreach (var image in images)
            {
                cardsAttachments.Add(new HeroCard
                {
                    Images = new List<CardImage>() { new CardImage(image.Url) },
                    Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Type = ActionTypes.OpenUrl,
                            Title = "browse",
                            Value = image.SourceUrl
                        }
                    }
                }.ToAttachment());
            }
            var reply = context.MakeMessage();

            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = cardsAttachments;

            await context.PostAsync(reply);




          


        }

        private async Task ResumeAfterPromptDialog(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var choice = await result;
                if (choice == "No")
                {
                    context.Done(true);
                }
                else if (choice == "Yes")
                {
                    await ShowCatPhotos(context);

                }

            }
            catch (Exception e)
            {
                await context.PostAsync("something went wrong with showing the images");
                context.Done(false);
            }
         


        }
    

    }




}