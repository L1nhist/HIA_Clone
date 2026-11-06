using HIA.Core.Base;
using HIA.Data.Catalogs;
using HIA.Data.Records;
using Microsoft.Extensions.Logging;

namespace HIA.Services.Validate130;

public class ValidateXML10 : BaseValidator<XML10>
{
    private readonly IEnumerable<DMBacSi> dmBACSI;

    public ValidateXML10
    (
        IEnumerable<DMBacSi> dMBacSis,
        ILogger<dynamic>? logger = null
    ) : base(logger)
    {
        dmBACSI = dMBacSis;
    }

    public override async Task<bool> Validate(XML10 xml)
    {
        //Kiem tra neu XML null thi ko chay tiep nua
        if (!await base.Validate(xml)) return false;

        //Uti.HandleCronJobLog(_logger, $"[130] Bat dau giam dinh ho so XML05", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML05)}");

        #region Kiểm tra các lỗi cơ bản
        //1911	XML10	MA_LK	Mã liên kết không được để trống
        //1912	XML10	MA_LK	Mã liên kết không được lớn hơn 100 ký tự
        Field(x => x.MA_LK).IsEmpty().ThenError(1911).LengthOver(100).ThenError(1912);

        //1913	XML10	SO_SERI	Số seri chứng từ không được để trống
        //1914	XML10	SO_SERI	Số seri chứng từ không được lớn hơn 200 ký tự
        Field(x => x.SO_SERI).IsEmpty().ThenError(1913).LengthOver(200).ThenError(1914);

        //1915	XML10	SO_CT	Số chứng từ không được để trống
        //1916	XML10	SO_CT	Số chứng từ không được lớn hơn 200 ký tự
        Field(x => x.SO_CT).IsEmpty().ThenError(1915).LengthOver(200).ThenError(1916);

        //1917	XML10	SO_NGAY	Số ngày không được để trống và phải là số nguyên có giá trị từ 1 đến 999
        Field(x => x.SO_NGAY).IsEmpty().NotBetweenOrEqual(1, 99).ThenError(1917);

        //1918	XML10	DON_VI	Tên đơn vị không được để trống
        //1919	XML10	DON_VI	Tên đơn vị không được lớn hơn 1024 ký tự
        Field(x => x.DON_VI).IsEmpty().ThenError(1918).LengthOver(1024).ThenError(1919);

        //1920	XML10	CHAN_DOAN_RV	Chẩn đoán ra viện không được để trống
        Field(x => x.CHAN_DOAN_RV).IsEmpty().ThenError(1920);

        //1921	XML10	TU_NGAY	Từ ngày không được để trống và phải đúng định dạng ngày tháng yyyymmdd
        var tuNgayPassed = Field(x => x.TU_NGAY).IsEmpty().ThenError(1921);

        //1922	XML10	DEN_NGAY	Đến ngày không được để trống và phải đúng định dạng ngày tháng yyyymmdd
        var denNgayPassed = Field(x => x.DEN_NGAY).IsEmpty().ThenError(1922);

        //2302	2302	0	XML10	SO_NGAY	[130] Số ngày nghỉ không khớp thông tin DENNGAY_TUNGAY
        //double songay = File.DEN_NGAY.Value - File.TU_NGAY.Value;
        //Field(x => x.SO_NGAY).NotValue(denNgayPassed - tuNgayPassed).ThenError(2302);


        //1923	XML10	MA_TTDV	Mã thủ trưởng đơn vị không được để trống
        //1924	XML10	MA_TTDV	Mã thủ trưởng đơn vị không được lớn hơn 10 ký tự
        //Linhist changed 250926
        //Field(x => x.MA_TTDV).IsEmpty().ThenError(1923).LengthOver(10).ThenError(1924);
        Field(x => x.MA_TTDV).IsEmpty().ThenError(1923).LengthOver(255).ThenError(1924);

        //1925	XML10	TEN_BS	Tên bác sĩ không được để trống
        //1926	XML10	TEN_BS	Tên bác sĩ không được lớn hơn 255 ký tự
        Field(x => x.TEN_BS).IsEmpty().ThenError(1925).LengthOver(255).ThenError(1926);

        //1927	XML10	MA_BS	Mã bác sĩ không được để trống
        //1928	XML10	MA_BS	Mã bác sĩ không được lớn hơn 200 ký tự
        //Linhist changed 250926
        //Field(x => x.MA_BS).IsEmpty().ThenError(1927).LengthOver(200).ThenError(1928);
        Field(x => x.MA_BS).IsEmpty().ThenError(1927).LengthOver(255).ThenError(1928);

        //1929	XML10	NGAY_CT	Ngày chứng từ không được để trống và phải đúng định dạng ngày tháng yyyymmdd
        Field(x => x.NGAY_CT).IsEmpty().ThenError(1929);
        #endregion

        #region Kiểm tra các lỗi có liên quan đến các trường khác
        //2298	2298	0	XML10	MA_TTDV	[130] Mã TTDV không khớp với thông tin khai báo tại XML1	NULL
        Field(x => x.MA_TTDV).NotValue(File.XML01.MA_TTDV).ThenError(2298);
        //2303	2303	0	XML10	CHAN_DOAN_RV	[130] Chẩn đoán ra viện không khớp thông tin khai báo tại XML1	NULL
        Field(x => x.CHAN_DOAN_RV).NotValue(File.XML01.CHAN_DOAN_RV).ThenError(2303);
        #endregion

        #region Kiểm tra các lỗi có liên quan đến các Danh mục
        //2299    2299    0   XML10 MA_BS[130] Mã Bác sĩ(Mã BHXH) không tồn tại trong danh mục phê duyệt
        //2300    2300    0   XML10 TEN_BS[130] Tên Bác sĩ không tồn tại trong danh mục phê duyệt
        //2301    2301    0   XML10 MA_BS[130] Bác sĩ không có vị trí là 2,3(Trưởng khoa hoặc Phó khoa ủy quyền)
        RaiseError(!dmBACSI.Any(x => x.MA_BHXH == File.MA_BS), 2299);
        RaiseError(!dmBACSI.Any(x => x.HO_TEN == File.MA_BS && x.MA_BHXH == File.MA_BS), 2299);
        RaiseError(!dmBACSI.Any(x => x.MA_BHXH == File.MA_BS && (x.MA_VI_TRI == "2" || x.MA_VI_TRI == "3" || x.MA_VI_TRI == "5")), 2301);

        //2406	3	XML10	MA_TTDV	[130] Mã TTDV không tìm thấy trong danh mục phê duyệt
        //2407    3   XML10 MA_TTDV[130] Tồn tại mã TTDV, nhưng chưa cấu hình vị trí cho người đứng đầu(1, 4)
        if(Field(x => x.MA_TTDV).NotEmpty())
        {
            
            var allTTDVs = dmBACSI.Where(x => x.MA_BHXH == File.MA_TTDV);
            if (!RaiseError(!allTTDVs.Any(), 2406))
            {
                //Chỉ kiểm tra khi có BS trong danh mục
                RaiseError(!dmBACSI.Any(x => Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "1;4;3")), 2407);
            }
        }
        //2868	3	XML10	MA_TTDV	[3176] Mã TTDV không tồn tại trong Danh mục phê duyệt
        //2869    3   XML10 MA_TTDV[3176] Mã TTDV phải mã hóa vị trí là 1,3 hoặc 4
        if (Field(x => x.MA_TTDV).NotEmpty())
        {

            var allTTDVs = dmBACSI.Where(x => x.MA_CCHN == File.MA_TTDV);
            if (!RaiseError(!allTTDVs.Any(), 2868))
            {
                //Chỉ kiểm tra khi có BS trong danh mục
                RaiseError(!dmBACSI.Any(x => Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "1;4;3")), 2869);
            }
        }

        //2870	3	XML10	TEN_BS	[3176] Tên BS không nằm trong Danh mục phê duyệt
        //2871    3   XML10 TEN_BS[3176] Tên BS không phải là người Trưởng Khoa, Phó Khoa hoặc được Ủy Quyền
        //2872    3   XML10 MA_BS[3176] Mã BS Không có trong Danh mục phê duyệt
        //2873    3   XML10 MA_BS[3176] Mã BS không phải Trưởng Khoa, Phó khoa hay người được ủy quyền
        RaiseError(!dmBACSI.Any(x => x.MA_CCHN == File.MA_BS),2872);
        RaiseError(!dmBACSI.Any(x => x.HO_TEN == File.TEN_BS && x.MA_CCHN == File.MA_BS), 2870);
        RaiseError(!dmBACSI.Any(x => x.MA_CCHN == File.MA_BS && (x.MA_VI_TRI == "2" || x.MA_VI_TRI == "3" || x.MA_VI_TRI == "5")), 2873);
        RaiseError(!dmBACSI.Any(x => x.HO_TEN == File.MA_BS && x.MA_CCHN == File.MA_BS && (x.MA_VI_TRI == "2" || x.MA_VI_TRI == "3" || x.MA_VI_TRI == "5")), 2871);
        #endregion

        return true;
    }
}
