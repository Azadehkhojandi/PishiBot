using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PishiBot.Services
{
    public interface ICatPhotoAnalyzerService
    {
        Task<string> MakeAnalysisRequest(byte[] image, string preferredLanguag);
    }
    [Serializable]
    public class CatPhotoAnalyzerService : ICatPhotoAnalyzerService
    {
        private readonly IComputerVisionService _computerVisionService;
        private readonly ITextTranslatorService _textTranslatorService;
        public CatPhotoAnalyzerService()
        {
            _computerVisionService = new ComputerVisionService();
            _textTranslatorService = new TextTranslatorService();
        }

        public async Task<string> MakeAnalysisRequest(byte[] image, string preferredLanguag)
        {
            var result = await _computerVisionService.MakeAnalysisRequest(image);
            if (result.Categories.Any(x=>x.name== "animal_cat" && x.score>0.5))
            {

                var catRatingMessage = await _textTranslatorService.TranslateFromEnglish(preferredLanguag, "That is a pretty kitty =^_^=  11/10");
                return catRatingMessage;
            }
            var imagecaption = result.Description.captions.OrderByDescending(x => x.confidence).FirstOrDefault();
            var catNotFound = await _textTranslatorService.TranslateFromEnglish(preferredLanguag, $"That is not a cat =v_v=  {(imagecaption != null ? $"I'm {Math.Round(imagecaption.confidence * 100, 2)} sure, I see {imagecaption.text}" : "")}");
            return catNotFound;
        }
    }
}