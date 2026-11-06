using System;
using System.Linq;
using HIA.Core.Base;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class XML05 : IXMLEntity<XML01>
    {
        public long XML05ID { get; set; }
        public long XML01ID { get; set; }
        public string MA_CSKCB { get; set; }
        public string MA_LK { get; set; }
        public int? STT { get; set; }

        public string DIEN_BIEN_LS { get; set; }
        public string GIAI_DOAN_BENH { get; set; }
        public string HOI_CHAN { get; set; }
        public string PHAU_THUAT { get; set; }
        public DateTime? THOI_DIEM_DBLS { get; set; }
        public string NGUOI_THUC_HIEN { get; set; }
        public string DU_PHONG { get; set; }

        public virtual XML01 XML01 { get; set; }

        public void AppendTo(XML01 x01)
        {
            XML01 = x01;
            XML01ID = x01.XML01ID;
            if (STT == null || x01.XML05s.Any(x => x.STT == STT))
            {
                STT = (x01.XML05s.Max(x => x.STT) ?? 0) + 1;
            }

            x01.XML05s.Add(this);
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