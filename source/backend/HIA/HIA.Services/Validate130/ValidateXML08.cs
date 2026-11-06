using HIA.Core.Base;
using HIA.Data.Catalogs;
using HIA.Data.Records;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;

namespace HIA.Services.Validate130;

public class ValidateXML08 : BaseValidator<XML08>
{
    private readonly IEnumerable<DMBacSi> dmBACSI;

    public ValidateXML08
    (
        IEnumerable<DMBacSi> dMBacSis,
        ILogger<dynamic>? logger = null
    ) : base(logger)
    {
        dmBACSI = dMBacSis;
    }

    public override async Task<bool> Validate(XML08 xml)
    {
        //Kiem tra neu XML null thi ko chay tiep nua
        if (!await base.Validate(xml)) return false;

        //Uti.HandleCronJobLog(_logger, $"[130] Bat dau giam dinh ho so XML05", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML05)}");

        #region Kiểm tra các lỗi cơ bản
        //1858	XML8	MA_LK	Mã liên kết không được để trống
        //1859	XML8	MA_LK	Mã liên kết không được lớn hơn 100 ký tự
        Field(x => x.MA_LK).IsEmpty().ThenError(1858).LengthOver(100).ThenError(1859);

        //1953	XML8	MA_LOAI_KCB	Mã loại KCB không được để trống và phải là số nguyên có giá trị từ 1 đến 10
        //2165    4   XML8 MA_LOAI_KCB[130] Mã loại KCB phải có giá trị là 02, 03 hoặc 04
        Field(x => x.MA_LOAI_KCB ?? string.Empty).IsEmpty().Not_LOAI_KCB().ThenError(1953);
        RaiseError(!Uti.CodeContain((File.MA_LOAI_KCB ?? string.Empty), "02;03;04"), 2165);
        //2702	3	XML8	MA_LOAI_KCB	[3176] Mã loại KCB phải mã hóa theo 02;03;04;05;06;08;09
        RaiseError(!Uti.CodeContain((File.MA_LOAI_KCB ?? string.Empty), "02;03;04;05;06;08;09"), 2702);
        //1860	XML8	NGAY_VAO	Ngày vào không được để trống và phải đúng định dạng ngày tháng yyyymmddHHMM
        Field(x => x.NGAY_VAO).IsEmpty().ThenError(1860);

        //1861	XML8	NGAY_RA	Ngày ra không được để trống và phải đúng định dạng ngày tháng yyyymmddHHMM
        Field(x => x.NGAY_RA).IsEmpty().ThenError(1861);

        //1862	XML8	CHAN_DOAN_VAO	Chẩn đoán vào viện không được để trống
        Field(x => x.CHAN_DOAN_VAO).IsEmpty().ThenError(1862);

        //1863	XML8	CHAN_DOAN_RV	Chẩn đoán ra viện không được để trống
        Field(x => x.CHAN_DOAN_RV).IsEmpty().ThenError(1863);

        //1864	XML8	QT_BENHLY	Quá trình bệnh lý không được để trống
        Field(x => x.QT_BENHLY).IsEmpty().ThenError(1864);

        //1865	XML8	TOMTAT_KQ	Tóm tắt kết quả không được để trống
        //2291    1   XML8 TOMTAT_KQ[130] Tóm tắt kết quả lớn hơn 2000 ký tự
        //2342	0	XML8	TOMTAT_KQ	[130] Nội dung tóm tắt kết quả quá ngắn, đề nghị xem lại
        Field(x => x.TOMTAT_KQ).IsEmpty().ThenError(1865);
        Field(x => x.TOMTAT_KQ).LengthOver(4000).ThenError(2291);
        Field(x => x.TOMTAT_KQ).LengthUnder(5).ThenError(2342);


        //1866	XML8	PP_DIEUTRI	Phương pháp điều trị không được để trống
        Field(x => x.PP_DIEUTRI).IsEmpty().ThenError(1866);

        //1867	XML8	KET_QUA_DTRI	Kết quả điều trị không được để trống và phải là số nguyên có giá trị từ 1 đến 8
        Field(x => x.KET_QUA_DTRI).IsEmpty().NotBetweenOrEqual(1, 8).ThenError(1867);

        //1868	XML8	MA_TTDV	Mã thủ trưởng đơn vị không được để trống
        //1869	XML8	MA_TTDV	Mã thủ trưởng đơn vị không được lớn hơn 10 ký tự
        //Linhist changed 250926
        //Field(x => x.MA_TTDV).IsEmpty().ThenError(1868).LengthOver(10).ThenError(1869);
        Field(x => x.MA_TTDV).IsEmpty().ThenError(1868).LengthOver(255).ThenError(1869);

        //1870	XML8	NGAY_CT	Ngày chứng từ không được để trống và phải đúng định dạng ngày tháng yyyymmdd
        Field(x => x.NGAY_CT).IsEmpty().ThenError(1870);
        #endregion

        #region Kiểm tra các lỗi có liên quan đến các trường khác
        //2166	4	XML8	MA_LOAI_KCB	[130] Mã loại KCB không trùng với thông tin nhập tại XML1
        Field(x => x.MA_LOAI_KCB).NotValue(File.XML01.MA_LOAI_KCB).ThenError(2166);

        //2167	4	XML8	NGAY_VAO	[130] Ngày vào không trùng với thông tin khai báo tại XML1
        //2168    4   XML8 NGAY_RA[130] Ngày ra không trùng với thông tin khai báo tại XML1
        Field(x => x.NGAY_RA).NotValue(File.XML01.NGAY_RA).ThenError(2168);
        Field(x=>x.NGAY_VAO).NotValue(File.XML01.NGAY_VAO).ThenError(2167);

        //2173    4   XML8 NGAY_CT[130] Ngày chứng tử không trùng với ngày ra viện
        //if (!Field(x => x.NGAY_CT).IsEmpty())
        //{
        //    Field(x => x.NGAY_CT).NotValue(File.XML01.NGAY_RA.Value.Date).ThenError(2173);
        //}
        //2174	4	XML8	MA_THE_TAM	[130] Mã thẻ tạm không trùng thông tin khai báo tại XML1
        if (Field(x => x.MA_THE_TAM).NotEmpty())
        {
            Field(x => x.MA_THE_TAM).NotValue(File.XML01.MA_THE_BHYT).ThenError(2174);
        }
        #endregion

        #region Kiểm tra các lỗi có liên quan đến các Danh mục

        //2171	4	XML8	MA_TTDV	[130] Mã định danh y tế người đứng đầu không tồn tại trong Danh mục phê duyệt
        //2172    4   XML8 MA_TTDV[130] Mã định danh y tế phải có vị trí 1,2 hoặc 3
        //RaiseError(!dmBACSI.Any(x => x.MA_BHXH == File.MA_TTDV), 2171);
        //RaiseError(!dmBACSI.Any(x => !string.IsNullOrEmpty(x.VI_TRI) && Uti.CodeContain("1;2;3", File.MA_TTDV) && x.MA_BHXH == File.MA_TTDV), 2172);
        if (Field(x => x.MA_TTDV).NotEmpty())
        {
            var allTTDVs = dmBACSI.Where(x => x.MA_BHXH == File.MA_TTDV);
            if (!RaiseError(!allTTDVs.Any(), 2171))
            {
                //Chỉ kiểm tra khi có BS trong danh mục
                RaiseError(!dmBACSI.Any(x => Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "1;4;3")), 2172);
            }
        }
        //2703	3	XML8	MA_TTDV	[3176] Mã TTDV phải mã hóa theo CCHN và nằm trong DM phê duyệt
        //2704    3   XML3 MA_TTDV[3176] Mã TTDV phải mã hóa vị trí là 1,3 hoặc 4
            if (Field(x => x.MA_TTDV).NotEmpty())
        {
            var allTTDVs = dmBACSI.Where(x => x.MA_CCHN == File.MA_TTDV);
            if (!RaiseError(!allTTDVs.Any(), 2703))
            {
                //Chỉ kiểm tra khi có BS trong danh mục
                RaiseError(!dmBACSI.Any(x => Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "1;4;3")), 2704);
            }
        }
        #endregion

        return true;
    }
}
