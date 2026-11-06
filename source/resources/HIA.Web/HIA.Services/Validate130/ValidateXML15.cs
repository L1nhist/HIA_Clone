using HIA.Core.Base;
using HIA.DatabaseModel.Entites;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIA.Services.Validate130
{
    public class ValidateXML15 : BaseValidator<XML15>
    {
        private readonly IEnumerable<DMBacSi> dmBACSI;

        public ValidateXML15
        (
            IEnumerable<DMBacSi> dMBacSis,
            ILogger<dynamic> logger = null
        ) : base(logger)
        {
            dmBACSI = dMBacSis;
        }

        public override async Task<bool> Validate(XML15 xml)
        {
            //Kiem tra neu XML null thi ko chay tiep nua
            if (!await base.Validate(xml)) return false;

            //Uti.HandleCronJobLog(_logger, $"[130] Bat dau giam dinh ho so XML05", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML05)}");

            #region Kiểm tra các lỗi cơ bản
            //2418    3   XML15 MA_LK[130] Mã liên kết không được để trống
            //2419    3   XML15 MA_BN[130] Mã bệnh nhân không được để trống
            //2420    3   XML15 MA_BN[130] Mã Bệnh nhân không trùng khớp với Mã BN tại XML1
            //2421    3   XML15 HO_TEN[130] Họ tên Bệnh nhân không khớp thông tin khai báo tại XML1
            //2422    3   XML15 SO_CCCD[130] Số CCCD không khớp thông tin khai báo tại XML1
            //2423    3   XML15 PHANLOAI_LAO_VITRI[130] Phân loại BN Lao bắt buộc phải nhập
            //2434    3   XML15 PHANLOAI_LAO_VITRI[130] Phân loại BN Lao chỉ nhập 1 hoặc 2
            //2435    3   XML15 PHANLOAI_LAO_TS[130] Mã phân loại BN Lao theo tiền sử điều trị bắt buộc phải nhập
            //2436    3   XML15 PHANLOAI_LAO_TS[130] Mã phân loại BN Lao theo tiền sử điều trị phải có giá trị từ 1 đến 6
            //2437    3   XML15 PHANLOAI_LAO_HIV[130] Mã phân loại BN theo tình trạng HIV bắt buộc phải nhập
            //2438    3   XML15 PHANLOAI_LAO_HIV[130] Mã phân loại BN theo tình trạng HIV phải có giá trị từ 1 đến 3
            //2439    3   XML15 PHANLOAI_LAO_VK[130] Mã phân loại BN Lao theo bằng chứng vi khuẩn học bắt buộc phải nhập
            //2440    3   XML15 PHANLOAI_LAO_VK[130] Mã phân loại BN Lao theo bằng chứng vi khuẩn học phải có giá trị 1 hoặc 2
            //2441    3   XML15 PHANLOAI_LAO_KT[130] Mã phân loại BN lao theo trình trạng kháng thuốc bắt buộc phải nhập
            //2442    3   XML15 PHANLOAI_LAO_KT[130] Mã phân loại BN lao theo trình trạng kháng thuốc phải có giá trị từ 1 đến 6
            //2443    3   XML15 LOAI_DTRI_LAO[130] Mã loại điều trị Lao bắt buộc phải nhập
            //2444    3   XML15 LOAI_DTRI_LAO[130] Mã loại điều trị Lao phải có giá trị từ 1 đến 3
            //2445    3   XML15 NGAYBD_DTRI_LAO[130] Ngày bắt đầu điều trị Lao bắt buộc phải nhập
            //2446    3   XML15 PHACDO_DTRI_LAO[130] Phác đồ điều trị Lao bắt buộc phải nhập
            //2447    3   XML15 PHACDO_DTRI_LAO[130] Mã phác đồ điều trị Lao phải có giá trị từ 1 đến 27
            //2448    3   XML15 NGAYKT_DTRI_LAO[130] Đẫ kết thúc điều trị cho BN, phải nhập thông tin này
            //2449    3   XML15 KET_QUA_DTRI_LAO[130] Kết quả điều trị Lao bắt buộc phải nhập
            //2450    3   XML15 KET_QUA_DTRI_LAO[130] Mã kết quả điều trị Lao phải có kết quả từ 1 đến 6
            //2451    3   XML15 MA_CSKCB[130] Mã CSKCB không khớp thông tin khai báo tại XML1

            Field(x => x.MA_LK).IsEmpty().ThenError(2418);
            Field(x => x.MA_BN).IsEmpty().ThenError(2419);
            Field(x => x.MA_BN).NotValue(File.XML01.MA_BN).ThenError(2420);
            Field(x => x.HO_TEN).NotValue(File.XML01.HO_TEN).ThenError(2421);
            Field(x => x.SO_CCCD).NotValue(File.XML01.SO_CCCD).ThenError(2422);
            Field(x => x.PHANLOAI_LAO_VITRI).IsEmpty().ThenError(2423);
            Field(x => x.PHANLOAI_LAO_VITRI).NotValue(1,2).ThenError(2434);
            Field(x => x.PHANLOAI_LAO_TS).IsEmpty().ThenError(2435);
            Field(x => x.PHANLOAI_LAO_TS).NotValue(1,2,3,4,5,6).ThenError(2436);
            Field(x => x.PHANLOAI_LAO_HIV).IsEmpty().ThenError(2437);
            Field(x => x.PHANLOAI_LAO_HIV).NotValue(1,2,3).ThenError(2438);
            Field(x => x.PHANLOAI_LAO_VK).IsEmpty().ThenError(2439);
            Field(x => x.PHANLOAI_LAO_VK).NotValue(1,2).ThenError(2440);
            Field(x => x.PHANLOAI_LAO_KT).IsEmpty().ThenError(2441);
            Field(x => x.PHANLOAI_LAO_KT).NotValue(1,2,3,4,5,6).ThenError(2442);
            Field(x => x.LOAI_DTRI_LAO).IsEmpty().ThenError(2443);
            Field(x => x.LOAI_DTRI_LAO).NotValue(1,2,3).ThenError(2444);
            Field(x => x.NGAYBD_DTRI_LAO).IsEmpty().ThenError(2445);
            Field(x => x.PHACDO_DTRI_LAO).IsEmpty().ThenError(2446);
            Field(x => x.PHACDO_DTRI_LAO).NotValue(1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,17,18,19,20,21,22,23,24,25,26,27).ThenError(2447);
           // Field(x => x.NGAYKT_DTRI_LAO).IsEmpty().ThenError(2448);
            Field(x => x.KET_QUA_DTRI_LAO).IsEmpty().ThenError(2449);
            Field(x => x.KET_QUA_DTRI_LAO).NotValue(1,2,3,4,5,6).ThenError(2450);
            Field(x => x.MA_CSKCB).NotValue(File.XML01.MA_CSKCB).ThenError(2451);

            

            #endregion

            #region Kiểm tra các lỗi có liên quan đến các trường khác

            #endregion

            #region Kiểm tra các lỗi có liên quan đến các Danh mục

            #endregion

            return true;
        }
    }
}