using HIA.Core.Base;
using HIA.DatabaseModel.Entites;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HIA.Services.Validate130
{
    public class ValidateXML14 : BaseValidator<XML14>
    {
        private readonly ILogger<dynamic> _logger;

        private readonly IEnumerable<DMBacSi> dmBACSI;
        private readonly IEnumerable<DMYHDTICD10> dmYHCT;


        public ValidateXML14
        (
            IEnumerable<DMYHDTICD10> dMYHCT,
            IEnumerable<DMBacSi> dMBacSis,
            ILogger<dynamic> logger = null
        ) : base(logger)
        {
            _logger = logger;
            dmYHCT = dMYHCT;
            dmBACSI = dMBacSis;
        }

        public override async Task<bool> Validate(XML14 xml)
        {
            //Kiem tra neu XML null thi ko chay tiep nua
            if (!await base.Validate(xml)) return false;

            //Uti.HandleCronJobLog(_logger, $"[130] Bat dau giam dinh ho so XML05", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML05)}");

            #region Kiểm tra các lỗi cơ bản
            //2226	4	XML14	MA_LK	[130] Mã LK không được để trống
            //2227    4   XML14 MA_LK[130] Mã LK không trùng với nội dung Mã LK khai báo tại XML1
            Field(x=>x.MA_LK).IsEmpty().ThenError(2226);
            //Field(x=>x.MA_TTDV).NotValue(File.XML01.MA_LK).ThenError(2227);
            Field(x => x.MA_LK).NotValue(File.XML01.MA_LK).ThenError(2227);

            //2228    4   XML14 SO_GIAYHEN_KL[130] Số giấy hẹn không được để trống hoặc lớn hơn 200 ký tự
            //Linhist changed 250926
            //Field(x=>x.SO_GIAYHEN_KL).IsEmpty().LengthOver(200).ThenError(2228);
            Field(x=>x.SO_GIAYHEN_KL).IsEmpty().LengthOver(50).ThenError(2228);

            //2229	4	XML14	MA_CSKCB  [130] Mã CSKCB không được để trống
            Field(x => x.MA_CSKCB).IsEmpty().ThenError(2229);

            //2230	4	XML14	HO_TEN	[130] Họ Tên không được để trống
            //2231    4   XML14 HO_TEN[130] Thông tin họ tên không khớp với nội dung khai báo tại XML1
            Field(x=>x.HO_TEN).IsEmpty().ThenError(2230);
            Field(x=>x.HO_TEN).NotValue(File.XML01.HO_TEN).ThenError(2231);
            #endregion

            #region Kiểm tra các lỗi có liên quan đến các trường khác
            //2232	4	XML14	NGAY_SINH	[130] Ngày sinh không khớp với thông tin khai báo tại XML1
            //2233    4   XML14 GIOI_TINH[130] Giới tính không khớp với thông tin khai báo tại XML1
            //2234    4   XML14 DIA_CHI[130] Địa chỉ không khớp với thông tin khai báo tại XML1
            //2235    4   XML14 MA_THE_BHYT[130] Thông tin thẻ BHYT không khớp với thông tin khai báo tại XML1
            //2236    4   XML14 GT_THE_DEN[130] Giá trị thẻ đến không khớp với thông tin khai báo tại XML1
            //2237    4   XML14 NGAY_VAO[130] Giá trị ngày vào không khớp với thông tin khai báo tại XML1
            //2238    4   XML14 NGAY_VAO_NOI_TRU[130] Ngày vào nội trú không khớp với thông tin khai báo tại XML1
            //2239    4   XML14 NGAY_RA[130] Giá trị ngày ra không khớp với thông tin khai báo tại XML1
            //2240    4   XML14 NGAY_HEN_KL[130] Ngày hẹn khám lại bắt buộc phải nhập
            //2241    4   XML14 NGAY_HEN_KL[130] Ngày hẹn khám lại đang nhỏ hơn ngày ra viện
            //2242    4   XML14 CHAN_DOAN_RV[130] Chuẩn đoán ra viện không khớp với thông tin khai báo tại XML1
            //2243    4   XML14 MA_BENH_CHINH[130] Mã bệnh chính không khớp với thông tin khai báo tại XML1
            //2244    4   XML14 MA_BENH_KT[130] Mã bệnh kèm theo không khớp với thông tin khai báo tại XML1
            //2245    4   XML14 MA_BENH_YHCT[130] Mã bệnh YHCT không khớp với thông tin khai báo tại XML1
            //2246    4   XML14 MA_DOITUONG_KCB[130] Mã đối tượng KCB không khớp với thông tin khai báo tại XML1
            //2247    4   XML14 MA_BAC_SI[130] Mã Bác sĩ bắt buộc phải nhập
            //2249    4   XML14 MA_TTDV[130] Mã số định danh người có thẩm quyền bắt buộc phải nhập
            //2250    4   XML14 MA_TTDV[130] Mã số định danh không khớp với thông tin khai báo tại XML1
            //2251    4   XML14 NGAY_CT[130] Ngày chứng từ không được nhỏ hơn ngày vào

            //if (Field(x => x.NGAY_SINH).HasValue())
            //{
            //    Field(x => x.NGAY_SINH).NotValue(File.XML01.NGAY_SINH).ThenError(2232);
            //}
            Field(x => x.NGAY_SINH).NotValue(File.XML01.NGAY_SINH).IgnoreEmpty().ThenError(2232);

            if (Field(x => x.GIOI_TINH).NotEmpty()) { Field(x => x.GIOI_TINH).NotValue(File.XML01.GIOI_TINH).ThenError(2233); }
            if (Field(x => x.DIA_CHI).NotEmpty()) { Field(x => x.DIA_CHI).NotValue(File.XML01.DIA_CHI).ThenError(2224); }

            if (Field(x => x.MA_THE_BHYT).NotEmpty()) { Field(x => x.MA_THE_BHYT).NotValue(File.XML01.MA_THE_BHYT).ThenError(2235); }
            if (Field(x => x.GT_THE_DEN).NotEmpty()) { Field(x => x.GT_THE_DEN).NotValue(File.XML01.GT_THE_DEN).ThenError(2236); }
            if (Field(x => x.NGAY_VAO).NotEmpty()) { Field(x => x.NGAY_VAO).NotValue(File.XML01.NGAY_VAO).ThenError(2237); }
            if (Field(x => x.NGAY_VAO_NOI_TRU).NotEmpty()) { Field(x => x.NGAY_VAO_NOI_TRU).NotValue(File.XML01.NGAY_VAO_NOI_TRU).ThenError(2238); }
            if (Field(x => x.NGAY_RA).NotEmpty()) { Field(x => x.NGAY_RA).NotValue(File.XML01.NGAY_RA).ThenError(2239); }
            Field(x => x.NGAY_HEN_KL).IsEmpty().ThenError(2240);
            if (Field(x => x.NGAY_HEN_KL).NotEmpty()) { Field(x => x.NGAY_HEN_KL).IsUnder(File.XML01.NGAY_RA).ThenError(2241); }
            if (Field(x => x.CHAN_DOAN_RV).NotEmpty()) { Field(x => x.CHAN_DOAN_RV).NotValue(File.XML01.CHAN_DOAN_RV).ThenError(2242); }
            if (Field(x => x.MA_BENH_CHINH).NotEmpty()) { Field(x => x.MA_BENH_CHINH).NotValue(File.XML01.MA_BENH_CHINH).ThenError(2243); }
            if (Field(x => x.MA_BENH_KT).NotEmpty()) { Field(x => x.MA_BENH_KT).NotValue(File.XML01.MA_BENH_KT).ThenError(2244); }
            if (Field(x => x.MA_BENH_YHCT).NotEmpty()) { Field(x => x.MA_BENH_YHCT).NotValue(File.XML01.MA_BENH_YHCT).ThenError(2245); }
            if (Field(x => x.MA_DOITUONG_KCB).NotEmpty()) { Field(x => x.MA_DOITUONG_KCB).NotValue(File.XML01.MA_DOITUONG_KCB).ThenError(2246); }
            Field(x => x.MA_BAC_SI).IsEmpty().ThenError(2247);
            Field(x => x.MA_TTDV).IsEmpty().ThenError(2249);
            //2366	0	XML14	MA_TTDV	[130] Mã TTDV không trùng khớp với thông tin khai báo tại XML1
            if (Field(x=>x.MA_TTDV).NotEmpty())
            {
                Field(x=>x.MA_TTDV).NotValue(File.XML01.MA_TTDV).ThenError(2366);
            }    
            if (Field(x => x.MA_TTDV).NotEmpty()) {Field(x => x.MA_TTDV).NotValue(File.XML01.MA_TTDV).ThenError(2250);}
            if (Field(x => x.NGAY_CT).NotEmpty()) 
            { 
                if(File.NGAY_CT.Value.Date<File.XML01.NGAY_VAO.Value.Date)
                {
                    RaiseError(true, 2251);
                }
            }
            #endregion

            #region Kiểm tra các lỗi có liên quan đến các Danh mục
            //2490	3	XML14	MA_BENH_YHCT	[13] Không tồn mã Mã Bệnh YHCT trong Quyết định số 6061/QĐ-BYT ngày 29/12/2017 của Bộ trưởng Bộ Y tế
            if(Field(x=>x.MA_BENH_YHCT).NotEmpty())
            {
                var mabenhs = File.MA_BENH_YHCT.Split(";");
                foreach (var item in mabenhs)
                {
                    if (!dmYHCT.Any(x => x.YHDTCode == item))
                    {
                        RaiseError(true, 2490);
                    }
                }
            }    

            //2248    4   XML14 MA_BAC_SI[130] Mã bác sĩ không tồn tại trong DM được phê duyệt
            RaiseError(!dmBACSI.Any(x => x.MA_CCHN == File.MA_BAC_SI), 2248);

            //2412	3	XML14	MA_TTDV	[130] Mã TTDV không tìm thấy trong danh mục phê duyệt
            //2413    3   XML14 MA_TTDV[130] Tồn tại mã TTDV, nhưng chưa cấu hình vị trí cho người đứng đầu(1, 4)
            if (Field(x => x.MA_TTDV).NotEmpty())
            {

                var allTTDVs = dmBACSI.Where(x => x.MA_BHXH == File.MA_TTDV);
                if (!RaiseError(!allTTDVs.Any(), 2412))
                {
                    //Chỉ kiểm tra khi có BS trong danh mục
                    RaiseError(!dmBACSI.Any(x => Uti.HCodeContain(x.MA_VI_TRI, "1;4;3")), 2413);
                }
            }
            //2883	3	XML14	MA_TTDV	[3176] Mã TTDV không tồn tại trong Danh mục phê duyệt
            //2284    3   XML14 MA_TTDV[3176] Mã TTDV phải mã hóa vị trí là 1,3 hoặc 4
            if (Field(x => x.MA_TTDV).NotEmpty())
            {

                var allTTDVs = dmBACSI.Where(x => x.MA_CCHN == File.MA_TTDV);
                if (!RaiseError(!allTTDVs.Any(), 2883))
                {
                    //Chỉ kiểm tra khi có BS trong danh mục
                    RaiseError(!dmBACSI.Any(x => Uti.HCodeContain(x.MA_VI_TRI, "1;4;3")), 2884);
                }
            }
            #endregion

            return true;
        }
    }
}