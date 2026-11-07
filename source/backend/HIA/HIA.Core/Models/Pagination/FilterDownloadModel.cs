using HIA.Core.Models.Files;
using HIA.Core.Models.Pagination;
using HIA.Data.Base;
namespace HIA.Core.Models.Categories;

public class FilterDownloadModel : IMapable<PaginationRequestModel>, IMapable<FilesPaginationRequest>
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

    // Map cap: PaginationRequestModel <-> FilterDownloadModel
    void IMapable<PaginationRequestModel>.MapFrom(PaginationRequestModel? item)
    {
        if (item == null) return;
        SearchKey = item.SearchKey;
        OrderByColumn = item.OrderByColumn;
        OrderType = item.OrderType;
    }

    PaginationRequestModel? IMapable<PaginationRequestModel>.MapTo()
    {
        return new PaginationRequestModel
        {
            SearchKey = SearchKey,
            OrderByColumn = OrderByColumn,
            OrderType = OrderType
        };
    }

    // Map cap: FilesPaginationRequest <-> FilterDownloadModel
    void IMapable<FilesPaginationRequest>.MapFrom(FilesPaginationRequest? item)
    {
        if (item == null) return;
        HospitalId = item.HospitalId;
        SearchKey = item.SearchKey;
        OrderByColumn = item.OrderByColumn;
        OrderType = item.OrderType;
        FromDateIn = item.FromDateIn;
        ToDateIn = item.ToDateIn;
        FromDateOut = item.FromDateOut;
        ToDateOut = item.ToDateOut;
        FromDateCreate = item.FromDateCreate;
        ToDateCreate = item.ToDateCreate;
        FromDatePaid = item.FromDatePaid;
        ToDatePaid = item.ToDatePaid;
        FromHour = item.FromHour;
        FromMin = item.FromMin;
        ToHour = item.ToHour;
        ToMin = item.ToMin;
        Status = item.Status;
        TypeKCB = item.TypeKCB;
        Departments = item.Departments?.ToList() ?? new List<string>();
        Errors = item.Errors?.ToList() ?? new List<string>();
    }

    FilesPaginationRequest? IMapable<FilesPaginationRequest>.MapTo()
    {
        return new FilesPaginationRequest
        {
            HospitalId = HospitalId,
            SearchKey = SearchKey,
            OrderByColumn = OrderByColumn,
            OrderType = OrderType,
            FromDateIn = FromDateIn,
            ToDateIn = ToDateIn,
            FromDateOut = FromDateOut,
            ToDateOut = ToDateOut,
            FromDateCreate = FromDateCreate,
            ToDateCreate = ToDateCreate,
            FromDatePaid = FromDatePaid,
            ToDatePaid = ToDatePaid,
            FromHour = FromHour,
            FromMin = FromMin,
            ToHour = ToHour,
            ToMin = ToMin,
            Status = Status,
            TypeKCB = TypeKCB,
            Departments = Departments?.ToList() ?? new List<string>(),
            Errors = Errors?.ToList() ?? new List<string>()
        };
    }
}
