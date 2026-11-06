using HIA.Data.Base;

namespace HIA.Data.Entities
{
    public partial class DMKhoaBV : ICategoryData
    {
        long ICategoryData.ID
        {
            get => KBVID;
            set => KBVID = (int)value;
        }

        string? ICategoryData.MA_DM
        {
            get => MA_KHOA;
            set => MA_KHOA = value;
        }

        public int KBVID { get; set; }
        public string MA_CSKCB { get; set; } = string.Empty;
        public string? MA_KHOA { get; set; }
        public string? TEN_KHOA { get; set; }
        public string? TU_NGAY { get; set; }
        public string? DEN_NGAY { get; set; }
    }
}