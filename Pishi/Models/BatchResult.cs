using System.Collections.Generic;

namespace PishiBot.Models
{
    public class BatchResult
    {
        public List<DocumentResult> Documents { get; set; }
        public List<object> Errors { get; set; }
    }
}