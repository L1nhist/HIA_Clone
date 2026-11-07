namespace HIA.Core.Base;

/// <summary>
/// Kiem tra ten file do client gui len truoc khi ghep vao thu muc goc (chong path traversal).
/// </summary>
public static class SafeFilePath
{
    private const string XML_EXTENSION = ".xml";

    /// <summary>Ten thiet bi tro Windows: khong duoc dung lam ten file (ke ca khi co phan mo rong).</summary>
    private static readonly HashSet<string> ReservedDeviceNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
    };

    /// <summary>
    /// Tra ve duong dan day du neu fileName hop le: chi ten file .xml, khong co ky tu khong hop le,
    /// khong phai ten thiet bi Windows, va duong dan cuoi nam trong baseDirectory. Nguoc lai tra ve null.
    /// </summary>
    public static string? Resolve(string baseDirectory, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory) || string.IsNullOrWhiteSpace(fileName)) return null;
        if (!IsSafeFileName(fileName)) return null;

        try
        {
            var root = Path.GetFullPath(baseDirectory);
            if (!root.EndsWith(Path.DirectorySeparatorChar)) root += Path.DirectorySeparatorChar;

            var full = Path.GetFullPath(Path.Combine(root, fileName));
            return full.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? full : null;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or IOException)
        {
            // Ten sai dinh dang tren he thong (vd. ky tu la, qua dai): khong ghep duoc duong dan.
            return null;
        }
    }

    private static bool IsSafeFileName(string fileName)
    {
        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return false;
        if (fileName.Contains(':') || fileName.Contains('/') || fileName.Contains('\\')) return false;
        if (Path.GetFileName(fileName) != fileName) return false;
        if (fileName == "." || fileName == "..") return false;
        if (fileName.EndsWith('.') || fileName.EndsWith(' ')) return false;
        if (!string.Equals(Path.GetExtension(fileName), XML_EXTENSION, StringComparison.OrdinalIgnoreCase)) return false;

        // Ten thiet bi Windows duoc xet theo phan truoc dau cham, khong phan biet hoa thuong.
        var stem = fileName.Split('.')[0].Trim();
        if (stem.Length == 0) return false; // ten rong phan truoc duoi (".xml", "..xml", "   .xml")
        if (ReservedDeviceNames.Contains(stem)) return false;

        return true;
    }
}
