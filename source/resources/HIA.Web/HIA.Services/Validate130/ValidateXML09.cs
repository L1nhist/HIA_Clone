using HIA.Core.Base;
using HIA.DatabaseModel.Entites;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HIA.Services.Validate130
{
    public class ValidateXML09 : BaseValidator<XML09>
    {
        private readonly IEnumerable<DMBacSi> dmBACSI;

        public ValidateXML09
        (
            IEnumerable<DMBacSi> dMBacSis,
            ILogger<dynamic> logger = null
        ) : base(logger)
        {
            dmBACSI = dMBacSis;
        }

        public override async Task<bool> Validate(XML09 xml)
        {
            //Kiem tra neu XML null thi ko chay tiep nua
            if (!await base.Validate(xml)) return false;

            //Uti.HandleCronJobLog(_logger, $"[130] Bat dau giam dinh ho so XML05", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML05)}");

            #region Kiểm tra các lỗi cơ bản
            //1871	XML9	MA_LK	Mã liên kết không được để trống
            //1872	XML9	MA_LK	Mã liên kết không được lớn hơn 100 ký tự
            Field(x => x.MA_LK).IsEmpty().ThenError(1871).LengthOver(100).ThenError(1872);

            //1873	XML9	HO_TEN_NND	Họ tên người nuôi dưỡng không được để trống
            //1874	XML9	HO_TEN_NND	Họ tên người nuôi dưỡng không được lớn hơn 255 ký tự
            Field(x => x.HO_TEN_NND).IsEmpty().ThenError(1873).LengthOver(255).ThenError(1874);

            //1875	XML9	NGAYSINH_NND	Ngày sinh của người nuôi dưỡng không được để trống và phải đúng định dạng ngày tháng yyyymmdd
            Field(x => x.NGAYSINH_NND).IsEmpty().NotDateTimeFormat().ThenError(1875);

            //1876	XML9	MA_DANTOC_NND	Mã dân tộc của người nuôi dưỡng không được để trống
            Field(x => x.MA_DANTOC_NND).IsEmpty().ThenError(1876);

            //1877	XML9	SO_CCCD_NND	Số CCCD của người nuôi dưỡng không được để trống
            //1878	XML9	SO_CCCD_NND	Số CCCD của người nuôi dưỡng không được lớn hơn 15 ký tự, không được tồn tại khoảng trắng ở giữa
            //2293	1	XML9	CO_CCCD_NND	[130] Số CCCD người nuôi dưỡng không đúng định dạng
            Field(x => x.SO_CCCD_NND).IsEmpty().ThenError(1877).HasSpaceAny().LengthOver(15).ThenError(1878);
            RaiseError(!Uti.checkCCCD(File.SO_CCCD_NND), 2293);
            //1879	XML9	NGAYCAP_CCCD_NND	Ngày cấp CCCD của người nuôi dưỡng không được để trống và phải đúng định dạng ngày tháng yyyymmdd
            Field(x => x.NGAYCAP_CCCD_NND).IsEmpty().NotDateTimeFormat().ThenError(1879);

            //1880	XML9	NOICAP_CCCD_NND	Nơi cấp CCCD của người nuôi dưỡng không được để trống
            //1881	XML9	NOICAP_CCCD_NND	Nơi cấp CCCD của người nuôi dưỡng không được lớn hơn 1024 ký tự
            Field(x => x.NOICAP_CCCD_NND).IsEmpty().ThenError(1880).LengthOver(1024).ThenError(1881);


            //1882	XML9	NOI_CU_TRU_NND	Nơi cư trú của người nuôi dưỡng không được để trống
            //1883	XML9	NOI_CU_TRU_NND	Nơi cư trú của người nuôi dưỡng không được lớn hơn 1024 ký tự
            Field(x => x.NOI_CU_TRU_NND).IsEmpty().ThenError(1882).LengthOver(1024).ThenError(1883);

            //1884	XML9	MA_QUOCTICH	Mã quốc tịch của người nuôi dưỡng không được để trống
            Field(x => x.MA_QUOCTICH).IsEmpty().ThenError(1884);

            //1885	XML9	MATINH_CU_TRU	Mã tỉnh của người nuôi dưỡng không được để trống
            Field(x => x.MATINH_CU_TRU).IsEmpty().ThenError(1885);

            //1886	XML9	MAHUYEN_CU_TRU	Mã huyện của người nuôi dưỡng không được để trống
            Field(x => x.MAHUYEN_CU_TRU).IsEmpty().ThenError(1886);

            //1887	XML9	MAXA_CU_TRU	Mã xã của người nuôi dưỡng không được để trống
            Field(x => x.MAXA_CU_TRU).IsEmpty().ThenError(1887);

            //1888	XML9	MA_THE_TAM	Mã thẻ tạm không được để trống
            //1954	XML9	MA_THE_TAM	Mã thẻ tạm không được lớn hơn 15 ký tự, không được tồn tại khoảng trắng ở giữa
            //2361	0	XML9	MA_THE_TAM	[130] Mã thẻ tạm không đúng định dạng

            //Field(x => x.MA_THE_TAM).IsEmpty().ThenError(1888).HasSpaceAny().LengthOver(15).ThenError(1954); Code nay chay sai
            Field(x => x.MA_THE_TAM).IsEmpty().ThenError(1888);
            if (Field(x => x.MA_THE_TAM).NotEmpty())
            {
                if (File.MA_THE_TAM.Trim().Length < 15)
                {
                    RaiseError(true, 1954);
                }
                else
                {
                    RaiseError(!File.MA_THE_TAM.Contains("TE1"), 2361);
                }
            }
            //1889	XML9	GIOI_TINH_CON	Giới tính của con không được để trống và phải có giá trị từ 1 đến 3
            Field(x => x.GIOI_TINH_CON).IsEmpty().NotBetweenOrEqual(1, 3).ThenError(1889);

            //1890	XML9	SO_CON	Số con không được để trống và phải có giá trị từ 1 đến 99
            Field(x => x.SO_CON).IsEmpty().NotBetweenOrEqual(1, 99).ThenError(1890);

            //1891	XML9	LAN_SINH	Lần sinh không được để trống và phải có giá trị từ 1 đến 99
            Field(x => x.LAN_SINH).IsEmpty().NotBetweenOrEqual(1, 99).ThenError(1891);

            //1892	XML9	SO_CON_SONG	Số con đang sống không được để trống và phải có giá trị từ 0 đến 99
            Field(x => x.SO_CON_SONG).IsEmpty().NotBetweenOrEqual(1, 99).ThenError(1892);

            //1893	XML9	CAN_NANG_CON	Cân nặng con không được để trống và phải là số nguyên lớn hơn 0
            Field(x => x.CAN_NANG_CON).IsEmpty().IsUnder(0).ThenError(1893);

            //1894	XML9	NGAY_SINH_CON	Ngày sinh con không được để trống và phải đúng định dạng ngày tháng yyyymmddHHMM
            Field(x => x.NGAY_SINH_CON).IsEmpty().NotDateTimeFormat().ThenError(1894);

            //1895	XML9	NOI_SINH_CON	Nơi sinh con không được để trống
            //1896	XML9	NOI_SINH_CON	Nơi sinh con không được lớn hơn 1024 ký tự
            Field(x => x.NOI_SINH_CON).IsEmpty().ThenError(1895).LengthOver(1024).ThenError(1896);

            //1897	XML9	TINH_TRANG_CON	Tình trạng con không được để trống
            //2197	4	XML9	TINH_TRANG_CON	[130] Tình trang con bắt buộc phải nhập thông tin
            Field(x => x.TINH_TRANG_CON).IsEmpty().ThenError(1897).IsOnlyWhitespace().LengthUnder(3).ThenError(2197);

            //1898	XML9	SINHCON_PHAUTHUAT	Sinh con phẫu thuật không được để trống và phải có giá trị là 0 hoặc 1
            Field(x => x.SINHCON_PHAUTHUAT).IsEmpty().NotValue(0, 1).ThenError(1898);

            //1899	XML9	SINHCON_DUOI32TUAN	Sinh con dưới 32 tuần không được để trống và phải có giá trị là 0 hoặc 1
            Field(x => x.SINHCON_DUOI32TUAN).IsEmpty().NotValue(0, 1).ThenError(1899);

            //2198	4	XML9	GHI_CHU	[130] Ghi chú không được để trống
            //Field(x => x.GHI_CHU).IsEmpty().ThenError(2198);

            //1900	XML9	NGUOI_DO_DE	Họ tên người đỡ đẻ không được để trống
            //1901	XML9	NGUOI_DO_DE	Họ tên người đỡ đẻ không được lớn hơn 255 ký tự
            Field(x => x.NGUOI_DO_DE).IsEmpty().ThenError(1900).LengthOver(255).ThenError(1901);

            //1902	XML9	NGUOI_GHI_PHIEU	Họ tên người ghi chứng từ không được để trống
            //1903	XML9	NGUOI_GHI_PHIEU	Họ tên người ghi chứng từ không được lớn hơn 255 ký tự
            Field(x => x.NGUOI_GHI_PHIEU).IsEmpty().ThenError(1902).LengthOver(255).ThenError(1903);

            //1904	XML9	NGAY_CT	Ngày chứng từ không được để trống và phải đúng định dạng ngày tháng yyyymmdd
            Field(x => x.NGAY_CT).IsEmpty().NotDateTimeFormat().ThenError(1904);

            //1905	XML9	SO	Số chứng từ không được để trống
            //1906	XML9	SO	Số chứng từ không được lớn hơn 200 ký tự
            Field(x => x.SO).IsEmpty().ThenError(1905).LengthOver(200).ThenError(1906);

            //1907	XML9	QUYEN_SO	Quyển số không được để trống
            //1908	XML9	QUYEN_SO	Quyển số không được lớn hơn 200 ký tự
            Field(x => x.QUYEN_SO).IsEmpty().ThenError(1907).LengthOver(200).ThenError(1908);

            //1909	XML9	MA_TTDV	Mã thủ trưởng đơn vị không được để trống
            //1910	XML9	MA_TTDV	Mã thủ trưởng đơn vị không được lớn hơn 10 ký tự
            //Linhist changed 250926
            //Field(x => x.MA_TTDV).IsEmpty().ThenError(1909).LengthOver(10).ThenError(1910);
            Field(x => x.MA_TTDV).IsEmpty().ThenError(1909).LengthOver(255).ThenError(1910);

            //4017	3	XML9	NGAYCAP_CCCD_NND	[3176] Ngày Cấp căn cước công dân người giám hộ, nuôi dưỡng không được lớn hơn thời điểm hiện tại (Thời điểm giám định hồ sơ)
            //4018    3   XML9 NGAYCAP_CCCD_NND[3176] Ngày Cấp căn cước công dân của người giám hộ, người nuôi dưỡng không được lớn hơn ngày ra viện
            if(Field(x => x.NGAYCAP_CCCD_NND).NotEmpty())
            {
                if(File.NGAYCAP_CCCD_NND.Value>DateTime.Now)
                {
                    RaiseError(true, 4017);
                }
                if(File.NGAYCAP_CCCD_NND.Value>File.XML01.NGAY_RA)
                {
                    RaiseError(true, 4018);
                }
            }


            #endregion

            #region Kiểm tra các lỗi có liên quan đến các trường khác
            //2200	4	XML9	MA_TTDV	[130] Mã số Định danh không khớp với thông tin khai báo tại XML1
            Field(x => x.MA_TTDV).NotValue(File.XML01.MA_TTDV).ThenError(2200);


            #endregion

            #region Kiểm tra các lỗi có liên quan đến các Danh mục
            if (Field(x => x.MA_TTDV).NotEmpty())
            {
                //2404	3	XML9	MA_TTDV	[130] Mã TTDV không tìm thấy trong danh mục phê duyệt
                //2405    3   XML9 MA_TTDV[130] Tồn tại mã TTDV, nhưng chưa cấu hình vị trí cho người đứng đầu(1, 4)
                var allTTDVs = dmBACSI.Where(x => x.MA_BHXH == File.MA_TTDV);
                if (!RaiseError(!allTTDVs.Any(), 2404))
                {
                    //Chỉ kiểm tra khi có BS trong danh mục
                    RaiseError(!dmBACSI.Any(x => Uti.HCodeContain(x.MA_VI_TRI, "1;4;3")), 2405);
                }
            }
            //2704	3	XML9	MA_TTDV	[3176] Mã TTDV phải mã hóa vị trí là 1,3 hoặc 4
            //2865	3	XML3	MA_TTDV	[3176] Mã TTDV không tồn tại trong Danh mục phê duyệt
            if (Field(x => x.MA_TTDV).NotEmpty())
            {
                
                var allTTDVs = dmBACSI.Where(x => x.MA_CCHN == File.MA_TTDV);
                if (!RaiseError(!allTTDVs.Any(), 2865))
                {
                    //Chỉ kiểm tra khi có BS trong danh mục
                    RaiseError(!dmBACSI.Any(x => Uti.HCodeContain(x.MA_VI_TRI, "1;4;3")), 2704);
                }
            }
            #endregion

            return true;
        }
    }
}