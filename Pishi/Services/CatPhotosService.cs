using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Microsoft.Bot.Connector;

namespace PishiBot.Services
{
    public class CatPhoto
    {
        public string Url { get; set; }
        public string SourceUrl { get; set; }
        public string Id { get; set; }
    }

    public class CatPhotoCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public interface ICatPhotosService
    {
        Task<IEnumerable<CatPhoto>> GetPhotos(int numberofPhotos, string catCategory = null);
        Task<IEnumerable<CatPhotoCategory>> GetPhotoCategories();
    }


    [Serializable]
    public class CatPhotosService : ICatPhotosService
    {


        public async Task<IEnumerable<CatPhotoCategory>> GetPhotoCategories()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync("http://thecatapi.com/api/categories/list");
            var str = XElement.Parse(response);
            var imageElements = str.DescendantsAndSelf().Where(x => x.Name == "category");

            var catCategories = new List<CatPhotoCategory>();
            catCategories.AddRange(imageElements.Select(x => new CatPhotoCategory()
            {

                Id = x.Element("id")?.Value,
                Name = x.Element("name")?.Value
            }));
            return catCategories;

        }

        public async Task<IEnumerable<CatPhoto>> GetPhotos(int numberofPhotos, string catCategory = null)
        {
            var apikey = ConfigurationManager.AppSettings["TheCatApi.ApiKey"];
            var httpClient = new HttpClient();
            var catcategorycriteria = string.IsNullOrEmpty(catCategory) ? "" : $"&category={catCategory}";
            var response = await httpClient.GetStringAsync($"http://thecatapi.com/api/images/get?format=xml&api_key={apikey}&results_per_page={numberofPhotos+5}{catcategorycriteria}");
            var str = XElement.Parse(response);
            var imageElements = str.DescendantsAndSelf().Where(x => x.Name == "image").Where(img => (!string.IsNullOrEmpty(img.Element("url")?.Value)) && RemoteFileExists(img.Element("url")?.Value)).Take(numberofPhotos);

            var catPhotos = new List<CatPhoto>();
            catPhotos.AddRange(imageElements.Select(x => new CatPhoto()
            {
                SourceUrl = x.Element("source_url")?.Value,
                Id = x.Element("id")?.Value,
                Url = x.Element("url")?.Value
            }));
            return catPhotos;

        }


        ///
        /// Checks the file exists or not.
        ///
        /// The URL of the remote file.
        /// True : If the file exits, False if file not exists
        private bool RemoteFileExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }

    }
}