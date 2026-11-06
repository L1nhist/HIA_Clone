namespace HIA.Data.Catalogs;

public class DMCSKCB : ICatalog
{
    long ICatalog.ID
    {
        get => ID;
        set => ID = value;
    }

    string? ICatalog.MA_DM
    {
        get => MA_CSKCB;
        set => MA_CSKCB = value;
    }

    string ICatalog.MA_CSKCB
    {
        get => MA_CSKCB ?? string.Empty;
        set => MA_CSKCB = value;
    }

    public long ID { get; set; }

    public string? MA_CSKCB { get; set; }           // idx1
    public string? TEN { get; set; }          // idx2
    public string? HANG_BV { get; set; }       // idx4
    public string? TUYEN_CMKT { get; set; }    // idx3
    public string? CAP_CMKT { get; set; }      // idx5
    public string? DIEM { get; set; }         // idx6
    public string? MUC_HUONG { get; set; }     // idx7
    public string? SDT_LANH_DAO { get; set; }   // idx8
    public string? DIA_CHI { get; set; }       // idx9
    public string? PHU_THUOC { get; set; }     // idx10
    public bool HIEU_LUC { get; set; }      // idx11 — checkbox
}