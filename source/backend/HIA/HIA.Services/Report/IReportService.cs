using HIA.Core.Models.Diagram;
using HIA.Data.Catalogs;
using HIA.Data.Records;

namespace HIA.Services.Report;

public interface IReportService
{
    Task<IEnumerable<XML01>> GetAllXML1();

    Task<DMBenh?> GetTenBen(string code);

    int TotalFileDate();

    int TotalFileMonth();

    int TotalFileDieuTriNoiTruDate();

    int TotalFileDieuTriNoiTruMonth();

    int TotalFileRaVienDate();

    int TotalFileRavienMonth();

    double? TotalFileTongTienDate();

    double? TotalFileTongTienMonth();

    Task<TotalizeReport> GetThongKe(DateTime? byDate = null);

    Task<Summary> GetSummary(DateTime? byDate = null);

    Task<Summary> GetDaily(DateTime dte);
}
