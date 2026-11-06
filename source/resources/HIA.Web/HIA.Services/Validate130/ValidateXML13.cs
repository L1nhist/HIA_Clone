using HIA.Core.Base;
using HIA.DatabaseModel.Entites;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HIA.Services.Validate130
{
    public class ValidateXML13 : BaseValidator<XML13>
    {
        private readonly ILogger<dynamic> _logger;

        private readonly IEnumerable<DMBacSi> dmBACSI;
        private readonly IEnumerable<DMYHDTICD10> dmYHCT;

        public ValidateXML13
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

        public override async Task<bool> Validate(XML13 xml)
        {
            //Kiem tra neu XML null thi ko chay tiep nua
            if (!await base.Validate(xml)) return false;



            #region Kiểm tra các lỗi cơ bản
            ////2253	0	XML13	MA_LK	[130] Mã liên kết bắt buộc phải nhập
            //2254    0   XML13 SO_HOSO[130] Số hồ sơ bắt buộc phải nhập
            //2255    0   XML13 SO_CHUYENTUYEN[130] Số chuyển tuyến bắt buộc phải nhập
            //2256    0   XML13 GIAY_CHUYEN_TUYEN[130] Giấy chuyển tuyến bắt buộc phải nhập
            //2257    1   XML13 MA_CSKCB[130] Mã cơ sở khám chữa bệnh bắt buộc phải nhập
            
            //2260    1   XML13 HO_TEN[130] Họ tên bắt buộc phải nhập
            
            //2274    1   XML13 DAU_HIEU_LS[130] Dấu hiệu lâm sàng bắt buộc phải nhập
            
            //2276    1   XML13 QT_BENHLY[130] Quá trình quản lý và diễn biến lâm sàng bắt buộc phải nhập
            //2277    1   XML13 PP_DIEUTRI[130] Phương pháp điều trị bắt buộc phải nhập
            
            //2281    1   XML13 TEN_DICH_VU[130] Tên dịch vụ kỹ thuật bắt buộc phải nhập
            
            //2284    1   XML13 MA_LYDO_CT[130] Mã lý do chuyển tuyến bắt buộc phải nhập
            //2285    1   XML13 MA_LYDO_CT[130] Mã lsy do chuyển tuyến phải có giá trị 1,2 hoặc 3
            //2286    1   XML13 HUONG_DIEU_TRI[130] Hướng điều trị bắt buộc phải nhập
            //2287    1   XML13 PHUONGTIEN_VC[130] Phương tiện vận chuyển bắt buộc phải nhập
            //2288    1   XML13 MA_BAC_SI[130] Mã bác sĩ bắt buộc phải nhập
            
            
            Field(x=>x.MA_LK).IsEmpty().ThenError(2253);
            Field(x=>x.SO_HOSO).IsEmpty().ThenError(2254);
            Field(x=>x.SO_CHUYENTUYEN).IsEmpty().ThenError(2255);
            Field(x=>x.GIAY_CHUYEN_TUYEN).IsEmpty().ThenError(2256);
            Field(x=>x.MA_CSKCB).IsEmpty().ThenError(2257);
            Field(x=>x.HO_TEN).IsEmpty().ThenError(2260);
            Field(x=>x.DAU_HIEU_LS).IsEmpty().ThenError(2274);
            Field(x=>x.QT_BENHLY).IsEmpty().ThenError(2276);
            Field(x=>x.PP_DIEUTRI).IsEmpty().ThenError(2277);
            Field(x=>x.TEN_DICH_VU).IsEmpty().ThenError(2281);
            Field(x=>x.MA_LYDO_CT).IsEmpty().ThenError(2284);
            Field(x=>x.MA_LYDO_CT).NotValue(1,2,3).ThenError(2285);
            Field(x=>x.HUONG_DIEU_TRI).IsEmpty().ThenError(2286);
            if (File.HUONG_DIEU_TRI.Trim().Length == 0) RaiseError(true, 2286);
            Field(x=>x.PHUONGTIEN_VC).IsEmpty().ThenError(2287);
            Field(x=>x.MA_BAC_SI).IsEmpty().ThenError(2288);


            #endregion

            #region Kiểm tra các lỗi có liên quan đến các trường khác
            //2258    1   XML13 MA_NOI_DI[130] Mã nơi đi không khớp với thông tin khai báo tại XML1
            //2259    1   XML13 MA_NOI_DEN[130] Mã nơi đến không khớp với thông tin khai báo tại XML1
            //2261    1   XML13 HO_TEN[130] Họ tên không khớp với thông tin khai báo tại XML1
            //2262    1   XML13 GIAY_CHUYEN_TUYEN[130] Giấy chuyển tuyến không khớp với thông tin khai báo tại XML1
            //2263    1   XML13 NGAY_SINH[130] Ngày sinh không khớp thông tin khai báo tại XML1
            //2264    1   XML13 GIOI_TINH[130] Giới tính không khớp thông tin khai báo tại XML1
            //2265    1   XML13 MA_QUOCTICH[130] Mã quốc tích không khớp thông tin khai báo tại XML1
            //2266    1   XML13 MA_DANTOC[130] Mã dân tộc không khớp thông tin khai báo tại XML1
            //2267    1   XML13 MA_NGHE_NGHIEP[130] Mã nghề nghiệp không khớp thông tin khai báo tại XML1
            //2268    1   XML13 DIA_CHI[130] Địa chỉ không khớp thông tin khai báo tại XML1
            //2269    1   XML13 MA_THE_BHYT[130] Mã thẻ BHYT không khớp thông tin khai báo tại XML1
            //2270    1   XML13 GT_THE_DEN[130] Giá trị thẻ đến không khớp thông tin khai báo tại XML1
            //2271    1   XML13 NGAY_VAO[130] Ngày vào không khớp thông tin khai báo tại XML1
            //2272    1   XML13 NGAY_VAO_NOI_TRU[130] Ngày vào nội trú không khớp với thông tin khai báo tại XML1
            //2273    1   XML13 NGAY_RA[130] Ngày ra không khớp thông tin khai báo tại XML1
            //2275    1   XML13 CHUAN_DOAN_RV[130] Chuẩn đoán ra viện không khớp thông tin khai báo tại XML1
            //2278    1   XML13 MA_BENH_CHINH[130] Mã bệnh chính không khớp với thông tin khai báo tại XML1
            //2279    1   XML13 MA_BENH_KT[130] Mã bệnh kèm theo không khớp thông tin khai báo tại XML1
            //2280    1   XML13 MA_BENH_YHCT[130] Mã bệnh y học cổ truyền không khớp thông tin khai báo tại XML1
            //2282    1   XML13 PP_DIEU_TRI[130] PP Điều trị không khớp thông tin khai báo tại XML1
            //2283    1   XML13 MA_LOAI_RV[130] Mã loại ra viện không khớp thông tin khai báo tại XML1
            //2290    1   XML13 MA_TTDV[130] Mã TTDV không khớp thông tin khai báo tại XML1
            if(Field(x=>x.MA_NOI_DI).NotEmpty()) { Field(x=>x.MA_NOI_DI).NotValue(File.XML01.MA_NOI_DI).ThenError(2258); }
            if(Field(x=>x.MA_NOI_DEN).NotEmpty()) { Field(x=>x.MA_NOI_DEN).NotValue(File.XML01.MA_NOI_DEN).ThenError(2259); }
            if(Field(x=>x.HO_TEN).NotEmpty()) { Field(x=>x.HO_TEN).NotValue(File.XML01.HO_TEN).ThenError(2261); }
            //if(Field(x=>x.GIAY_CHUYEN_TUYEN).NotEmpty()) { Field(x=>x.GIAY_CHUYEN_TUYEN).NotValue(File.XML01.GIAY_CHUYEN_TUYEN).ThenError(2262); }
            //if(Field(x=>x.NGAY_SINH).NotEmpty()) { Field(x=>x.NGAY_SINH).NotValue(File.XML01.NGAY_SINH).ThenError(2263); }
            if(Field(x=>x.GIOI_TINH).NotEmpty()) { Field(x=>x.GIOI_TINH).NotValue(File.XML01.GIOI_TINH).ThenError(2264); }
            if(Field(x=>x.MA_QUOCTICH).NotEmpty()) { Field(x=>x.MA_QUOCTICH).NotValue(File.XML01.MA_QUOCTICH).ThenError(2265); }
            if(Field(x=>x.MA_DANTOC).NotEmpty()) { Field(x=>x.MA_DANTOC).NotValue(File.XML01.MA_DANTOC).ThenError(2266); }
            if(Field(x=>x.MA_NGHE_NGHIEP).NotEmpty()) { Field(x=>x.MA_NGHE_NGHIEP).NotValue(File.XML01.MA_NGHE_NGHIEP).ThenError(2267); }
            if(Field(x=>x.DIA_CHI).NotEmpty()) { Field(x=>x.DIA_CHI).NotValue(File.XML01.DIA_CHI).ThenError(2268); }
            if(Field(x=>x.MA_THE_BHYT).NotEmpty()) { Field(x=>x.MA_THE_BHYT).NotValue(File.XML01.MA_THE_BHYT).ThenError(2269); }
            if(Field(x=>x.GT_THE_DEN).NotEmpty()) { Field(x=>x.GT_THE_DEN).NotValue(File.XML01.GT_THE_DEN).ThenError(2270); }
            if(Field(x=>x.NGAY_VAO).NotEmpty()) { Field(x=>x.NGAY_VAO).NotValue(File.XML01.NGAY_VAO).ThenError(2271); }
            if(Field(x=>x.NGAY_VAO_NOI_TRU).NotEmpty()) { Field(x=>x.NGAY_VAO_NOI_TRU).NotValue(File.XML01.NGAY_VAO_NOI_TRU).ThenError(2272); }
            if(Field(x=>x.NGAY_RA).NotEmpty()) { Field(x=>x.NGAY_RA).NotValue(File.XML01.NGAY_RA).ThenError(2273); }
            if(Field(x=>x.CHAN_DOAN_RV).NotEmpty()) { Field(x=>x.CHAN_DOAN_RV).NotValue(File.XML01.CHAN_DOAN_RV).ThenError(2275); }
            if(Field(x=>x.MA_BENH_CHINH).NotEmpty()) { Field(x=>x.MA_BENH_CHINH).NotValue(File.XML01.MA_BENH_CHINH).ThenError(2278); }
            if(Field(x=>x.MA_BENH_KT).NotEmpty()) { Field(x=>x.MA_BENH_KT).NotValue(File.XML01.MA_BENH_KT).ThenError(2279); }
            if(Field(x=>x.MA_BENH_YHCT).NotEmpty()) { Field(x=>x.MA_BENH_YHCT).NotValue(File.XML01.MA_BENH_YHCT).ThenError(2280); }
            if(Field(x=>x.PP_DIEU_TRI).NotEmpty()) { Field(x=>x.PP_DIEU_TRI).NotValue(File.XML01.PP_DIEU_TRI).ThenError(2282); }
            if(Field(x=>x.MA_LOAI_RV).NotEmpty()) { Field(x=>x.MA_LOAI_RV).NotValue(File.XML01.MA_LOAI_RV).ThenError(2283); }
            if(Field(x=>x.MA_TTDV).NotEmpty()) { Field(x=>x.MA_TTDV).NotValue(File.XML01.MA_TTDV).ThenError(2290); }


            #endregion

            #region Kiểm tra các lỗi có liên quan đến các Danh mục
            //2489	3	XML13	MA_BENH_YHCT	[13] Không tồn mã Mã Bệnh YHCT trong Quyết định số 6061/QĐ-BYT ngày 29/12/2017 của Bộ trưởng Bộ Y tế
            if(Field(x=>x.MA_BENH_YHCT).NotEmpty())
            {
                var mabenhs = File.MA_BENH_YHCT.Split(";");
                foreach (var item in mabenhs)
                {
                    if (!dmYHCT.Any(x => x.YHDTCode == item))
                    {
                        RaiseError(true, 2489);
                    }
                }
            }
            //2289    1   XML13 MA_BAC_SI[130] Mã bác sĩ không có trong danh mục phê duyệt
            RaiseError(!dmBACSI.Any(x => x.MA_CCHN == File.MA_BAC_SI), 2289);
            //2410	3	XML13	MA_TTDV	[130] Mã TTDV không tìm thấy trong danh mục phê duyệt
            //2411    3   XML13 MA_TTDV[130] Tồn tại mã TTDV, nhưng chưa cấu hình vị trí cho người đứng đầu(1, 4)
            if (Field(x => x.MA_TTDV).NotEmpty())
            {

                var allTTDVs = dmBACSI.Where(x => x.MA_BHXH == File.MA_TTDV);
                if (!RaiseError(!allTTDVs.Any(), 2410))
                {
                    //Chỉ kiểm tra khi có BS trong danh mục
                    RaiseError(!dmBACSI.Any(x => Uti.HCodeContain(x.MA_VI_TRI, "1;4;3")), 2411);
                }
            }
            //2879	3	XML13	MA_BS	[3176] Mã BS Không có trong Danh mục phê duyệt
            //2880    3   XML13 MA_BS[3176] Mã BS không phải Trưởng Khoa, Phó khoa hay người được ủy quyền
            //2881    3   XML13 MA_TTDV[3176] Mã TTDV không tồn tại trong Danh mục phê duyệt
            //2882    3   XML13 MA_TTDV[3176] Mã TTDV phải mã hóa vị trí là 1,3 hoặc 4
            if (Field(x => x.MA_TTDV).NotEmpty())
            {

                var allTTDVs = dmBACSI.Where(x => x.MA_CCHN == File.MA_TTDV);
                if (!RaiseError(!allTTDVs.Any(), 2879))
                {
                    //Chỉ kiểm tra khi có BS trong danh mục
                    RaiseError(!dmBACSI.Any(x => Uti.HCodeContain(x.MA_VI_TRI, "1;4;3")), 2880);
                }
            }
            RaiseError(!dmBACSI.Any(x => x.MA_CCHN == File.MA_BAC_SI), 2881);
            //RaiseError(!dmBACSI.Any(x => x.MACCHN == File.MA_BAC_SI && (x.VI_TRI == "2" || x.VI_TRI == "3" || x.VI_TRI == "5")), 2882);
            #endregion

            return true;
        }
    }
}