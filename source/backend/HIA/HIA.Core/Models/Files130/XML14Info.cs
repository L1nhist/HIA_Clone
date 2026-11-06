using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Records;

namespace HIA.Core.Models.Files130;

public class XML14Info : IXMLModel<XML14>
{
    public long xml14id { get; set; }
    public long xml01id { get; set; }
    public string? ma_cskcb { get; set; }
    public string? ma_lk { get; set; }
    public int? stt { get; set; }
    public string? so_giayhen_kl { get; set; }
    public string? ho_ten { get; set; }
    public string? ngay_sinh { get; set; }
    public byte? gioi_tinh { get; set; }
    public string? dia_chi { get; set; }
    public string? ma_the_bhyt { get; set; }
    public string? gt_the_den { get; set; }
    public string? ngay_vao { get; set; }
    public string? ngay_vao_noi_tru { get; set; }
    public string? ngay_ra { get; set; }
    public string? ngay_hen_kl { get; set; }
    public string? chan_doan_rv { get; set; }
    public string? ma_benh_chinh { get; set; }
    public string? ma_benh_kt { get; set; }
    public string? ma_benh_yhct { get; set; }
    public string? ma_doituong_kcb { get; set; }
    public string? ma_bac_si { get; set; }
    public string? ma_ttdv { get; set; }
    public string? ngay_ct { get; set; }
    public string? du_phong { get; set; }

    public XML14Info(XML14? entity = null)
    {
        if (entity != null) Import(entity);
    }

    public void Import(XML14 entity)
    {
        xml14id = entity.XML14ID;
        xml01id = entity.XML01ID;
        ma_cskcb = entity.MA_CSKCB;
        ma_lk = entity.MA_LK;
        stt = entity.STT;
        so_giayhen_kl = entity.SO_GIAYHEN_KL;
        ma_cskcb = entity.MA_CSKCB;
        ho_ten = entity.HO_TEN;
        ngay_sinh = EvalHelper.ToDateView(entity.NGAY_SINH);
        gioi_tinh = entity.GIOI_TINH;
        dia_chi = entity.DIA_CHI;
        ma_the_bhyt = entity.MA_THE_BHYT;
        gt_the_den = entity.GT_THE_DEN;
        ngay_vao = EvalHelper.ToDateView(entity.NGAY_VAO);
        ngay_vao_noi_tru = EvalHelper.ToDateView(entity.NGAY_VAO_NOI_TRU);
        ngay_ra = EvalHelper.ToDateView(entity.NGAY_RA);
        ngay_hen_kl = EvalHelper.ToDateView(entity.NGAY_HEN_KL);
        chan_doan_rv = entity.CHAN_DOAN_RV;
        ma_benh_chinh = entity.MA_BENH_CHINH;
        ma_benh_kt = entity.MA_BENH_KT;
        ma_benh_yhct = entity.MA_BENH_YHCT;
        ma_doituong_kcb = entity.MA_DOITUONG_KCB;
        ma_bac_si = entity.MA_BAC_SI;
        ma_ttdv = entity.MA_TTDV;
        ngay_ct = EvalHelper.ToDateView(entity.NGAY_CT);
        du_phong = entity.DU_PHONG;

    }
}
