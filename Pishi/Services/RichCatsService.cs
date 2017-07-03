using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace PishiBot.Services
{
    public class RichCats
    {
        public List<CatInfo> Cats { get; set; }
    }
    public class CatInfo
    {
        public string Name { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
        public string MoreInfoUrl { get; set; }
    }
    public interface IRichCatsService
    {
        Task<IList<CatInfo>> RichCats();
        Task<List<Attachment>> RichCatsCards(string preferredLanguage);

    }

    [Serializable]
    public class RichCatsService : IRichCatsService
    {
        private readonly ITextTranslatorService _textTranslatorService;

        public RichCatsService()
        {
            _textTranslatorService = new TextTranslatorService();
        }
        public async Task<IList<CatInfo>> RichCats()
        {
            using (var client = new HttpClient())
            {
                var url = "http://pishiapiappname.azurewebsites.net/Content/RichCats.json";
                using (var r = await client.GetAsync(new Uri(url)))
                {
                    var result = await r.Content.ReadAsStringAsync();

                    var cats = JsonConvert.DeserializeObject<RichCats>(result);
                    return cats.Cats;

                }
            }
        }

        public async Task<List<Attachment>> RichCatsCards(string preferredLanguage)
        {
            var cats = await RichCats();
            var cardsAttachments = new List<Attachment>();

            string tmpCatName = "";
            string tmpCatBio = "";


            var tmpReadMore = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "read more");

            foreach (var cat in cats)
            {

                tmpCatName = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, cat.Name);
                tmpCatBio = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, cat.Bio);



                cardsAttachments.Add(new HeroCard
                {
                    Images = new List<CardImage>() { new CardImage(cat.Image) },
                    Title = tmpCatName,
                    Text = tmpCatBio,
                    Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Type = ActionTypes.OpenUrl,
                            Title =tmpReadMore,
                            Value = cat.MoreInfoUrl
                        }
                    }
                }.ToAttachment());
            }
            return cardsAttachments;
        }


    }
}