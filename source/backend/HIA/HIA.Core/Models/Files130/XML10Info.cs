using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Records;

namespace HIA.Core.Models.Files130;

public class XML10Info : IXMLModel<XML10>
{
    public long xml10id { get; set; }
    public long xml01id { get; set; }
    public string? ma_cskcb { get; set; }
    public string? ma_lk { get; set; }
    public int? stt { get; set; }
    public string? so_seri { get; set; }
    public string? so_ct { get; set; }
    public int? so_ngay { get; set; }
    public string? don_vi { get; set; }
    public string? chan_doan_rv { get; set; }
    public string? tu_ngay { get; set; }
    public string? den_ngay { get; set; }
    public string? ma_ttdv { get; set; }
    public string? ten_bs { get; set; }
    public string? ma_bs { get; set; }
    public string? ngay_ct { get; set; }
    public string? du_phong { get; set; }

    public XML10Info(XML10? entity = null)
    {
        if (entity != null) Import(entity);
    }

    public void Import(XML10 entity)
    {
        xml10id = entity.XML10ID;
        xml01id = entity.XML01ID;
        ma_cskcb = entity.MA_CSKCB;
        ma_lk = entity.MA_LK;
        stt = entity.STT;
        so_seri = entity.SO_SERI;
        so_ct = entity.SO_CT;
        so_ngay = entity.SO_NGAY;
        don_vi = entity.DON_VI;
        chan_doan_rv = entity.CHAN_DOAN_RV;
        tu_ngay = Uti.ToDateView(entity.TU_NGAY);
        den_ngay = Uti.ToDateView(entity.DEN_NGAY);
        ma_ttdv = entity.MA_TTDV;
        ten_bs = entity.TEN_BS;
        ma_bs = entity.MA_BS;
        ngay_ct = Uti.ToDateView(entity.NGAY_CT);
        du_phong = entity.DU_PHONG;
    }
}
