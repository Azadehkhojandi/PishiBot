using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
    public interface ICatPhotosService
    {
        Task<IEnumerable<CatPhoto>> GetPhotos(int numberofPhotos);
    }
    [Serializable]
    public class CatPhotosService : ICatPhotosService
    {
        public async Task<IEnumerable<CatPhoto>> GetPhotos(int numberofPhotos)
        {
            var apikey = ConfigurationManager.AppSettings["TheCatApi.ApiKey"];
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync($"http://thecatapi.com/api/images/get?format=xml&api_key={apikey}&results_per_page={numberofPhotos}");
            var str = XElement.Parse(response);
            var imageElements = str.DescendantsAndSelf().Where(x => x.Name == "image").Where(img => !string.IsNullOrEmpty(img.Element("url")?.Value));

            var catPhotos = new List<CatPhoto>();
            catPhotos.AddRange(imageElements.Select(x => new CatPhoto()
            {
                SourceUrl = x.Element("source_url")?.Value,
                Id = x.Element("id")?.Value,
                Url = x.Element("url")?.Value
            }));
            return catPhotos;

        }


    }
}