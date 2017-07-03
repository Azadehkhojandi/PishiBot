using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Connector;

namespace PishiBot.Services
{
    public interface ICatReplyService
    {
        Task<string> CatReply(string text, string preferredLanguage);
        Task<string> CatFoodReply(string text, string preferredLanguage);

        Task<string> PlayTimeReply(string resultQuery, string detectedLanguage);
        Task<string> GreetingReply(string resultQuery, string detectedLanguage);

        Task<Attachment> Aboutme(string preferredLanguage);
        Task<string> ErrorMessage(string preferredLanguage);

        Task<string> NoRichCatsMessage(string preferredLanguage);

        Task<string> UploadYourCatPhoto(string detectedLanguage);
        Task<string> ReceivedImage(string mediaType, long? contentLenghtBytes, string detectedLanguage);
    }
    [Serializable]
    public class CatReplyService : ICatReplyService
    {
        private readonly ITextAnalyticsService _textAnalyticsService;
        private readonly ITextTranslatorService _textTranslatorService;

        public CatReplyService()
        {
            //todo di
            _textAnalyticsService = new TextAnalyticsService();
            _textTranslatorService = new TextTranslatorService();
        }
        public async Task<string> CatReply(string text, string preferredLanguage)
        {
            var sentimentScore = await SentimentScore(text);

            var upsetReply = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "maybe you can say something nicer. I like to hear how cute I'm");
            var happyReply = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "I like you");


            var replyTextInPreferredLanguage = sentimentScore >= 0.5
                ? happyReply + " ,Meow xoxo"
                : "Hiss, " + upsetReply;
            return replyTextInPreferredLanguage;
        }

        public async Task<string> PlayTimeReply(string text, string preferredLanguage)
        {
            var sentimentScore = await SentimentScore(text);
            var upsetReply = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "you want me to play with you? no way!"); ;
            var happyReply = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "I like playing!"); ;


            var replyTextInPreferredLanguage = sentimentScore > 0.1
                ? happyReply + " Meow xoxo"
                : "Hiss, " + upsetReply;
            return replyTextInPreferredLanguage;
        }
        public async Task<string> GreetingReply(string text, string preferredLanguage)
        {
            var sentimentScore = await SentimentScore(text);
            var upsetReply = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "hi rude human being! be nice to cats!");
            var happyReply = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "hello :)");


            var replyTextInPreferredLanguage = sentimentScore > 0.1
                ? happyReply + " ,Meow xoxo"
                : "Hiss, " + upsetReply;
            return replyTextInPreferredLanguage;
        }

        public async Task<string> CatFoodReply(string text, string preferredLanguage)
        {
            var sentimentScore = await SentimentScore(text);
            var upsetReply = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "food and unpleasant words can not be in the same statement!");
            var happyReply = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "I heard food! you are the best");


            var replyTextInPreferredLanguage = sentimentScore > 0.1
                ? happyReply + " ,Meow xoxo"
                : "Hiss, " + upsetReply;
            return replyTextInPreferredLanguage;
        }
        public async Task<Attachment> Aboutme(string preferredLanguage)
        {


            var title = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "I'm Pishi the cat bot");
            var subtitle = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "Your digital predictable cat!");
            var text = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "I like being patted and hear how cute I am! You can also ask me cat related questions. If you say positive and nice sentences to cat bot you will get Meow, If you say negative sentences you will get Hiss! Few samples: You are a cute cat! Would you like to play? Do you want food? Start conversation or Type help whenever you need help!");
            var action1 = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "Show me Rich Cats");
            var action2 = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "Show cat Photos");
            var action3 = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "Rate my cat");



            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage> { new CardImage("http://pishiapiappname.azurewebsites.net/Images/cat-icon.png") },
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.PostBack, action1, value: action1),
                    new CardAction(ActionTypes.PostBack, action2, value: action2),
                    new CardAction(ActionTypes.PostBack, action3, value: action3)
                }
            };

            return heroCard.ToAttachment();
        }

        public async Task<string> ErrorMessage(string preferredLanguage)
        {
            var reply = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "My cat brain is not working! Please try later");
            return reply;
        }

        public async Task<string> NoRichCatsMessage(string preferredLanguage)
        {

            var reply = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "Couldn't find any rich cat near you");
            return reply;
        }
        public async Task<string> UploadYourCatPhoto(string preferredLanguage)
        {

            var reply = await _textTranslatorService.TranslateFromEnglish(preferredLanguage, "Please upload your cat photo.");
            return reply;
        }

        public Task<string> ReceivedImage(string mediaType, long? contentLenghtBytes, string detectedLanguage)
        {
            var reply = _textTranslatorService.TranslateFromEnglish(detectedLanguage,
                $"I Received {mediaType} with {contentLenghtBytes??0}.{((contentLenghtBytes??0)>0?" let me see your cat photo":"")} ");
            return reply;
        }

        private async Task<double> SentimentScore(string text)
        {

            var sentiments = await _textAnalyticsService.MakeSentimentRequest(text);
            var sentimentScore = sentiments?.Documents?.FirstOrDefault()?.Score ?? 0;
            return sentimentScore;
        }
    }
}