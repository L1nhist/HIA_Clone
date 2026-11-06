using HIA.Data.Base;

namespace HIA.Data.Entities
{
    public partial class DMDVGiuong : ICategoryData
    {
        long ICategoryData.ID
        {
            get => GID;
            set => GID = value;
        }

        string? ICategoryData.MA_DM
        {
            get => MA_DICH_VU;
            set => MA_DICH_VU = value;
        }
        public long GID { get; set; }
        public string MA_CSKCB { get; set; } = string.Empty;
        public string? MA_DICH_VU { get; set; }
        public string? TEN_DICH_VU { get; set; }
        public string? MA_KHOA { get; set; }
        public string? MA_TT37 { get; set; }
        public float? DON_GIA { get; set; }
        public string? TU_NGAY { get; set; }
        public string? DEN_NGAY { get; set; }
        public bool? HIEU_LUC { get; set; }
    }
}