
using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Diagram;
using HIA.Core.Models.Files;
using HIA.Core.Models.Token;
using HIA.Data.Records;
using HIA.Services.Files;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.Net;
namespace HIA.Services.Report;

public partial class ReportHandleService
{
    private const string NHOM8_THEM = "02.0094.0321;02.0111.1798;03.1245.1823;03.1652.0751;03.2126.0884;03.2381.0305;14.0224.0751;14.0241.0864;14.0245.0864;14.0246.0742;14.0247.0864;14.0248.0864;14.0249.0844;14.0250.0852;14.0251.0852;14.0252.0801;14.0253.0757;14.0254.0757;14.0255.0755;14.0256.0843;14.0257.0848;14.0258.0754;14.0259.0753;14.0262.0751;14.0263.0751;14.0264.0751;14.0265.0751;14.0266.0865;14.0267.0750;14.0268.0752;14.0269.0750;14.0270.0750;14.0271.0865;14.0272.0744;14.0273.0747;14.0274.0747;14.0275.0758;14.0276.0752;14.0277.0865;14.0278.0865;18.0064.0177;18.0134.0019;18.0626.0608;18.0627.0146;18.0628.0081;18.0636.0171;18.0637.0171;18.0638.0171;18.0639.0172;18.0640.0171;18.0641.0171;18.0644.0171;18.0645.0171;18.0646.0171;18.0648.0172;18.0650.0088;18.0651.0088;19.0378.1823;19.0379.1823;19.0380.1823;19.0381.1823;19.0382.1823;19.0383.1823;19.0384.1823;19.0385.1823;19.0386.1823;19.0387.1823;19.0388.1823;19.0389.1823;19.0390.1823;21.0060.0890;21.0062.0891;21.0064.0885;21.0065.0887;21.0066.0886;21.0068.0888;21.0079.0801;21.0082.0843;22.0126.0092;22.0127.0091;24.0225.2041";
    private const string NHOM8_LOAI_1 = "08.0022.0252;01.0032.0299;01.0066.1888;01.0055.0114;01.0055.0114;01.0055.0114;03.0091.0300;03.0092.0299;13.0192.0103;03.2191.0898;02.0150.0114;03.2191.0898;13.0192.0103;02.0150.0114;02.0495.0196;02.0209.0194;02.0496.0195;01.0132.0209;08.0015.0252;01.0131.0209;01.0132.0209;01.0133.0209;01.0175.0196;02.0209.0194;03.0463.0230;03.0464.0230;03.0467.0230;03.0478.0230;03.0479.0230;03.0483.0230;03.0484.0230;03.0497.0230;03.0522.0230;03.0527.0230;03.0529.0230;03.0530.0230;17.0026.0220;17.0007.0234;17.0008.0253;17.0001.0254;08.0293.0230;08.0301.0230;08.0313.0230;17.0011.0237;17.0006.0231;17.0004.0232";
    private const string NHOM8_LOAI_2 = "01.0182.0118;01.0128.0209;01.0132.0209;01.0142.0209;01.0142.0209;01.0175.0196;02.0209.0194;01.0142.0209;03.0082.0209;03.0058.0209;08.0022.0252;01.0032.0299;01.0066.1888;01.0055.0114;01.0055.0114;01.0055.0114;03.0091.0300;03.0092.0299;13.0192.0103;03.2191.0898;02.0150.0114;03.2191.0898;13.0192.0103;02.0150.0114;02.0495.0196;02.0209.0194;02.0496.0195;01.0132.0209;08.0015.0252;01.0131.0209;01.0132.0209;01.0133.0209;01.0175.0196;02.0209.0194;03.0463.0230;03.0464.0230;03.0467.0230;03.0478.0230;03.0479.0230;03.0483.0230;03.0484.0230;03.0497.0230;03.0522.0230;03.0527.0230;03.0529.0230;03.0530.0230;17.0026.0220;17.0007.0234;17.0008.0253;17.0001.0254;08.0293.0230;08.0301.0230;08.0313.0230;17.0011.0237;17.0006.0231;17.0004.0232";

    public async Task<ResponseModel<byte[]>> BaoCao_TrungDichVu(string filterString)
    {
        var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
        var maCSKCB = GetMaCSKCB();

        try
        {
            #region Linhist changed 16/04/26



            #endregion

            var filterModel = JsonDefaults.Deserialize<FilterDownloadModel>(filterString);
            filterModel ??= new FilterDownloadModel()
            {
                FromDateCreate = DateTime.Now.Date,
                ToDateCreate = DateTime.Now.Date.AddDays(1).AddTicks(-1),
                MaCSKCB = maCSKCB
            };

            if (string.IsNullOrEmpty(filterModel.MaCSKCB))
            {
                filterModel.MaCSKCB = maCSKCB;
            }

            var queryable = FilesService130.BuildQueryToGetListData(filterModel);
            var data1 = await _hosoRepository.GetListAsync(queryable.FilterQueryable, x => x.OrderByDescending(x1 => x1.XML01ID));

            if (data1.Count() >= 50000)
            {
                Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin BC Trùng DV, Đơn vị: {maCSKCB} - Số lượng hồ sơ: {data1.Count()}, người lấy báo cáo:", $"{nameof(ReportHandleService)} - {nameof(AttachInnerXML)} - BC Tổng hợp");
                return new ResponseModel<byte[]>(
                    HttpStatusCode.RequestEntityTooLarge,
                    $"Report too large: {data1.Count()} records. Narrow your filter or export in smaller batches.",
                    null
                );

            }
            await AttachInnerXML(data1, "2;3");
            var allXML2 = data1.SelectMany(x => x.XML02s);
            var allXMl3 = data1.SelectMany(x => x.XML03s);

            var dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == maCSKCB);
            var dmKhoas = await _dmKhoaRepository.GetListAsync(k => k.MA_CSKCB == maCSKCB);

            IWorkbook workbook = CreateWorkBook(out ICellStyle[] cellStyles);

            string fmBacsi = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:B{dmBacsis.Count() + 5},2,FALSE),\"\")";
            string fmChucvu = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:C{dmBacsis.Count() + 5},3,FALSE),\"\")";
            string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{dmKhoas.Count() + 5},2,FALSE),\"\")";


            using (var fs = new MemoryStream())
            {
                try
                {
                    var skip_nhom8 = NHOM8_LOAI_1;
                    if (maCSKCB == "17018")
                    {
                        skip_nhom8 = skip_nhom8.Replace(";17.0011.0237;", ";");
                    }

                    var nhom8 = allXMl3.Where(x => (x.MA_NHOM == 8 || x.MA_NHOM == 18 || ContainsCode(x.MA_DICH_VU, NHOM8_THEM)) &&
                                                        !ContainsCode(x.MA_DICH_VU, skip_nhom8) &&
                                                        !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.NGUOI_THUC_HIEN) &&
                                                        Uti.IsValidDate(x.NGAY_YL, x.NGAY_KQ, x.NGAY_TH_YL) &&
                                                        x.NGAY_TH_YL.GetValueOrDefault().Date == x.NGAY_KQ.GetValueOrDefault().Date
                                                    );

                    var dataNhom8 = nhom8.SelectMany(x => (x.NGUOI_THUC_HIEN ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries).Select(e => new { Data = x, NGUOI_TH = e.Trim() }))
                                            .Where(e => !string.IsNullOrEmpty(e.NGUOI_TH)).Distinct().ToList();

                    #region Sheet 1: Loi BS DV Trung TG - Nhom 8
                    try
                    {
                        var xml3Nhom8Trung = dataNhom8.Join(dataNhom8, a => a.NGUOI_TH, b => b.NGUOI_TH, (a, b) => new { x1 = a.Data, x2 = b.Data, a.NGUOI_TH })
                                                .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x2.NGAY_KQ >= a.x1.NGAY_TH_YL && a.x2.NGAY_TH_YL <= a.x1.NGAY_KQ)
                                                .Distinct().ToList();

                        ISheet xlSheetG8 = workbook.CreateSheet("Trùng Nhóm 8 (Phẫu thuật) và nhóm 18 (Thủ thuật)");
                        int rows = 2;
                        int cols = CreateHeader(xlSheetG8, cellStyles,
                                    "Danh sách Bác sĩ & Dịch vụ trùng thời gian - Nhóm 8 (Phãu thuật) và nhóm 18 (Thủ Thuật)", // Sheet title
                                    "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                    "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                    "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                    "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                    "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1",
                                    "Mã CCHN trùng", "Tên Bác sĩ trùng",
                                    "Mã Lượt khám 2", "Mã Bác sĩ TH 2", "Tên Bác sĩ TH 2", "Chức danh 2",
                                    "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                    "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                    "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                    "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2");

                        foreach (var t in xml3Nhom8Trung)
                        {
                            string? maBS1 = t.x1.NGUOI_THUC_HIEN;
                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.x2.NGUOI_THUC_HIEN;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);
                            string? maTrung = t.NGUOI_TH;
                            string[] ttTrung = GetFormular_BacSi(new string[] { fmBacsi }, ref maTrung);

                            AppendRow(xlSheetG8, cellStyles, rows++,
                                    rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                    t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                    t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                    t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ),
                                    t.NGUOI_TH, ttTrung[0],
                                    t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                    t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                    t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                    t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ)
                                );
                        }
                        SetAutoFeatures(xlSheetG8, cols, 3, 4, 11, 18, 19, 26);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV - Nhóm 8 - 18, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Nhóm 8-18: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }
                    #endregion

                    #region Sheet 2: Báo cáo trùng thời gian ra y lệnh khi đang thực hiện Phẫu thuật, thủ thuật
                    try
                    {
                        var nhom8A1 = allXMl3.Where(x => (x.MA_NHOM == 8 || x.MA_NHOM == 18 || ContainsCode(x.MA_DICH_VU, NHOM8_THEM)) &&
                                                            !ContainsCode(x.MA_DICH_VU, NHOM8_LOAI_2) &&
                                                            !string.IsNullOrEmpty(x.MA_DICH_VU) && Uti.IsValidDate(x.NGAY_YL, x.NGAY_KQ, x.NGAY_TH_YL) &&
                                                            x.NGAY_TH_YL.GetValueOrDefault().Date == x.NGAY_KQ.GetValueOrDefault().Date
                                                        );
                        var dataNhom8A1 = nhom8A1.SelectMany(x => (x.NGUOI_THUC_HIEN ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries).Select(e => new { Data = x, NGUOI_TH = e.Trim() }))
                                                    .Where(e => !string.IsNullOrEmpty(e.NGUOI_TH)).Distinct().ToList();

                        var dataNhomKhac = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_BAC_SI) && !ContainsCode(x.MA_DICH_VU, NHOM8_LOAI_2) && Uti.IsValidDate(x.NGAY_YL))
                                                    .Distinct().ToList();

                        var datatrungnhom8vaYL = dataNhom8A1.Join(dataNhomKhac, a => a.NGUOI_TH, b => b.MA_BAC_SI.Trim(), (a, b) => new { Nhom8 = a.Data, NhomK = b })
                                                            .Where(g => g.NhomK.MA_LK != g.Nhom8.MA_LK && g.NhomK.NGAY_YL >= g.Nhom8.NGAY_TH_YL && g.NhomK.NGAY_YL <= g.Nhom8.NGAY_KQ)
                                                            .Distinct().ToList();

                        ISheet xlSheetG85 = workbook.CreateSheet("Bác sĩ ra Y lệnh dịch vụ trong quá trình thực hiện Phẫu thuật, Thủ thuật");
                        int rowss = 2;
                        int colss = CreateHeader(xlSheetG85, cellStyles,
                                    "Danh sách Bác sĩ Bác sĩ ra Y lệnh dịch vụ trong quá trình thực hiện Phẫu thuật, Thủ thuật", // Sheet title
                                    "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                    "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                    "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                    "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                    "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1",
                                    "Mã Lượt khám 2", /*"/*Mã Bác sĩ TH 2"*/ "Mã bác sĩ Y lệnh 2", "Tên Bác sĩ YL 2", "Chức danh 2",
                                    "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                    "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                    "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                    "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Mã Vật Tư 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2");

                        foreach (var t in datatrungnhom8vaYL)
                        {
                            string? maBS1 = t.Nhom8.NGUOI_THUC_HIEN;
                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.NhomK.MA_BAC_SI;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                            AppendRow(xlSheetG85, cellStyles, rowss++,
                                    rowss - 2, t.Nhom8.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.Nhom8.XML01.MA_BN, t.Nhom8.XML01.HO_TEN,
                                    t.Nhom8.XML01.MA_THE_BHYT, GetNgayThang(t.Nhom8.XML01.NGAY_SINH, true), GetType_GioiTinh(t.Nhom8.XML01.GIOI_TINH),
                                    t.Nhom8.MA_KHOA, string.Format(fmKhoa, t.Nhom8.MA_KHOA), t.Nhom8.MA_NHOM,
                                    t.Nhom8.MA_DICH_VU, t.Nhom8.TEN_DICH_VU, GetNgayThang(t.Nhom8.NGAY_YL), GetNgayThang(t.Nhom8.NGAY_TH_YL), GetNgayThang(t.Nhom8.NGAY_KQ),
                                    t.NhomK.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.NhomK.XML01.MA_BN, t.NhomK.XML01.HO_TEN,
                                    t.NhomK.XML01.MA_THE_BHYT, GetNgayThang(t.NhomK.XML01.NGAY_SINH, true), GetType_GioiTinh(t.NhomK.XML01.GIOI_TINH),
                                    t.NhomK.MA_KHOA, string.Format(fmKhoa, t.NhomK.MA_KHOA), t.NhomK.MA_NHOM,
                                    t.NhomK.MA_DICH_VU, t.NhomK.TEN_DICH_VU, t.NhomK.MA_VAT_TU, GetNgayThang(t.NhomK.NGAY_YL), GetNgayThang(t.NhomK.NGAY_TH_YL), GetNgayThang(t.NhomK.NGAY_KQ)
                                );
                        }
                        SetAutoFeatures(xlSheetG85, colss, 3, 4, 11, 18, 19, 26);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Y lệnh trong quá trình PT-TT: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }
                    #endregion

                    #region Sheet 3: Báo cáo trùng thời gian thực hiện y lệnh khi đang thực hiện Phẫu thuật, thủ thuật
                    try
                    {

                        var dataNhomKhac2 = allXMl3.Where(x => !Uti.NumExists(x.MA_NHOM, 8, 13, 14, 15, 18) && !string.IsNullOrEmpty(x.NGUOI_THUC_HIEN) && Uti.IsValidDate(x.NGAY_TH_YL))
                                                    .SelectMany(x => (x.NGUOI_THUC_HIEN ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries).Select(e => new { Data = x, NGUOI_TH = e.Trim() }))
                                                    .Where(e => !string.IsNullOrEmpty(e.NGUOI_TH)).Distinct().ToList();

                        var datatrungnhom8vaYL2 = dataNhom8.Join(dataNhomKhac2, a => a.NGUOI_TH, b => b.NGUOI_TH, (a, b) => new { Nhom8 = a.Data, NhomK = b.Data, a.NGUOI_TH })
                                                            .Where(g => g.NhomK.MA_DICH_VU != g.Nhom8.MA_DICH_VU && g.NhomK.NGAY_KQ >= g.Nhom8.NGAY_TH_YL && g.NhomK.NGAY_TH_YL <= g.Nhom8.NGAY_KQ)
                                                            .Distinct().ToList();

                        ISheet xlSheetG86 = workbook.CreateSheet("Bác sĩ TH Y lệnh dịch vụ khác trong quá trình thực hiện Phẫu thuật, Thủ thuật");
                        int rowsss = 2;
                        int colsss = CreateHeader(xlSheetG86, cellStyles,
                                    "Danh sách Bác sĩ Bác sĩ thực hiện Y lệnh dịch vụ trong quá trình thực hiện Phẫu thuật, Thủ thuật", // Sheet title
                                    "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                    "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                    "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                    "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                    "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Mã Vật tư 1", "Tên Vật tư 1",
                                    "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1",
                                    "Mã CCHN trùng", "Tên Bác sĩ trùng",
                                    "Mã Lượt khám 2", "Mã Bác sĩ TH 2", "Tên Bác sĩ YL 2", "Chức danh 2",
                                    "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                    "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                    "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                    "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Mã Vật tư 2", "Tên Vật tư 2",
                                    "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2");

                        foreach (var t in datatrungnhom8vaYL2)
                        {
                            string? maBS1 = t.Nhom8.NGUOI_THUC_HIEN;
                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.NhomK.NGUOI_THUC_HIEN;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);
                            string? maTrung = t.NGUOI_TH;
                            string[] ttTrung = GetFormular_BacSi(new string[] { fmBacsi }, ref maTrung);

                            AppendRow(xlSheetG86, cellStyles, rowsss++,
                                    rowsss - 2, t.Nhom8.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.Nhom8.XML01.MA_BN, t.Nhom8.XML01.HO_TEN,
                                    t.Nhom8.XML01.MA_THE_BHYT, GetNgayThang(t.Nhom8.XML01.NGAY_SINH, true), GetType_GioiTinh(t.Nhom8.XML01.GIOI_TINH),
                                    t.Nhom8.MA_KHOA, string.Format(fmKhoa, t.Nhom8.MA_KHOA), t.Nhom8.MA_NHOM,
                                    t.Nhom8.MA_DICH_VU, t.Nhom8.TEN_DICH_VU, t.Nhom8.MA_VAT_TU, t.Nhom8.TEN_VAT_TU,
                                    GetNgayThang(t.Nhom8.NGAY_YL), GetNgayThang(t.Nhom8.NGAY_TH_YL), GetNgayThang(t.Nhom8.NGAY_KQ),
                                    maTrung, ttTrung[0],
                                    t.NhomK.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.NhomK.XML01.MA_BN, t.NhomK.XML01.HO_TEN,
                                    t.NhomK.XML01.MA_THE_BHYT, GetNgayThang(t.NhomK.XML01.NGAY_SINH, true), GetType_GioiTinh(t.NhomK.XML01.GIOI_TINH),
                                    t.NhomK.MA_KHOA, string.Format(fmKhoa, t.NhomK.MA_KHOA), t.NhomK.MA_NHOM,
                                    t.NhomK.MA_DICH_VU, t.NhomK.TEN_DICH_VU, t.NhomK.MA_VAT_TU, t.NhomK.TEN_VAT_TU,
                                    GetNgayThang(t.NhomK.NGAY_YL), GetNgayThang(t.NhomK.NGAY_TH_YL), GetNgayThang(t.NhomK.NGAY_KQ)
                                );
                        }
                        SetAutoFeatures(xlSheetG86, colsss, 3, 4, 11, 18, 19, 26);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet TH Y lệnh trong quá trình PT-TT: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }
                    #endregion

                    #region Sheet 4: Báo cáo trùng thời gian ra y lệnh thuốc trong lúc khám bệnh
                    try
                    {

                        var dataNhomKB = allXMl3.Where(x => x.MA_NHOM == 13 && !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.NGUOI_THUC_HIEN) && Uti.IsValidDate(x.NGAY_YL, x.NGAY_TH_YL, x.NGAY_KQ))
                                                .SelectMany(x => (x.NGUOI_THUC_HIEN ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries).Select(e => new { Data = x, NGUOI_TH = e.Trim() }))
                                                .Where(e => !string.IsNullOrEmpty(e.NGUOI_TH)).Distinct().ToList();

                        var datanhomThuoc = allXML2.Where(x => !string.IsNullOrEmpty(x.MA_BAC_SI) && Uti.IsValidDate(x.NGAY_YL));

                        var dataTrungnhom = dataNhomKB.Join(datanhomThuoc, a => a.NGUOI_TH, b => b.MA_BAC_SI.Trim(), (a, b) => new { Kham = a.Data, Thuoc = b })
                                                        .Where(g => g.Kham.MA_LK != g.Thuoc.MA_LK && g.Thuoc.NGAY_YL >= g.Kham.NGAY_TH_YL && g.Thuoc.NGAY_YL <= g.Kham.NGAY_KQ)
                                                        .Distinct().ToList();

                        ISheet slSheetNhomKB = workbook.CreateSheet("Thực hiện y lệnh thuốc khi đang khám");
                        int rowssanhom = 2;
                        int colssathuoc = CreateHeader(slSheetNhomKB, cellStyles,
                                    "Danh sách Bác sĩ đang khám thực hiện y lệnh thuốc cho BN khác", // Sheet title
                                    "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                    "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                    "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                    "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                    "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1",
                                    "Mã Lượt khám 2", /*"/*Mã Bác sĩ TH 2"*/ "Mã bác sĩ Y lệnh 2", "Tên Bác sĩ YL 2", "Chức danh 2",
                                    "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                    "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                    "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                    "Mã Thuốc 2", "Tên Thuốc 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2");

                        foreach (var t in dataTrungnhom)
                        {
                            string? maBS1 = t.Kham.NGUOI_THUC_HIEN;
                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.Thuoc.MA_BAC_SI;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                            AppendRow(slSheetNhomKB, cellStyles, rowssanhom++,
                                    colssathuoc - 2, t.Kham.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.Kham.XML01.MA_BN, t.Kham.XML01.HO_TEN,
                                    t.Kham.XML01.MA_THE_BHYT, GetNgayThang(t.Kham.XML01.NGAY_SINH, true), GetType_GioiTinh(t.Kham.XML01.GIOI_TINH),
                                    t.Kham.MA_KHOA, string.Format(fmKhoa, t.Kham.MA_KHOA), t.Kham.MA_NHOM,
                                    t.Kham.MA_DICH_VU, t.Kham.TEN_DICH_VU, GetNgayThang(t.Kham.NGAY_YL), GetNgayThang(t.Kham.NGAY_TH_YL), GetNgayThang(t.Kham.NGAY_KQ),
                                    t.Thuoc.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.Thuoc.XML01.MA_BN, t.Thuoc.XML01.HO_TEN,
                                    t.Thuoc.XML01.MA_THE_BHYT, GetNgayThang(t.Thuoc.XML01.NGAY_SINH, true), GetType_GioiTinh(t.Thuoc.XML01.GIOI_TINH),
                                    t.Thuoc.MA_KHOA, string.Format(fmKhoa, t.Thuoc.MA_KHOA), t.Thuoc.MA_NHOM,
                                    t.Thuoc.MA_THUOC, t.Thuoc.TEN_THUOC, GetNgayThang(t.Thuoc.NGAY_YL), GetNgayThang(t.Thuoc.NGAY_TH_YL)
                                );
                        }
                        SetAutoFeatures(slSheetNhomKB, colssathuoc, 3, 4, 11, 18, 19, 26);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Y lệnh trong quá trình khám bệnh: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }

                    #endregion

                    #region Sheet 5: Báo cáo trùng thời gian ra y lệnh thuốc khi đang thực hiện Phẫu thuật, thủ thuật
                    try
                    {

                        var dataNhomKhac3 = allXML2.Where(x => !string.IsNullOrEmpty(x.MA_BAC_SI) && Uti.IsValidDate(x.NGAY_YL));

                        var datatrungnhom8vaYL3 = dataNhom8.Join(dataNhomKhac3, a => a.NGUOI_TH, b => b.MA_BAC_SI.Trim(), (a, b) => new { Nhom8 = a.Data, NhomK = b })
                                                            .Where(e => e.NhomK.NGAY_YL >= e.Nhom8.NGAY_TH_YL && e.NhomK.NGAY_YL <= e.Nhom8.NGAY_KQ)
                                                            .Distinct().ToList();

                        ISheet xlSheetG87 = workbook.CreateSheet("Bác sĩ ra Y lệnh thuốc trong quá trình thực hiện Phẫu thuật, Thủ thuật");
                        int rowssa = 2;
                        int colssa = CreateHeader(xlSheetG87, cellStyles,
                                    "Danh sách Bác sĩ Bác sĩ ra Y lệnh Thuốc trong quá trình thực hiện Phẫu thuật, Thủ thuật", // Sheet title
                                    "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                    "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                    "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                    "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                    "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1",
                                    "Mã Lượt khám 2", /*"/*Mã Bác sĩ TH 2"*/ "Mã bác sĩ Y lệnh 2", "Tên Bác sĩ YL 2", "Chức danh 2",
                                    "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                    "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                    "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                    "Mã Thuốc 2", "Tên Thuốc 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2");

                        foreach (var t in datatrungnhom8vaYL3)
                        {
                            string? maBS1 = t.Nhom8.NGUOI_THUC_HIEN;
                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.NhomK.MA_BAC_SI;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                            AppendRow(xlSheetG87, cellStyles, rowssa++,
                                    rowssa - 2, t.Nhom8.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.Nhom8.XML01.MA_BN, t.Nhom8.XML01.HO_TEN,
                                    t.Nhom8.XML01.MA_THE_BHYT, GetNgayThang(t.Nhom8.XML01.NGAY_SINH, true), GetType_GioiTinh(t.Nhom8.XML01.GIOI_TINH),
                                    t.Nhom8.MA_KHOA, string.Format(fmKhoa, t.Nhom8.MA_KHOA), t.Nhom8.MA_NHOM,
                                    t.Nhom8.MA_DICH_VU, t.Nhom8.TEN_DICH_VU, GetNgayThang(t.Nhom8.NGAY_YL), GetNgayThang(t.Nhom8.NGAY_TH_YL), GetNgayThang(t.Nhom8.NGAY_KQ),
                                    t.NhomK.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.NhomK.XML01.MA_BN, t.NhomK.XML01.HO_TEN,
                                    t.NhomK.XML01.MA_THE_BHYT, GetNgayThang(t.NhomK.XML01.NGAY_SINH, true), GetType_GioiTinh(t.NhomK.XML01.GIOI_TINH),
                                    t.NhomK.MA_KHOA, string.Format(fmKhoa, t.NhomK.MA_KHOA), t.NhomK.MA_NHOM,
                                    t.NhomK.MA_THUOC, t.NhomK.TEN_THUOC, GetNgayThang(t.NhomK.NGAY_YL), GetNgayThang(t.NhomK.NGAY_TH_YL)
                                );
                        }
                        SetAutoFeatures(xlSheetG87, colssa, 3, 4, 11, 18, 19, 26);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Y lệnh trong quá trình PT-TT: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }
                    #endregion

                    #region Sheet 6: Loi BS DV Trung TG - Nhom 2
                    try
                    {

                        var nhom2 = allXMl3.Where(x => (x.MA_NHOM == 2 || x.MA_NHOM == 3) &&
                                                        !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.NGUOI_THUC_HIEN) &&
                                                        Uti.IsValidDate(x.NGAY_YL, x.NGAY_KQ, x.NGAY_TH_YL) && !string.IsNullOrWhiteSpace(x.MA_MAY));

                        var dataNhom2 = nhom2.SelectMany(x => (x.NGUOI_THUC_HIEN ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries).Select(e => new { Data = x, NGUOI_TH = e.Trim() }))
                                        .Where(e => !string.IsNullOrEmpty(e.NGUOI_TH)).Distinct().ToList();

                        var xml3Nhom2Trung = dataNhom2.Join(dataNhom2, a => a.NGUOI_TH, b => b.NGUOI_TH, (a, b) => new { x1 = a.Data, x2 = b.Data, a.NGUOI_TH })
                                                .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.MA_MAY.Trim() == a.x2.MA_MAY.Trim() && a.x1.NGAY_TH_YL == a.x2.NGAY_TH_YL)
                                                .Distinct().ToList();

                        ISheet xlSheetG2 = workbook.CreateSheet("Trùng thực hiện  Nhóm 2,3 - CĐHA -TDCN");
                        int rows = 2;
                        int cols = CreateHeader(xlSheetG2, cellStyles,
                                            "Danh sách Bác sĩ thực hiện trên cùng mã máy - Nhóm 2,3 CĐHA -TDCN", // Sheet title
                                            "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                            "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                            "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                            "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                            "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1", "Mã máy 1",
                                            "Mã CCHN trùng", "Tên Bác sĩ trùng",
                                            "Mã Lượt khám 2", "Mã Bác sĩ TH 2", "Tên Bác sĩ TH 2", "Chức danh 2",
                                            "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                            "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                            "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                            "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện Y lệnh 2", "Ngày Kết quả 2", "Mã máy 2");

                        foreach (var t in xml3Nhom2Trung)
                        {
                            string? maBS1 = t.x1.NGUOI_THUC_HIEN;
                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.x2.NGUOI_THUC_HIEN;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);
                            string? maTrung = t.NGUOI_TH;
                            string[] ttTrung = GetFormular_BacSi(new string[] { fmBacsi }, ref maTrung);

                            AppendRow(xlSheetG2, cellStyles, rows++,
                                    rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                    t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                    t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                    t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ), t.x1.MA_MAY,
                                    maTrung, ttTrung[0],
                                    t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                    t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                    t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                    t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ), t.x2.MA_MAY
                                );
                        }

                        SetAutoFeatures(xlSheetG2, cols, 3, 4, 11, 18, 19, 26);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng thực hiện Nhóm 2,3: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }
                    #endregion

                    #region Sheet 7: Báo cáo trùng thời gian thực hiện y lệnh dịch vụ
                    try
                    {

                        var trungTHYL = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.NGUOI_THUC_HIEN) &&
                                                            !MatchCode(x.MA_DICH_VU, "08.0015.0252;08.0022.0252") &&
                                                            Uti.IsValidDate(x.NGAY_TH_YL) && !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15, 1, 13));

                        var dataTrungTHYL = trungTHYL.SelectMany(x => (x.NGUOI_THUC_HIEN ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries).Select(e => new { Data = x, NGUOI_TH = e.Trim() }))
                                                .Where(e => !string.IsNullOrEmpty(e.NGUOI_TH))
                                                .Distinct().ToList();

                        var dataTrung = dataTrungTHYL.Join(dataTrungTHYL, a => a.NGUOI_TH, b => b.NGUOI_TH, (a, b) => new { x1 = a.Data, x2 = b.Data, a.NGUOI_TH })
                                                .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_TH_YL == a.x2.NGAY_TH_YL)
                                                .Distinct().ToList();

                        ISheet xlSheetG8TH = workbook.CreateSheet("Trùng thời gian thực hiện XML3 dịch vụ kỹ thuật");
                        int rows = 2;
                        int cols = CreateHeader(xlSheetG8TH, cellStyles,
                                   "Danh sách Trùng thời gian thực hiện XML3 Thực hiện Y Lệnh", // Sheet title
                                   "STT", "Mã lượt khám 1", "Mã Bác sĩ thực hiện 1", "Tên Bác sĩ thực hiện 1", "Chức danh 1",
                                   "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                   "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                   "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                   "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện Y lệnh 1", "Ngày Kết quả 1", "Mã máy 1",
                                    "Mã CCHN trùng", "Tên Bác sĩ trùng",
                                   "Mã Lượt khám 2", "Mã Bác sĩ thực hiện 2", "Tên Bác sĩ thực hiện 2", "Chức danh 2",
                                   "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                   "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                   "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                   "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2", "Mã máy 2");

                        foreach (var t in dataTrung)
                        {
                            string? maBS1 = t.x1.NGUOI_THUC_HIEN;
                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.x2.NGUOI_THUC_HIEN;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);
                            string? maTrung = t.NGUOI_TH;
                            string[] ttTrung = GetFormular_BacSi(new string[] { fmBacsi }, ref maTrung);

                            AppendRow(xlSheetG8TH, cellStyles, rows++,
                                    rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                    t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                    t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                    t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ), t.x1.MA_MAY,
                                    maTrung, ttTrung[0],
                                    t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                    t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                    t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                    t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ), t.x2.MA_MAY
                                );
                        }

                        SetAutoFeatures(xlSheetG8TH, cols, 3, 4, 11, 18, 19, 26);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng thời gian thực hiện y lệnh dịch vụ: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }
                    #endregion

                    var dataTrungKB = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.MA_BAC_SI) && Uti.IsValidDate(x.NGAY_YL, x.NGAY_KQ, x.NGAY_TH_YL) && Uti.NumExists(x.MA_NHOM, 13))
                                                .Distinct().ToList();

                    #region Sheet 8: Báo cáo trùng thời gian thực hiện y lệnh khám bệnh (Trùng giờ Thực hiện)
                    try
                    {

                        var dataTrungx = dataTrungKB.Join(dataTrungKB, x1 => x1.MA_BAC_SI?.Trim(), x2 => x2.MA_BAC_SI?.Trim(), (x1, x2) => new { x1, x2 })
                                                .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL == a.x2.NGAY_YL)
                                                .Distinct().ToList();

                        ISheet xlSheetGxTH = workbook.CreateSheet("Trùng thời gian y lệnh Khám");
                        int rows = 2;
                        int cols = CreateHeader(xlSheetGxTH, cellStyles,
                                   "Danh sách Trùng thời gian y lệnh Khám Bệnh", // Sheet title
                                   "STT", "Mã lượt khám 1", "Mã Bác sĩ khám", "Tên Bác sĩ khám", "Chức danh 1",
                                   "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                   "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                   "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                   "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện Y lệnh 1", "Ngày Kết quả 1",
                                   "Mã Lượt khám 2", "Mã Bác sĩ khám", "Tên Bác sĩ khám", "Chức danh 2",
                                   "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                   "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                   "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                   "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2");

                        foreach (var t in dataTrungx)
                        {
                            string? maBS1 = t.x1.MA_BAC_SI;
                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.x2.MA_BAC_SI;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                            AppendRow(xlSheetGxTH, cellStyles, rows++,
                                    rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                    t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                    t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                    t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ),
                                    t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                    t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                    t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                    t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ)
                                );
                        }

                        SetAutoFeatures(xlSheetGxTH, cols, 3, 4, 11, 18, 19, 26);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng thời gian y lệnh khám bệnh: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }
                    #endregion

                    #region Sheet 9: Báo cáo trùng thời gian thực hiện y lệnh khám bệnh
                    try
                    {

                        var dataTrungxx = dataTrungKB.Join(dataTrungKB, x1 => x1.MA_BAC_SI?.Trim(), x2 => x2.MA_BAC_SI?.Trim(), (x1, x2) => new { x1, x2 })
                                                .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_TH_YL == a.x2.NGAY_TH_YL)
                                                .Distinct().ToList();

                        ISheet xlSheetGxTHx = workbook.CreateSheet("Trùng thời gian thực hiện YL Khám");
                        int rows = 2;
                        int cols = CreateHeader(xlSheetGxTHx, cellStyles,
                                   "Danh sách Trùng thời gian thực hiện YL Khám Bệnh", // Sheet title
                                   "STT", "Mã lượt khám 1", "Mã Bác sĩ khám", "Tên Bác sĩ khám", "Chức danh 1",
                                   "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                   "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                   "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                   "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện Y lệnh 1", "Ngày Kết quả 1",
                                   "Mã Lượt khám 2", "Mã Bác sĩ khám", "Tên Bác sĩ khám", "Chức danh 2",
                                   "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                   "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                   "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                   "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2");

                        foreach (var t in dataTrungxx)
                        {
                            string? maBS1 = t.x1.MA_BAC_SI;
                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.x2.MA_BAC_SI;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                            AppendRow(xlSheetGxTHx, cellStyles, rows++,
                                    rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                    t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                    t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                    t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ),
                                    t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                    t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                    t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                    t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ)
                                );
                        }
                        SetAutoFeatures(xlSheetGxTHx, cols, 3, 4, 11, 18, 19, 26);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng thời gian thực hiện y lệnh khám bệnh: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }

                    #endregion

                    #region Sheet 10: Báo cáo lỗi trùng giường
                    try
                    {
                        var dataGiuong = allXMl3.Where(x => (x.MA_NHOM == 14 || x.MA_NHOM == 15) && Uti.IsValidDate(x.NGAY_YL, x.NGAY_KQ))
                                                .Distinct().ToList();

                        var xml3GiuongTrung = dataGiuong.Join(dataGiuong, x1 => x1.MA_GIUONG?.Trim(), x2 => x2.MA_GIUONG?.Trim(), (x1, x2) => new { x1, x2 })
                                                .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL &&
                                                                a.x1.MA_KHOA == a.x2.MA_KHOA && (a.x1.TYLE_TT_DV > 50 || a.x2.TYLE_TT_DV > 50))
                                                .Distinct().ToList();

                        ISheet xlSheetGG = workbook.CreateSheet("Trùng Giường");
                        int rows = 2;
                        int cols = CreateHeader(xlSheetGG, cellStyles,
                                    "Danh sách Dịch vụ Giường trùng thời gian", // Sheet caption
                                    "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
                                    "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                    "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                    "Mã Khoa 1", "Tên Khoa 1",
                                    "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Giường 1", "Tỷ lệ thanh toán DV 1",
                                    "Ngày Kết quả 1", "Ngày Y lệnh 1", "Ngày thực hiện Y lệnh 1",
                                    "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
                                    "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                    "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                    "Mã Khoa 2", "Tên Khoa 2",
                                    "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Giường 2", "Tỷ lệ thanh toán DV 2",
                                    "Ngày Kết quả 2", "Ngày Y lệnh 2", "Ngày Thực hiện YL 2");

                        foreach (var t in xml3GiuongTrung)
                        {
                            string? maBS1 = t.x1.MA_BAC_SI;
                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.x2.MA_BAC_SI;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                            AppendRow(xlSheetGG, cellStyles, rows++,
                                rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA),
                                t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, t.x1.MA_GIUONG, t.x1.TYLE_TT_DV,
                                GetNgayThang(t.x1.NGAY_KQ), GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL),
                                t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA),
                                t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, t.x2.MA_GIUONG, t.x2.TYLE_TT_DV,
                                GetNgayThang(t.x2.NGAY_KQ), GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL));
                        }

                        SetAutoFeatures(xlSheetGG, cols, 3, 4, 11, 19, 20, 27);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng Giường: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }
                    #endregion

                    #region Sheet 11: Báo cáo trùng thời gian y lệnh dịch vụ
                    try
                    {
                        var dataTrungYL = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.MA_BAC_SI) && Uti.IsValidDate(x.NGAY_YL) && !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15))
                                                .Distinct().ToList();

                        var dataTrungTYL = dataTrungYL.Join(dataTrungYL, x1 => x1.MA_BAC_SI?.Trim(), x2 => x2.MA_BAC_SI?.Trim(), (x1, x2) => new { x1, x2 })
                                                .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL == a.x2.NGAY_YL)
                                                .Distinct().ToList();

                        ISheet xlSheetG9TH = workbook.CreateSheet("Trùng thời gian y lệnh XML3 dịch vụ kỹ thuật");
                        int rows = 2;
                        int cols = CreateHeader(xlSheetG9TH, cellStyles,
                                   "Danh sách Trùng thời gian XML3 Y Lệnh", // Sheet title
                                   "STT", "Mã lượt khám 1", "Mã Bác sĩ YL 1", "Tên Bác sĩ YL 1", "Chức danh 1",
                                   "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                   "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                   "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                   "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện Y lệnh 1", "Ngày Kết quả 1", "Mã máy 1",
                                   "Mã Lượt khám 2", "Mã Bác sĩ YL 2", "Tên Bác sĩ YL 2", "Chức danh 2",
                                   "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                   "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                   "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                   "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2", "Mã máy 2");

                        foreach (var t in dataTrungTYL)
                        {
                            string? maBS1 = t.x1.MA_BAC_SI;

                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.x2.MA_BAC_SI;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                            AppendRow(xlSheetG9TH, cellStyles, rows++,
                                    rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                    t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                    t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                    t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ), t.x1.MA_MAY,
                                    t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                    t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                    t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                    t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ), t.x2.MA_MAY
                                );
                        }

                        SetAutoFeatures(xlSheetG9TH, cols, 3, 4, 11, 18, 19, 26);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng thời gian y lệnh dịch vụ: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }
                    #endregion

                    #region Sheet 12: Báo cáo trùng thời gian y lệnh thuốc
                    try
                    {
                        var dataTrungYLThuoc = allXML2.Where(x => !string.IsNullOrEmpty(x.MA_THUOC) && Uti.IsValidDate(x.NGAY_YL) && Uti.NumExists(x.MA_NHOM, 4))
                                                        .Distinct().ToList();

                        var dataTrungThuoc = dataTrungYLThuoc.Join(dataTrungYLThuoc, x1 => x1.MA_BAC_SI?.Trim(), x2 => x2.MA_BAC_SI?.Trim(), (x1, x2) => new { x1, x2 })
                                                .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL == a.x2.NGAY_YL)
                                                .Distinct().ToList();

                        ISheet xlSheetThuoc = workbook.CreateSheet("Trùng thời gian y lệnh XML2 Thuốc");
                        int rows = 2;
                        int cols = CreateHeader(xlSheetThuoc, cellStyles,
                                   "Danh sách Trùng thời gian XML2 Y Lệnh", // Sheet title
                                   "STT", "Mã lượt khám 1", "Mã Bác sĩ YL 1", "Tên Bác sĩ YL 1", "Chức danh 1",
                                   "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                   "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                   "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                   "Mã Thuốc 1", "Tên Thuốc 1", "Ngày Y lệnh 1", "Ngày thực hiện Y lệnh 1",
                                   "Mã Lượt khám 2", "Mã Bác sĩ YL 2", "Tên Bác sĩ YL 2", "Chức danh 2",
                                   "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                   "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                   "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                   "Mã Thuốc 2", "Tên Thuốc 2", "Ngày Y lệnh 2", "Ngày thực hiện Y lệnh 2");

                        foreach (var t in dataTrungThuoc)
                        {
                            string? maBS1 = t.x1.MA_BAC_SI;

                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.x2.MA_BAC_SI;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                            AppendRow(xlSheetThuoc, cellStyles, rows++,
                                    rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                    t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                    t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                    t.x1.MA_THUOC, t.x1.TEN_THUOC, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL),
                                    t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                    t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                    t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                    t.x2.MA_THUOC, t.x2.TEN_THUOC, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL)
                                );
                        }

                        SetAutoFeatures(xlSheetThuoc, cols, 3, 4, 11, 18, 19, 26);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng thời gian y lệnh thuốc: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }
                    #endregion

                    #region Sheet 13: Loi BS DV Trung TG - Nhom 2,3
                    try
                    {
                        var dataNhom2MM = allXMl3.Where(x => (x.MA_NHOM == 2 || x.MA_NHOM == 3) && !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.MA_MAY) && Uti.IsValidDate(x.NGAY_TH_YL))
                                                .Distinct().ToList();

                        var xml3Nhom2TrungMM = dataNhom2MM.Join(dataNhom2MM, x1 => x1.MA_MAY?.Trim(), x2 => x2.MA_MAY?.Trim(), (x1, x2) => new { x1, x2 })
                                                .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_TH_YL == a.x2.NGAY_TH_YL)
                                                .Distinct().ToList();

                        ISheet xlSheetG2MM = workbook.CreateSheet("Mã Máy  Nhóm 2,3 - CĐHA -TDCN");
                        int rows = 2;
                        int cols = CreateHeader(xlSheetG2MM, cellStyles,
                                            "Danh sách cùng mã máy - Nhóm 2,3 CĐHA -TDCN", // Sheet title
                                            "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                            "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                            "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                            "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                            "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1", "Mã máy 1",
                                            "Mã Lượt khám 2", "Mã Bác sĩ TH 2", "Tên Bác sĩ TH 2", "Chức danh 2",
                                            "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                            "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                            "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                            "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện Y lệnh 2", "Ngày Kết quả 2", "Mã máy 2");

                        foreach (var t in xml3Nhom2TrungMM)
                        {
                            string? maBS1 = t.x1.NGUOI_THUC_HIEN;
                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.x2.NGUOI_THUC_HIEN;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                            AppendRow(xlSheetG2MM, cellStyles, rows++,
                                    rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                    t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                    t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                    t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ), t.x1.MA_MAY,
                                    t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                    t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                    t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                    t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ), t.x2.MA_MAY
                                );
                        }

                        SetAutoFeatures(xlSheetG2MM, cols, 3, 4, 11, 18, 19, 26);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Mã Máy  Nhóm 2,3 - CĐHA -TDCN: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }
                    #endregion

                    #region Sheet 14: Loi MM- Nhom 8
                    try
                    {


                        var nhom8MM = nhom8.Where(x => !string.IsNullOrEmpty(x.MA_MAY));
                        var xml3Nhom8TrungMM = nhom8MM.Join(nhom8MM, x1 => x1.MA_MAY, x2 => x2.MA_MAY, (x1, x2) => new { x1, x2 })
                                                .Where(a => a.x1.MA_LK != a.x2.MA_LK && !(a.x2.NGAY_KQ < a.x1.NGAY_TH_YL || a.x2.NGAY_TH_YL > a.x1.NGAY_KQ))
                                                .Distinct().ToList();

                        ISheet xlSheetG8MM = workbook.CreateSheet("Trùng Mã Máy Nhóm 8 (Phẫu thuật) và nhóm 18 (Thủ thuật)");
                        int rows = 2;
                        int cols = CreateHeader(xlSheetG8MM, cellStyles,
                                   "Danh sách Mã máy trùng thời gian - Nhóm 8 (Phãu thuật) và nhóm 18 (Thủ Thuật)", // Sheet title
                                   "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                   "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                   "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                   "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                   "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1", "Mã máy 1",
                                   "Mã Lượt khám 2", "Mã Bác sĩ TH 2", "Tên Bác sĩ TH 2", "Chức danh 2",
                                   "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                   "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                   "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                   "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2", "Mã máy 2");

                        foreach (var t in xml3Nhom8TrungMM)
                        {
                            string? maBS1 = t.x1.NGUOI_THUC_HIEN;
                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.x2.NGUOI_THUC_HIEN;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                            AppendRow(xlSheetG8MM, cellStyles, rows++,
                                    rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                    t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                    t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                    t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ), t.x1.MA_MAY,
                                    t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                    t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                    t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                    t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ), t.x2.MA_MAY
                                );
                        }
                        SetAutoFeatures(xlSheetG8MM, cols, 3, 4, 11, 18, 19, 26);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng Mã Máy Nhóm 8 (Phẫu thuật) và nhóm 18 (Thủ thuật): {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }
                    #endregion

                    #region Sheet 15: Báo cáo trùng thời gian y lệnh thuốc & dịch vụ
                    try
                    {
                        var dataYLThuoc = allXML2.Where(x => !string.IsNullOrEmpty(x.MA_THUOC) && Uti.IsValidDate(x.NGAY_YL) && x.MA_NHOM != 7)
                                                        .Distinct().ToList();
                        var dataYLDichvu = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.MA_BAC_SI) && Uti.IsValidDate(x.NGAY_YL) && !Uti.NumExists(x.MA_NHOM, 10, 13, 14, 15))
                                                .Distinct().ToList();

                        var dataYLTrung = dataYLThuoc.Join(dataYLDichvu, x1 => x1.MA_BAC_SI?.Trim(), x2 => x2.MA_BAC_SI?.Trim(), (x1, x2) => new { x1, x2 })
                                                .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL == a.x2.NGAY_YL)
                                                .Distinct().ToList();

                        ISheet xlSheetYLT = workbook.CreateSheet("Trùng Y lệnh XML2 & XML3");
                        int rows = 2;
                        int cols = CreateHeader(xlSheetYLT, cellStyles,
                                   "Danh sách Trùng thời gian Y Lệnh Thuốc & Dịch vụ", // Sheet title
                                   "STT", "Mã lượt khám 1", "Mã Bác sĩ YL 1", "Tên Bác sĩ YL 1", "Chức danh 1",
                                   "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                   "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                   "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                   "Mã Thuốc 1", "Tên Thuốc 1", "Ngày Y lệnh 1",
                                   "Mã Lượt khám 2", "Mã Bác sĩ YL 2", "Tên Bác sĩ YL 2", "Chức danh 2",
                                   "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                   "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                   "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                   "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2");

                        foreach (var t in dataYLTrung)
                        {
                            string? maBS1 = t.x1.MA_BAC_SI;

                            string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                            string? maBS2 = t.x2.MA_BAC_SI;
                            string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                            AppendRow(xlSheetYLT, cellStyles, rows++,
                                    rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                    t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                    t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                    t.x1.MA_THUOC, t.x1.TEN_THUOC, GetNgayThang(t.x1.NGAY_YL),
                                    t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                    t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                    t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                    t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL)
                                );
                        }

                        SetAutoFeatures(xlSheetYLT, cols, 3, 4, 11, 17, 18, 25);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng thời gian y lệnh thuốc: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }
                    #endregion

                    CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);

                    workbook.Write(fs);
                    res.Data = fs.ToArray();
                }
                catch (Exception ex)
                {
                    res = Fail(ex, nameof(BaoCao_TrungDichVu));
                    Uti.HandleCronJobLog(_logger, $"Đơn vị: {maCSKCB} lấy báo cáo -LỖI - {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                }
            }
        }
        catch (Exception ex)
        {
            res = Fail(ex, nameof(BaoCao_TrungDichVu));
            Uti.HandleCronJobLog(_logger, $"Đơn vị: {maCSKCB} lấy báo cáo -LỖI - {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
        }

        return res;
    }
}
