namespace HIA.Core.Models.Files;

public class FileContentModel
{
    public string Name { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] ByteArray { get; set; } = [];
}
