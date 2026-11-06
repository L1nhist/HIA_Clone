
namespace HIA.Data.Entities
{
    public partial class DMErrorMHTC
    {
        public int EMID { get; set; }
        public string MA_CSKCB { get; set; } = string.Empty;
        public int? EMErrorCode { get; set; }
        public bool? EMEnabled { get; set; }
        public DateTime? EMCreateDate { get; set; }
        public string? EMCreator { get; set; }

        //Linhist added 200826
        public bool? EMInternal { get; set; }
    }
}