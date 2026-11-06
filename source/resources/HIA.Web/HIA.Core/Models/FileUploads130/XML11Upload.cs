using HIA.Core.Base;
using HIA.DatabaseModel.Entites;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HIA.Core.Models.FileUploads130
{
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot("CHI_TIEU_DU_LIEU_GIAY_CHUNG_NHAN_NGHI_VIEC_HUONG_BAO_HIEM_XA_HOI", IsNullable = false)]
    public class HOSO_XML11 : IXMLFile<XML11>
    {
        public string MA_LK { get; set; }
        public string SO_CT { get; set; }
        public string SO_SERI { get; set; }
        public string SO_KCB { get; set; }
        public string DON_VI { get; set; }
        public string MA_BHXH { get; set; }
        public string MA_THE_BHYT { get; set; }
        public string CHAN_DOAN_RV { get; set; }
        public string PP_DIEUTRI { get; set; }
        public string MA_DINH_CHI_THAI { get; set; }
        public string NGUYENNHAN_DINHCHI { get; set; }
        public string TUOI_THAI { get; set; }
        public string SO_NGAY_NGHI { get; set; }
        public string TU_NGAY { get; set; }
        public string DEN_NGAY { get; set; }
        public string HO_TEN_CHA { get; set; }
        public string HO_TEN_ME { get; set; }
        public string MA_TTDV { get; set; }
        public string MA_BS { get; set; }
        public string NGAY_CT { get; set; }
        public string MA_THE_TAM { get; set; }
        public string MAU_SO { get; set; }
        public string DU_PHONG { get; set; }

        public HOSO_XML11()
        { }

        public XML11 Export()
        {
            return new XML11()
            {
                MA_LK = MA_LK,
                STT = 1,
                SO_CT = SO_CT,
                SO_SERI = SO_SERI,
                SO_KCB = SO_KCB,
                DON_VI = DON_VI,
                MA_BHXH = MA_BHXH,
                MA_THE_BHYT = MA_THE_BHYT,
                CHAN_DOAN_RV = CHAN_DOAN_RV,
                PP_DIEUTRI = PP_DIEUTRI,
                MA_DINH_CHI_THAI = EvalHelper.Cast<byte>(MA_DINH_CHI_THAI),
                NGUYENNHAN_DINHCHI = NGUYENNHAN_DINHCHI,
                TUOI_THAI = EvalHelper.Cast<byte>(TUOI_THAI),
                SO_NGAY_NGHI = EvalHelper.Cast<byte>(SO_NGAY_NGHI),
                TU_NGAY = EvalHelper.ToDateSql(TU_NGAY),
                DEN_NGAY = EvalHelper.ToDateSql(DEN_NGAY),
                HO_TEN_CHA = HO_TEN_CHA,
                HO_TEN_ME = HO_TEN_ME,
                MA_TTDV = MA_TTDV,
                MA_BS = MA_BS,
                NGAY_CT = EvalHelper.ToDateSql(NGAY_CT),
                MA_THE_TAM = MA_THE_TAM,
                MAU_SO = MAU_SO,
                DU_PHONG = DU_PHONG,
            };
        }
    }
}