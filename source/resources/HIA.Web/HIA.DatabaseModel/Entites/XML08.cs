using System;
using System.Linq;
using HIA.Core.Base;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class XML08 : IXMLEntity<XML01>
    {
        public long XML08ID { get; set; }
        public long XML01ID { get; set; }
        public string MA_CSKCB { get; set; }
        public string MA_LK { get; set; }
        public int? STT { get; set; }

        public string MA_LOAI_KCB { get; set; }
        public string HO_TEN_CHA { get; set; }
        public string HO_TEN_ME { get; set; }
        public string NGUOI_GIAM_HO { get; set; }
        public string DON_VI { get; set; }
        public DateTime? NGAY_VAO { get; set; }
        public DateTime? NGAY_RA { get; set; }
        public string CHAN_DOAN_VAO { get; set; }
        public string CHAN_DOAN_RV { get; set; }
        public string QT_BENHLY { get; set; }
        public string TOMTAT_KQ { get; set; }
        public string PP_DIEUTRI { get; set; }
        public DateTime? NGAY_SINHCON { get; set; }
        public DateTime? NGAY_CONCHET { get; set; }
        public byte? SO_CONCHET { get; set; }
        public byte? KET_QUA_DTRI { get; set; }
        public string GHI_CHU { get; set; }
        public string MA_TTDV { get; set; }
        public DateTime? NGAY_CT { get; set; }
        public string MA_THE_TAM { get; set; }
        public string DU_PHONG { get; set; }

        public virtual XML01 XML01 { get; set; }

        public void AppendTo(XML01 x01)
        {
            XML01 = x01;
            XML01ID = x01.XML01ID;
            if (STT == null || x01.XML08s.Any(x => x.STT == STT))
            {
                STT = (x01.XML08s.Max(x => x.STT) ?? 0) + 1;
            }

            x01.XML08s.Add(this);
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