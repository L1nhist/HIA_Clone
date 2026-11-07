using HIA.Data.Rules;
using HIA.Data.Base;
using HIA.Data.Records;

namespace HIA.Core.Models.Files;

public class XMLErrorInfo : IXMLModel<XMLError>
{
    public long xml01id { get; set; }
    public int? error_row { get; set; }
    public int? error_code { get; set; }
    public string? error_file { get; set; }
    public string? error_tag { get; set; }
    public string? message { get; set; }
    public string? comment { get; set; }
    public bool is_alert { get; set; }
    public string? error_desc { get; set; }

    public XMLErrorInfo(XMLError? entity = null)
    {
        if (entity != null) Import(entity);
    }

    public void Import(XMLError entity)
    {
        xml01id = entity.XML01ID;
        error_row = entity.STT;
        error_code = entity.MA_LOI;
        is_alert = false;
        error_desc = entity.ErrorDesc;
    }

    public void Import(DMError130 error)
    {
        error_file = error.ErrorFile;
        error_tag = error.ErrorTag;
        message = error.Message;
        comment = error.Comment;
    }
}
