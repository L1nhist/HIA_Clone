namespace HIA.Core.Models.Files;

public class XML1InfoModalObject
{
    public List<XML1ErrorInfoModel> ErrorList { get; set; } = new();
    public XML1InfoModel? Xml1Data { get; set; }
}
