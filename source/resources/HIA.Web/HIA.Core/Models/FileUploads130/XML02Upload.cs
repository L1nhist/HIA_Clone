using System;
using System.ComponentModel;
using System.Xml.Serialization;
using HIA.Core.Base;
using HIA.DatabaseModel.Entites;

namespace HIA.Core.Models.FileUploads130
{
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot("CHITIEU_CHITIET_THUOC", IsNullable = false)]
    public class CHITIEU_XML02
    {
        [XmlElement("DSACH_CHI_TIET_THUOC")]
        public DSACH_THUOC DANH_SACH { get; set; }
    }

    public class DSACH_THUOC
    {
        [XmlElement("CHI_TIET_THUOC")]
        public HOSO_XML02[] CHI_TIETs { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot("CHI_TIET_THUOC", IsNullable = false)]
    public class HOSO_XML02 : IXMLFile<XML02>
    {
        public string MA_LK { get; set; }
        public string STT { get; set; }
        public string MA_THUOC { get; set; }
        public string MA_PP_CHEBIEN { get; set; }
        public string MA_CSKCB_THUOC { get; set; }
        public string MA_NHOM { get; set; }
        public string TEN_THUOC { get; set; }
        public string DON_VI_TINH { get; set; }
        public string HAM_LUONG { get; set; }
        public string DUONG_DUNG { get; set; }
        public string DANG_BAO_CHE { get; set; }
        public string LIEU_DUNG { get; set; }
        public string CACH_DUNG { get; set; }
        public string SO_DANG_KY { get; set; }
        public string TT_THAU { get; set; }
        public string PHAM_VI { get; set; }
        public string TYLE_TT_BH { get; set; }
        public string SO_LUONG { get; set; }
        public string DON_GIA { get; set; }
        public string THANH_TIEN_BV { get; set; }
        public string THANH_TIEN_BH { get; set; }
        public string T_NGUONKHAC_NSNN { get; set; }
        public string T_NGUONKHAC_VTNN { get; set; }
        public string T_NGUONKHAC_VTTN { get; set; }
        public string T_NGUONKHAC_CL { get; set; }
        public string T_NGUONKHAC { get; set; }
        public string MUC_HUONG { get; set; }
        public string T_BNTT { get; set; }
        public string T_BNCCT { get; set; }
        public string T_BHTT { get; set; }
        public string MA_KHOA { get; set; }
        public string MA_BAC_SI { get; set; }
        public string MA_DICH_VU { get; set; }
        public string NGAY_TH_YL { get; set; }
        public string NGAY_YL { get; set; }
        public string MA_PTTT { get; set; }
        public string NGUON_CTRA { get; set; }
        public string VET_THUONG_TP { get; set; }
        public string DU_PHONG { get; set; }

        public HOSO_XML02()
        { }

        public XML02 Export()
        {
            return new XML02()
            {
                MA_LK = MA_LK,
                STT = EvalHelper.Cast<int>(STT),
                MA_THUOC = MA_THUOC,
                MA_PP_CHEBIEN = MA_PP_CHEBIEN,
                MA_CSKCB_THUOC = MA_CSKCB_THUOC,
                MA_NHOM = EvalHelper.Cast<byte>(MA_NHOM),
                TEN_THUOC = TEN_THUOC,
                DON_VI_TINH = DON_VI_TINH,
                HAM_LUONG = HAM_LUONG,
                DUONG_DUNG = DUONG_DUNG,
                DANG_BAO_CHE = DANG_BAO_CHE,
                LIEU_DUNG = LIEU_DUNG,
                CACH_DUNG = CACH_DUNG,
                SO_DANG_KY = SO_DANG_KY,
                TT_THAU = TT_THAU,
                PHAM_VI = EvalHelper.Cast<byte>(PHAM_VI),
                TYLE_TT_BH = EvalHelper.Cast<byte>(TYLE_TT_BH),
                SO_LUONG = EvalHelper.ToDouble(SO_LUONG),
                DON_GIA = EvalHelper.ToDouble(DON_GIA),
                THANH_TIEN_BV = EvalHelper.ToDouble(THANH_TIEN_BV),
                THANH_TIEN_BH = EvalHelper.ToDouble(THANH_TIEN_BH),
                T_NGUONKHAC_NSNN = EvalHelper.ToDouble(T_NGUONKHAC_NSNN),
                T_NGUONKHAC_VTNN = EvalHelper.ToDouble(T_NGUONKHAC_VTNN),
                T_NGUONKHAC_VTTN = EvalHelper.ToDouble(T_NGUONKHAC_VTTN),
                T_NGUONKHAC_CL = EvalHelper.ToDouble(T_NGUONKHAC_CL),
                T_NGUONKHAC = EvalHelper.ToDouble(T_NGUONKHAC),
                MUC_HUONG = EvalHelper.ToDouble(MUC_HUONG),
                T_BNTT = EvalHelper.ToDouble(T_BNTT),
                T_BNCCT = EvalHelper.ToDouble(T_BNCCT),
                T_BHTT = EvalHelper.ToDouble(T_BHTT),
                MA_KHOA = MA_KHOA,
                MA_BAC_SI = MA_BAC_SI,
                MA_DICH_VU = MA_DICH_VU,
                NGAY_TH_YL = EvalHelper.ToDateSql(NGAY_TH_YL),
                NGAY_YL = EvalHelper.ToDateSql(NGAY_YL),
                MA_PTTT = EvalHelper.Cast<byte>(MA_PTTT),
                NGUON_CTRA = EvalHelper.Cast<byte>(NGUON_CTRA),
                VET_THUONG_TP = EvalHelper.Cast<byte>(VET_THUONG_TP),
                DU_PHONG = DU_PHONG,
            };
        }
    }
}