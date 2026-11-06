namespace HIA.Core.Models.Categories
{
    public class ErrorCounterModel
    {
        public int? ErrorCode { get; set; }

        public int? ErrorCount { get; set; }

        public string ErrorAtXMLFile { get; set; }

        public string ErrorAtXMLTag { get; set; }

        public string ErrorContent { get; set; }

        public bool? ErrorEnabled { get; set; }
    }
}
