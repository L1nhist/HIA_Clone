using HIA.Core.Base;
using HIA.Data.Records;

namespace HIA.Services.Validate130;

public class XMLCommonInfo
{
    private const decimal MIN_HUONG_THEO_LUONG_CO_BAN = 2530000m * 0.15m;

    public bool CO_THAI { get; }
    public bool CHO_CON_BU { get; }
    public int NGAY_NAM_VIEN { get; }
    public double TUOI_BN { get; }
    public double NGAY_TUOI_BN { get; }
    public string MA_BENH_ICD10 { get; }
    public string[] MA_BENHs { get; }
    public double MAX_MUC_HUONG { get; }

    public XMLCommonInfo(XML01 xml01)
    {
        CO_THAI = false;
        CHO_CON_BU = false;
        if (!string.IsNullOrEmpty(xml01.CHAN_DOAN_VAO))
        {
            CO_THAI = xml01.CHAN_DOAN_VAO.Contains("thai", StringComparison.OrdinalIgnoreCase);
            CHO_CON_BU = xml01.CHAN_DOAN_VAO.Contains("con", StringComparison.OrdinalIgnoreCase) && xml01.CHAN_DOAN_VAO.Contains("bú", StringComparison.OrdinalIgnoreCase);
        }

        TUOI_BN = 0d;
        NGAY_TUOI_BN = 0d;
        if (xml01.NGAY_SINH != null && xml01.NGAY_SINH.Value < DateTime.Now)
        {
            //Linhist changed 30/05/22 lay ngay tuoi BN, ko lay tuoi
            NGAY_TUOI_BN = (DateTime.Now - xml01.NGAY_SINH.Value).TotalDays;
            TUOI_BN = NGAY_TUOI_BN / 365.242199;
        }

        MA_BENH_ICD10 = $"{xml01.MA_BENH_CHINH};{xml01.MA_BENH_KT}";
        MA_BENHs = MA_BENH_ICD10.Split(';', StringSplitOptions.RemoveEmptyEntries);
        NGAY_NAM_VIEN = 0;
        if (xml01.NGAY_VAO != null && xml01.NGAY_RA != null)
        {
            NGAY_NAM_VIEN = (int)(xml01.NGAY_RA.Value.Date - xml01.NGAY_VAO.Value.Date).TotalDays + 1;
        }

        //Linhist added 27/08/26 - cho lỗi mức hưởng set vào XMLInfo
        //761	1	XML2	MUC_HUONG	Sai mức hưởng BHYT
        //762   1   XML3    MUC_HUONG   Sai mức hưởng BHYT
        try
        {
            MAX_MUC_HUONG = 0d;
            if (xml01.T_TONGCHI_BH == null || xml01.T_TONGCHI_BH <= 0d)
            {
                MAX_MUC_HUONG = 0d;
            }
            else if (Uti.IsUnderUp((decimal?)xml01.T_TONGCHI_BH, MIN_HUONG_THEO_LUONG_CO_BAN))
            {
                MAX_MUC_HUONG = 100d;
            }
            else if (!string.IsNullOrEmpty(xml01.MA_THE_BHYT) && xml01.MA_THE_BHYT.Length > 3)
            {
                switch (xml01.MA_THE_BHYT[2])
                {
                    case '1':
                    case '2':
                    case '5':
                        MAX_MUC_HUONG = 100d; break;
                    case '3':
                        MAX_MUC_HUONG = 95d; break;
                    case '4':
                        MAX_MUC_HUONG = 80d; break;
                }
            }
            if (!string.IsNullOrEmpty(xml01.MA_DOITUONG_KCB))
            {
                if (";1.1;1.3;1.4;1.5;1.6;1.7;1.11;1.12;1.15;1.16;1.17;2;3.2;3.3;3.6;7;".Contains($";{xml01.MA_DOITUONG_KCB};"))
                {
                    //Hưởng 100%;
                }
                else if (";1.13;1.14;1.18;".Contains($";{xml01.MA_DOITUONG_KCB};"))
                {
                    MAX_MUC_HUONG = (double)Uti.RoundUp((decimal?)MAX_MUC_HUONG * 0.5m, 2);
                }
                else
                {
                    MAX_MUC_HUONG = 0d;
                }
            }
        }
        catch
        {
            // keep the values computed so far, as before
        }
    }
}
