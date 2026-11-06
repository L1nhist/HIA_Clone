using System.Text.Json;
using System.Text.Json.Serialization;

namespace HIA.Core.Base;

public class FormatConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string text;
        if (reader.TokenType == JsonTokenType.Number)
        {
            text = reader.TryGetInt64(out var l) ? l.ToString() : reader.GetDouble().ToString();
        }
        else
        {
            text = reader.GetString() ?? string.Empty;
        }

        return Convert.ToInt32(text.Replace(".", string.Empty));
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        => throw new NotImplementedException();
}
