using System.Text.Json.Serialization;

namespace HIA.Core.Models.DataTable;

/// <summary>
/// Phan hoi server-side cua DataTables: draw, recordsTotal, recordsFiltered, data.
/// recordsTotal/recordsFiltered giu nguyen hoa nho [JsonPropertyName] (DataTables yeu cau dung ten nay).
/// </summary>
public class DataTableResponse<T>
{
    public int Draw { get; set; }

    [JsonPropertyName("recordsTotal")]
    public int RecordsTotal { get; set; }

    [JsonPropertyName("recordsFiltered")]
    public int RecordsFiltered { get; set; }

    public IEnumerable<T> Data { get; set; } = [];
}

public static class DataTableResponse
{
    public static DataTableResponse<T> Create<T>(int draw, int total, IEnumerable<T> data) => new()
    {
        Draw = draw,
        RecordsTotal = total,
        RecordsFiltered = total,
        Data = data
    };
}

/// <summary>
/// Phan hoi chi co recordsTotal (POST action tra so luong, client doc res.recordsTotal).
/// </summary>
public class RecordsTotalResponse
{
    [JsonPropertyName("recordsTotal")]
    public int RecordsTotal { get; set; }
}

/// <summary>
/// Phan hoi co data va recordsTotal.
/// </summary>
public class RecordsTotalResponse<T>
{
    public T? Data { get; set; }

    [JsonPropertyName("recordsTotal")]
    public int RecordsTotal { get; set; }
}
