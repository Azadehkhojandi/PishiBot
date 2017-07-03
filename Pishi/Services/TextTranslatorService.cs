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


    public class TextTranslatorService


    {



        //translator
        public async Task<string> Detect(string textToDetect)
        {
            //todo refactor 
            var authTokenSource = new TextTranslatorAzureAuthToken(ConfigurationManager.AppSettings["TextTranslator.Key"]);
            var authToken = await authTokenSource.GetAccessTokenAsync();


            string uri = ConfigurationManager.AppSettings["TextTranslator.DetectUrl"] + "?text=" + textToDetect;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", authToken);

            using (WebResponse response = httpWebRequest.GetResponse())
            {

                using (Stream stream = response.GetResponseStream())
                {
                    DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                    string languageDetected = (string)dcs.ReadObject(stream);
                    return languageDetected;
                }
            }


        }

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

        public async Task<string> Translate(string fromLanguage, string toLanguage, string text)
        {
            //todo refactor 
            var authTokenSource = new TextTranslatorAzureAuthToken(ConfigurationManager.AppSettings["TextTranslator.Key"]);
            var authToken = await authTokenSource.GetAccessTokenAsync();


            string uri = "https://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + HttpUtility.UrlEncode(text) + "&from=" + fromLanguage + "&to=" + toLanguage;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", authToken);
            using (WebResponse response = httpWebRequest.GetResponse())
            using (Stream stream = response.GetResponseStream())
            {
                DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String"));
                string translation = (string)dcs.ReadObject(stream);
                return translation;
            }
        }


    }
}