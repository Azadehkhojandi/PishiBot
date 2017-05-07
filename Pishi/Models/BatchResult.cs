using System.Collections.Generic;

namespace Pishi.Models
{
    public class BatchResult
    {
        public List<DocumentResult> Documents { get; set; }
        public List<object> Errors { get; set; }
    }
}