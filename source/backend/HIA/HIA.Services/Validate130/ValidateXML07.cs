using HIA.Core.Base;
using HIA.Data.Catalogs;
using HIA.Data.Records;
using HIA.Services.HosoManagement130;
using Microsoft.Extensions.Logging;

namespace HIA.Services.Validate130;

public class ValidateXML07 : BaseValidator<XML07>
{
    private readonly IEnumerable<DMBacSi> dmBACSI;

    public ValidateXML07
    (
        IEnumerable<DMBacSi> dMBacSis,
        ILogger<dynamic>? logger = null
    ) : base(logger)
    {
        dmBACSI = dMBacSis;
    }

    public override async Task<bool> Validate(XML07 xml)
    {

        //Kiem tra neu XML null thi ko chay tiep nua
        //if (xml == null) { Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML07 loi null HOSO", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML07)}"); }
        //else
        
        if (!await base.Validate(xml)) return false;
        //Uti.HandleCronJobLog(_logger, $"[130] Bat dau giam dinh ho so XML05", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML05)}");

        #region Kiểm tra các lỗi cơ bản
        //1838	XML7	MA_LK	Mã liên kết không được để trống
        //1839	XML7	MA_LK	Mã liên kết không được lớn hơn 100 ký tự
        Field(x => x.MA_LK).IsEmpty().ThenError(1838).LengthOver(100).ThenError(1839);

        //1840	XML7	SO_LUU_TRU	Số lưu trữ không được để trống
        //1952	XML7	SO_LUU_TRU	Số lưu trữ không được lớn hơn 200 ký tự
        Field(x => x.SO_LUU_TRU).IsEmpty().ThenError(1840).LengthOver(200).ThenError(1952);

        //1841	XML7	MA_YTE	Mã y tế không được để trống
        //1842	XML7	MA_YTE	Mã y tế không được lớn hơn 200 ký tự
        Field(x => x.MA_YTE).IsEmpty().ThenError(1841).LengthOver(200).ThenError(1842);

        //1843	XML7	MA_KHOA_RV	Mã khoa ra viện không được để trống
        //1844	XML7	MA_KHOA_RV	Mã khoa ra viện không được lớn hơn 200 ký tự
        Field(x => x.MA_KHOA_RV).IsEmpty().ThenError(1843).LengthOver(200).ThenError(1844);

        //1845	XML7	NGAY_VAO	Ngày vào không được để trống và phải đúng định dạng ngày tháng yyyymmddHHMM
        Field(x => x.NGAY_VAO).IsEmpty().ThenError(1845);

        //1846	XML7	NGAY_RA	Ngày ra không được để trống và phải đúng định dạng ngày tháng yyyymmddHHMM
        Field(x => x.NGAY_RA).IsEmpty().ThenError(1846);

        //1847	XML7	CHAN_DOAN_RV	Chẩn đoán ra viện không được để trống
        //1848	XML7	CHAN_DOAN_RV	Chẩn đoán ra viện không được lớn hơn 1500 ký tự
        Field(x => x.CHAN_DOAN_RV).IsEmpty().ThenError(1847).LengthOver(1500).ThenError(1848);

        //1849	XML7	PP_DIEUTRI	Phương pháp điều trị không được để trống
        //1850	XML7	PP_DIEUTRI	Phương pháp điều trị không được lớn hơn 1500 ký tự
        Field(x => x.PP_DIEUTRI).IsEmpty().ThenError(1849).LengthOver(1500).ThenError(1850);

        //1851	XML7	MA_TTDV	Mã thủ trưởng đơn vị không được để trống
        //	
        //  1852		XML7	MA_TTDV	Mã thủ trưởng đơn vị không được lớn 10 ký tự
        //Linhist changed 250926
        //Field(x => x.MA_TTDV).IsEmpty().ThenError(1851).LengthOver(10).ThenError(1852);
        Field(x => x.MA_TTDV).IsEmpty().ThenError(1851).LengthOver(255).ThenError(1852);

        //1853	XML7	MA_BS	Mã bác sĩ không được để trống
        //1854	XML7	MA_BS	Mã bác sĩ không được lớn hơn 200 ký tự
        Field(x => x.MA_BS).IsEmpty().ThenError(1853).LengthOver(255).ThenError(1854);

        //1855	XML7	TEN_BS	Tên bác sĩ không được để trống
        //1856	XML7	TEN_BS	Tên bác sĩ không được lớn hơn 255 ký tự
        //Linhist changed 250926
        //Field(x => x.TEN_BS).IsEmpty().ThenError(1855).LengthOver(200).ThenError(1856);
        Field(x => x.TEN_BS).IsEmpty().ThenError(1855).LengthOver(255).ThenError(1856);

        //1857	XML7	NGAY_CT	Ngày chứng từ không được để trống và phải đúng định dạng ngày tháng yyyymmdd
        Field(x => x.NGAY_CT).IsEmpty().ThenError(1857);
        #endregion

        #region Kiểm tra các lỗi có liên quan đến các trường khác
        //2150    4   XML7 MA_YTE[130] Mã y tế không trùng với Mã BN tại XML1
        //2151    4   XML7 MA_KHOA_RV[130] Mã Khoa ra Viện không trùng thông tin Mã Khoa tại XML1
        //2152    4   XML7 NGAY_VAO[130 Ngày vào không trùng thông tin ngày vào tại XML1
        //2153    4   XML7    NGAY_RA[130] Ngày ra không trùng với thông tin ngày ra tại XML1
        //2360	0	XML7	MA_THE_TAM	[130] Mã thẻ tạm không đúng định dạng
        Field(x => x.MA_YTE).NotValue(File.XML01.MA_BN).ThenError(2150);

        RaiseError(!Uti.CodeContain((File.XML01.MA_KHOA ?? string.Empty), (File.MA_KHOA_RV ?? string.Empty)), 2151);
        
        Field(x => x.NGAY_VAO).NotValue(File.XML01.NGAY_VAO).ThenError(2152);
        Field(x => x.NGAY_RA).NotValue(File.XML01.NGAY_RA).ThenError(2153);
        if(Field(x=>x.MA_THE_TAM).NotEmpty())
        {
            if((File.MA_THE_TAM ?? string.Empty).Trim().Length<15)
            {
                RaiseError(true, 2360); 
            }
            else
            {
                RaiseError(!(File.MA_THE_TAM ?? string.Empty).Contains("TE1"), 2360);
            }
        }    


        //2154	4	XML7	NGUYENNHAN_DINHCHI	[130] Nguyên nhân đình chỉ thai bắt buộc phải nhập
        //2155    4   XML7 MA_DINH_CHI_THAI[130] Mã đình chỉ thai chỉ có giá trị là 0 và 1
        //2156    4   XML7 THOIGIAN_DINHCHI[130] Thời gian đình chỉ thai bắt buộc phải nhập
        //2157    4   XML7 TUOI_THAI[130] Tuổi thai bắt buộc phải nhập
        if (!Field(x=>x.MA_DINH_CHI_THAI).IsEmpty())
        {
            Field(x => x.MA_DINH_CHI_THAI).NotBetweenOrEqual(0, 1).ThenError(2155);
            if (Field(x => x.MA_DINH_CHI_THAI).HasValue(1))
            {
                Field(x => x.NGUYENNHAN_DINHCHI).IsEmpty().ThenError(2154);
                Field(x => x.THOIGIAN_DINHCHI).IsEmpty().ThenError(2156);
                Field(x => x.TUOI_THAI).IsEmpty().ThenError(2157);
            }
        }
        //2162	4	XML7	NGAY_CT	[130] Ngày chứng tử không trùng với ngày ra viện
        if(!Field(x=>x.NGAY_CT).IsEmpty())
        {
            Field(x => x.NGAY_CT).NotValue(File.XML01.NGAY_RA.GetValueOrDefault().Date).ThenError(2162);
        }
        //2163	4	XML7	SO_NGAY_NGHI	[130] Số ngày nghỉ không đúng (đến ngày - từ ngày)
        ///*** Linh code
        ///haimaster update code 9.7.2024 thay doi kieu check null
        //if (Field(x => x.NGOAITRU_DENNGAY).NotEmpty().Field(x => x.NGOAITRU_TUNGAY).NotEmpty().Field(x=>x.SO_NGAY_NGHI).NotEmpty())
        if (Field(x => x.NGOAITRU_DENNGAY).NotEmpty())
        {
            if (Field(x => x.NGOAITRU_DENNGAY).NotEmpty())
            {
                if(Field(x=>x.SO_NGAY_NGHI).NotEmpty())
                {
                    var so_ngay_nghi = (File.NGOAITRU_DENNGAY.GetValueOrDefault() - File.NGOAITRU_TUNGAY.GetValueOrDefault()).Days + 1;
                    Field(x => x.SO_NGAY_NGHI).IsUnder((byte)so_ngay_nghi).ThenError(2163);
                }
            }
        }
        //2195	4	XML7	MA_TTDV	[130] Mã định danh y tế không trùng với thông tin khai báo tại XML1
        //2402	3	XML7	MA_TTDV	[130] Mã TTDV không tìm thấy trong danh mục phê duyệt
        //2403    3   XML7 MA_TTDV[130] Tồn tại mã TTDV, nhưng chưa cấu hình vị trí cho người đứng đầu(1, 4)
        if (Field(x => x.MA_TTDV).NotEmpty())
        {  
            Field(x => x.MA_TTDV).NotValue(File.XML01.MA_TTDV).ThenError(2195);
            var dmTTDVs = dmBACSI.Where(x => x.MA_BHXH == File.MA_TTDV);
            if (!RaiseError(!dmTTDVs.Any(), 2402))
            {
                //Chỉ kiểm tra khi có BS trong danh mục
                RaiseError(!dmTTDVs.Any(x => Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "1;4;3")), 2403);
                //RaiseError(!dmTTDVs.Any(x => x.VI_TRI == "1" || x.VI_TRI == "2" || x.VI_TRI == "3"), 2158);
            }

        }
        //2310    2310    0   XML7 SO_NGAY_NGHI[[130] Số ngày nghỉ bắt buộc phải nhập
        //2311    2311    0   XML7    NGOAITRU_TUNGAY[130] Bắt đầu nghỉ ngoại trú bắt buộc phải nhập
        //2312    2312    0   XML7    NGOAITRU_DENNGAY[130] Ngoại trú đến ngày bắt buộc phải nhập
        //2313    2313    0   XML7    NGOAITRU_TUNGAY[130] Bắt đầu nghỉ ngoại trú không được nhỏ hơn ngày ra viện
        //2314    2314    0   XML7    NGOAITRU_DENNGAY[130] Ngoại trú đến ngày không nhỏ hơn ngày ra viện
        //2315    2315    0   XML7    NGOAITRU_DENNGAY[130] Ngoại trú đến ngày ko được nhỏ hơn ngày bắt đầu nghỉ
        //2316    2316    0   XML7    SO_NGAY_NGHI[130] Số ngày nghỉ không đúng đến ngày - từ ngày
        if(Field(x=>x.SO_NGAY_NGHI).NotEmpty())
        {
            if (Field(x => x.SO_NGAY_NGHI).NotValue(0))
            {
                Field(x => x.NGOAITRU_TUNGAY).IsEmpty().ThenError(2311);
                Field(x => x.NGOAITRU_DENNGAY).IsEmpty().ThenError(2312);
            }
            
        }
        if(Field(x=>x.NGOAITRU_TUNGAY).NotEmpty())
        {
            Field(x=>x.SO_NGAY_NGHI).IsEmpty().ThenError(2310);
            Field(x=>x.NGOAITRU_DENNGAY).IsEmpty().ThenError(2312);
            if(File.NGOAITRU_TUNGAY.GetValueOrDefault().Date<File.NGAY_RA.GetValueOrDefault().Date)
            {
                RaiseError(true, 2313);
            }    
            //Field(x=>x.NGOAITRU_TUNGAY).IsUnder(File.NGAY_RA.Value).ThenError(2313);
            //2399	3	XML7	NGOAITRU_TUNGAY	[130] Ngoại trú từ ngày đang lớn hơn ngày ra viện (>ngày ra+1)
            if(File.NGOAITRU_TUNGAY.GetValueOrDefault().Date>File.NGAY_RA.GetValueOrDefault().Date.AddDays(1))
            {
                RaiseError(true, 2399);
            }

        }
        if (Field(x=>x.NGOAITRU_DENNGAY).NotEmpty())
        {
            Field(x => x.SO_NGAY_NGHI).IsEmpty().ThenError(2310);
            Field(x=>x.NGOAITRU_DENNGAY).IsEmpty().ThenError(2312);
            Field(x => x.NGOAITRU_DENNGAY).IsUnder(File.NGAY_RA.GetValueOrDefault()).ThenError(2314);
            //code nay dang sai, Linh xem laij
            //Field(x => x.NGOAITRU_DENNGAY).IsUnder(File.NGOAITRU_TUNGAY.Value).ThenError(2315); 
            if(File.NGOAITRU_DENNGAY.GetValueOrDefault()<File.NGOAITRU_TUNGAY.GetValueOrDefault())
            {
                RaiseError(true, 2315);
            }
        }
        #endregion

        #region Kiểm tra các lỗi có liên quan đến các Danh mục
        //2159	4	XML7	MA_BS	[130] Mã bác sĩ không nằm trong Danh mục được phê duyệt
        //2160    4   XML7 MA_BS[130] Bác sĩ chưa phải là Trưởng khoa hay Phó Khoa được ủy quyền
        //2161    4   XML7 TEN_BS[130] Tên Bác sĩ không đúng với tên trong danh mục phê duyệt
        //2164  4	XML7	TEN_BS	[130] Tên Bác sĩ không đúng với Tên BS Trưởng khoa/ Phó khoa được ủy quyền
        //RaiseError(!dmBACSI.Any(x => x.MA_BHXH == File.MA_BS), 2159);
        //RaiseError(!dmBACSI.Any(x => x.MA_BHXH == File.MA_BS && Uti.CodeContain(x.VI_TRI, "1;2;3")), 2160);
        //RaiseError(!dmBACSI.Any(x => Uti.CodeMatch(x.TEN_NHANVIEN,File.TEN_BS) && x.MA_BHXH == File.MA_BS), 2161);
        //RaiseError(!dmBACSI.Any(x => Uti.CodeMatch(x.TEN_NHANVIEN, File.TEN_BS) && x.MA_BHXH == File.MA_BS && Uti.CodeContain(x.VI_TRI, "1;2;3")), 2161);
        //UPDATE CHECK MA MA_CCHN VA MA BHXH
        //var allBSs = dmBACSI.Where(x => x.MA_BHXH == File.MA_BS);

        //2385	3	XML7	TEN_BS	[130] Tên Bác sĩ không khớp với DM phê duyệt (Không phân biệt hoa thường)
        //2386    3   XML7 TEN_BS[130] Tên Bác sĩ không phải người trưởng khoa, phó khoa được ủy quyền(Không phân biệt chữ hoa thường)

        if (!dmBACSI.Any(x => x.MA_BHXH == File.MA_BS) && !dmBACSI.Any(x => x.MA_CCHN == File.MA_BS))
        {
            RaiseError(true, 2159);
        }
        //31.3.2025 tam thoi them Ma VT4
        if (!dmBACSI.Any(x => x.MA_BHXH == File.MA_BS && Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "2;3;5;4")) && !dmBACSI.Any(x => x.MA_CCHN == File.MA_BS && Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "2;3;5;4")))
        {
            RaiseError(true, 2160);
        }
        
        
        if (!dmBACSI.Any(x => x.MA_BHXH == File.MA_BS &&  x.HO_TEN==File.TEN_BS) && !dmBACSI.Any(x => x.MA_CCHN == File.MA_BS && x.HO_TEN== File.TEN_BS))
        {
            RaiseError(true, 2161);
        }

        if (!dmBACSI.Any(x => x.MA_BHXH == File.MA_BS && Uti.IsEqual((x.HO_TEN ?? string.Empty), (File.TEN_BS ?? string.Empty)) && Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "2;3;5;4")) && !dmBACSI.Any(x => x.MA_CCHN == File.MA_BS && Uti.IsEqual((x.HO_TEN ?? string.Empty), (File.TEN_BS ?? string.Empty)) && Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "2;3;5;4")))
        {
            RaiseError(true, 2164);
        }


        if (!dmBACSI.Any(x => x.MA_BHXH == File.MA_BS && (x.HO_TEN ?? string.Empty).ToUpper() == (File.TEN_BS ?? string.Empty).ToUpper()) && !dmBACSI.Any(x => x.MA_CCHN == File.MA_BS && (x.HO_TEN ?? string.Empty).ToUpper() == (File.TEN_BS ?? string.Empty).ToUpper()))
        {
            RaiseError(true, 2385);
        }

        if (!dmBACSI.Any(x => x.MA_BHXH == File.MA_BS && Uti.IsEqual((x.HO_TEN ?? string.Empty).ToUpper(), (File.TEN_BS ?? string.Empty).ToUpper()) && Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "2;3;5;4")) && !dmBACSI.Any(x => x.MA_CCHN == File.MA_BS && Uti.IsEqual((x.HO_TEN ?? string.Empty).ToUpper(), (File.TEN_BS ?? string.Empty).ToUpper()) && Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "2;3;5;4")))
        {
            RaiseError(true, 2386);
        }

        //2695	3	XML7	MA_BS	[3176] Mã BS phải mã hóa theo CCHN và Nằm trong DM phê duyệt
        if ( !dmBACSI.Any(x => x.MA_CCHN == File.MA_BS))
        {
            RaiseError(true, 2695);
        }

        //Linhist removed 30/07/26
        //if (!dmBACSI.Any(x => x.MACCHN == File.MA_BS && Uti.HCodeContain(x.VI_TRI, "2;3;5")))
        //{
        //    RaiseError(true, 2697);
        //}


        //2696    3   XML7 TEN_BS[3176] Tên BS không có trong DM phê duyệt
        if (!dmBACSI.Any(x => x.MA_CCHN == File.MA_BS && x.HO_TEN == File.TEN_BS))
        {
            RaiseError(true, 2696);
        }

        //2697    3   XML7 MA_BS[3176] Mã BS chưa phải là Trưởng khoa, Phó khoa được ủy quyền
        if(!dmBACSI.Any(x => x.MA_CCHN == File.MA_BS && Uti.IsEqual((x.HO_TEN ?? string.Empty), (File.TEN_BS ?? string.Empty)) && Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "2;3;5")))
        {
            RaiseError(true, 2697);
        }

        //2701	3	XML7	TEN_BS	[3176] Tên Bác sĩ không khớp với DM phê duyệt (Không phân biệt hoa thường)
        if (!dmBACSI.Any(x => x.MA_CCHN == File.MA_BS && (x.HO_TEN ?? string.Empty).ToUpper() == (File.TEN_BS ?? string.Empty).ToUpper()))
        {
            RaiseError(true, 2701);
        }

        //2700    3   XML7 TEN_BS[3176] Tên Bác sĩ không phải người trưởng khoa, phó khoa được ủy quyền(Không phân biệt chữ hoa thường)
        if (!dmBACSI.Any(x => x.MA_CCHN == File.MA_BS && Uti.IsEqual((x.HO_TEN ?? string.Empty).ToUpper(), (File.TEN_BS ?? string.Empty).ToUpper()) && Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "2;3;5")))
        {
            RaiseError(true, 2700);
        }


        //2698    3   XML7 MA_TTDV[3176] Mã TTDV phải được mã hóa theo CCHN và nằm trong DM Được phê duyệt
        //2699    3   XML7 MA_TTDV[3176] Tồn tại mã TTDV, nhưng chưa cấu hình vị trí cho người đứng đầu(1, 3, 4)
        if (Field(x => x.MA_TTDV).NotEmpty())
        {
            
            var dmTTDVs = dmBACSI.Where(x => x.MA_CCHN == File.MA_TTDV);
            if (!RaiseError(!dmTTDVs.Any(), 2698))
            {
                //Chỉ kiểm tra khi có BS trong danh mục
                RaiseError(!dmTTDVs.Any(x => Uti.HCodeContain((x.MA_VI_TRI ?? string.Empty), "1;4;3")), 2699);
                
            }

        }

        #endregion

        return true;
    }
}
