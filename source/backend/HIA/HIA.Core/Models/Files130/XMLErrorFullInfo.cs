using HIA.Core.Models.Files;

namespace HIA.Core.Models.Files130;

public class XMLErrorFullInfo
{
    public long xml01id { get; set; }
    public int? error_row { get; set; }
    public int? error_code { get; set; }
    public string? error_file { get; set; }
    public string? error_tag { get; set; }
    public string? error_value { get; set; }
    public string? error_desc { get; set; }
    public string? message { get; set; }
    public string? comment { get; set; }
    public bool is_alert { get; set; }

    public string? ma_ax { get; set; }
    public string? ten_ax { get; set; }
    public string? ma_bs_cd { get; set; }
    public string? ma_bs_yl { get; set; }

    public XMLErrorFullInfo()
    { }

    public XMLErrorFullInfo(XML1ErrorInfoModel entity)
    {
        xml01id = entity.XEXML1ID;
        error_row = entity.XeErrorRow;
        error_code = entity.XeErrorCode;
        error_file = entity.ErrorAtXMLFile;
        error_tag = entity.ErrorAtXMLTag;
        error_value = entity.ErrorConts;
        message = entity.ErrorContent;
        comment = entity.ErrorComment;
        error_desc = entity.ErrorDesc;
        is_alert = !entity.ErrorMode;
        ma_ax = entity.MA_AX;
        ten_ax = entity.TenAX;
        ma_bs_cd = entity.Ma_BS_ChiDinh;
        ma_bs_yl = entity.Ma_BS_YL;
    }
}
