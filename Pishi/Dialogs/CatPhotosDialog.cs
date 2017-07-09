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
            await ShowCatCategories(context);
        }
        private async Task ShowCatCategories(IDialogContext context)
        {

            var categories = await _catPhotosService.GetPhotoCategories();
            PromptDialog.Choice(
                context: context,
                resume: ResumeAfterCategoriesDialog,
                descriptions: categories.Select(x => x.Name).ToArray(),
                options: categories.Select(x => x.Name).ToArray(),
                prompt: "what sort of cat photos?"

            );

        }

       
        private async Task ResumeAfterCategoriesDialog(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string catCategory = null;
                var choice = await result;
                if (!string.IsNullOrEmpty(choice))
                {
                    //making sure it's valid category
                    var categories = await _catPhotosService.GetPhotoCategories();
                    if (categories.Any(x => x.Name == choice))
                    {
                        catCategory = choice;
                        context.UserData.SetValue("catphotoCategory", catCategory);
                       
                    }

                }
                await ShowCatPhotos( context, catCategory);

            }
            catch (Exception e)
            {
                await context.PostAsync("something went wrong with showing the images");
                context.Done(false);
            }
        }

        private async Task ShowCatPhotos(IDialogContext context, string catCategory = null)
        {
            await ShowImages(context, catCategory);
            PromptDialog.Choice(
                context: context,
                resume: ResumeAfterPromptDialog,
                descriptions: new[] { "Yes please", "No Thanks" },
                options: new[] { "Yes please", "No Thanks" },
                prompt: "Would you like to see more?"

            );
        }


        private async Task ShowImages(IDialogContext context,string catCategory = null)
        {
            var images = await _catPhotosService.GetPhotos(10, catCategory);
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
                if (choice.ToLower().Contains("no") )
                {
                    context.Done(true);
                }
                else 
                {
                    string catphotoCategory;
                    context.UserData.TryGetValue("catphotoCategory", out catphotoCategory);
                    await ShowCatPhotos(context, catphotoCategory);

                }

            }
            catch (Exception e)
            {
                context.Fail(e);
            }



        }


    }




}