﻿using System;
using System.Collections.Generic;
using System.Linq;
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
    }
    [Serializable]
    public class CatReplyService: ICatReplyService
    {
        private ITextAnalyticsService _textAnalyticsService;
        public CatReplyService()
        {
            //todo di
            _textAnalyticsService = new TextAnalyticsService();
        }
        public  async Task<string> CatReply(string text, string preferredLanguage)
        {
            var sentimentScore = await SentimentScore(text);
            var upsetReply = "maybe you can say something nicer. I like to hear how cute I'm";
            var happyReply = "I like you";
            if (!string.IsNullOrEmpty(preferredLanguage) && preferredLanguage != "en")
            {
                var textTranslatorService = new TextTranslatorService();
                upsetReply = await textTranslatorService.Translate("en", preferredLanguage, upsetReply);
                happyReply = await textTranslatorService.Translate("en", preferredLanguage, happyReply);
            }

            var replyTextInPreferredLanguage = sentimentScore >= 0.5
                ? happyReply + " ,Meow xoxo"
                : "Hiss, " + upsetReply;
            return replyTextInPreferredLanguage;
        }

        public async Task<string> PlayTimeReply(string text, string preferredLanguage)
        {
            var sentimentScore = await SentimentScore(text);
            var upsetReply = "you want me to play with you? no way!";
            var happyReply = "I like playing!";
            if (!string.IsNullOrEmpty(preferredLanguage) && preferredLanguage != "en")
            {
                var textTranslatorService = new TextTranslatorService();
                upsetReply = await textTranslatorService.Translate("en", preferredLanguage, upsetReply);
            }

            var replyTextInPreferredLanguage = sentimentScore > 0.1
                ? happyReply + " Meow xoxo"
                : "Hiss, " + upsetReply;
            return replyTextInPreferredLanguage;
        }
        public async Task<string> GreetingReply(string text, string preferredLanguage)
        {
            var sentimentScore = await SentimentScore(text);
            var upsetReply = "hi rude human being! be nice to cats!";
            var happyReply = "hello :)";
            if (!string.IsNullOrEmpty(preferredLanguage) && preferredLanguage != "en")
            {
                var textTranslatorService = new TextTranslatorService();
                upsetReply = await textTranslatorService.Translate("en", preferredLanguage, upsetReply);
            }

            var replyTextInPreferredLanguage = sentimentScore > 0.1
                ? happyReply + " ,Meow xoxo"
                : "Hiss, " + upsetReply;
            return replyTextInPreferredLanguage;
        }

        public async Task<string> CatFoodReply(string text, string preferredLanguage)
        {
            var sentimentScore = await SentimentScore(text);
            var upsetReply = "food and unpleasant words can not be in the same statement!";
            var happyReply = "I heard food! you are the best";
            if (!string.IsNullOrEmpty(preferredLanguage) && preferredLanguage != "en")
            {
                var textTranslatorService = new TextTranslatorService();
                upsetReply = await textTranslatorService.Translate("en", preferredLanguage, upsetReply);
            }

            var replyTextInPreferredLanguage = sentimentScore > 0.1
                ? happyReply + " ,Meow xoxo"
                : "Hiss, " + upsetReply;
            return replyTextInPreferredLanguage;
        }
        private async Task<double> SentimentScore(string text)
        {
            
            var sentiments = await _textAnalyticsService.MakeSentimentRequest(text);
            var sentimentScore = sentiments?.Documents?.FirstOrDefault()?.Score ?? 0;
            return sentimentScore;
        }
    }
}