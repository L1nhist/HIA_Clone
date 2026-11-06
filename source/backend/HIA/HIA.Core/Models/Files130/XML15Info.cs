using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Records;

namespace HIA.Core.Models.Files130;

public class XML15Info : IXMLModel<XML15>
{
    public long xml15id { get; set; }
    public long xml01id { get; set; }
    public string? ma_cskcb { get; set; }
    public string? ma_lk { get; set; }
    public int? stt { get; set; }
    public string? ma_bn { get; set; }
    public string? ho_ten { get; set; }
    public string? so_cccd { get; set; }
    public byte? phanloai_lao_vitri { get; set; }
    public byte? phanloai_lao_ts { get; set; }
    public byte? phanloai_lao_hiv { get; set; }
    public byte? phanloai_lao_vk { get; set; }
    public byte? phanloai_lao_kt { get; set; }
    public byte? loai_dtri_lao { get; set; }
    public string? ngaybd_dtri_lao { get; set; }
    public byte? phacdo_dtri_lao { get; set; }
    public string? ngaykt_dtri_lao { get; set; }
    public byte? ket_qua_dtri_lao { get; set; }
    public string? ngaykd_hiv { get; set; }
    public string? bddt_arv { get; set; }
    public string? ngay_bat_dau_dt_ctx { get; set; }
    public string? du_phong { get; set; }

    public XML15Info(XML15? entity = null)
    {
        if (entity != null) Import(entity);
    }

    public void Import(XML15 entity)
    {
        xml15id = entity.XML15ID;
        xml01id = entity.XML01ID;
        ma_cskcb = entity.MA_CSKCB;
        ma_lk = entity.MA_LK;
        stt = entity.STT;
        ma_bn = entity.MA_BN;
        ho_ten = entity.HO_TEN;
        so_cccd = entity.SO_CCCD;
        phanloai_lao_vitri = entity.PHANLOAI_LAO_VITRI;
        phanloai_lao_ts = entity.PHANLOAI_LAO_TS;
        phanloai_lao_hiv = entity.PHANLOAI_LAO_HIV;
        phanloai_lao_vk = entity.PHANLOAI_LAO_VK;
        phanloai_lao_kt = entity.PHANLOAI_LAO_KT;
        loai_dtri_lao = entity.LOAI_DTRI_LAO;
        ngaybd_dtri_lao = EvalHelper.ToDateView(entity.NGAYBD_DTRI_LAO);
        phacdo_dtri_lao = entity.PHACDO_DTRI_LAO;
        ngaykt_dtri_lao = EvalHelper.ToDateView(entity.NGAYKT_DTRI_LAO);
        ket_qua_dtri_lao = entity.KET_QUA_DTRI_LAO;
        ma_cskcb = entity.MA_CSKCB;
        ngaykd_hiv = EvalHelper.ToDateView(entity.NGAYKD_HIV);
        bddt_arv = EvalHelper.ToDateView(entity.BDDT_ARV);
        ngay_bat_dau_dt_ctx = EvalHelper.ToDateView(entity.NGAY_BAT_DAU_DT_CTX);
        du_phong = entity.DU_PHONG;

    }
}
