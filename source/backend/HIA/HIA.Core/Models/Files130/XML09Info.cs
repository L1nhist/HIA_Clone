using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Entities;

namespace HIA.Core.Models.Files130;

public class XML09Info : IXMLModel<XML09>
{
    public long xml09id { get; set; }
    public long xml01id { get; set; }
    public string? ma_cskcb { get; set; }
    public string? ma_lk { get; set; }
    public int? stt { get; set; }
    public string? ma_bhxh_nnd { get; set; }
    public string? ma_the_nnd { get; set; }
    public string? ho_ten_nnd { get; set; }
    public string? ngaysinh_nnd { get; set; }
    public string? ma_dantoc_nnd { get; set; }
    public string? so_cccd_nnd { get; set; }
    public string? ngaycap_cccd_nnd { get; set; }
    public string? noicap_cccd_nnd { get; set; }
    public string? noi_cu_tru_nnd { get; set; }
    public string? ma_quoctich { get; set; }
    public string? matinh_cu_tru { get; set; }
    public string? mahuyen_cu_tru { get; set; }
    public string? maxa_cu_tru { get; set; }
    public string? ho_ten_cha { get; set; }
    public string? ma_the_tam { get; set; }
    public string? ho_ten_con { get; set; }
    public byte? gioi_tinh_con { get; set; }
    public byte? so_con { get; set; }
    public byte? lan_sinh { get; set; }
    public byte? so_con_song { get; set; }
    public int? can_nang_con { get; set; }
    public string? ngay_sinh_con { get; set; }
    public string? noi_sinh_con { get; set; }
    public string? tinh_trang_con { get; set; }
    public byte? sinhcon_phauthuat { get; set; }
    public byte? sinhcon_duoi32tuan { get; set; }
    public string? ghi_chu { get; set; }
    public string? nguoi_do_de { get; set; }
    public string? nguoi_ghi_phieu { get; set; }
    public string? ngay_ct { get; set; }
    public string? so { get; set; }
    public string? quyen_so { get; set; }
    public string? ma_ttdv { get; set; }
    public string? du_phong { get; set; }

    public XML09Info(XML09? entity = null)
    {
        if (entity != null) Import(entity);
    }

    public void Import(XML09 entity)
    {
        xml09id = entity.XML09ID;
        xml01id = entity.XML01ID;
        ma_cskcb = entity.MA_CSKCB;
        ma_lk = entity.MA_LK;
        stt = entity.STT;
        ma_bhxh_nnd = entity.MA_BHXH_NND;
        ma_the_nnd = entity.MA_THE_NND;
        ho_ten_nnd = entity.HO_TEN_NND;
        ngaysinh_nnd = Uti.ToDateView(entity.NGAYSINH_NND);
        ma_dantoc_nnd = entity.MA_DANTOC_NND;
        so_cccd_nnd = entity.SO_CCCD_NND;
        ngaycap_cccd_nnd = Uti.ToDateView(entity.NGAYCAP_CCCD_NND);
        noicap_cccd_nnd = entity.NOICAP_CCCD_NND;
        noi_cu_tru_nnd = entity.NOI_CU_TRU_NND;
        ma_quoctich = entity.MA_QUOCTICH;
        matinh_cu_tru = entity.MATINH_CU_TRU;
        mahuyen_cu_tru = entity.MAHUYEN_CU_TRU;
        maxa_cu_tru = entity.MAXA_CU_TRU;
        ho_ten_cha = entity.HO_TEN_CHA;
        ma_the_tam = entity.MA_THE_TAM;
        ho_ten_con = entity.HO_TEN_CON;
        gioi_tinh_con = entity.GIOI_TINH_CON;
        so_con = entity.SO_CON;
        lan_sinh = entity.LAN_SINH;
        so_con_song = entity.SO_CON_SONG;
        can_nang_con = entity.CAN_NANG_CON;
        ngay_sinh_con = Uti.ToDateView(entity.NGAY_SINH_CON);
        noi_sinh_con = entity.NOI_SINH_CON;
        tinh_trang_con = entity.TINH_TRANG_CON;
        sinhcon_phauthuat = entity.SINHCON_PHAUTHUAT;
        sinhcon_duoi32tuan = entity.SINHCON_DUOI32TUAN;
        ghi_chu = entity.GHI_CHU;
        nguoi_do_de = entity.NGUOI_DO_DE;
        nguoi_ghi_phieu = entity.NGUOI_GHI_PHIEU;
        ngay_ct = Uti.ToDateView(entity.NGAY_CT);
        so = entity.SO;
        quyen_so = entity.QUYEN_SO;
        ma_ttdv = entity.MA_TTDV;
        du_phong = entity.DU_PHONG;
    }
}
