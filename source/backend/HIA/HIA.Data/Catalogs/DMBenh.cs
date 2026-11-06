namespace HIA.Data.Catalogs;

public partial class DMBenh
{
    public int BenhID { get; set; }
    public string? BenhCode { get; set; }
    public string? TenBenh_Original_EN { get; set; }
    public string? TenBenh_Original_VN { get; set; }
    public string? TenBenh_Modify { get; set; }
    public string? BenhMaNhomBC_BYT { get; set; }
    public string? BenhMaChiTiet { get; set; }
    public string? BenhMaGocBS_MCT { get; set; }
    public int? BenhLBID { get; set; }
    public string? BenhLBCode { get; set; }

    public virtual DMBenhPhanLoai? BenhLB { get; set; }
}
