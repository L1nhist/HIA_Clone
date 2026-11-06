using HIA.Core.Base;
using HIA.Data.Catalogs;
using HIA.Data.Records;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;

namespace HIA.Services.Validate130;

public class ValidateXML11 : BaseValidator<XML11>
{
    private readonly IEnumerable<DMBacSi> dmBACSI;

    public ValidateXML11
    (
        IEnumerable<DMBacSi> dMBacSis,
        ILogger<dynamic>? logger = null
    ) : base(logger)
    {
        dmBACSI = dMBacSis;
    }

    public override async Task<bool> Validate(XML11 xml)
    {
        //Kiem tra neu XML null thi ko chay tiep nua
        if (!await base.Validate(xml)) return false;

        //Uti.HandleCronJobLog(_logger, $"[130] Bat dau giam dinh ho so XML05", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML05)}");

        #region Kiểm tra các lỗi cơ bản
        //1930	XML11	MA_LK	Mã liên kết không được để trống
        //1931	XML11	MA_LK	Mã liên kết không được lớn hơn 100 ký tự
        Field(x => x.MA_LK).IsEmpty().ThenError(1930).LengthOver(100).ThenError(1931);

        //1932	XML11	SO_CT	Số chứng từ không được để trống
        //1933	XML11	SO_CT	Số chứng từ không được lớn hơn 200 ký tự
        Field(x => x.SO_CT).IsEmpty().ThenError(1932).LengthOver(200).ThenError(1933);

        //1934	XML11	SO_SERI	Số seri chứng từ không được để trống
        //1935	XML11	SO_SERI	Số seri chứng từ không được lớn hơn 200 ký tự
        Field(x => x.SO_SERI).IsEmpty().ThenError(1934).LengthOver(200).ThenError(1935);

        //1936	XML11	SO_KCB	Số KCB không được để trống
        //1937	XML11	SO_KCB	Số KCB không được lớn hơn 200 ký tự
        Field(x => x.SO_KCB).IsEmpty().ThenError(1936).LengthOver(200).ThenError(1937);

        //1938	XML11	DON_VI	Tên đơn vị không được để trống
        //1939	XML11	DON_VI	Tên đơn vị không được lớn hơn 1024 ký tự
        Field(x => x.DON_VI).IsEmpty().ThenError(1938).LengthOver(1024).ThenError(1939);

        //1940	XML11	MA_BHXH	Mã BHXH không được để trống
        //1941	XML11	MA_BHXH	Mã BHXH phải gồm 10 ký tự, không được tồn tại khoảng trắng ở giữa
        Field(x => x.MA_BHXH ?? string.Empty).IsEmpty().ThenError(1940).HasSpaceAny().LengthOver(10).ThenError(1941);

        //1942	XML11	CHAN_DOAN_RV	Chẩn đoán ra viện không được để trống
        Field(x => x.CHAN_DOAN_RV).IsEmpty().ThenError(1942);

        //1943	XML11	PP_DIEUTRI	Phương pháp điều trị không được để trống
        Field(x => x.PP_DIEUTRI).IsEmpty().ThenError(1943);

        //1944	XML11	SO_NGAY_NGHI	Số ngày nghỉ không được để trống và phải số nguyên có giá trị từ 1 đến 999
        Field(x => x.SO_NGAY_NGHI).IsEmpty().NotBetweenOrEqual(1, 99).ThenError(1944);

        //1945	XML11	TU_NGAY	Từ ngày không được để trống và phải đúng định dạng ngày tháng yyyymmdd
        Field(x => x.TU_NGAY).IsEmpty().ThenError(1945);

        //1946	XML11	DEN_NGAY	Đến ngày không được để trống và phải đúng định dạng ngày tháng yyyymmdd
        Field(x => x.DEN_NGAY).IsEmpty().ThenError(1946);

        //1947	XML11	MA_TTDV	Mã thủ trưởng đơn vị không được để trống
        //1948	XML11	MA_TTDV	Mã thủ trưởng đơn vị không được lớn hơn 10 ký tự
        //Linhist changed 250926
        //Field(x => x.MA_TTDV).IsEmpty().ThenError(1947).LengthOver(10).ThenError(1948);
        Field(x => x.MA_TTDV).IsEmpty().ThenError(1947).LengthOver(255).ThenError(1948);

        //1949	XML11	MA_BS	Mã bác sĩ không được để trống
        //1950	XML11	MA_BS	Mã bác sĩ không được lớn hơn 200 ký tự
        //Linhist changed 250926
        //Field(x => x.MA_BS).IsEmpty().ThenError(1949).LengthOver(200).ThenError(1950);
        Field(x => x.MA_BS).IsEmpty().ThenError(1949).LengthOver(255).ThenError(1950);

        //1951	XML11	NGAY_CT	Ngày chứng từ không được để trống và phải đúng định dạng ngày tháng yyyymmdd
        Field(x => x.NGAY_CT).IsEmpty().ThenError(1951);
        #endregion

        #region Kiểm tra các lỗi có liên quan đến các trường khác
        //2304	2304	0	XML11	MA_THE_BHYT	[130] Mã thẻ bảo hiểm không khớp thông tin khai báo tại XML1
        //2305    2305    0   XML11 CHAN_DOAN_RV[130] Chẩn đoán ra viện không khớp thông tin khai báo tại XML1
        //2306    2306    0   XML11 PP_DIEUTRI[130] Phương pháp điều trị không khớp thông tin khai báo tại XML1
        //2587	3	XML11	SO_NGAY_NGHI	[130] Số ngày nghỉ không đúng (đến ngày - từ ngày + 1)
        Field(x=>x.MA_THE_BHYT).NotValue(File.XML01.MA_THE_BHYT).ThenError(2304);
        Field(x=>x.CHAN_DOAN_RV).NotValue(File.XML01.CHAN_DOAN_RV).ThenError(2305);
        Field(x=>x.PP_DIEUTRI).NotValue(File.XML01.PP_DIEU_TRI).ThenError(2306);
        if(File.SO_NGAY_NGHI.HasValue && File.TU_NGAY.HasValue && File.DEN_NGAY.HasValue)
        {
            TimeSpan? songay = File.DEN_NGAY - File.TU_NGAY;
            if(File.SO_NGAY_NGHI != (byte?)(songay?.Days + 1))
            {
                RaiseError(true, 2587);
            }
            
        }
        //2307    2307    0   XML11 MA_DINH_CHI_THAI[130] Mã đình chỉ thai chỉ có giá trị là 0 và 1
        //2308    2308    0   XML11 NGUYENNHAN_DINHCHI[130] Nguyên nhân đình chỉ thai bắt buộc phải nhập nếu Mã đình chỉ thai là 1
        //2309    2309    0   XML11 TUOI_THAI[130] Tuổi thai bắt buộc phải nhập khi mã đình chỉ thai là 1
        if (Field(x=>x.MA_DINH_CHI_THAI).NotEmpty())
        {
            Field(x=>x.MA_DINH_CHI_THAI).NotValue(0,1).ThenError(2307);
            if(Field(x=>x.MA_DINH_CHI_THAI).HasValue(1))
            {
                Field(x=>x.NGUYENNHAN_DINHCHI).NotEmpty().ThenError(2308);
                Field(x=>x.TUOI_THAI).NotEmpty().ThenError(2309);
            }
        }


        #endregion

        #region Kiểm tra các lỗi có liên quan đến các Danh mục
        //2408	3	XML11	MA_TTDV	[130] Mã TTDV không tìm thấy trong danh mục phê duyệt
        //2409    3   XML11 MA_TTDV[130] Tồn tại mã TTDV, nhưng chưa cấu hình vị trí cho người đứng đầu(1, 4)
        if (Field(x => x.MA_TTDV).NotEmpty())
        {

            var allTTDVs = dmBACSI.Where(x => x.MA_BHXH == File.MA_TTDV);
            if (!RaiseError(!allTTDVs.Any(), 2408))
            {
                //Chỉ kiểm tra khi có BS trong danh mục
                RaiseError(!allTTDVs.Any(x => Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "1;4;3")), 2409);
            }
        }
        //2874	3	XML11	MA_BS	[3176] Mã BS Không có trong Danh mục phê duyệt
        //2875    3   XML11 MA_BS[3176] Mã BS không phải Trưởng Khoa, Phó khoa hay người được ủy quyền
        //2876    3   XML11 MA_TTDV[3176] Mã TTDV không tồn tại trong Danh mục phê duyệt
        //2877    3   XML11 MA_TTDV[3176] Mã TTDV phải mã hóa vị trí là 1,3 hoặc 4
        if (Field(x => x.MA_TTDV).NotEmpty())
        {

            var allTTDVs = dmBACSI.Where(x => x.MA_CCHN == File.MA_TTDV);
            if (!RaiseError(!allTTDVs.Any(), 2876))
            {
                //Chỉ kiểm tra khi có BS trong danh mục
                RaiseError(!allTTDVs.Any(x => Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "1;4;3")), 2877);
            }
        }
        RaiseError(!dmBACSI.Any(x => x.MA_CCHN == File.MA_BS), 2874);
        RaiseError(!dmBACSI.Any(x => x.MA_CCHN == File.MA_BS && ((x.MA_VI_TRI ?? string.Empty).Contains("2") || (x.MA_VI_TRI ?? string.Empty).Contains("3") || (x.MA_VI_TRI ?? string.Empty).Contains("5"))), 2875);
        #endregion

        return true;
    }
}
