

namespace HIA.Data.Entities
{
    public partial class DMPhauThuat
    {
        public int PhauThuatID { get; set; }
        public string? PhauThuatName { get; set; }
        public string? PhauThuatNameTT15 { get; set; }
        public string? PhauThuatCode { get; set; }
        public string? PhauThuatMaChuong { get; set; }
        public string? PhauThuatTuyen { get; set; }
        public string? PhauThuatLoai { get; set; }
        public double? PhauThuatGia { get; set; }
        public string? PhauThuatChuyenKhoa { get; set; }
        public byte? PhauThuatGiuongLoai { get; set; }
    }
}
