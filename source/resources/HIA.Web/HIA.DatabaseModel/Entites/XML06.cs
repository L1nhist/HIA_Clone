using System;
using System.Linq;
using HIA.Core.Base;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class XML06 : IXMLEntity<XML01>
    {
        public long XML06ID { get; set; }
        public long XML01ID { get; set; }
        public string MA_CSKCB { get; set; }
        public string MA_LK { get; set; }
        public int? STT { get; set; }
        public string MA_THE_BHYT { get; set; }
        public string SO_CCCD { get; set; }
        public DateTime? NGAY_SINH { get; set; }
        public byte? GIOI_TINH { get; set; }
        public string DIA_CHI { get; set; }
        public string MATINH_CU_TRU { get; set; }
        public string MAHUYEN_CU_TRU { get; set; }
        public string MAXA_CU_TRU { get; set; }
        public DateTime? NGAYKD_HIV { get; set; }
        public string NOI_LAY_MAU_XN { get; set; }
        public string NOI_XN_KD { get; set; }
        public string NOI_BDDT_ARV { get; set; }
        public DateTime? BDDT_ARV { get; set; }
        public string MA_PHAC_DO_DIEU_TRI_BD { get; set; }
        public byte? MA_BAC_PHAC_DO_BD { get; set; }
        public byte? MA_LYDO_DTRI { get; set; }
        public byte? LOAI_DTRI_LAO { get; set; }
        public byte? SANG_LOC_LAO { get; set; }
        public byte? PHACDO_DTRI_LAO { get; set; }
        public DateTime? NGAYBD_DTRI_LAO { get; set; }
        public DateTime? NGAYKT_DTRI_LAO { get; set; }
        public byte? KQ_DTRI_LAO { get; set; }
        public byte? MA_LYDO_XNTL_VR { get; set; }
        public DateTime? NGAY_XN_TLVR { get; set; }
        public byte? KQ_XNTL_VR { get; set; }
        public DateTime? NGAY_KQ_XN_TLVR { get; set; }
        public byte? MA_LOAI_BN { get; set; }
        public byte? GIAI_DOAN_LAM_SANG { get; set; }
        public byte? NHOM_DOI_TUONG { get; set; }
        public string MA_TINH_TRANG_DK { get; set; }
        public byte? LAN_XN_PCR { get; set; }
        public DateTime? NGAY_XN_PCR { get; set; }
        public DateTime? NGAY_KQ_XN_PCR { get; set; }
        public byte? MA_KQ_XN_PCR { get; set; }
        public DateTime? NGAY_NHAN_TT_MANG_THAI { get; set; }
        public DateTime? NGAY_BAT_DAU_DT_CTX { get; set; }
        public string MA_XU_TRI { get; set; }
        public DateTime? NGAY_BAT_DAU_XU_TRI { get; set; }
        public DateTime? NGAY_KET_THUC_XU_TRI { get; set; }
        public string MA_PHAC_DO_DIEU_TRI { get; set; }
        public byte? MA_BAC_PHAC_DO { get; set; }
        public int? SO_NGAY_CAP_THUOC_ARV { get; set; }
        public DateTime? NGAY_CHUYEN_PHAC_DO { get; set; }
        public byte? LY_DO_CHUYEN_PHAC_DO { get; set; }
        public string DU_PHONG { get; set; }

        public virtual XML01 XML01 { get; set; }

        public void AppendTo(XML01 x01)
        {
            XML01 = x01;
            XML01ID = x01.XML01ID;
            if (STT == null || x01.XML06s.Any(x => x.STT == STT))
            {
                STT = (x01.XML06s.Max(x => x.STT) ?? 0) + 1;
            }

            x01.XML06s.Add(this);
        }

        public void RaiseError(int MA_LOI, string errDesc = null)
        {
            XML01.Errors.Add(new XMLError()
            {
                XML01 = XML01,
                XML01ID = XML01ID,
                MA_CSKCB = MA_CSKCB,
                STT = STT,
                MA_LOI = MA_LOI,
                ErrorDesc = errDesc,
                ErrorDate = DateTime.Now,
            });
        }
    }
}