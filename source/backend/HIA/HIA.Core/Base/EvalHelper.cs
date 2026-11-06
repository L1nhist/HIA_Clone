using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using HIA.Data.Base;

namespace HIA.Core.Base;
public static class EvalHelper
{
    private static readonly DateTime minSqlDte = new(1753, 1, 1);
    private static readonly string[] fixDateFormats = new[] { "yyyyMMdd", "yyyyMMddHH", "yyyyMMddHHmm" };
    private static readonly string[] dteFormats = new[] { "dd/MM/yyyy", "HH:mm:ss dd/MM/yyyy", "yyyyMMdd", "yyyyMMddHHmm", "yyyyMMddHHmmss" };
    private static readonly object?[] emptyVals = new object?[] { null, "", int.MinValue, short.MinValue, long.MinValue, decimal.MinValue, double.MinValue, float.MinValue, DateTime.MinValue };
    private static readonly string[] uniCodes = new[] { "aáàảãạâấầẩẫậăắằẳẵặ", "dđ", "eéèẻẽẹêếềểễệ", "iíìỉĩị", "oóòỏõọôốồổỗộơớờởỡợ", "uúùủũụưứừửữự", "yýỳỷỹỵ" };

    static EvalHelper()
    { }

    #region Utility function
    public static bool IsEmpty(object? value) => emptyVals.Contains(value);

    public static bool IsEmptyAny(IEnumerable values)
    {
        foreach (var v in values)
        {
            if (IsEmpty(v)) return true;
        }
        return false;
    }

    public static bool IsEmptyAll(IEnumerable values)
    {
        foreach (var v in values)
        {
            if (!IsEmpty(v)) return false;
        }
        return true;
    }

    public static bool CodeLike(string val, string pats, bool ignoreCase = false)
    {
        if (pats == null || (pats = pats.Replace(" ", "")).Length == 0) return false;
        if (val == pats) return true;

        bool isCode = pats.Contains('*');
        StringComparison comp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (pats.Contains(';'))
        {
            if (isCode) return pats.Split(';', StringSplitOptions.RemoveEmptyEntries).Any(p => CodeLike(val, p, ignoreCase));
            return pats.Split(';').Any(p => CodeLike(val, p, ignoreCase));
        }

        if (!isCode) return $";{val};".Contains($";{pats};", comp);

        pats = pats[..pats.IndexOf("*")];
        if (pats.EndsWith("."))
        {
            return $";{val};".Contains($";{pats}", comp) || $";{val};".Contains($";{pats[..^1]};", comp);
        }

        return $";{val};".Contains($";{pats}", comp);
    }

    public static string StripUnicode(string val)
    {
        var sb = new StringBuilder();
        foreach (char c in val)
        {
            var notUni = true;
            foreach (var codes in uniCodes)
            {
                if (codes.Contains(c))
                {
                    sb.Append(codes[0]);
                    notUni = false;
                    break;
                }
            }
            if (!notUni)
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
    #endregion

    #region Covertion function
    public static T? Cast<T>(string value)
        where T : struct
    {
        if (IsEmpty(value)) return null;

        T? res = null;
        Type tpe = typeof(T);
        if (tpe == typeof(DateTime))
        {
            try
            {
                if (value.Length == 4)
                {
                    value += "0101";
                }
                res = DateTime.ParseExact(value, dteFormats, CultureInfo.InvariantCulture, DateTimeStyles.None) as T?;
            }
            catch { }
        }

        try
        {
            res = (T)Convert.ChangeType(value, tpe);
        }
        catch { }

        return res;
    }

    public static double? ToDouble(string value)
        => string.IsNullOrWhiteSpace(value) ? null : (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture,  out double d) ? d :double.MinValue);

    public static string ToDoubleView(double? value)
        => value == null || value == double.MinValue ? "" : value.Value.ToString("##,##0.0#");

    public static DateTime? ToDateSql(string value)
    {
        if (IsEmpty(value)) return null;

        int len = value.Length;
        try
        {
            //Fix trường hợp ngày chỉ có năm. Vd: 1992, 199200000
            if (len == 4)
            {
                value += "0101";
            }
            else if (len == 8 && value.EndsWith("0000"))
            {
                value = value[..^4] + "0101";
            }
            else if (len == 10 && value.EndsWith("000000"))
            {
                value = value[..^6] + "0101";
            }
            else if (len == 12 && value.EndsWith("00000000"))
            {
                value = value[..^8] + "0101";
            }

            if (DateTime.TryParseExact(value, fixDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
            {
                return dt < minSqlDte ? minSqlDte : dt;
            }
        }
        catch
        { }

        return minSqlDte;
    }

    public static string ToDateView(DateTime? date)
        => date != null && date > minSqlDte ? date.Value.ToString("HH:mm dd/MM/yyyy") : "";

    public static string ToDateFullView(DateTime? date)
        => date != null && date > minSqlDte ? date.Value.ToString("HH:mm:ss dd/MM/yyyy") : "";

    public static double Round2Digit(double value)
    {
        if (value % 0.01 >= 0.0049) return Math.Round(value + 0.001, 2);
        return Math.Round(value, 2);
    }
    #endregion

    /// <summary>
    /// Bắt đầu kiểm tra một trường dữ liệu của Entity.
    /// VD: xml01.Field(x => x.MA_LK)....
    /// </summary>
    public static Evaluation<TOwner, TField> Field<TOwner, TField>(this TOwner xml, Expression<Func<TOwner, TField>> expression)
        where TOwner : IXMLEntity
    {
        return new Evaluation<TOwner, TField>(xml, expression);
    }

    #region Extra function for Evaluation String
    public static Evaluation<TOwner, string> CodeContain<TOwner>(this Evaluation<TOwner, string> eval, string patterns, bool ignoreSpace = false)
        where TOwner : IXMLEntity
        => eval.Check(f => Uti.CodeContain(f, patterns, ignoreSpace));

    public static Evaluation<TOwner, string> CodeMatch<TOwner>(this Evaluation<TOwner, string> eval, string patterns, bool ignoreSpace = false)
        where TOwner : IXMLEntity
        => eval.Check(f => Uti.CodeMatch(f, patterns, ignoreSpace));

    public static Evaluation<TOwner, string> IsOnlyWhitespace<TOwner>(this Evaluation<TOwner, string> eval)
        where TOwner : IXMLEntity
        => eval.Check(f => f.Trim() == "");

    /// <summary>
    /// Kiểm tra xem chuỗi có bằng giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, string> IsEqual<TOwner>(this Evaluation<TOwner, string> eval, string value, bool ignoreCase = false)
        where TOwner : IXMLEntity
        => eval.Check(f => f.Equals(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));

    /// <summary>
    /// Kiểm tra xem chuỗi có bằng giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, string> IsEqual<TOwner>(this Evaluation<TOwner, string> eval, Evaluation<TOwner, string> other, bool ignoreCase = false)
        where TOwner : IXMLEntity
        => eval.Check((f, o) => f.Equals(o, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal), other);

    /// <summary>
    /// Kiểm tra xem chuỗi có không bằng giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, string> IsNotEqual<TOwner>(this Evaluation<TOwner, string> eval, string value, bool ignoreCase = false)
        where TOwner : IXMLEntity
        => eval.CheckNot(f => f.Equals(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));

    /// <summary>
    /// Kiểm tra xem chuỗi có không bằng giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, string> IsNotEqual<TOwner>(this Evaluation<TOwner, string> eval, Evaluation<TOwner, string> other, bool ignoreCase = false)
        where TOwner : IXMLEntity
        => eval.CheckNot((f, o) => f.Equals(o, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal), other);

    /// <summary>
    /// Kiểm tra xem có đúng định dạng truyền vào (kiểu 12.*; AB*.1;...)
    /// </summary>
    public static Evaluation<TOwner, string> LikesAny<TOwner>(this Evaluation<TOwner, string> eval, string patterns, bool ignoreCase = false)
        where TOwner : IXMLEntity
        => eval.Check(f => CodeLike(f, patterns, ignoreCase));

    /// <summary>
    /// Kiểm tra xem có không đúng định dạng truyền vào (kiểu 12.*; AB*.1;...)
    /// </summary>
    public static Evaluation<TOwner, string> NotLikes<TOwner>(this Evaluation<TOwner, string> eval, string patterns, bool ignoreCase = false)
        where TOwner : IXMLEntity
        => eval.CheckNot(f => CodeLike(f, patterns, ignoreCase));

    /// <summary>
    /// Kiểm tra xem có khoảng trống (dấu cách) trong chuỗi hay không?
    /// </summary>
    public static Evaluation<TOwner, string> HasSpaceAny<TOwner>(this Evaluation<TOwner, string> eval)
        where TOwner : IXMLEntity
        => eval.Check(f => f.Trim().Contains(' '));

    /// <summary>
    /// Kiểm tra xem không có khoảng trống (dấu cách) trong chuỗi hay không?
    /// </summary>
    public static Evaluation<TOwner, string> NotHasSpace<TOwner>(this Evaluation<TOwner, string> eval)
        where TOwner : IXMLEntity
        => eval.CheckNot(f => f.Trim().Contains(' '));

    /// <summary>
    /// Kiểm tra chuỗi dạng mảng có đáp ứng ít nhất 1 điều kiện truyền vào
    /// </summary>
    public static Evaluation<TOwner, string> EachAny<TOwner>(this Evaluation<TOwner, string> eval, string spliters, Func<string, bool> predicate)
        where TOwner : IXMLEntity
        => eval.Check(f => f.Split((spliters ?? " ").ToCharArray()).Any(predicate));

    /// <summary>
    /// Kiểm tra chuỗi dạng mảng có đáp ứng tất cả theo điều kiện truyền vào
    /// </summary>
    public static Evaluation<TOwner, string> EachAll<TOwner>(this Evaluation<TOwner, string> eval, string spliters, Func<string, bool> predicate)
        where TOwner : IXMLEntity
        => eval.Check(f => f.Split((spliters ?? " ").ToCharArray()).All(predicate));

    /// <summary>
    /// Kiểm tra số lượng chuỗi trong mảng có lớn hơn giá trị truyền vào không
    /// </summary>
    public static Evaluation<TOwner, string> EachCountOver<TOwner>(this Evaluation<TOwner, string> eval, string spliters, int? min)
        where TOwner : IXMLEntity
        => eval.Check(f => f.Split((spliters ?? " ").ToCharArray()).Count() > min);

    /// <summary>
    /// Kiểm tra số lượng chuỗi trong mảng có lớn hơn hoặc bằng giá trị truyền vào không
    /// </summary>
    public static Evaluation<TOwner, string> EachCountOverOrEqual<TOwner>(this Evaluation<TOwner, string> eval, string spliters, int? min)
        where TOwner : IXMLEntity
        => eval.Check(f => f.Split((spliters ?? " ").ToCharArray()).Count() >= min);

    /// <summary>
    /// Kiểm tra số lượng chuỗi trong mảng có nhỏ hơn giá trị truyền vào không
    /// </summary>
    public static Evaluation<TOwner, string> EachCountUnder<TOwner>(this Evaluation<TOwner, string> eval, string spliters, int? max)
        where TOwner : IXMLEntity
        => eval.Check(f => f.Split((spliters ?? " ").ToCharArray()).Count() < max);

    /// <summary>
    /// Kiểm tra số lượng chuỗi trong mảng có nhỏ hơn hoặc bằng giá trị truyền vào không
    /// </summary>
    public static Evaluation<TOwner, string> EachCountUnderOrEqual<TOwner>(this Evaluation<TOwner, string> eval, string spliters, int? max)
        where TOwner : IXMLEntity
        => eval.Check(f => f.Split((spliters ?? " ").ToCharArray()).Count() < max);

    /// <summary>
    /// Kiểm tra số lượng chuỗi trong mảng có nằm ngoài khoảng giá trị truyền vào không
    /// </summary>
    public static Evaluation<TOwner, string> EachCountNotBetween<TOwner>(this Evaluation<TOwner, string> eval, string spliters, int? min, int? max)
        where TOwner : IXMLEntity
    {
        Func<int, bool> exec = e => e < min || e > max;
        return eval.Check(f => exec.Invoke(f.Split((spliters ?? " ").ToCharArray()).Count()));
    }

    #endregion

    #region Extra function for Evaluation Number
    /// <summary>
    /// Kiểm tra xem có lớn hơn giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, TComp?> IsOver<TOwner, TComp>(this Evaluation<TOwner, TComp?> eval, TComp? min)
        where TOwner : IXMLEntity
        where TComp : struct, IComparable
        => eval.Check(f => (f ?? default).CompareTo(min) > 0);

    /// <summary>
    /// Kiểm tra xem có lớn hơn giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, TComp?> IsOver<TOwner, TComp>(this Evaluation<TOwner, TComp?> eval, Evaluation<TOwner, TComp?> other)
        where TOwner : IXMLEntity
        where TComp : struct, IComparable
        => eval.Check((f, o) => (f ?? default).CompareTo(o ?? default) > 0, other);

    /// <summary>
    /// Kiểm tra xem có lớn hơn hoặc bằng giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, TComp?> IsOverOrEqual<TOwner, TComp>(this Evaluation<TOwner, TComp?> eval, TComp? min)
        where TOwner : IXMLEntity
        where TComp : struct, IComparable
        => eval.Check(f => (f ?? default).CompareTo(min) >= 0);

    /// <summary>
    /// Kiểm tra xem có lớn hơn hoặc bằng giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, TComp?> IsOverOrEqual<TOwner, TComp>(this Evaluation<TOwner, TComp?> eval, Evaluation<TOwner, TComp?> other)
        where TOwner : IXMLEntity
        where TComp : struct, IComparable
        => eval.Check((f, o) => (f ?? default).CompareTo(o) >= 0, other);

    /// <summary>
    /// Kiểm tra xem có nhỏ hơn giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, TComp?> IsUnder<TOwner, TComp>(this Evaluation<TOwner, TComp?> eval, TComp? max)
        where TOwner : IXMLEntity
        where TComp : struct, IComparable
        => eval.Check(f => (f ?? default).CompareTo(max) < 0);

    /// <summary>
    /// Kiểm tra xem có nhỏ hơn giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, TComp?> IsUnder<TOwner, TComp>(this Evaluation<TOwner, TComp?> eval, Evaluation<TOwner, TComp?> other)
        where TOwner : IXMLEntity
        where TComp : struct, IComparable
        => eval.Check((f, o) => (f ?? default).CompareTo(o) < 0, other);

    /// <summary>
    /// Kiểm tra xem có nhỏ hơn hoặc bằng giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, TComp?> IsUnderOrEqual<TOwner, TComp>(this Evaluation<TOwner, TComp?> eval, TComp? max)
        where TOwner : IXMLEntity
        where TComp : struct, IComparable
        => eval.Check(f => (f ?? default).CompareTo(max) <= 0);

    /// <summary>
    /// Kiểm tra xem có nhỏ hơn hoặc bằng giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, TComp> IsUnderOrEqual<TOwner, TComp>(this Evaluation<TOwner, TComp> eval, Evaluation<TOwner, TComp> other)
        where TOwner : IXMLEntity
        where TComp : struct, IComparable
        => eval.Check((f, o) => f.CompareTo(o) <= 0, other);

    /// <summary>
    /// Kiểm tra xem có nằm trong khoảng giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, TComp?> Between<TOwner, TComp>(this Evaluation<TOwner, TComp?> eval, TComp? min, TComp? max)
        where TOwner : IXMLEntity
        where TComp : struct, IComparable
        => eval.Check(f => (f ?? default).CompareTo(min) > 0 && (f ?? default).CompareTo(max) < 0);

    /// <summary>
    /// Kiểm tra xem có nằm trong khoảng hoặc bằng giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, TComp?> BetweenOrEqual<TOwner, TComp>(this Evaluation<TOwner, TComp?> eval, TComp? min, TComp? max)
        where TOwner : IXMLEntity
        where TComp : struct, IComparable
        => eval.Check(f => (f ?? default).CompareTo(min) >= 0 && (f ?? default).CompareTo(max) <= 0);

    /// <summary>
    /// Kiểm tra xem có nằm ngoài khoảng giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, TComp> NotBetween<TOwner, TComp>(this Evaluation<TOwner, TComp> eval, TComp? min, TComp? max)
        where TOwner : IXMLEntity
        where TComp : struct, IComparable
        => eval.CheckNot(f => f.CompareTo(min) > 0 && f.CompareTo(max) < 0);

    /// <summary>
    /// Kiểm tra xem có nằm ngoài khoảng hoặc bằng giá trị truyền vào không?
    /// </summary>
    public static Evaluation<TOwner, TComp?> NotBetweenOrEqual<TOwner, TComp>(this Evaluation<TOwner, TComp?> eval, TComp? min, TComp? max)
        where TOwner : IXMLEntity
        where TComp : struct, IComparable
        => eval.CheckNot(f => (f ?? default).CompareTo(min) >= 0 && (f ?? default).CompareTo(max) <= 0);

    /// <summary>
    /// Kiểm tra xem có vượt quá số phút không?
    /// </summary>
    public static Evaluation<TOwner, DateTime?> TimeOverMinute<TOwner>(this Evaluation<TOwner, DateTime?> eval, DateTime? date, int minute)
        where TOwner : IXMLEntity
        => eval.Check(f => date == null || (date.Value - (f ?? default)).TotalMinutes > minute);

    /// <summary>
    /// Kiểm tra xem có vượt quá số phút không?
    /// </summary>
    public static Evaluation<TOwner, DateTime?> TimeOverOrEqualMinute<TOwner>(this Evaluation<TOwner, DateTime?> eval, DateTime? date, int minute)
        where TOwner : IXMLEntity
        => eval.Check(f => date == null || (date.Value - (f ?? default)).TotalMinutes >= minute);

    /// <summary>
    /// Kiểm tra xem có ít hơn số phút không?
    /// 12:10 - 12:05; 12:11 - 12:05
    /// </summary>
    public static Evaluation<TOwner, DateTime?> TimeUnderMinute<TOwner>(this Evaluation<TOwner, DateTime?> eval, DateTime? date, int minute)
        where TOwner : IXMLEntity
        => eval.Check(f => date == null || (date.Value - (f ?? default)).TotalMinutes < minute);

    /// <summary>
    /// Kiểm tra xem có ít hơn số phút không?
    /// </summary>
    public static Evaluation<TOwner, DateTime?> TimeUnderOrEqualMinute<TOwner>(this Evaluation<TOwner, DateTime?> eval, DateTime? date, int minute)
        where TOwner : IXMLEntity
        => eval.Check(f => date == null || (date.Value - (f ?? default)).TotalMinutes <= minute);

    #endregion

    #region Extra function for Evaluation Special

    /// <summary>
    /// Kiểm tra xem có không chứa một trong các giá trị truyền vào làm tròn không?
    /// </summary>
    public static Evaluation<TOwner, double?> HasRoundedEquals<TOwner>(this Evaluation<TOwner, double?> eval, double? value, int decimals)
        where TOwner : IXMLEntity
        => eval.Check(f => Math.Round(f ?? 0, 2).Equals(Math.Round(value ?? 0, 2)));

    /// <summary>
    /// Kiểm tra xem có không chứa một trong các giá trị truyền vào làm tròn không?
    /// </summary>
    public static Evaluation<TOwner, double?> NotRoundedEquals<TOwner>(this Evaluation<TOwner, double?> eval, double? value, int decimals)
        where TOwner : IXMLEntity
        => eval.CheckNot(f => Math.Round(f ?? 0, 2).Equals(Math.Round(value ?? 0, 2)));

    /// <summary>
    /// Không đúng định dạng ngày tháng yyyymmddHHMM hoặc yyyymmdd hoặc yyyy
    /// </summary>
    public static Evaluation<TOwner, DateTime?> NotDateTimeFormat<TOwner>(this Evaluation<TOwner, DateTime?> eval)
        where TOwner : IXMLEntity
        => eval.Check(f => f == minSqlDte);

    /// <summary>
    /// Không đúng định dạng #####.###
    /// </summary>
    public static Evaluation<TOwner, double?> NotNumberFormat<TOwner>(this Evaluation<TOwner, double?> eval)
        where TOwner : IXMLEntity
        => eval.CheckIgnoreEmpty(f => f == double.MinValue);

    /// <summary>
    /// Kiểm tra xem có phải là giá trị 1,2,3 hay không
    /// </summary>
    public static Evaluation<TOwner, byte?> Not_1To3<TOwner>(this Evaluation<TOwner, byte?> eval)
        where TOwner : IXMLEntity
        => eval.NotBetweenOrEqual(1, 3);

    /// <summary>
    /// Kiểm tra xem có phải là giá trị 1,2,3 hay không
    /// </summary>
    public static Evaluation<TOwner, int?> Not_1To3<TOwner>(this Evaluation<TOwner, int?> eval)
        where TOwner : IXMLEntity
        => eval.NotBetweenOrEqual(1, 3);

    /// <summary>
    /// Kiểm tra xem có phải là giá trị 1,2,3,4,5 hay không
    /// </summary>
    public static Evaluation<TOwner, byte?> Not_1To5<TOwner>(this Evaluation<TOwner, byte?> eval)
        where TOwner : IXMLEntity
        => eval.NotBetweenOrEqual(1, 5);

    /// <summary>
    /// Kiểm tra xem có phải là giá trị 1,2,3,4,5 hay không
    /// </summary>
    public static Evaluation<TOwner, int?> Not_1To5<TOwner>(this Evaluation<TOwner, int?> eval)
        where TOwner : IXMLEntity
        => eval.NotBetweenOrEqual(1, 5);

    /// <summary>
    /// Kiểm tra xem có phải là giá trị 1,2,3,4,5,6,7,8 hay không
    /// </summary>
    public static Evaluation<TOwner, byte?> Not_1To8<TOwner>(this Evaluation<TOwner, byte?> eval)
        where TOwner : IXMLEntity
        => eval.NotBetweenOrEqual(1, 8);

    /// <summary>
    /// Kiểm tra xem có phải là giá trị 1,2,3,4,5,6,7,8 hay không
    /// </summary>
    public static Evaluation<TOwner, int?> Not_1To8<TOwner>(this Evaluation<TOwner, int?> eval)
        where TOwner : IXMLEntity
        => eval.NotBetweenOrEqual(1, 8);

    /// <summary>
    /// Kiểm tra xem có phải là giá trị 1,2,3,4,5,6,7,8,9 hay không
    /// </summary>
    public static Evaluation<TOwner, byte?> Not_1To9<TOwner>(this Evaluation<TOwner, byte?> eval)
        where TOwner : IXMLEntity
        => eval.NotBetweenOrEqual(1, 9);

    /// <summary>
    /// Kiểm tra xem có phải là giá trị 1,2,3,4,5,6,7,8,9 hay không
    /// </summary>
    public static Evaluation<TOwner, int?> Not_1To9<TOwner>(this Evaluation<TOwner, int?> eval)
        where TOwner : IXMLEntity
        => eval.NotBetweenOrEqual(1, 9);

    /// <summary>
    /// 1   Khỏi
    /// 2   Đỡ
    /// 3   Không thay đổi
    /// 4   Nặng hơn
    /// 5   Tử vong
    /// 6   Tiên lượng nặng xin về
    /// 7   Chưa xác định
    /// 8   Tử vong ngoại viện
    /// </summary>
    public static Evaluation<TOwner, byte?> Not_KQ_DTRI<TOwner>(this Evaluation<TOwner, byte?> eval)
        where TOwner : IXMLEntity
        => eval.NotBetweenOrEqual(1, 8);

    /// <summary>
    /// 1   Ra viện
    /// 2   Chuyển tuyến theo yêu cầu chuyên môn
    /// 3   Trốn viện
    /// 4   Xin ra viện 
    /// 5   Chuyển tuyến theo yêu cầu người bệnh
    /// </summary>
    public static Evaluation<TOwner, byte?> Not_LOAI_RV<TOwner>(this Evaluation<TOwner, byte?> eval)
        where TOwner : IXMLEntity
        => eval.NotBetweenOrEqual(1, 5);

    /// <summary>
    /// ...
    /// </summary>
    public static Evaluation<TOwner, string> Not_LOAI_KCB<TOwner>(this Evaluation<TOwner, string> eval)
        where TOwner : IXMLEntity
        => eval.NotValue("01", "02", "03", "04", "05", "06", "07", "08", "09","10");
    #endregion
}
