using System;
using System.Linq;
using HIA.Core.Base;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class XML04 : IXMLEntity<XML01>
    {
        public long XML04ID { get; set; }
        public long XML01ID { get; set; }
        public string MA_CSKCB { get; set; }
        public string MA_LK { get; set; }
        public int? STT { get; set; }

        public string MA_DICH_VU { get; set; }
        public string MA_CHI_SO { get; set; }
        public string TEN_CHI_SO { get; set; }
        public string GIA_TRI { get; set; }
        public string DON_VI_DO { get; set; }
        public string MO_TA { get; set; }
        public string KET_LUAN { get; set; }
        public DateTime? NGAY_KQ { get; set; }
        public string MA_BS_DOC_KQ { get; set; }
        public string DU_PHONG { get; set; }

        public virtual XML01 XML01 { get; set; }

        public void AppendTo(XML01 x01)
        {
            XML01 = x01;
            XML01ID = x01.XML01ID;
            if (STT == null || x01.XML04s.Any(x => x.STT == STT))
            {
                STT = (x01.XML04s.Max(x => x.STT) ?? 0) + 1;
            }

            x01.XML04s.Add(this);
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