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

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
}
