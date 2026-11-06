namespace HIA.Core.Models.Categories;

public class FilterDownloadModel
{
    public string? HospitalId { get; set; }

    public string? MaCSKCB { get; set; }

    public string? SearchKey { get; set; }
    public string? OrderByColumn { get; set; }
    public string? OrderType { get; set; }

    public DateTime? FromDateIn { get; set; }
    public DateTime? ToDateIn { get; set; }
    public DateTime? FromDateOut { get; set; }
    public DateTime? ToDateOut { get; set; }
    public DateTime? FromDateCreate { get; set; }
    public DateTime? ToDateCreate { get; set; }
    public int? Status { get; set; }

    public List<string>? IgnoreKeysForSearch { get; set; }
    public List<string>? Departments { get; set; }
    public List<string>? Errors { get; set; }

    public DateTime? FromDatePaid { get; set; }
    public DateTime? ToDatePaid { get; set; }

    public int? FromHour { get; set; }

    public int? FromMin { get; set; }

    public int? ToHour { get; set; }

    public int? ToMin { get; set; }

    public string? TypeKCB { get; set; }
}
