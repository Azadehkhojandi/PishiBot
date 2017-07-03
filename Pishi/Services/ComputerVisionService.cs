using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace PishiBot.Services
{
    //http://json2csharp.com/
    public class ComputerVisionCategory
    {
        public string name { get; set; }
        public double score { get; set; }
    }

    public class ComputerVisionCaption
    {
        public string text { get; set; }
        public double confidence { get; set; }
    }

    public class ComputerVisionDescription
    {
        public List<string> tags { get; set; }
        public List<ComputerVisionCaption> captions { get; set; }
    }

    public class ComputerVisionMetadata
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; }
    }

    public class ComputerVisionColor
    {
        public string DominantColorForeground { get; set; }
        public string DominantColorBackground { get; set; }
        public List<string> DominantColors { get; set; }
        public string AccentColor { get; set; }
        public bool IsBWImg { get; set; }
    }

    public class ComputerVisionResult
    {
        public List<ComputerVisionCategory> Categories { get; set; }
        public ComputerVisionDescription Description { get; set; }
        public string RequestId { get; set; }
        public ComputerVisionMetadata Metadata { get; set; }
        public Color Color { get; set; }
    }


    public interface IComputerVisionService
    {
        Task<ComputerVisionResult> MakeAnalysisRequest(byte[] image);
        Task<ComputerVisionResult> MakeAnalysisRequest(string imagePath);

    }
    [Serializable]
    public class ComputerVisionService: IComputerVisionService
    {

        public async Task<ComputerVisionResult> MakeAnalysisRequest(byte[] image)
        {
            var client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ConfigurationManager.AppSettings["ComputerVision.Key"]);

            // Request parameters. A third optional parameter is "details".
            var requestParameters = "visualFeatures=Categories,Description&language=en";

            // Assemble the URI for the REST API Call.
            var uri = ConfigurationManager.AppSettings["ComputerVision.AnalyzeUrl"] + "?" + requestParameters;


            using (var content = new ByteArrayContent(image))
            {

                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var response = await client.PostAsync(uri, content);

                var contentString = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<ComputerVisionResult>(contentString);

                return result;

            }

        }


        public async Task<ComputerVisionResult> MakeAnalysisRequest(string imageFilePath)
        {
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            return await MakeAnalysisRequest(byteData);
        }



        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            var fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            var binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

    }
}
