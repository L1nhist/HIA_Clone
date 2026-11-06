using HIA.Core.Base;
using HIA.Data.Catalogs;
using HIA.Data.Records;
using HIA.Data.Rules;
using HIA.Repositories.Base;
using HIA.Repositories.Files;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HIA.Services.Validate130;
public class ValidateAllXML23 : BaseValidator<XML01>
{
    private Dictionary<int, string> errorlist = new();
    string benhchinhInfo = string.Empty;

    private readonly ValidateXML02 X2Validator;
    private readonly ValidateXML03 X3Validator;
    private readonly ILogger<dynamic>? _logger;
    
    private XMLCommonInfo Info = new(new XML01());

    internal bool has_VTNong_DongMach = false;
    internal bool has_CongKham_Chinh = false;
    internal bool has_CongKham_Noitru = false;
    internal bool has_XetNghiem_Nhom1 = false;

    #region các thông tin khai báo check thời gian Khám và Thuốc
    internal DateTime _CongkhamMin;
    internal DateTime _DVMin;
    internal DateTime _VTMin;
    internal DateTime _ThuocMin;
    private readonly Dictionary<string, string> dmDacBiet;
    private readonly Dictionary<string, string> dmGiuong;
    private readonly Dictionary<byte, string> dmPhauThuat;
    private readonly IEnumerable<DMBacSi> dmBACSI;
    private readonly IEnumerable<DMError130> dmError130;
    private readonly IEnumerable<DMChung130> dmChung130;

    #endregion

    internal double sumall_KimLuon = 0d;

    internal DateTime Min_NgayYL_KhamChinh = DateTime.MaxValue;

    internal List<XML03> All_DVNong_DongMach = new();
    internal List<XML03> All_DienTims = new();
    internal List<XML03> All_SieuAmTims = new();
    internal List<XML03> All_PhauThuats = new();
    internal List<XML03> All_VuotTuyens = new();
    internal List<XML03> All_KimLuons = new();
    internal List<XML03> All_GiuongNgoais = new();
    internal List<XML03> All_CongKhamPhu = new();
    internal List<XML03> _congkhamchinh = new();
    internal List<XML03> _Allcongkham = new();

    internal List<XML02> _xml2min = new();
    internal List<XML03> _xml3VTmin = new();
    internal List<XML03> _xml3DVmin = new();
    internal List<XML03> _xml3Giuongmin = new();

    //Linhist added 170926
    internal List<XML03> All_GiuongNois = new();

    internal List<XML03> All_XML03_ToiThieus = new();
    internal List<HDSDDichVu> All_HDSD_ToiThieus = new();

    #region cac loi 4210 chuyen qua (logic)
    internal List<XML03> _theodoimonitorSK = new();
    internal List<XML03> _dvdode = new();
    internal List<XML03> _allPT = new();
    #endregion

    public ValidateAllXML23(
        //Linhist changed 20/03/26
        TimePeriod officialTime,
        IEnumerable<DMBacSi> DMBacsi,
        IEnumerable<DMErrorMHTC> DMErrorMHTC,
        IEnumerable<DMYHDTICD10> DMYHCT,
        Dictionary<string, string> dicDacBiets,
        Dictionary<string, string> dicGiuongs,
        Dictionary<byte, string> dicPhauThuats,
        IEnumerable<ChiDinhDongThoi> RuleDongthoi,
        IEnumerable<DMThuoc> DMThuocs,
        IEnumerable<DMKhoaBV> dMKhoaBVs,
        IEnumerable<DMDichVu> DMDichVus,
        IEnumerable<DMVTYT> DMVtyts,
        IEnumerable<DMBacSi> DMBacSis,
        IEnumerable<DMDVThuCong> DMThuCongs,
        IEnumerable<DMThietBi> DMThietBis,
        IEnumerable<HDSDDichVu> HDSDDichVus,
        IEnumerable<HDSDThuoc> HDSDThuocs,
        IEnumerable<DMChung130> DMChung130,
        IEnumerable<DMError130> DMError130,
        IXml01Repository hosoRepository,
        IBaseRepository<XML03> xml03Repository,
        IBaseRepository<XML02> xml02Repository,
        IEnumerable<DMErrorMHTC> _dMErrorMHTCs,
        IEnumerable<DichVuMaMay> _dvuMamays,
        ILogger<dynamic>? logger = null
    ) : base(logger)
    {
        dmDacBiet = dicDacBiets;
        dmGiuong = dicGiuongs;
        dmPhauThuat = dicPhauThuats;
        dmBACSI = DMBacsi;
        dmChung130= DMChung130; 
        dmError130= DMError130;
        _logger = logger;
        X2Validator = new ValidateXML02(this, officialTime, dicDacBiets,DMError130, DMChung130, DMErrorMHTC, dMKhoaBVs, DMThuocs, DMBacSis, HDSDThuocs, logger);
        X3Validator = new ValidateXML03(officialTime, DMYHCT, DMChung130, RuleDongthoi, dMKhoaBVs, DMDichVus, DMVtyts, DMBacSis, DMThuCongs, DMThietBis, HDSDDichVus,DMError130, hosoRepository, xml03Repository, xml02Repository, _dMErrorMHTCs, _dvuMamays, logger);
    }

    #region Update check loại phẫu thuật và loại giường (27.7.2024)
    //Lay toan bo DMPhauThuat, phan ra thanh list code cho de kiem tra

    ////Lay toan bo DMGiuong, phan ra thanh list code cho de kiem tra

    //    //Lay danh sach giuong noi

    //    //Lay danh sach giuong cap cuu

    #endregion

    public async Task<bool> Validate(XML01 xml, XMLCommonInfo info)
    {
        Info = info;
        return await Validate(xml);
    }

    public override async Task<bool> Validate(XML01 xml)
    {
        // Kiem tra neu XML null thi ko chay tiep nua
        if (!await base.Validate(xml)) return false;

        Stopwatch _watch = Stopwatch.StartNew();

        #region Reset lại dữ liệu chung cho mỗi XML01 mới
        _CongkhamMin = DateTime.Now;
        _DVMin = DateTime.Now;
        _VTMin = DateTime.Now;
        _ThuocMin = DateTime.Now;
        has_VTNong_DongMach = false;
        has_CongKham_Chinh = false;
        has_CongKham_Noitru = false;
        has_XetNghiem_Nhom1 = false;

        sumall_KimLuon = 0d;
        Min_NgayYL_KhamChinh = DateTime.MaxValue;

        All_DVNong_DongMach = new();
        All_DienTims = new();
        All_SieuAmTims = new();
        All_PhauThuats = new();
        All_VuotTuyens = new();
        All_KimLuons = new();
        All_GiuongNgoais = new();
        All_CongKhamPhu = new();
        All_XML03_ToiThieus = new();
        All_HDSD_ToiThieus = new();
        _congkhamchinh = new();
        _Allcongkham = new();
        _xml2min = new();
        _xml3VTmin = new();
        _xml3DVmin = new();
        _xml3Giuongmin = new();
        _allPT = new();

        _theodoimonitorSK = new();
        _dvdode = new();

        //Linhist added 170926
        All_GiuongNois = new();

        List<int> allPhauThuatGiuongLoais = new();
        #endregion

        #region Check Thời gian tối thiệu HDSD thuốc

        #endregion

        List<XML02> All_XML02_ThuocCanQuangs = new();
        List<XML03> All_XML03_ThuocCanQuangs = new();
        errorlist = dmError130
            .Where(x => x.ErrorCode.HasValue) // Filter out entries where ErrorCode is null
            .ToDictionary(x => x.ErrorCode.GetValueOrDefault(), x => x.Message ?? string.Empty); // Use Value to access the non-nullable int
        benhchinhInfo = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == xml.MA_BENH_CHINH)?.TEN ?? "Không có thông tin bệnh";
        string DVCanQuang = "18.0192.0041.K.10062;18.0133.0019;18.0220.0041.K.10062;18.0229.0041;18.0260.0041;18.0265.0041;18.0266.0041;18.0267.0041;18.0150.0041;18.0156.0041;18.0159.0041;18.0192.0041;18.0219.0041;18.0220.0041;18.0221.0041;18.0222.0041;18.0223.0041;18.0297.0065;18.0299.0065;18.0301.0065;18.0302.0065;18.0304.0065;18.0309.0065;18.0342.0065;18.0344.0065;18.0351.0065;18.0353.0065;18.0355.0065;18.0359.0065;18.0360.0065;18.0361.0065;18.0305.0065;18.0311.0065;18.0317.0065;18.0320.0065;18.0322.0065;18.0323.0065;18.0335.0065;18.0337.0065;18.0339.0065;18.0341.0065;18.0346.0065;18.0347.0065;18.0348.0065;18.0349.0065;18.0350.0065;18.0217.0046;18.0205.0042;18.0207.0042;18.0281.0042;18.0169.0042;18.0232.0042;18.0224.0041;18.0236.0042;18.0234.0042;18.0223.0041;18.0235.0042;18.0231.0042;18.0233.0042;18.0154.0041;18.0170.0042;18.0152.0041;18.0351.0065;18.0305.0065;18.0322.0065;18.0309.0065";
        string ThuocCanQuang = "40.641;40.642;40.644;40.645;40.30.647;40.644;40.30.647;40.642;40.646;40.638;40.636;40.367;";
        var sortedX3 = xml.XML03s.OrderBy(x => x.MA_DICH_VU).ThenBy(x => x.MA_VAT_TU).ToList();

        foreach (var x03 in sortedX3)
        {

            try
            {
                if (!await X3Validator.Validate(x03, Info)) continue;
            }
            catch
            {
                continue;
            }

            try
            {
                #region Kim Luon
                if (Uti.CodeMatch(x03.MA_DICH_VU ?? string.Empty, "N07.01.220"))
                {
                    has_VTNong_DongMach = true;
                }

                if (Uti.CodeContain(x03.MA_DICH_VU ?? string.Empty, "18.0658.0054;18.0588.0061"))
                {
                    All_DVNong_DongMach.Add(x03);
                }

                if (Uti.CodeContain(x03.MA_VAT_TU ?? string.Empty, "N07.01.110;N03.02.070"))
                {
                    sumall_KimLuon += x03.SO_LUONG ?? 0d;
                    All_KimLuons.Add(x03);
                }
                #endregion

                #region Lấy all công khám (Kiểm tra khám chính khám phụ- time khám phụ phải sau khám chính)

                if (x03.PHAM_VI.HasValue && x03.PHAM_VI == 1 && x03.MA_NHOM.HasValue && x03.TYLE_TT_DV.HasValue && x03.MA_NHOM == 13 && x03.TYLE_TT_DV != 100)
                {
                    All_CongKhamPhu.Add(x03);
                }
                #endregion

                //Có thuốc cản quang
                if (x03.NGAY_KQ != null && Uti.CodeMatch(x03.MA_DICH_VU ?? string.Empty, DVCanQuang))
                {
                    All_XML03_ThuocCanQuangs.Add(x03);
                }

                #region Min công khám chính
                if (x03.NGAY_YL.HasValue && x03.MA_NHOM.HasValue && x03.TYLE_TT_DV.HasValue && x03.MA_NHOM == 13 && x03.TYLE_TT_DV == 100)
                {
                    if (X3Validator.Field(x => x.MA_NHOM).HasValue(13).Field(x => x.TYLE_TT_DV).HasValue(100).Field(x => x.PHAM_VI).HasValue(1))
                    {
                        if (x03.NGAY_TH_YL.HasValue)
                        {
                            if (x03.NGAY_TH_YL < _CongkhamMin)
                            {
                                _CongkhamMin = (DateTime)x03.NGAY_TH_YL;

                            }
                        }
                        else
                        {
                            if (x03.NGAY_YL < _CongkhamMin)
                            {
                                _CongkhamMin = (DateTime)x03.NGAY_YL;

                            }
                        }
                        _congkhamchinh.Add(x03);

                    }
                }
                if (x03.NGAY_YL.HasValue && x03.MA_NHOM.HasValue && x03.MA_NHOM == 13)
                {
                    _Allcongkham.Add(x03);
                }

                if (x03.NGAY_YL.HasValue && x03.MA_NHOM.HasValue && (x03.MA_NHOM == 1 || x03.MA_NHOM == 2 || x03.MA_NHOM == 3 || x03.MA_NHOM == 14 || x03.MA_NHOM == 15 || x03.MA_NHOM == 18 || x03.MA_NHOM == 8))
                {
                    if (x03.NGAY_YL <= _DVMin)
                    {
                        _DVMin = (DateTime)x03.NGAY_YL;
                        _xml3DVmin.Clear();
                        _xml3DVmin.Add(x03);

                    }
                }

                if (x03.NGAY_YL.HasValue && x03.MA_NHOM.HasValue && (x03.MA_NHOM == 14 || x03.MA_NHOM == 15))
                {
                    if (x03.NGAY_YL < _DVMin)
                    {
                        _DVMin = (DateTime)x03.NGAY_YL;
                        _xml3Giuongmin.Clear();
                        _xml3Giuongmin.Add(x03);

                    }
                }

                if (x03.NGAY_YL.HasValue && x03.MA_NHOM.HasValue && (x03.MA_NHOM == 10 || x03.MA_NHOM == 11))
                {
                    if (x03.NGAY_YL < _VTMin)
                    {
                        _VTMin = (DateTime)x03.NGAY_YL;
                        _xml3VTmin.Clear();
                        _xml3VTmin.Add(x03);
                    }
                }

                #endregion

                #region theo dõi monitor nhịp tim phải trước khi đỡ đẻ (Các dịch vụ liên quan đến đẻ)
                if (!String.IsNullOrEmpty(x03.MA_DICH_VU) && x03.MA_DICH_VU == "13.0023.2023")
                {
                    _theodoimonitorSK.Add(x03);
                }

                if (Uti.HCodeMach(x03.MA_DICH_VU ?? string.Empty, "13.0024.0613;13.0033.0614;13.0026.0615"))
                {
                    _dvdode.Add(x03);
                }
                #endregion

                #region Giường ngoại khoa
                if (x03.MA_NHOM.HasValue && !String.IsNullOrEmpty(x03.MA_DICH_VU))
                {
                    if ((x03.MA_NHOM == 14 || x03.MA_NHOM == 15) && Uti.CodeMatch(x03.MA_DICH_VU, dmDacBiet["giuongkhamngoai"]))//_dbGiuongNgoai))
                    {
                        All_GiuongNgoais.Add(x03);
                    }

                    //Linhist added 170926
                    if (x03.MA_NHOM == 15 && Uti.CodeMatch(x03.MA_DICH_VU, dmDacBiet["giuongnoikhoa"]))
                    {
                        All_GiuongNois.Add(x03);
                    }
                }
                #endregion

                //1251  1   XML3    MA_DICH_VU  Kê giường ngoại khoa sai TT39/ 2018
                //Kiem tra gia tri PhauThuatGiuongLoai:

                foreach (var d in dmPhauThuat)
                {
                    if (Uti.CodeContain(x03.MA_DICH_VU ?? string.Empty, d.Value) && !allPhauThuatGiuongLoais.Contains(d.Key))
                    {
                        allPhauThuatGiuongLoais.Append(d.Key);
                        break;
                    }
                }

                //Linhist added 19/06/26
                //891   1   XML3 TYLE_TT   Phẫu thuật lần 2 trong ngày thanh toán 100% sai với CV 824-BYT

                if (x03.MA_NHOM == 8)
                {
                    _allPT.Add(x03);
                }
            }
            catch (Exception ex)
            {
                AddNewLog($"ValidateAllXML23: giam dinh all ho so XML03 loi: {x03.XML01.MA_CSKCB} {x03.XML01.MA_BN} {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        ////***RISK: đoạn này phải xem lại mã loại trừ & mã bắt buộc
        //Kiểm tra các dịch vụ mã nhóm 8,18 (Không bao gồm dịch vụ đặt ống thở 24h) để so sánh trùng
        //421	1	XML3	NGAY_TH_YL	Dịch vụ phẫu thuật, thủ thuật không được phép thực hiện cùng thời điểm
        //4181	2	XML3	DON_GIA_BH	Dịch vụ PTTT có phẫu thuật chính chưa phải giá cáo nhất
        //Linhist changed 23/07/26
        //***RISK: đoạn này phải xem lại mã loại trừ & mã bắt buộc (nếu xem lại check thêm lỗi 4181)
        var maLoaiTru = "01.0008.0100;01.0009.0098;01.0087.0898;01.0128.0209;01.0130.0209;01.0131.0209;01.0132.0209;01.0133.0209;01.0134.0209;01.0135.0209;01.0136.0209;01.0137.0209;01.0138.0209;01.0142.0209;01.0144.0209;01.0175.0196;02.0150.0114;02.0305.0135;03.0082.0209;03.0058.0209;08.0022.0252;08.0015.0252;09.9000.1894_BS";
        var maBatBuoc = "03.0082.0209;03.0058.0209;0;01.0009.0098;01.0128.0209;01.0130.0209;01.0131.0209;01.0132.0209;01.0133.0209;01.0134.0209;01.0135.0209;01.0144.0209;01.0136.0209;01.0137.0209;01.0138.0209";

        try
        {
            //    YL1 - KQ1
            //YL2 - KQ2YL2 - KQ2
            //421	1	XML3	NGAY_TH_YL	Dịch vụ phẫu thuật, thủ thuật không được phép thực hiện cùng thời điểm
            var x3LoaiTrus = xml.XML03s.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && Uti.NumExists(x.MA_NHOM, 8, 18) && !Uti.HCodeMach(x.MA_DICH_VU, maLoaiTru)).ToList();
            var x3TrungGios = x3LoaiTrus.Where(x => x.TYLE_TT_DV == 100).ToList();
            x3TrungGios = x3TrungGios.Where(x => x3TrungGios.Any(n => x.XML03ID != n.XML03ID && n.NGAY_TH_YL <= x.NGAY_KQ && n.NGAY_KQ >= x.NGAY_TH_YL)).ToList();
            if (x3TrungGios.Any())
            {
                var fstTrung = x3TrungGios.FirstOrDefault();
                var loiTrungGio = $"Dịch vụ trùng: {fstTrung?.MA_DICH_VU}|{fstTrung?.TEN_DICH_VU} có mã nhóm:{fstTrung?.MA_NHOM}, thực hiện y lệnh:{fstTrung?.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, ngày kết quả:{fstTrung?.NGAY_KQ:dd/MM/yyyy HH:mm:ss}";
                foreach (var x in x3TrungGios)
                {
                    x.RaiseError(421,
                            $"Bệnh nhân: {xml.HO_TEN} (Mã BN: {xml.MA_BN})\n" +
                            $"Mã ra viện: {xml.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, xml.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {xml.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(421, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {x.MA_DICH_VU}|{x.TEN_DICH_VU}, có mã nhóm:{x.MA_NHOM}, thời điểm TH y lệnh: {x.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} ngày kết quả: {x.NGAY_KQ:dd/MM/yyyy HH:mm:ss} trùng khoảng thời gian thực hiện với dịch vụ khác trong hồ sơ XML3::\n" +
                            loiTrungGio
                        );
                }
            }

            var x3BatBuocs = xml.XML03s.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && Uti.NumExists(x.MA_NHOM, 8, 18) && Uti.HCodeMach(x.MA_DICH_VU, maBatBuoc)).ToList();
            x3TrungGios = x3BatBuocs.Where(x => x.TYLE_TT_DV == 100).ToList();
            x3TrungGios = x3TrungGios.Where(x => x3TrungGios.Any(n => x.XML03ID != n.XML03ID && x.MA_DICH_VU == n.MA_DICH_VU && n.NGAY_TH_YL >= x.NGAY_KQ && n.NGAY_KQ <= x.NGAY_TH_YL)).ToList();
            if (x3TrungGios.Any())
            {
                var fstTrung = x3TrungGios.FirstOrDefault();
                var loiTrungGio = $"Dịch vụ trùng: {fstTrung?.MA_DICH_VU}|{fstTrung?.TEN_DICH_VU} có mã nhóm:{fstTrung?.MA_NHOM}, thực hiện y lệnh:{fstTrung?.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, ngày kết quả:{fstTrung?.NGAY_KQ:dd/MM/yyyy HH:mm:ss}";
                foreach (var x in x3TrungGios)
                {
                    x.RaiseError(421,
                            $"Bệnh nhân: {xml.HO_TEN} (Mã BN: {xml.MA_BN})\n" +
                            $"Mã ra viện: {xml.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, xml.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {xml.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(421, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {x.MA_DICH_VU}|{x.TEN_DICH_VU}, có mã nhóm:{x.MA_NHOM}, thời điểm TH y lệnh: {x.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} ngày kết quả: {x.NGAY_KQ:dd/MM/yyyy HH:mm:ss} trùng khoảng thời gian thực hiện với dịch vụ khác trong hồ sơ XML3::\n" +
                            loiTrungGio
                        );
                }
            }

            //4181	2	XML3	DON_GIA_BH	Dịch vụ PTTT có phẫu thuật chính chưa phải giá cáo nhất
            var x3MaxGias1 = x3LoaiTrus.Where(x => x3LoaiTrus.Any(n => x.XML03ID != n.XML03ID && n.NGAY_TH_YL >= x.NGAY_KQ && n.NGAY_KQ <= x.NGAY_TH_YL)).ToList();
            var x3MaxGias2 = x3BatBuocs.Where(x => x3BatBuocs.Any(n => x.XML03ID != n.XML03ID && x.MA_DICH_VU == n.MA_DICH_VU && n.NGAY_TH_YL >= x.NGAY_KQ && n.NGAY_KQ <= x.NGAY_TH_YL)).ToList();
            if (x3MaxGias1.Any() || x3MaxGias2.Any())
            {
                var maxGia = Math.Max(x3MaxGias1.Max(x => x.DON_GIA_BH) ?? 0d, x3MaxGias2.Max(x => x.DON_GIA_BH) ?? 0d);
                var fstMax = x3MaxGias1.FirstOrDefault(x => x.DON_GIA_BH == maxGia);
                fstMax ??= x3MaxGias2.FirstOrDefault(x => x.DON_GIA_BH == maxGia);
                var loiMaxGia = $"Dịch vụ trùng có giá cao nhất: {fstMax?.MA_DICH_VU}|{fstMax?.TEN_DICH_VU} có mã nhóm: {fstMax?.MA_NHOM}, thực hiện y lệnh:{fstMax?.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, ngày kết quả:{fstMax?.NGAY_KQ:dd/MM/yyyy HH:mm:ss}, đơn giá BH: {fstMax?.DON_GIA_BH:##000.00}";

                foreach (var x in x3MaxGias1)
                {
                    if (x.TYLE_TT_DV == 100 && x.DON_GIA_BH < maxGia)
                    {
                        x.RaiseError(4181,
                            $"Bệnh nhân: {xml.HO_TEN} (Mã BN: {xml.MA_BN})\n" +
                            $"Mã ra viện: {xml.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, xml.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {xml.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(4181, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {x.MA_DICH_VU}|{x.TEN_DICH_VU}, có mã nhóm:{x.MA_NHOM}, thời điểm TH y lệnh: {x.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} ngày kết quả: {x.NGAY_KQ:dd/MM/yyyy HH:mm:ss} " +
                            $"có tỷ lệ thanh toán DV 100% có trùng khoảng thời gian thực hiện với dịch vụ khác trong hồ sơ XML3 khác có mức giá cao hơn::\n" +
                            loiMaxGia
                        );
                    }
                }
                foreach (var x in x3MaxGias2)
                {
                    if (x.TYLE_TT_DV == 100 && x.DON_GIA_BH < maxGia)
                    {
                        x.RaiseError(4181,
                            $"Bệnh nhân: {xml.HO_TEN} (Mã BN: {xml.MA_BN})\n" +
                            $"Mã ra viện: {xml.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, xml.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {xml.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(4181, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {x.MA_DICH_VU}|{x.TEN_DICH_VU}, có mã nhóm:{x.MA_NHOM}, thời điểm TH y lệnh: {x.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} ngày kết quả: {x.NGAY_KQ:dd/MM/yyyy HH:mm:ss} " +
                            $"có tỷ lệ thanh toán DV 100% có trùng khoảng thời gian thực hiện với dịch vụ khác trong hồ sơ XML3 khác có mức giá cao hơn::\n" +
                            loiMaxGia
                        );
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Lỗi khi kiểm tra lỗi 421/4181 cho bệnh nhân {xml.MA_BN} - {xml.HO_TEN}, Mã CSKCB:{xml.MA_CSKCB}");
        }

        //Linhist added 24/07/26
        if (xml.MA_LOAI_KCB == "03" && xml.NGAY_VAO_NOI_TRU != null && xml.NGAY_RA != null)
        { 
            var allGiuongs = xml.XML03s.Where(x => x.MA_NHOM == 15 && x.NGAY_TH_YL != null && x.NGAY_KQ != null).ToList();
            if (allGiuongs.Count > 1)
            {
                var minNgayGiuong = allGiuongs.Min(x => x.NGAY_TH_YL.GetValueOrDefault());
                var maxNgayGiuong = allGiuongs.Max(x => x.NGAY_KQ.GetValueOrDefault());

                //4185    2   XML3 NGAY_TH_YL  Thời gian thực hiện y lệnh giường từ khi vào viện quá 5 phút
                if (minNgayGiuong < xml.NGAY_VAO_NOI_TRU.Value || minNgayGiuong > xml.NGAY_VAO_NOI_TRU.Value.AddMinutes(5))
                {
                    var minGiuongs = allGiuongs.Where(x => x.NGAY_TH_YL == minNgayGiuong).ToList();
                    foreach (var x in minGiuongs)
                    {
                        x.RaiseError(4185,
                            $"Bệnh nhân: {xml.HO_TEN} (Mã BN: {xml.MA_BN})\n" +
                            $"Mã ra viện: {xml.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, xml.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {xml.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(4185, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê giường đầu tiên: {x.MA_DICH_VU}|{x.TEN_DICH_VU}, có thời điểm TH y lệnh: {x.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} quá thời điểm vào viện ({xml.NGAY_VAO_NOI_TRU:dd/MM/yyyy HH:mm:ss}) hơn 5 phút"
                        );
                    }
                }

                //4186	2	XML3	NGÀY_KQ	Thời gian kết quả giường nhỏ hơn 5 phút so với thời gian ra viện
                if (maxNgayGiuong > xml.NGAY_RA.Value || maxNgayGiuong < xml.NGAY_RA.Value.AddMinutes(-5))
                {
                    var maxGiuongs = allGiuongs.Where(x => x.NGAY_KQ == maxNgayGiuong).ToList();
                    foreach (var x in maxGiuongs)
                    {
                        x.RaiseError(4186,
                            $"Bệnh nhân: {xml.HO_TEN} (Mã BN: {xml.MA_BN})\n" +
                            $"Mã ra viện: {xml.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, xml.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {xml.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(4186, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {x.MA_DICH_VU}|{x.TEN_DICH_VU}, có mã nhóm:{x.MA_NHOM}, thời điểm TH y lệnh: {x.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} quá thời điểm ra viện ({xml.NGAY_RA:dd/MM/yyyy HH:mm:ss}) hơn 5 phút"
                        );
                    }
                }
            }
        }   

        var sortedX2 = xml.XML02s.OrderBy(x => x.MA_THUOC).ToList();
        foreach (var x02 in sortedX2)
        {
            try
            {
                if (!await X2Validator.Validate(x02, Info)) continue;

                //Có thuốc cản quang
                if (x02.NGAY_YL.HasValue && Uti.HCodeMach(x02.MA_THUOC ?? string.Empty, ThuocCanQuang))
                {
                    All_XML02_ThuocCanQuangs.Add(x02);
                }
                if(x02.MA_NHOM==4 && x02.NGAY_YL.HasValue)
                {
                    if(x02.NGAY_YL < _ThuocMin)
                    {
                        _ThuocMin = (DateTime)x02.NGAY_YL;
                        _xml2min.Clear();
                        _xml2min.Add(x02);
                    }
                }

            }
            catch (Exception ex)
            {
                AddNewLog($"ValidateAllXML23: Giam dinh all ho so XML02 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        //Kiểm tra các nội dung khác

        if ((int)sumall_KimLuon > Info.NGAY_NAM_VIEN)
        {
            foreach (var a in All_KimLuons)
            {
                a.RaiseError(888);
            }
        }

        if (has_VTNong_DongMach)
        {
            foreach (var a in All_KimLuons)
            {
                a.RaiseError(935);
            }
        }
        // haimaster update 20.6.2026
        //891 1   XML3 TYLE_TT[3716] Phẫu thuật lần 2 trong ngày thanh toán 100 % sai với CV 824 - BYT / QĐ 3176 Bộ Y Tế
        if (_allPT.Count() > 1)
        {
            foreach (var pt in _allPT)
            {
                if(pt.TYLE_TT_DV.HasValue && pt.TYLE_TT_DV.Value == 100)
                {
                   var sameDayPT = _allPT.Where(x => x.NGAY_TH_YL.HasValue  && x.NGAY_TH_YL.Value.Date == pt.NGAY_TH_YL.GetValueOrDefault().Date && x.XML03ID != pt.XML03ID && x.TYLE_TT_DV==100);
                    if(sameDayPT.Any())
                    {
                        foreach(var s in sameDayPT)
                        {
                            s.RaiseError(891);
                        }
                    }
                }
            }
        }

        #region Thuoc can quang
        List<long> idX2ThuocCQs = new();
        List<long> idX3ThuocCQs = new();

        foreach (var x2 in All_XML02_ThuocCanQuangs)
        {
            if (string.IsNullOrEmpty(x2.MA_THUOC) || "18.0151.0041;18.0159.0041;18.0260.0041;18.0262.0041;18.0263.0041;18.0265.0041;18.0156.0041;18.0192.0041;18.0256.0041;18.0258.0041".Contains(x2.MA_DICH_VU ?? string.Empty))
            {
                x2.RaiseError(4045, $"Bệnh nhân: {x2.XML01.HO_TEN} (Mã BN: {x2.XML01.MA_BN})\n" +
                    $"Mã ra viện: {x2.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, x2.XML01.MA_LOAI_RV.ToString())}\n" +
                    $"Mã bệnh chính: {x2.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    $"Nội dung lỗi: {(errorlist.TryGetValue(4045, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    $"Chi tiết: Bệnh nhân được kê: {x2.MA_THUOC}|{x2.TEN_THUOC}, nhưng không có dịch vụ CLVT có tiêm thuốc tương ứng.");
            }
            var x3CungNgay = All_XML03_ThuocCanQuangs.Where(x3 => x3.NGAY_TH_YL.GetValueOrDefault().Date == x2.NGAY_YL.GetValueOrDefault().Date && !idX3ThuocCQs.Contains(x3.XML03ID));

            #region Linhist removed 19/03/26 - case này bị lỗi null hasX3
            //Neu co XML3 cung ngay
            //    //Linhist changed 28/07/23

            //    //Truong hop con lai, trong cung ngay co ton tai XML2 & XMl3 ma NGAY_YL > NGAY_KQ thi bao loi 1602
            ////Neu khong co XML3 cung ngay ma chi co XML2 thi bao loi 1604
            ////1604	1	XML2	MA_THUOC	Kê thuốc cản quang mà không thực hiện Chụp CT, MRI có tiêm thuốc cản quang trong cùng ngày
            //    x2.RaiseError(1604
            #endregion
            //Neu co XML3 cung ngay
            if (x3CungNgay.Any())
            {
                //Linhist changed 28/07/23
                var hasX3 = x3CungNgay.FirstOrDefault(x3 => x2.NGAY_YL <= x3.NGAY_KQ);

                if (hasX3 != null)
                {
                    idX2ThuocCQs.Add(x2.XML02ID);
                    idX3ThuocCQs.Add(hasX3.XML03ID);
                }
                else
                {
                    hasX3 = x3CungNgay.FirstOrDefault(x3 => x2.NGAY_YL > x3.NGAY_KQ);
                    if (hasX3 != null)
                    {
                        //Truong hop con lai, trong cung ngay co ton tai XML2 & XMl3 ma NGAY_YL > NGAY_KQ thi bao loi 1602
                        x2.RaiseError(1602,
                            $"Bệnh nhân: {x2.XML01.HO_TEN} (Mã BN: {x2.XML01.MA_BN})\n" +
                            $"Mã ra viện: {x2.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, x2.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {x2.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(1602, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {x2.MA_THUOC}|{x2.TEN_THUOC}, được kê thuốc vào:{x2.NGAY_YL:dd/MM/yyyy HH:mm}, sau thời điểm Y lệnh dịch vụ CT/MRI, Dịch vụ:{hasX3.TEN_DICH_VU}, y lệnh: {hasX3.NGAY_YL:dd/MM/yyyy HH:mm}, thực hiện Y Lệnh:{hasX3.NGAY_TH_YL?.ToString("dd/MM/yyyy HH:mm") ?? ""}"
                            );
                    }
                }
            }
            //Neu khong co XML3 cung ngay ma chi co XML2 thi bao loi 1604
            //1604	1	XML2	MA_THUOC	Kê thuốc cản quang mà không thực hiện Chụp CT, MRI có tiêm thuốc cản quang trong cùng ngày
            else
            {
                x2.RaiseError(1604
                    , $"Bệnh nhân: {x2.XML01.HO_TEN} (Mã BN: {x2.XML01.MA_BN})\n" +
                        $"Mã ra viện: {x2.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, x2.XML01.MA_LOAI_RV.ToString())}\n" +
                        $"Mã bệnh chính: {x2.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        $"Nội dung lỗi: {(errorlist.TryGetValue(1604, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        $"Chi tiết:" +
                        $"Bệnh nhân được kê: {x2.MA_THUOC}|{x2.TEN_THUOC}, được kê thuốc vào:{x2.NGAY_YL:dd/MM/yyyy HH:mm}, Nhưng trong ngày đó không thấy thực hiện dịch vụ chụp CT, MRI có tiêm thuốc cản quang"
                    );
            }
        }

        All_XML02_ThuocCanQuangs = All_XML02_ThuocCanQuangs.Where(x => !idX2ThuocCQs.Contains(x.XML02ID)).ToList();
        All_XML03_ThuocCanQuangs = All_XML03_ThuocCanQuangs.Where(x => !idX3ThuocCQs.Contains(x.XML03ID)).ToList();

        idX2ThuocCQs = new();
        idX3ThuocCQs = new();

        foreach (var x3 in All_XML03_ThuocCanQuangs)
        {
            var x2CungNgay = All_XML02_ThuocCanQuangs.Where(x2 => x3.NGAY_TH_YL.GetValueOrDefault().Date == x2.NGAY_YL.GetValueOrDefault().Date && !idX2ThuocCQs.Contains(x2.XML02ID));

            #region Linhist removed 19/03/26 - case này bị lỗi null hasX2
            ////Neu co XML3 cung ngay
            //    //Linhist changed 28/07/23
            //    //Truong hop con lai, trong cung ngay co ton tai XML2 & XMl3 ma NGAY_YL > NGAY_KQ thi bao loi 1603
            //        x3.RaiseError(1603
            ////Neu khong co XML2 cung ngay ma chi co XML3 thi bao loi 1605
            ////1605	1	XML3	MA_DICH_VU	Chụp CT, MRI có tiêm thuốc cản quang mà không Kê thuốc cản quang trong cùng ngày
            //    x3.RaiseError(1605
            #endregion
            //Neu co XML3 cung ngay
            if (x2CungNgay.Any())
            {
                //Linhist changed 28/07/23
                var hasX2 = x2CungNgay.FirstOrDefault(x2 => x2.NGAY_YL < x3.NGAY_KQ);
                if (hasX2 != null)
                {
                    idX2ThuocCQs.Add(hasX2.XML02ID);
                    idX3ThuocCQs.Add(x3.XML03ID);
                }
                else
                {
                    hasX2 = x2CungNgay.FirstOrDefault(x2 => x2.NGAY_YL > x3.NGAY_KQ);
                    if (hasX2 != null)
                    {
                        //Truong hop con lai, trong cung ngay co ton tai XML2 & XMl3 ma NGAY_YL > NGAY_KQ thi bao loi 1603
                        x3.RaiseError(1603
                            , $"Bệnh nhân: {x3.XML01.HO_TEN} (Mã BN: {x3.XML01.MA_BN})\n" +
                            $"Mã ra viện: {x3.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, x3.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {x3.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(1603, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {x3.MA_DICH_VU}|{x3.TEN_DICH_VU}, được y lệnh vào:{x3.NGAY_YL:dd/MM/yyyy HH:mm}, Thực yện Y Lệnh:{x3.NGAY_TH_YL?.ToString("dd/MM/yyyy HH:mm") ?? ""}, sau thời điểm y lệnh thuốc:{hasX2.TEN_THUOC}, y lệnh: {hasX2.NGAY_YL:dd/MM/yyyy HH:mm}"
                            );
                    }
                }
            }
            //Neu khong co XML2 cung ngay ma chi co XML3 thi bao loi 1605
            //1605	1	XML3	MA_DICH_VU	Chụp CT, MRI có tiêm thuốc cản quang mà không Kê thuốc cản quang trong cùng ngày
            else
            {
                x3.RaiseError(1605
                    , $"Bệnh nhân: {x3.XML01.HO_TEN} (Mã BN: {x3.XML01.MA_BN})\n" +
                    $"Mã ra viện: {x3.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, x3.XML01.MA_LOAI_RV.ToString())}\n" +
                    $"Mã bệnh chính: {x3.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    $"Nội dung lỗi: {(errorlist.TryGetValue(1603, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    $"Chi tiết:" +
                    $"Bệnh nhân được kê: {x3.MA_DICH_VU}|{x3.TEN_DICH_VU}, được y lệnh vào:{x3.NGAY_YL:dd/MM/yyyy HH:mm}, Thực yện Y Lệnh:{x3.NGAY_TH_YL?.ToString("dd/MM/yyyy HH:mm") ?? ""}, Nhưng trong ngày không thấy kê thuốc Cản quang"
                    );
            }
        }
        #endregion

        #region Kiểm tra công khám
        //2460	3	XML3	TYLE_TT_DV	[130] Bệnh nhân chỉ thực hiện duy nhất 1 công khám, Tỷ lệ thanh toán DV phải để 100%
        if (_Allcongkham.Count == 1 && _Allcongkham[0].TYLE_TT_DV != 100)
        {
            _Allcongkham[0].RaiseError(2460
                , $"Bệnh nhân: {_Allcongkham.First().XML01.HO_TEN} (Mã BN: {_Allcongkham.First().XML01.MA_BN})\n" +
                $"Mã ra viện: {_Allcongkham.First().XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, _Allcongkham.First().XML01.MA_LOAI_RV.ToString())}\n" +
                $"Mã bệnh chính: {_Allcongkham.First().XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                $"Nội dung lỗi: {(errorlist.TryGetValue(2460, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                $"Chi tiết:" +
                $"Bệnh nhân được kê: {_Allcongkham.First().MA_DICH_VU}|{_Allcongkham.First().TEN_DICH_VU}, đây là công khám duy nhất, tuy nhiên tỷ lệ thanh toán: {_Allcongkham.First().TYLE_TT_DV} không phải là 100%, kiểm tra lại"
                );
        }
        else if (_congkhamchinh.Any())
        {
            //1239	1	XML3	MA_DICH_VU	Công khám lần thứ 2 trở lên sai tỷ lệ thanh toán
           
           
            int i = 0;
            foreach (var congkc in _congkhamchinh)
            {
                i++;
                if (i >= 2)
                {
                    congkc.RaiseError(1239
                        , $"Bệnh nhân: {congkc.XML01.HO_TEN} (Mã BN: {congkc.XML01.MA_BN})\n" +
                        $"Mã ra viện: {congkc.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, congkc.XML01.MA_LOAI_RV.ToString())}\n" +
                        $"Mã bệnh chính: {congkc.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        $"Nội dung lỗi: {(errorlist.TryGetValue(1239, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        $"Chi tiết:" +
                        $"Bệnh nhân được kê: {congkc.MA_DICH_VU}|{congkc.TEN_DICH_VU}, Đây là công khám thứ {i}, tuy nhiên tỷ lệ thanh toán: {congkc.TYLE_TT_DV}. Đề nghị kiểm tra lại"
                        );
                }

                if (All_CongKhamPhu.Any())
                {
                    //1229	1	XML3	NGAY_YL	Thời gian khám chính sau hoặc trùng thời gian khám phụ
                    //750	1	XML3	MA_DICH_VU	Trùng công khám chữa bệnh trong một lần đi khám
                    foreach (var a in All_CongKhamPhu)
                    {
                        if (a.NGAY_TH_YL.HasValue)
                        {
                            if (a.NGAY_TH_YL <= congkc.NGAY_TH_YL) a.RaiseError(1229);
                        }
                        else
                        {
                            if (a.NGAY_YL <= congkc.NGAY_YL) a.RaiseError(1229);
                        }
                        if (a.MA_DICH_VU == congkc.MA_DICH_VU) a.RaiseError(750);
                    }
                }
                //1984	0	XML2	NGAY_YL	[130] Y lệnh thuốc trước thời gian khám bệnh
                //1985    0   XML3 NGAY_YL[130] Y lệnh dịch vụ trước thời gian khám bệnh
                //1986    0   XML3 NGAY_YL[130] Y lệnh vật tư trước thời gian khám bệnh
                //1996	0	XML3	NGAY_YL	[130] Y lệnh vật tư trùng thời gian khám bệnh
                //1997    0   XML3 NGAY_YL[130] Y lệnh dịch vụ trùng thời gian khám bệnh
                //1998    0   XML2 NGAY_YL[130] Y lệnh thuốc trùng thời gian khám bệnh
                //haimaster update code 20/6/2026
                    foreach (var T in xml.XML02s)
                    {
                        if (congkc.NGAY_TH_YL.HasValue)
                        {
                            if (T.NGAY_YL < congkc.NGAY_TH_YL) T.RaiseError(1984);
                            if (T.NGAY_YL == congkc.NGAY_TH_YL) T.RaiseError(1998);
                        }
                        else
                        {
                            if (T.NGAY_YL < congkc.NGAY_YL) T.RaiseError(1984);
                            if (T.NGAY_YL == congkc.NGAY_YL) T.RaiseError(1998);
                        }    
                    }

                    foreach (var T in xml.XML03s)
                    {
                        if (T.MA_NHOM != 13)
                        {
                            if (congkc.NGAY_TH_YL.HasValue)
                            {
                                if (T.NGAY_YL < congkc.NGAY_TH_YL) T.RaiseError(1985);
                                if (T.NGAY_YL == congkc.NGAY_TH_YL) T.RaiseError(1997);
                            }
                            else
                            {
                                if (T.NGAY_YL < congkc.NGAY_YL) T.RaiseError(1985);
                                if (T.NGAY_YL == congkc.NGAY_YL) T.RaiseError(1997);
                            }
                        }
                    }

            }
        }
        #endregion

        #region Kiểm tra các dịch vụ đẻ
        //2459	3	XML3	NGAY_YL	[130] Ngày kết quả của Dịch vụ Theo dõi nhịp tim thai và cơn co tử cung bằng monitor sản khoa không được lớn hơn hoặc bằng thời gian thực hiện dịch vụ đỡ đẻ
        //1243	1	XML3	MA_DICH_VU	Thực hiện theo dõi tim thai và cơn co tử cung bàng monitor sản nhi trong theo dõi cuộc đẻ nhiều hơn 1 lần/ ngày điều trị
        var ngaytheodoimonitor = DateTime.MinValue;
        if (_dvdode.Any() && _theodoimonitorSK.Any())
        {
            foreach (var t in _theodoimonitorSK)
            {
                foreach (var v in _dvdode)
                {
                    if (t.NGAY_KQ >= v.NGAY_TH_YL)
                    {
                        t.RaiseError(2459);
                    }
                }
                if(t.NGAY_TH_YL.GetValueOrDefault().Date==ngaytheodoimonitor.Date)
                {
                    t.RaiseError(1243);
                }
                else
                {
                    ngaytheodoimonitor = t.NGAY_TH_YL.GetValueOrDefault().Date;
                }    

            }
        }
        #endregion

        #region Giường Ngoại
        if (All_GiuongNgoais.Any())
        {
            //Linhist changed 01/07/22 kiem tra dich vu phau thuat theo nhom 8
            //Linhist added 25/10/22 them dieu kien benh bong (T20 - T25) thi ko can kiem tra
            if (!xml.XML03s.Any(x => x.MA_NHOM == 8 || Uti.CodeMatch(x.MA_BENH ?? string.Empty, "T20.*;T21.*;T22.*;T23.*;T24.*;T25.*;T26.*;T27.*;T28.*;T29.*;T30.*;T31.*;T32.*")))
            {
                foreach (var xml3 in All_GiuongNgoais)
                {
                    xml3.RaiseError(941);
                }
            }

            //Linhist added 08/11/22
            //1245	1	XML3	MA_DICH_VU	Giường ngoại khoa không được kê quá 10 ngày
            double totalDays_GiuongNgoai = 0d;

            //Linhist added 10/04/23 haimaster update 5.5.2023
            double tongday = xml.XML03s.Count(x => x.MA_NHOM == 8);
            double totalDays_PhauThuat = 1;
            if (tongday == 0)
            {
                totalDays_PhauThuat = 1;
            }
            else
            {
                totalDays_PhauThuat = tongday;
            }
            totalDays_PhauThuat = totalDays_PhauThuat * 10d;

            var xml3GNs = All_GiuongNgoais.OrderBy(x => x.STT);
            foreach (var x3g in xml3GNs)
            {
                if (x3g.SO_LUONG != null)
                {
                    totalDays_GiuongNgoai += x3g.SO_LUONG.Value;
                }
                if (totalDays_GiuongNgoai > totalDays_PhauThuat)
                {
                    x3g.RaiseError(1245);
                }
            }

            //Linhist added 22/01/24
            //1958	1	XML3	MA_DICH_VU	Kê giường ngoại khoa trước thời gian Phẫu thuật
            //Linhist added 29/05/24 - Vao vien do Bong thi ko bao loi nay
            if (!Uti.CodeMatch(Info.MA_BENH_ICD10, "T29.*;T30.*"))
            {
                var minDate = xml.XML03s.Where(x => x.MA_NHOM == 8).Min(x => x.NGAY_TH_YL);
                if (minDate != null)
                {
                    var smallerMinDate = All_GiuongNgoais.Where(x => x.NGAY_YL < minDate);
                    foreach (var x3 in smallerMinDate)
                    {
                        x3.RaiseError(1958);
                    }
                }
            }
        }
        #endregion

        #region Phẫu thuật Giường loại
        //Linhist added 14/11/22
        //1251  1   XML3    MA_DICH_VU  Kê giường ngoại khoa sai TT39/ 2018
        if (allPhauThuatGiuongLoais.Any())
        {
            string maGiuongs = "";
            foreach (var g in dmGiuong)
            {
                if (g.Key.StartsWith("GiuongNgoai") && !allPhauThuatGiuongLoais.Any(e => $"GiuongNgoai{e}" == g.Key))
                {
                    maGiuongs += ";" + g.Value;
                }
            }
            maGiuongs = maGiuongs.Substring(1);

            var allGiuongs = xml.XML03s.Where(x => Uti.CodeContain(x.MA_DICH_VU ?? string.Empty, maGiuongs));
            foreach (var x3 in allGiuongs)
            {
                x3.RaiseError(1251);
            }
        }
        #endregion

        //Linhist added 170926
        //4226	2	XML3	NGAY_KQ	Bệnh nhân phẫu thuật lần đầu, trong ngày phải kê cả nội khoa và giường ngoại khoa
        if (xml.MA_LOAI_KCB == "03")
        {
            var fstPhauThuat = xml.XML03s.Where(x => x.MA_NHOM == 8 && x.MA_DICH_VU != null && x.NGAY_KQ != null).OrderBy(x => x.NGAY_KQ).FirstOrDefault();
            if (fstPhauThuat != null)
            {
                var fstPTDate = fstPhauThuat.NGAY_KQ.GetValueOrDefault().Date;
                if (!All_GiuongNois.Any(x => x.NGAY_YL != null && x.NGAY_YL.Value.Date == fstPTDate) || !All_GiuongNgoais.Any(x => x.MA_NHOM == 15 && x.NGAY_YL != null && x.NGAY_YL.Value.Date == fstPTDate))
                {
                    RaiseError(fstPhauThuat, true,
                            4226,
                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(4218, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân phẫu thuật lần đầu mã Dịch vụ {fstPhauThuat.MA_DICH_VU} ngày: {fstPTDate:dd/MM/yyyy} nhưng không kê đủ giường nội & ngoại khoa, đề nghị kiểm tra lại."
                        );
                }
            }
        }

        #region Các code lẻ từ 4210
        //Linhist added 08/11/23
        //1796	  1	  XML3	MA_DICH_VU	Các kỹ thuật ngôn ngữ trị liệu thanh toán 02 KT/ngày theo TT 50/2017/TT-BYT
        var grpNgonNgu = xml.XML03s.Where(x => Uti.CodeMatch(x.MA_DICH_VU ?? string.Empty, "17.0108.0260;17.0111.0265")).GroupBy(x => x.NGAY_TH_YL);
        foreach (var grp in grpNgonNgu)
        {
            var lst = grp.OrderBy(x => x.NGAY_YL).Skip(2);
            foreach (var x3 in lst)
            {
                x3.RaiseError(1796);
            }
        }
        //1798    1   XML3  MA_DICH_VU  Các kỹ thuật vận động trị liệu thanh toán 03 KT/ngày theo TT 50/2017/TT-BYT
        var grpVDTL = xml.XML03s.Where(x => Uti.CodeMatch(x.MA_DICH_VU ?? string.Empty, "17.0053.0267;17.0065.0269;17.0071.0270;17.0085.0282;17.0052.0267;17.0039.0267;17.0037.0267;17.0034.0267;17.0090.0267;17.0056.0267")).GroupBy(x => x.NGAY_TH_YL);
        foreach (var grp in grpVDTL)
        {
            var lst = grp.OrderBy(x => x.NGAY_YL).Skip(3);
            foreach (var x3 in lst)
            {
                x3.RaiseError(1798);
            }
        }

        //1797    1   XML3  MA_DICH_VU  Các kỹ thuật vật lý trị liệu thanh toán 04 KT/ngày theo TT 50/2017/TT-BYT
        var grpVLTL = xml.XML03s.Where(x => Uti.CodeMatch(x.MA_DICH_VU ?? string.Empty, "17.0001.0254;17.0007.0234;17.0008.0253;17.0009.0255;17.0026.0220;17.0004.0232;17.0011.0237;01.0156.1116;17.0012.0243")).GroupBy(x => x.NGAY_TH_YL);
        foreach (var grp in grpVLTL)
        {
            var lst = grp.OrderBy(x => x.NGAY_TH_YL).Skip(4);
            foreach (var x3 in lst)
            {
                x3.RaiseError(1797);                    
            }
        }

        //Linhist added 22/04/24
        //1982	3	XML3	SO_LUONG	BV ĐK Quốc Tế Hải Phòng không kê giường lẻ theo TT39
        var allNgayGiuong = xml.XML03s.Where(x => Uti.NumExists(x.MA_NHOM, 14, 15) && x.SO_LUONG != null);
        if (allNgayGiuong.Any())
        {
            var sumNgayGiuong = allNgayGiuong.Sum(x => x.SO_LUONG);
            if (sumNgayGiuong % 1d != 0d)
            {
                var lastNgayGiuong = allNgayGiuong.OrderByDescending(x => x.NGAY_YL).FirstOrDefault();
                lastNgayGiuong?.RaiseError(1982);
            }
        }
        #endregion

        //Linhist added 080926
        var x3cdha = File.XML03s.Where(x => x.MA_NHOM == 2 && x.NGAY_KQ != null).ToList();
        var x4cdha = File.XML04s.Where(x => x3cdha.Any(a => a.MA_DICH_VU == x.MA_DICH_VU && !(a.MA_BAC_SI == x.MA_BS_DOC_KQ && a.NGAY_KQ == x.NGAY_KQ))).ToList();
        foreach (var x in x4cdha)
        {
            var bsSai = dmBACSI.FirstOrDefault(b => x.MA_BS_DOC_KQ == b.MA_CCHN && !Uti.CodeMatch(b.PHAMVI_CM ?? string.Empty, "118.*;511"));
            if (bsSai != null)
            {
                RaiseError(x, true,
                        4218,
                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
                        $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
                        $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        $"Nội dung lỗi: {(errorlist.TryGetValue(4218, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        $"Chi tiết:" +
                        $"Bác sĩ: {bsSai.MA_CCHN} có PVCM {bsSai.PHAMVI_CM} không phù hợp chuyên khoa CĐHA, kỹ thuật y CĐHA trình độ đại học, đề nghị kiểm tra lại."
                    );
            }
        }

        //Linhist added 301026
        var x3ckxn = File.XML03s.Where(x => x.MA_NHOM == 1 && x.NGAY_KQ != null).ToList();
        var x4ckxn = File.XML04s.Where(x => x3ckxn.Any(a => a.MA_DICH_VU == x.MA_DICH_VU && !(a.MA_BAC_SI == x.MA_BS_DOC_KQ && a.NGAY_KQ == x.NGAY_KQ))).ToList();
        foreach (var x in x4ckxn)
        {
            var bsSai = dmBACSI.FirstOrDefault(b => x.MA_BS_DOC_KQ == b.MA_CCHN && !Uti.CodeMatch(b.PHAMVI_CM ?? string.Empty, "122;123;124;125;501;502;503;504"));
            if (bsSai != null)
            {
                RaiseError(x, true,
                        4234,
                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
                        $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
                        $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        $"Nội dung lỗi: {(errorlist.TryGetValue(4234, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        $"Chi tiết:" +
                        $"Bác sĩ: {bsSai.MA_CCHN} có PVCM {bsSai.PHAMVI_CM} không phù hợp chuyên khoa Xét nghiệm hoặc KTY xét nghiệm trên ĐH, đề nghị kiểm tra lại."
                    );
            }
        }

        //Linhist added 100926 
        //3625	3	XML2	MA_THUOC	[3176] Thực hiện y lệnh thuốc trước khi có kết quả CLS
        //3739	3	XML2	MA_THUOC	[3176] Kê thuốc trước khi có kết quả CLS
        if (File.MA_LOAI_KCB == "01")
        {
            var allKhams = File.XML03s.Where(x => x.MA_NHOM == 13 && x.NGAY_YL != null).OrderBy(x => x.NGAY_YL).Select(x => x.NGAY_YL).ToList();
            for (var i = 0; i < allKhams.Count; i++)
            {
                var minDte = allKhams[i];
                var maxDte = i == allKhams.Count - 1 ? DateTime.MaxValue : allKhams[i + 1];
                var lastCLS = File.XML03s.Where(x => (x.MA_NHOM == 1 || x.MA_NHOM == 2 || x.MA_NHOM == 3) && x.NGAY_KQ != null && (x.NGAY_KQ >= minDte && x.NGAY_KQ < maxDte)).OrderByDescending(x => x.NGAY_KQ).FirstOrDefault();
                if (lastCLS == null) continue;

                var errYLs = File.XML02s.Where(x => x.NGAY_YL >= minDte && x.NGAY_YL < lastCLS.NGAY_KQ).ToList();
                foreach (var e in errYLs)
                {
                    RaiseError(true,
                            3739,
                                $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(3739, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{e.TEN_THUOC}, ngày y lệnh:{e.NGAY_YL} trước ngày có kết quả CLS gần nhất:" +
                                $"Dịch vụ: {lastCLS.MA_DICH_VU}|{lastCLS.TEN_DICH_VU}, ngày KQ: {lastCLS.NGAY_KQ}. Không hợp lý về mặt logic thời gian"
                            );

                }
                var errTHYLs = File.XML02s.Where(x => x.NGAY_TH_YL >= minDte && x.NGAY_TH_YL < lastCLS.NGAY_KQ).ToList();
                foreach (var e in errTHYLs)
                {
                    RaiseError(true,
                            3625,
                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(3739, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{e.TEN_THUOC}, ngày TH_YL:{e.NGAY_TH_YL} trước ngày có kết quả CLS gần nhất:" +
                            $"Dịch vụ: {lastCLS.MA_DICH_VU}|{lastCLS.TEN_DICH_VU}, ngày KQ: {lastCLS.NGAY_KQ}. Không hợp lý về mặt logic thời gian"
                            );

                }
            }
        }

        return true;
    }
}
