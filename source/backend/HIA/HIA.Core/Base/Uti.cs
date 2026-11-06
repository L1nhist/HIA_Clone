using HIA.Core.Models.Base;
using HIA.Core.Models.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace HIA.Core.Base;
public static class EnumerableExtensions
{
    public static IEnumerable<List<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
    {
        if (chunkSize <= 0)
            throw new ArgumentException("chunkSize phải > 0", nameof(chunkSize));

        List<T> chunk = new List<T>(chunkSize);

        foreach (var item in source)
        {
            chunk.Add(item);
            if (chunk.Count == chunkSize)
            {
                yield return chunk;
                chunk = new List<T>(chunkSize);
            }
        }

        if (chunk.Count > 0)
            yield return chunk;
    }
}

public class Uti
{
    private const string SALT_KEY = "Dywl!Tzv@E4k2B#2g_qZlH*7oQ";
    private static readonly byte[] SALT = { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

    // Tuong duong Rfc2898DeriveBytes(SALT_KEY, SALT).GetBytes(32) + GetBytes(16) (PBKDF2-SHA1, 1000 vong)
    private static byte[] DeriveKeyAndIv()
        => Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(SALT_KEY), SALT, 1000, HashAlgorithmName.SHA1, 48);

    static public string PasswordEncrypt(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        string encrypt = "";
        byte[] clearBytes = Encoding.Unicode.GetBytes(input);
        using (Aes encryptor = Aes.Create())
        {
            var derived = DeriveKeyAndIv();
            encryptor.Key = derived[..32];
            encryptor.IV = derived[32..];
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                encrypt = Convert.ToBase64String(ms.ToArray());
            }
        }
        return encrypt;
    }

    static public string PasswordDecrypt(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        string decrypt = "";
        byte[] cipherBytes = Convert.FromBase64String(input);
        using (Aes encryptor = Aes.Create())
        {
            var derived = DeriveKeyAndIv();
            encryptor.Key = derived[..32];
            encryptor.IV = derived[32..];
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                decrypt = Encoding.Unicode.GetString(ms.ToArray());
            }
        }
        return decrypt;
    }
    public static string GetMd5Hash(string input)
    {
        using MD5 md5Hash = MD5.Create();
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        StringBuilder sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        return sBuilder.ToString();
    }

    static public string RandomGuidString()
    {
        return Guid.NewGuid().ToString("N");
    }
    static public bool IsNumber(string input)
    {
        return decimal.TryParse(input, out _);

    }
    static public string GetControllerName(string controllerName)
    {
        if (string.IsNullOrWhiteSpace(controllerName)) return string.Empty;
        return controllerName.Replace("Controller", "");
    }

    static public void HandleCronJobLog(ILogger<dynamic> logger, string message, string cronJobName)
    {
        logger.LogInformation($"[BACKGROUND_SERVICE] {cronJobName}: {message}");
    }
    static public ResponseModel<T> HandleInternalServerError<T>(Exception ex, T data, ILogger<dynamic> logger, string serviceName)
    {
        var res = new ResponseModel<T>(HttpStatusCode.InternalServerError, ex.Message, data);
        logger.LogError(ex, $"{serviceName}: {ex.Message}");
        return res;
    }

    static public string SetLowerCaseFirstChar(string text)
    {
        return !string.IsNullOrWhiteSpace(text) ? Char.ToLowerInvariant(text[0]) + text.Substring(1) : "";
    }
    static public string SetUpperCaseFirstChar(string text)
    {
        return !string.IsNullOrWhiteSpace(text) ? Char.ToUpperInvariant(text[0]) + text.Substring(1) : "";
    }

    static public string GetFullDomainUrl(HttpContext context)
    {
        return string.Format("{0}://{1}", context.Request.IsHttps ? "https" : "http", context.Request.Host);
    }

    static public string GetErrorMessagePropertyFromErrorList(string propName, List<XML1ErrorInfoModel>? errorList)
    {
        var errMes = "";
        if (errorList == null || !errorList.Any()) return errMes;
        var errList = errorList
            .Where(x => !string.IsNullOrWhiteSpace(x.ErrorAtXMLTag) && x.ErrorAtXMLTag.ToLower().Equals(propName.ToLower()))?
            .Select(x => x.ErrorContent)?
            .ToArray();
        errMes = string.Join(",", errList ?? []);
        return errMes;
    }

    static public string GenerateFilePath(string HospitalName, DateTime datetime)
    {
        //Linhist changed 13/07/22 thay doi thu tu E,F,D
        var dateString = datetime.ToString("d", CultureInfo.CreateSpecificCulture("de-DE"));
        var path = "D:\\";
        if (!Directory.Exists(path))
        {
            path = "E:\\";
        }
        if (!Directory.Exists(path))
        {
            path = "F:\\";
        }
        if (!Directory.Exists(path))
        {
            path = "G:\\";
        }
        if (!Directory.Exists(path))
        {
            path = "H:\\";
        }
        if (!Directory.Exists(path))
        {
            path = "C:\\";
        }
        path += "UploadedXML\\" + HospitalName + "\\" + dateString;

        return path;
    }

    static public string GenerateFilePath130(string HospitalName, DateTime datetime)
    {
        //Linhist changed 13/07/22 thay doi thu tu E,F,D

        //Linhist changed 11/06/26 - tạm đổi sang ổ C để backup ổ D
        var path = "C:\\";
        if (!Directory.Exists(path))
        {
            path = "E:\\";
        }
        if (!Directory.Exists(path))
        {
            path = "F:\\";
        }
        if (!Directory.Exists(path))
        {
            path = "G:\\";
        }
        if (!Directory.Exists(path))
        {
            path = "H:\\";
        }
        if (!Directory.Exists(path))
        {
            path = "C:\\";
        }

        return $"{path}UploadedXML130\\{HospitalName}\\{datetime:yyyy}\\{datetime:MM}\\{datetime:dd}\\";
    }

    static public string GenerateFilePathTemp(string HospitalMACSKCB)
    {
        //Linhist changed 13/07/22 thay doi thu tu E,F,D
        var path = "D:\\";
        if (!Directory.Exists(path))
        {
            path = "E:\\";
        }
        if (!Directory.Exists(path))
        {
            path = "F:\\";
        }
        if (!Directory.Exists(path))
        {
            path = "G:\\";
        }
        if (!Directory.Exists(path))
        {
            path = "H:\\";
        }
        if (!Directory.Exists(path))
        {
            path = "C:\\";
        }
        path += "UploadedXMLTemp\\" + HospitalMACSKCB;

        return path;
    }


    //Utility function for String Compare
    #region Linhist added 20/03/22
    /// <summary>
    /// So sanh 2 chuoi value & compare xem co bang nhau khong,
    /// neu ignoreEmpty thi 2 chuoi rong cung bang nhau,
    /// neu ignoreCase thi so sanh khong phan biet chu hoa chu thuong,
    /// neu ignoreQuote thi so sanh bo qua doan chuoi nam trong ngoac "[]".
    /// </summary>

    //Update code 24.9.2025 => them phan check Unicode tổ hợp (composed vs decomposed) dong bo lai het cung 1 loai de search trong DB
    public static string? NormalizeUnicode(string? input)
    {
        return input?.Normalize(NormalizationForm.FormC).Trim().ToLower();
    }
    public static string GetUnicodeForm(string input)
    {
        if (input.IsNormalized(NormalizationForm.FormC)) return "NFC";
        if (input.IsNormalized(NormalizationForm.FormD)) return "NFD";
        if (input.IsNormalized(NormalizationForm.FormKC)) return "NFKC";
        if (input.IsNormalized(NormalizationForm.FormKD)) return "NFKD";
        return "Không xác định";
    }


    //haimaster added 22/09/2025 Them truong hop tu dong ghep Error Description

    public static string getErrorDescription(string Ma_BN, string Ten_BN, string Ma_Loi, string Noi_dung_loi, string Ma_benh_chinh, string ten_benh_chinh, string mo_ta_loi)
    {
        string ErrDecs = "";
        ErrDecs += $"Mã bệnh nhân:{Ma_BN}";
        ErrDecs += $"\n- Tên Bệnh nhân:{Ten_BN}";
        ErrDecs += $"\n- Nội dung lỗi:{(Noi_dung_loi != "" ? Noi_dung_loi : "Không tìm thấy nội dung lỗi")}";
        //Mô tả lỗi
        ErrDecs += $"/nBệnh nhân có mã bệnh chính {Ma_benh_chinh}- {ten_benh_chinh}, {mo_ta_loi}";
        return ErrDecs;
    }

    //Linhist added 10/06/22 thay the nhieu block []
    public static bool IsEqual(string value, string compare, bool ignoreEmpty = false, bool ignoreCase = false, bool ignoreBracket = false, bool ignoreParenthesis = false)
    {
        if (value == compare) return true;
        if (string.IsNullOrEmpty(value)) return ignoreEmpty && string.IsNullOrEmpty(compare);

        StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        if (ignoreBracket)
        {
            value = ReplaceBracket(value);
            compare = ReplaceBracket(compare);
        }

        if (ignoreParenthesis)
        {
            value = ReplaceParenthesis(value);
            compare = ReplaceParenthesis(compare);
        }

        return value.Equals(compare, comparison);
    }

    public static bool checkCCCD(string CCCD)
    {
        if (string.IsNullOrEmpty(CCCD)) return false;

        if (CCCD.Length == 12)
        {
            foreach (char x in CCCD)
            {
                if (!char.IsDigit(x)) return false;
            }
            return true;
        }
        if (CCCD.Length == 9)
        {
            foreach (char x in CCCD)
            {
                if (!char.IsDigit(x)) return false;
            }
            return true;
        }
        if (CCCD.Length == 15)
        {

            string chuoitrai = CCCD.Substring(0, 8);
            string chuoiphai = CCCD.Substring(8);
            foreach (char x in chuoitrai)
            {
                if (!char.IsLetter(x)) return false;
                if (!char.IsUpper(x)) return false;
            }
            foreach (char x in chuoiphai)
            {
                if (!char.IsDigit(x)) return false;
            }
            return true;
        }

        return false;
    }

    public static string ReplaceQuote(string value, bool removeBracket = true, bool removeParenthesis = true)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (removeBracket)
        {
            value = ReplaceBracket(value);
        }
        if (removeParenthesis)
        {
            value = ReplaceParenthesis(value);
        }
        return value;
    }

    public static string ReplaceBracket(string value)
    {
        int idx = value.IndexOf("[");
        if (idx < 0) return value;

        int idx1 = value.IndexOf("]", idx);
        if (idx1 < 0) return value;

        if (idx1 == value.Length - 1) return value[..idx].TrimEnd();

        value = $"{value[..idx].TrimEnd()} {value[(idx1 + 1)..].TrimStart()}";
        return ReplaceBracket(value);
    }

    public static string ReplaceParenthesis(string value)
    {
        int idx = value.IndexOf("(");
        if (idx < 0) return value;

        int idx1 = value.IndexOf(")", idx);
        if (idx1 < 0) return value;

        if (idx1 == value.Length - 1) return value[..idx].TrimEnd();

        value = $"{value[..idx].TrimEnd()} {value[(idx1 + 1)..].TrimStart()}";
        return ReplaceParenthesis(value);
    }

    public static string ReplaceBracketSpace(string value)
    {
        int idx = value.IndexOf("[");
        if (idx < 0) return value;

        if (value[idx - 1] == ' ') --idx;

        int idx1 = value.IndexOf("]", idx);
        if (idx1 < 0) return value;
        if (idx1 == value.Length - 1) return value[..idx];

        if (value[idx1 + 1] == ' ') ++idx1;

        value = $"{value[..idx]} {value[(idx1 + 1)..]}";
        return ReplaceBracketSpace(value);
    }

    /// <summary>
    /// Kiem tra xem it nhat co 1 trong cac mau patterns xuat hien trong chuoi value,
    /// neu ignoreCase thi kiem tra khong phan biet chu hoa chu thuong (ignore case) 
    /// </summary>
    public static bool StrExists(string value, bool ignoreCase = false, params string[] patterns)
    {
        if (string.IsNullOrEmpty(value)) return false;

        StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        foreach (string pat in patterns)
        {
            if (value.Contains(pat, comparison)) return true;
        }

        return false;
    }

    /// <summary>
    /// Kiem tra chuoi value co thanh phan tuong ung voi mau pattern cho truoc khong (chi danh cho viec kiem tra cac ma theo dang xx.xxx...),
    /// neu ignoreSpace thi se loai bo khoang trang khi kiem tra
    /// </summary>
    /// 
    public static bool HCodeMach(string value, string pattern)
    {
        if (value == pattern) return true;
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern)) return false;
        if (value.Contains(";"))
        {
            var dts = value.Split(';');
            foreach (var dt in dts)
            {
                if (dt == pattern) return true;
            }
        }
        if (pattern.Contains(";"))
        {
            var dts = pattern.Split(';');
            foreach (var dt in dts)
            {
                if (dt == value) return true;
            }
        }
        return false;
    }
    public static bool CodeMatch(string value, string pattern, bool ignoreSpace = false)
    {
        if (value == pattern) return true;
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern)) return false;
        if (ignoreSpace)
        {
            pattern = pattern.Replace(" ", "");
            value = value.Replace(" ", "");
        }

        if (!pattern.Contains("*"))
        {
            return CodeContain(value, pattern);
        }

        string comVal = $";{value};";
        string[] patLst = pattern.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (string pat in patLst)
        {
            if (pat.EndsWith("*"))
            {
                if (comVal.Contains($";{pat[0..^1]}")) return true;
                if (pat.EndsWith(".*") && comVal.Contains($";{pat[0..^2]};")) return true;
            }
            else
            {
                if (CodeContain(value, pat)) return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Kiem tra chuoi value co xuat hien bat cu mau nao trong chuoi pattern cho truoc khong (chi danh cho viec kiem tra cac ma theo dang xx.xxx....),
    /// neu ignoreSpace thi se loai bo khoang trang khi kiem tra
    /// </summary>
    public static bool CodeContain(string value, string pattern, bool ignoreSpace = false)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern)) return false;
        if (ignoreSpace)
        {
            pattern = pattern.Replace(" ", "");
            value = value.Replace(" ", "");
        }

        if (pattern.Length > value.Length) return IsContainAny(pattern, value);
        return IsContainAny(value, pattern);
    }

    public static bool HCodeContain(string value, string pattern, bool ignoreSpace = false)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern)) return false;
        if (ignoreSpace)
        {
            pattern = pattern.Replace(" ", "");
            value = value.Replace(" ", "");
        }
        if (value.Contains(";"))
        {
            var pa = value.Split(";");
            var contains = false;
            foreach (var p in pa)
            {
                if (pattern.Contains(p)) contains = true;
            }
            return contains;
        }

        if (pattern.Length > value.Length) return pattern.IndexOf(value) > -1;
        return value.IndexOf(pattern) > -1;
    }
    private static bool IsContainAny(string value, string pattern)
    {
        if (value == pattern) return true;
        if (!value.Contains(";") && !pattern.Contains(";")) return false;

        value = $";{value};";
        pattern = $";{pattern};";

        int idx = 0;
        int idn = 0;
        int len = pattern.Length;
        string part = "";
        while (idx > -1)
        {
            idn = pattern.IndexOf(";", idx + 1);
            if (idn == -1) return false;
            if (idn - idx < 3)
            {
                idx = idn;
                continue;
            }

            part = pattern[idx..(idn + 1)];
            if (value.Contains(part)) return true;

            idx = idn;
            if (idx - len >= 2) return false;
        }

        return false;
    }

    //Cách nhau bằng dấu |
    public static bool MaMayContain(string value, string pattern)
    {
        if (string.IsNullOrEmpty(value)) return false;
        if (string.IsNullOrEmpty(pattern)) return true;

        value = value.ToLower();
        pattern = pattern.ToLower();
        if (value == pattern) return true;

        value = $"|{value}|";
        pattern = $"|{pattern}|";
        return pattern.Contains(value);
    }

    /// <summary>
    /// Kiem tra so value co phu hop voi dieu kien so sanh expression cho truoc khong (chi so sanh >, >=, <, <=, =)
    /// </summary>
    public static bool NumMatch(double value, string expression)
    {
        if (string.IsNullOrEmpty(expression)) return true;

        if (expression.Contains("~"))
        {
            string[] vals = expression.Split('~', StringSplitOptions.RemoveEmptyEntries);
            if (vals.Length != 2) return false;
            return NumMatch(value, ">" + vals[0]) && NumMatch(value, "<" + vals[1]);
        }

        string sign = "=";
        if ("<>".Contains(expression[0]))
        {
            sign = expression.Substring(0, 1);
            expression = expression.Substring(1);
        }

        if (expression[0] == '=')
        {
            sign += expression.Substring(0, 1);
            expression = expression.Substring(1);
        }

        bool result = false;
        double comparer = 0d;
        if (!double.TryParse(expression, out comparer)) return false;

        if (sign.Contains("="))
        {
            result = Math.Round(value, 3) == Math.Round(comparer, 3);
        }
        if (sign.Contains(">"))
        {
            result |= value > comparer;
        }
        if (sign.Contains("<"))
        {
            result |= value < comparer;
        }

        return result;
    }

    public static string ToFilename(string filename)
    {
        return filename.Replace("#", "-")
                        .Replace("%", "-")
                        .Replace("&", "-")
                        .Replace("*", "-")
                        .Replace("$", "-")
                        .Replace("`", "-")
                        .Replace(":", "-")
                        .Replace("@", "-")
                        .Replace("+", "-")
                        .Replace("!", "-")
                        .Replace("?", "-")
                        .Replace("#", "-")
                        .Replace("'", "-")
                        .Replace("\"", "-")
                        .Replace("=", "-")
                        .Replace("<", "-")
                        .Replace(">", "-")
                        .Replace("{", "-")
                        .Replace("}", "-")
                        .Replace("\\", "-")
                        .Replace("/", "-")
                        .Replace("|", "-")
                        .Replace(" ", "-")
                         .Replace(",", "-");
    }
    #endregion

    #region Linhist added 11/05/22
    /// <summary>
    /// Kiem tra xem gia tri value co phai thuoc 1 trong cac so comparers khong
    /// </summary>
    public static bool NumExists(object? value, params int[] comparers)
    {
        if (value == null || comparers.Length < 1) return false;

        try
        {
            int num = Convert.ToInt32(value);
            foreach (int p in comparers)
            {
                if (p == num) return true;
            }
        }
        catch { }

        return false;
    }
    #endregion

    private static readonly string[] _GIOI_TINH = new string[] { "nam", "nữ", "chưa xác định" };

    public static bool IsMatchGender(string gioitinh, byte? GIOI_TINH)
    {
        if (string.IsNullOrEmpty(gioitinh) || GIOI_TINH == null || GIOI_TINH < 1 || GIOI_TINH > 3) return false;
        if (GIOI_TINH.ToString() == gioitinh) return true;

        int idx = Array.IndexOf(_GIOI_TINH, gioitinh.Trim().ToLower());
        return (idx + 1 == (int)GIOI_TINH.Value);
    }

    #region Linhist added 26/10/22
    public static bool FirstMatch(string code1, string code2)
    {
        if (string.IsNullOrWhiteSpace(code1) || string.IsNullOrWhiteSpace(code2)) return false;

        int idx = code1.IndexOf(";");
        if (idx == 0)
        {
            code1 = code1.Substring(1);
            idx = code1.IndexOf(";");
        }
        if (idx > 0)
        {
            code1 = code1.Substring(0, idx);
        }

        idx = code2.IndexOf(";");
        if (idx == 0)
        {
            code2 = code2.Substring(1);
            idx = code2.IndexOf(";");
        }
        if (idx > 0)
        {
            code2 = code2.Substring(0, idx);
        }

        return (code1 == code2);
    }
    #endregion

    #region Linhist added 26/12/22
    public static string FormatDate(DateTime? date)
    {
        if (date == null || date.Value == DateTime.MinValue) return "";
        return date.Value.ToString("HH:mm:ss dd-MM-yyyy");
    }
    #endregion


    #region Linhist added 24/03/23
    public static bool Check_MAMAY_Chuan4210(string MA_MAY, string MA_CSKCB)
    {
        //MA_MAY phai theo dinh dang XX.n.YYYYY.ZZZZZZ
        //XX: Ma nhom (viet tat ky tu dau toi da 3 ky tu)
        //n: Ma nguon kinh phi (gia tri: 1,2,3)
        //YYYYY: Ma CSKCB cua benh vien hien hanh
        //ZZZZZZ: Serie cua may (6 ky tu ko co khoang trang, co the co nhieu so serie cach nhau bang dau ';')
        var maMays = MA_MAY.Split('.');
        if (maMays.Length != 4) return false;

        //code XX: ma nhom may (vd: HH, VS, SA, MRI,...)
        if (maMays[0] == "" || maMays[0].Contains(" ") || maMays[0].Length > 3) return false;

        //code n: nguon kinh phi (1: ngan sach, 2: xa hoi hoa, 3: khac)
        if (maMays[1].Length != 1 || !"123".Contains(maMays[1])) return false;

        //code YYYYY: ma cskcb
        if (maMays[2] != MA_CSKCB) return false;

        //code ZZZZZZ: so serie may (6 chu so)
        string[] seriesMays = maMays[3].Split(';');
        foreach (string s in seriesMays)
        {
            if (s.Contains(" ") || s.Length != 6)
            {
                return false;
            }
        }

        return true;
    }
    public static bool Check_MAMAY_Chuan4750(string MA_MAY, string MA_CSKCB)
    {
        //MA_MAY phai theo dinh dang XX.n.YYYYY.ZZZZZZ
        //XX: Ma nhom (viet tat ky tu dau toi da 3 ky tu)
        //n: Ma nguon kinh phi (gia tri: 1,2,3)
        //YYYYY: Ma CSKCB cua benh vien hien hanh
        //ZZZZZZ: Serie cua may ( co the co nhieu so serie cach nhau bang dau ';')
        var maMays = MA_MAY.Split('.');
        if (maMays.Count() != 4) return false;

        //code XX: ma nhom may (vd: HH, VS, SA, MRI,...)
        if (maMays[0] == "" || maMays[0].Contains(" ") || maMays[0].Length > 3) return false;

        //code n: nguon kinh phi (1: ngan sach, 2: xa hoi hoa, 3: khac)
        if (maMays[1].Length != 1 || !"123".Contains(maMays[1])) return false;

        //code YYYYY: ma cskcb
        if (maMays[2] != MA_CSKCB) return false;

        //code ZZZZZZ: so serie may (6 chu so)
        if (maMays[3].IndexOf(";") != -1)
        {
            string[] seriesMays = maMays[3].Split(';');
            foreach (string s in seriesMays)
            {
                if (s.Contains(" "))
                {
                    return false;
                }
            }
        }
        else
        {
            if (maMays[3].Contains(" "))
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    public static bool IsNumEqual(string value, int comparer)
    {
        int val = 0;
        if (int.TryParse(value, out val)) return val == comparer;
        return false;
    }

    #region Linhist added 01/04/23
    static public string[]? GetAlertErrorProp(string propName, List<XML1ErrorInfoModel>? errorList)
    {
        if (errorList == null || !errorList.Any()) return null;

        var errMes = new string[] { "", "" };
        var errList = errorList
            .Where(x => !string.IsNullOrWhiteSpace(x.ErrorAtXMLTag) && x.ErrorAtXMLTag.ToLower().Equals(propName.ToLower()));
        if (!errList.Any()) return null;

        errMes[0] = string.Join(",", errList.Select(e => e.ErrorContent).ToArray());
        errMes[1] = string.Join(",", errList.Select(e => e.XeErrorCode?.ToString()).ToArray());
        return errMes;
    }
    #endregion

    public static int? ToNum(string value)
    {
        return int.TryParse(value, out int result) ? result : null;
    }

    public static string? ToDateView(DateTime? date)
    {
        return date?.ToString("HH:mm dd/MM/yyyy");
    }

    public static DateTime? ToDateShort(string shorten)
    {
        if (string.IsNullOrEmpty(shorten) || shorten.Length != 8) return null;

        try
        {
            var yy = shorten.Substring(0, 4);
            var mm = shorten.Substring(4, 2);
            var dd = shorten.Substring(6, 2);
            return DateTime.Parse($"{yy}/{mm}/{dd}");
        }
        catch { }

        return null;
    }

    public static string FixDateOrYear(string date)
    {
        if (date.Length == 4) return date;
        if (date.Length >= 8)
        {
            var yy = date.Substring(0, 4);
            var mm = date.Substring(4, 2);
            var dd = date.Substring(6, 2);
            return mm == "00" && dd == "00" ? yy : $"{dd}/{mm}/{yy}";
        }

        return DateTime.Now.ToString("dd/MM/yyyy");
    }

    public static string? ToFloatView(double? value)
    {
        if (value == null) return null;
        return value.Value.ToString("##,##0.00").Replace(",", "$").Replace(".", ",").Replace("$", ".");
    }

    //Linhist added 02/10/23
    public static string StrCutLen(string str, int length, bool trim = true)
    {
        string res = (str ?? "").Trim();
        if (res.Length > length)
        {
            res = res[..length];
        }

        return res;
    }

    //Linhist added 06/03/24
    public static string ReplaceValuePairs(string? value, string[][]? patterns, bool ignoreCase = false)
    {
        value = value?.Trim();
        if (string.IsNullOrEmpty(value)) return "";
        if (patterns == null || patterns.Length < 2 || patterns[0] == null || patterns[1] == null) return value;

        int idx = -1;
        StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        foreach (string pat in patterns[0])
        {
            ++idx;
            if (value.Equals(pat, comparison)) return idx < patterns[1].Length ? patterns[1][idx] : value;
        }

        return value;
    }

    public static string HLeftNumberCharOnly(string value)
    {

        // Lấy tất cả ký tự số liên tiếp từ đầu chuỗi
        int idx = 0;
        while (idx < value.Length && char.IsDigit(value[idx]))
        {
            idx++;
        }

        // Nếu có số ở đầu thì cắt ra
        if (idx > 0)
        {
            return value.Substring(0, idx);
        }

        // Nếu không có số nào thì trả về rỗng
        return "";
    }
    public static string LeftNumberCharOnly(string value)
    {
        int idx = value.Length - 1;
        for (; idx > -1; idx--)
        {
            if (char.IsDigit(value[idx])) break;
        }

        if (idx < value.Length - 1)
        {
            value = value[0..(idx + 1)];
        }

        return value;
    }

    public static bool HasCharNotNumber(string value)
    {
        foreach (char c in value)
        {
            if (!char.IsDigit(c)) return true;
        }
        return false;
    }

    public static bool IsValidDate(params DateTime?[] dates)
    {
        foreach (var d in dates)
        {
            if (d == null || d == DateTime.MinValue || d == DateTime.MaxValue) return false;
        }

        return true;
    }

    //Linhist added 03/04/26
    public static string GetMoneyVN(double? money)
        => money == null || money == double.MinValue ? "" : $"{money:##,##0.0#}".Replace(",", "$").Replace(".", ",").Replace("$", ".");

    //Linhist added 26/06/26
    public static decimal RoundUp(decimal? value, int digit = 2)
        => Math.Round(value ?? 0m, digit, MidpointRounding.AwayFromZero);

    public static decimal RoundUp(double? value, int digit = 2)
        => Math.Round((decimal)(value ?? 0d), digit, MidpointRounding.AwayFromZero);

    public static decimal RoundUp(int? value, int digit = 2)
        => Math.Round((decimal)(value ?? 0), digit, MidpointRounding.AwayFromZero);

    public static bool IsEqualUp(decimal? value1, decimal? value2, int digit = 2)
        => RoundUp(value1, digit) == RoundUp(value2, digit);

    public static bool IsEqualUp(double? value1, double? value2, int digit = 2)
        => RoundUp(value1, digit) == RoundUp(value2, digit);

    public static bool IsEqualUp(int? value1, int? value2, int digit = 2)
        => RoundUp(value1, digit) == RoundUp(value2, digit);

    public static bool IsOverUp(decimal? value1, decimal? value2, int digit = 2)
        => RoundUp(value1, digit) > RoundUp(value2, digit);

    public static bool IsOverUp(double? value1, double? value2, int digit = 2)
        => RoundUp(value1, digit) > RoundUp(value2, digit);

    public static bool IsOverUp(int? value1, int? value2, int digit = 2)
        => RoundUp(value1, digit) > RoundUp(value2, digit);

    public static bool IsUnderUp(decimal? value1, decimal? value2, int digit = 2)
        => RoundUp(value1, digit) < RoundUp(value2, digit);

    public static bool IsUnderUp(double? value1, double? value2, int digit = 2)
        => RoundUp(value1, digit) < RoundUp(value2, digit);

    public static bool IsUnderUp(int? value1, int? value2, int digit = 2)
        => RoundUp(value1, digit) < RoundUp(value2, digit);

    //Linhist added 041126
    //Cách ghi “1”: Bác sỹ, “2”: Y sỹ, “3”: Điều dưỡng, “4”: Hộ sinh, “5”: Kỹ thuật y, “6”: Cử nhân tâm lý lâm sàng, “7”: Lương y, “8”: Dược sỹ, “9”: người tham gia vào quá trình khám bệnh, chữa bệnh nhưng không cần phải cấp giấy phép hành nghề
    public static string Get_ChucDanh(string? value)
    {
        switch (value?.Trim())
        {
            case "1": return "Bác sỹ";
            case "2": return "Y sỹ";
            case "3": return "Điều dưỡng";
            case "4": return "Hộ sinh";
            case "5": return "Kỹ thuật y";
            case "6": return "Cử nhân tâm lý lâm sàng";
            case "7": return "Lương y";
            case "8": return "Dược sỹ";
            case "9": return "Người tham gia vào quá trình khám bệnh, chữa bệnh nhưng không cần phải cấp giấy phép hành nghề";
        }
        return string.Empty;
    }

    public static string Get_CDNN(string? value)
    {
        value = (value ?? string.Empty).Trim().ToLower();
        switch (value)
        {
            case "bác sỹ":
            case "bác sĩ":
                return "1";
            case "y sỹ":
            case "y sĩ":
                return "2";
            case "điều dưỡng":
                return "3";
            case "hộ sinh":
                return "4";
            case "kỹ thuật y":
                return "5";
            case "cử nhân":
            case "cử nhân tâm lý":
            case "cử nhân tâm lý lâm sàng":
                return "6";
            case "lương y": return "7";
            case "dược sỹ": return "8";
        }
        return value == string.Empty ? string.Empty : "9";
    }

    //“1”: Người chịu trách nhiệm chuyên môn;
    //“2”: Trưởng khoa/ Trưởng đơn nguyên/người phụ trách phòng khám chuyên khoa đối với cơ sở khám bệnh, chữa bệnh có hình thức Tổ chức là phòng khám liên chuyên khoa;
    //“3”: Người chịu trách nhiệm chuyên môn kiêm Trưởng khoa;
    //“4”: Người đứng đầu cơ sở KCB hoặc người được người đứng đầu cơ sở KCB ủy quyền được ký giấy chuyển tuyến KCB BHYT, giấy ra viện;
    //“5”: Người hành nghề được giao phụ trách khoa trong trường hợp không có trưởng khoa;
    //“6”: Người được ủy quyền chịu trách nhiệm chuyên môn kỹ thuật theo khoản 11 Điều 27 Nghị định số 96/2023/NĐ-CP
    public static string Get_ViTri(string? value)
    {
        var vals = (value ?? string.Empty).Trim().ToLower().Split(';', StringSplitOptions.RemoveEmptyEntries);
        var res = string.Empty;
        foreach (var v in vals)
        {
            switch (v.Trim())
            {
                case "1": res += "; Người chịu trách nhiệm chuyên môn"; break;
                case "2": res += "; Trưởng khoa"; break;
                case "3": res += "; Người chịu trách nhiệm chuyên môn kiêm Trưởng khoa"; break;
                case "4": res += "; Người đứng đầu cơ sở KCB hoặc người được người đứng đầu cơ sở KCB ủy quyền được ký giấy chuyển tuyến KCB BHYT, giấy ra viện"; break;
                case "5": res += "; Người hành nghề được giao phụ trách khoa trong trường hợp không có trưởng khoa"; break;
                case "6": res += "; Người được ủy quyền chịu trách nhiệm chuyên môn kỹ thuật"; break;
            }
        }
        return res.Length > 0 ? res[2..] : string.Empty;
    }

    public static string Get_MaViTri(string? value)
    {
        var vals = (value ?? string.Empty).Trim().ToLower().Split(';', StringSplitOptions.RemoveEmptyEntries);
        var res = string.Empty;
        foreach (var v in vals)
        {
            var va = v.Trim();
            if (va.Contains("người đứng đầu") || va.Contains("ủy quyền được ký giấy"))
            {
                res += ";4";
                continue;
            }
            if (va.Contains("được giao phụ trách") || va.Contains("không có trưởng khoa"))
            {
                res += ";5";
                continue;
            }
            if (va.Contains("chịu trách nhiệm chuyên môn"))
            {
                res += va.Contains("kiêm trưởng khoa") ? ";3" : va.Contains("được ủy quyền") ? ";6" : ";1";
                continue;
            }
            if (va.Equals("trưởng khoa") || va.Contains("trưởng") ||va.Contains("phụ trách phòng khám"))
            {
                res += ";2";
                continue;
            }
        }

        return res.Length > 0 ? res[1..] : string.Empty;
    }
}
