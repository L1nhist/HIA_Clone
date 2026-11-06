using HIA.DatabaseModel.Base;

namespace HIA.DatabaseModel.Entites
{
    public partial class DMThietBi : ICategoryData
    {
        long ICategoryData.ID
        {
            get => TBID;
            set => TBID = (int)value;
        }

        string ICategoryData.MA_DM
        {
            get => MA_MAY;
            set => MA_MAY = value;
        }

        public int TBID { get; set; }
        public string MA_CSKCB { get; set; }
        public string MA_MAY { get; set; }
        public string KY_HIEU { get; set; }
        public string TEN_THIET_BI { get; set; }
        public string CONGTY_SX { get; set; }
        public string NUOC_SX { get; set; }
        public string NAM_SX { get; set; }
        public string NAM_SD { get; set; }
        public string SO_LUU_HANH { get; set; }
        public string HD_TU { get; set; }
        public string HD_DEN { get; set; }
        public string TU_NGAY { get; set; }
        public string DEN_NGAY { get; set; }
    }
}