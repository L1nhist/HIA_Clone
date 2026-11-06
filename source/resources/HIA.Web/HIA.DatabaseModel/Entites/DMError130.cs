#nullable disable

using System.ComponentModel.DataAnnotations;

namespace HIA.DatabaseModel.Entites
{
    public partial class DMError130
    {
        public int ErrorId { get; set; }
        public int? ErrorCode { get; set; }
        public int? ErrorType { get; set; }
        public string ErrorFile { get; set; }
        public string ErrorTag { get; set; }
        public string Message { get; set; }
        public string Comment { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsDelete { get; set; }
        public string ErrorCatetory { get; set; }
        public string ErrorExplanation { get; set; }
        public string ErrorKeyword { get; set; }
        public string FixSugesstion { get; set; }
        public string HospitalScope { get; set; }
        public string ApplicableDecision { get; set; }
        public string ApplicableLink { get; set; }
        public string ReferenceExcept { get; set; }
        public string RelatedField { get; set; }
        public string Response { get; set; }
        public string TriggerCondition { get; set; }
    }
}