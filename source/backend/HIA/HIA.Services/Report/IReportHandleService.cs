using HIA.Core.Models.Base;
using HIA.Core.Models.Diagram;

namespace HIA.Services.Report;

public interface IReportHandleService
{
    Task<ResponseModel<byte[]>> BaoCao_TongHop(string filterString);

    Task<ResponseModel<byte[]>> BaoCao_TrungDichVu(string filterString);

    Task<ResponseModel<byte[]>> BaoCao_TrungThuocDV(string filterString);

    Task<ResponseModel<byte[]>> BaoCao_ChiDinhDV(string filterString);

    Task<ResponseModel<byte[]>> BaoCao_ThongkeLoi(string filterString);

    Task<ResponseModel<byte[]>> BaoCao_TrungMaMay(string filterString);

    Task<ResponseModel<byte[]>> BaoCao_ChamCong(string filterString);

    Task<ResponseModel<byte[]>> BaoCaoDayDuLieu(IEnumerable<HosoDayLaiInfo> hosos, string maCSKCB, int type);
}
