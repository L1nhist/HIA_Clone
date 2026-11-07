
using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Records;
namespace HIA.Core.Models.Files;

public class XML08Info : IXMLModel<XML08>
{
    public long xml08id { get; set; }
    public long xml01id { get; set; }
    public string? ma_cskcb { get; set; }
    public string? ma_lk { get; set; }
    public int? stt { get; set; }
    public string? ma_loai_kcb { get; set; }
    public string? ho_ten_cha { get; set; }
    public string? ho_ten_me { get; set; }
    public string? nguoi_giam_ho { get; set; }
    public string? don_vi { get; set; }
    public string? ngay_vao { get; set; }
    public string? ngay_ra { get; set; }
    public string? chan_doan_vao { get; set; }
    public string? chan_doan_rv { get; set; }
    public string? qt_benhly { get; set; }
    public string? tomtat_kq { get; set; }
    public string? pp_dieutri { get; set; }
    public string? ngay_sinhcon { get; set; }
    public string? ngay_conchet { get; set; }
    public byte? so_conchet { get; set; }
    public byte? ket_qua_dtri { get; set; }
    public string? ghi_chu { get; set; }
    public string? ma_ttdv { get; set; }
    public string? ngay_ct { get; set; }
    public string? ma_the_tam { get; set; }
    public string? du_phong { get; set; }

    public XML08Info(XML08? entity = null)
    {
        if (entity != null) Import(entity);
    }

    public void Import(XML08 entity)
    {
        xml08id = entity.XML08ID;
        xml01id = entity.XML01ID;
        ma_cskcb = entity.MA_CSKCB;
        ma_lk = entity.MA_LK;
        stt = entity.STT;
        ma_loai_kcb = entity.MA_LOAI_KCB;
        ho_ten_cha = entity.HO_TEN_CHA;
        ho_ten_me = entity.HO_TEN_ME;
        nguoi_giam_ho = entity.NGUOI_GIAM_HO;
        don_vi = entity.DON_VI;
        ngay_vao = Uti.ToDateView(entity.NGAY_VAO);
        ngay_ra = Uti.ToDateView(entity.NGAY_RA);
        chan_doan_vao = entity.CHAN_DOAN_VAO;
        chan_doan_rv = entity.CHAN_DOAN_RV;
        qt_benhly = entity.QT_BENHLY;
        tomtat_kq = entity.TOMTAT_KQ;
        pp_dieutri = entity.PP_DIEUTRI;
        ngay_sinhcon = Uti.ToDateView(entity.NGAY_SINHCON);
        ngay_conchet = Uti.ToDateView(entity.NGAY_CONCHET);
        so_conchet = entity.SO_CONCHET;
        ket_qua_dtri = entity.KET_QUA_DTRI;
        ghi_chu = entity.GHI_CHU;
        ma_ttdv = entity.MA_TTDV;
        ngay_ct = Uti.ToDateView(entity.NGAY_CT);
        ma_the_tam = entity.MA_THE_TAM;
        du_phong = entity.DU_PHONG;
    }

}
