using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace PishiBot.Services
{

    public interface ITextTranslatorService
    {
        Task<string> Detect(string textToDetect);
        Task<List<string>> GetLanguagesForTranslate();

        Task<string> Translate(string fromLanguage, string toLanguage, string text);
        Task<string> TranslateFromEnglish(string toLanguage, string text);
    }

    [Serializable]
    public class TextTranslatorService : ITextTranslatorService

    {



        public async Task<List<string>> GetLanguagesForTranslate()
        {
            //todo refactor 
            var authTokenSource = new TextTranslatorAzureAuthToken(ConfigurationManager.AppSettings["TextTranslator.Key"]);
            var authToken = await authTokenSource.GetAccessTokenAsync();

            string uri = "https://api.microsofttranslator.com/v2/Http.svc/GetLanguagesForTranslate";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", authToken);
            using (WebResponse response = httpWebRequest.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    DataContractSerializer dcs = new DataContractSerializer(typeof(List<string>));
                    List<string> languagesForTranslate = (List<string>)dcs.ReadObject(stream);
                    return languagesForTranslate;

                }
            }
        }

        public async Task<string> TranslateFromEnglish(string toLanguage, string text)
        {
            if (!string.IsNullOrEmpty(toLanguage) && toLanguage != "en")
            {
                return await Translate("en", toLanguage, text);
            }
            return text;
        }
        //translator
        public async Task<string> Detect(string textToDetect)
        {
            //todo refactor 
            var authTokenSource = new TextTranslatorAzureAuthToken(ConfigurationManager.AppSettings["TextTranslator.Key"]);
            var authToken = await authTokenSource.GetAccessTokenAsync();

            var uri = ConfigurationManager.AppSettings["TextTranslator.DetectUrl"] + "?text=" + textToDetect;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", authToken);

            using (var response = httpWebRequest.GetResponse())
            {

                using (Stream stream = response.GetResponseStream())
                {
                    var dcs = new DataContractSerializer(Type.GetType("System.String"));
                    var languageDetected = (string)dcs.ReadObject(stream);
                    return languageDetected;
                }
            }
        }


        public async Task<string> Translate(string fromLanguage, string toLanguage, string text)
        {
            //todo refactor 
            var authTokenSource = new TextTranslatorAzureAuthToken(ConfigurationManager.AppSettings["TextTranslator.Key"]);
            var authToken = await authTokenSource.GetAccessTokenAsync();
            var uri = "https://api.microsofttranslator.com/v2/Http.svc/Translate?text=" +
                HttpUtility.UrlEncode(text) + "&from=" + fromLanguage + "&to=" + toLanguage;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);

            httpWebRequest.Headers.Add("Authorization", authToken);

            using (var response = httpWebRequest.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                var dcs = new DataContractSerializer(Type.GetType("System.String"));
                var translation = (string)dcs.ReadObject(stream);
                return translation;
            }
        }


    }
}