using System.Text.Encodings.Web;
using System.Text.Json;

namespace HIA.Core.Base;

/// <summary>
/// Chinh sach ten thuoc tinh JSON: ToLowerInvariant (toan bo chu thuong).
/// Dung chung cho JsonDefaults va MVC (Startup.AddJsonOptions) de khop nhau.
/// Thuoc tinh co [JsonPropertyName] thi giu nguyen ten do attribute.
/// </summary>
public sealed class LowerCaseNamingPolicy : JsonNamingPolicy
{
    public static readonly LowerCaseNamingPolicy Instance = new();

    public override string ConvertName(string name) => name.ToLowerInvariant();
}

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = LowerCaseNamingPolicy.Instance,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Options cho JSON dua vao Html.Raw (ben trong &lt;script&gt;): escape &lt; &gt; &amp; ' " va ky tu ngoai ASCII.
    /// Khong dung UnsafeRelaxedJsonEscaping de chuoi du lieu khong the thoat khoi the script.
    /// </summary>
    private static readonly JsonSerializerOptions HtmlOptions = new()
    {
        PropertyNamingPolicy = LowerCaseNamingPolicy.Instance,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.Default
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static string SerializeForHtml<T>(T value) => JsonSerializer.Serialize(value, HtmlOptions);

    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
}
