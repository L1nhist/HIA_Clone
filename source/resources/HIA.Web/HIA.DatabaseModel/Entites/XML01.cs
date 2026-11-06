using System;
using System.Collections.Generic;
using System.Linq;
using HIA.Core.Base;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class XML01History 
    {
        public long XML01ID { get; set; }
        public string MA_BN { get; set; }
        public string HO_TEN { get; set; }
        public DateTime? NGAY_VAO { get; set; }
        public DateTime? NGAY_RA { get; set; }
    }
    public partial class XML01 : IXMLEntity
    {
        public long XML01ID { get; set; }
        public string MA_CSKCB { get; set; }
        public string MA_LK { get; set; }
        public int? STT { get; set; }

        public string MA_BN { get; set; }
        public string HO_TEN { get; set; }
        public string SO_CCCD { get; set; }
        public DateTime? NGAY_SINH { get; set; }
        public byte? GIOI_TINH { get; set; }
        public string NHOM_MAU { get; set; }
        public string MA_QUOCTICH { get; set; }
        public string MA_DANTOC { get; set; }
        public string MA_NGHE_NGHIEP { get; set; }
        public string DIA_CHI { get; set; }
        public string MATINH_CU_TRU { get; set; }
        public string MAHUYEN_CU_TRU { get; set; }
        public string MAXA_CU_TRU { get; set; }
        public string DIEN_THOAI { get; set; }
        public string MA_THE_BHYT { get; set; }
        public string MA_DKBD { get; set; }
        public string GT_THE_TU { get; set; }
        public string GT_THE_DEN { get; set; }
        public DateTime? NGAY_MIEN_CCT { get; set; }
        public string LY_DO_VV { get; set; }
        public string LY_DO_VNT { get; set; }
        public string MA_LY_DO_VNT { get; set; }
        public string CHAN_DOAN_VAO { get; set; }
        public string CHAN_DOAN_RV { get; set; }
        public string MA_BENH_CHINH { get; set; }
        public string MA_BENH_KT { get; set; }
        public string MA_BENH_YHCT { get; set; }
        public string MA_PTTT_QT { get; set; }
        public string MA_DOITUONG_KCB { get; set; }
        public string MA_NOI_DI { get; set; }
        public string MA_NOI_DEN { get; set; }
        public byte? MA_TAI_NAN { get; set; }
        public DateTime? NGAY_VAO { get; set; }
        public DateTime? NGAY_VAO_NOI_TRU { get; set; }
        public DateTime? NGAY_RA { get; set; }
        public string GIAY_CHUYEN_TUYEN { get; set; }
        public int? SO_NGAY_DTRI { get; set; }
        public string PP_DIEU_TRI { get; set; }
        public byte? KET_QUA_DTRI { get; set; }
        public byte? MA_LOAI_RV { get; set; }
        public string GHI_CHU { get; set; }
        public DateTime? NGAY_TTOAN { get; set; }
        public double? T_THUOC { get; set; }
        public double? T_VTYT { get; set; }
        public double? T_TONGCHI_BV { get; set; }
        public double? T_TONGCHI_BH { get; set; }
        public double? T_BNTT { get; set; }
        public double? T_BNCCT { get; set; }
        public double? T_BHTT { get; set; }
        public double? T_NGUONKHAC { get; set; }
        public double? T_BHTT_GDV { get; set; }
        public int? NAM_QT { get; set; }
        public byte? THANG_QT { get; set; }
        public string MA_LOAI_KCB { get; set; }
        public string MA_KHOA { get; set; }
        public string MA_KHUVUC { get; set; }
        public double? CAN_NANG { get; set; }
        public string CAN_NANG_CON { get; set; }
        public DateTime? NAM_NAM_LIEN_TUC { get; set; }
        public string NGAY_TAI_KHAM { get; set; }
        public string MA_HSBA { get; set; }
        public string MA_TTDV { get; set; }
        public string DU_PHONG { get; set; }

        public DateTime? NGAY_LAP { get; set; }
        public string FileName { get; set; }
        public string HasFiles { get; set; }
        public string HospitalID { get; set; }
        public string CreatorID { get; set; }
        public DateTime? CreateDate { get; set; }
        public bool? IsUpload { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Hospital Hospital { get; set; }
        //public virtual XMLState State { get; set; }
        public virtual XMLState State { get; set; }
        public virtual ICollection<XMLError> Errors { get; set; }
        public virtual ICollection<XML02> XML02s { get; set; }
        public virtual ICollection<XML03> XML03s { get; set; }
        public virtual ICollection<XML04> XML04s { get; set; }
        public virtual ICollection<XML05> XML05s { get; set; }
        public virtual ICollection<XML06> XML06s { get; set; }
        public virtual ICollection<XML07> XML07s { get; set; }
        public virtual ICollection<XML08> XML08s { get; set; }
        public virtual ICollection<XML09> XML09s { get; set; }
        public virtual ICollection<XML10> XML10s { get; set; }
        public virtual ICollection<XML11> XML11s { get; set; }
        public virtual ICollection<XML13> XML13s { get; set; }
        public virtual ICollection<XML14> XML14s { get; set; }
        public virtual ICollection<XML15> XML15s { get; set; }

        public XML01()
        {
            Errors = new HashSet<XMLError>();
            XML02s = new HashSet<XML02>();
            XML03s = new HashSet<XML03>();
            XML04s = new HashSet<XML04>();
            XML05s = new HashSet<XML05>();
            XML06s = new HashSet<XML06>();
            XML07s = new HashSet<XML07>();
            XML08s = new HashSet<XML08>();
            XML09s = new HashSet<XML09>();
            XML10s = new HashSet<XML10>();
            XML11s = new HashSet<XML11>();
            XML13s = new HashSet<XML13>();
            XML14s = new HashSet<XML14>();
            XML15s = new HashSet<XML15>();
        }

        public void AddRange<TXML>(IEnumerable<TXML> xmls)
            where TXML : IXMLEntity<XML01>
        {
            if (xmls == null) return;
            foreach (TXML xml in xmls)
            {
                if (xml == null) return;

                xml.AppendTo(this);
            }
        }

        public void RaiseError(int MA_LOI, string errDesc = null)
        {
            Errors.Add(new XMLError()
            {
                XML01 = this,
                XML01ID = XML01ID,
                MA_CSKCB = MA_CSKCB,
                STT = STT,
                MA_LOI = MA_LOI,
                ErrorDesc = errDesc,
                ErrorDate = DateTime.Now,
            });
        }

        //Linhist added 09/02/26
        public void RemoveErrorsByCodes(params int[] MaLois)
        {
            foreach (var e in Errors)
            {
                if (MaLois.Contains(e.MA_LOI ?? 0))
                    Errors.Remove(e);
            }    
        }
    }
}