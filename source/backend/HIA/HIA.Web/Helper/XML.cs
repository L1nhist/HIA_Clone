using System.IO;
using System.Xml;
using System.Xml.Serialization;
using HIA.Core.Base;

namespace HIA.Web.Helper;

internal static class ParseHelpers
{
    public static Stream ToStream(this string @this)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(@this);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    public static T? ParseXML<T>(this string @this) where T : class
    {
        var reader = XmlReader.Create(@this.Trim().ToStream(), new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document });
        return new XmlSerializer(typeof(T)).Deserialize(reader) as T;
    }

    public static T? ParseJSON<T>(this string @this) where T : class
    {
        return JsonDefaults.Deserialize<T>(@this.Trim());
    }

    public static T? FromXml<T>(this string @this)
    {
        T? returnedXmlClass = default;

        try
        {
            using TextReader reader = new StringReader(@this);
            try
            {
                returnedXmlClass = (T?)new XmlSerializer(typeof(T)).Deserialize(reader);
            }
            catch (InvalidOperationException)
            {
                // String passed is not XML, simply return defaultXmlClass
            }
        }
        catch
        {
        }

        return returnedXmlClass;
    }
}
