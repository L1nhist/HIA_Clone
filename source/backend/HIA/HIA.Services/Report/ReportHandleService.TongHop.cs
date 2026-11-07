
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
    public async Task<ResponseModel<byte[]>> BaoCao_TongHop(string filterString)
    {
        var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
        var maCSKCB = GetMaCSKCB();

        try
        {
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
                Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{data1.Count()}, người lấy báo cáo:", $"{nameof(ReportHandleService)} - {nameof(AttachInnerXML)} - BC Tổng hợp");
                return new ResponseModel<byte[]>(
                    HttpStatusCode.RequestEntityTooLarge,
                    $"Report too large: {data1.Count()} records. Narrow your filter or export in smaller batches.",
                    null
                );
            
            }
            await AttachInnerXML(data1);

            var errors = await AttachXMLError(maCSKCB, data1);
            var errCodes = errors.Select(e => e.MA_LOI).Distinct().ToList();
            var dmErrors = await GetDMErrors(maCSKCB, errCodes);
            var dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == maCSKCB);
            var dmKhoas = await _dmKhoaRepository.GetListAsync(k => k.MA_CSKCB == maCSKCB);

            IWorkbook workbook = CreateWorkBook(out ICellStyle[] cellStyles);

            var grpErrors = errors.GroupBy(x => x.MA_LOI, (k, g) => new { ErrorCode = k, Count = g.Count(), Files = g.Select(e => e.XML01ID).Distinct().Count() })
                                    .Join(dmErrors, x => x.ErrorCode, y => y.error_code, (x, y) => new
                                    {
                                        y.error_code,
                                        y.error_file,
                                        y.error_tag,
                                        y.message,
                                        y.is_alert,
                                        x.Count,
                                        x.Files
                                    })
                                    .Where(e => e.error_code != null)
                                    .OrderBy(e => e.error_file);


            string fmBacsi = $"IFERROR(INDEX('DM Bác sĩ'!B:B, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\")";
            string fmChucvu = $"IFERROR(INDEX('DM Bác sĩ'!C:C, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\")";
            string fmKhoaTH = $"IFERROR(INDEX('DM Bác sĩ'!D:D, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\")";
            string fmKhoaTen = $"IFERROR(INDEX('DM Bác sĩ'!E:E, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\")";
            string fmBHXH = $"IFERROR(INDEX('DM Bác sĩ'!F:F, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\")";

            string fmMaNV = $"IFERROR(INDEX('DM Bác sĩ'!A:A, MATCH(\"{{0}}\", 'DM Bác sĩ'!F:F, 0)),\"{{0}}\")";
            string fmTenNV = $"IFERROR(INDEX('DM Bác sĩ'!B:B, MATCH(\"{{0}}\", 'DM Bác sĩ'!F:F, 0)),IFERROR(INDEX('DM Bác sĩ'!B:B, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\"))";
            string fmChucNV = $"IFERROR(INDEX('DM Bác sĩ'!C:C, MATCH(\"{{0}}\", 'DM Bác sĩ'!F:F, 0)),IFERROR(INDEX('DM Bác sĩ'!C:C, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\"))";
            string fmKhoaNV = $"IFERROR(INDEX('DM Bác sĩ'!D:D, MATCH(\"{{0}}\", 'DM Bác sĩ'!F:F, 0)),IFERROR(INDEX('DM Bác sĩ'!D:D, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\"))";
            string fmTKNV = $"IFERROR(INDEX('DM Bác sĩ'!E:E, MATCH(\"{{0}}\", 'DM Bác sĩ'!F:F, 0)),IFERROR(INDEX('DM Bác sĩ'!E:E, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\"))";
            string fmBHNV = $"IFERROR(INDEX('DM Bác sĩ'!F:F, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"{{0}}\")";

            string fmKhoa = $"$=IFERROR(INDEX('DM Khoa'!B:B, MATCH(\"{{0}}\", 'DM Khoa'!A:A, 0)),\"\")";

            string fmError = $"$=IFERROR(INDEX('Tổng Hợp Lỗi'!D:D, MATCH(\"{{0}}\", 'Tổng Hợp Lỗi'!B:B, 0)),\"\")";
            string fmEtype = $"$=IFERROR(INDEX('Tổng Hợp Lỗi'!C:C, MATCH(\"{{0}}\", 'Tổng Hợp Lỗi'!B:B, 0)),\"\")";
            string fmEtag = $"$=IFERROR(INDEX('Tổng Hợp Lỗi'!F:F, MATCH(\"{{0}}\", 'Tổng Hợp Lỗi'!B:B, 0)),\"\")";

            using (var fs = new MemoryStream())
            {
                #region Sheet 1: DS File Bao cao
                var grpFiles = data1.OrderBy(x => x.FileName);
                ISheet xlSheetF = workbook.CreateSheet("DS File Báo cáo");

                int rows = 2;
                int cols = CreateHeader(xlSheetF, cellStyles,
                            $"Báo Cáo Tổng Hợp {data1.Count()} Hồ Sơ - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
                            "STT", "Tên File", "Số lỗi", "Mã CSKCB", "Mã đối tượng KCB",
                            "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã bệnh chính", "Mã bệnh kèm theo", "Mã bệnh YHCT",
                            "Mã Thẻ", "Ngày sinh", "Giới tính", "Địa chỉ",
                            "Mã Khoa", "Tên Khoa",
                            "Ngày vào", "Ngày ra", "Ngày lập");

                foreach (var x in grpFiles)
                {
                    AppendRow(xlSheetF, cellStyles, rows++,
                                rows - 2, x.FileName, errors?.Count(e => e.XML01ID == x.XML01ID) ?? 0, x.MA_CSKCB,x.MA_DOITUONG_KCB,
                                x.MA_LK, x.MA_BN, x.HO_TEN, x.MA_BENH_CHINH, x.MA_BENH_KT,x.MA_BENH_YHCT,
                                x.MA_THE_BHYT, GetNgayThang(x.NGAY_SINH, true), GetType_GioiTinh(x.GIOI_TINH), x.DIA_CHI,
                                x.MA_KHOA, GetFormular_TenKhoa(fmKhoa, x.MA_KHOA, true),
                                GetNgayThang(x.NGAY_VAO), GetNgayThang(x.NGAY_RA), GetNgayThang(x.CreateDate));
                }

                SetAutoFeatures(xlSheetF, cols, -1, 9, 14);
                #endregion

                #region Sheet 2: Tong hop Loi
                ISheet xlSheetE = workbook.CreateSheet("Tổng Hợp Lỗi");
                rows = 2;
                cols = CreateHeader(xlSheetE, cellStyles,
                                    "Tổng hợp số lỗi trong file hồ sơ", // Sheet title
                                    "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi", "Loại File lỗi", "Lỗi tại Thẻ", "Số lỗi", "Số File lỗi");

                foreach (var x2 in grpErrors)
                {
                    AppendRow(xlSheetE, cellStyles, rows++,
                                rows - 2, x2.error_code, x2.is_alert ? "CẢNH BÁO" : "BÁO LỖI", x2.message, x2.error_file, x2.error_tag, x2.Count, x2.Files);
                }

                SetAutoFeatures(xlSheetE, cols, 2, -3);
                #endregion


                #region Sheet 3: DS Loi XML
                ISheet xlSheet1 = workbook.CreateSheet("Lỗi XML1");
                rows = 2;
                cols = CreateHeader(xlSheet1, cellStyles,
                        "Danh sách lỗi XML1", // Sheet title
                        "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                        "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân", "Số CCCD", "Ngày sinh", "Giới tính", "Nhóm máu",
                        "Mã quốc tịch", "Mã dân tộc", "Mã nghề nghiệp", "Địa chỉ",
                        "Mã tỉnh cư trú", "Mã Huyện", "Mã xã cư trú", "Điện thoại",
                        "Mã Thẻ", "Mã ĐKKCBBĐ", "Thẻ từ", "Thẻ đến", "Ngày miễn CCT",
                        "Lý do vào viện", "Lý do vào viện nội trú", "Mã lý do vào viện nội trú",
                        "Chẩn đoán vào", "Chẩn đoán ra", "Mã bệnh chính", "Mã bệnh kèm theo", "Mã bệnh YHCT",
                        "Mã PTTT QT", "Mã đối tượng KCB", "Mã nơi đi", "Mã nơi đến", "Mã tai nạn",
                        "Ngày vào", "Ngày vào nội trú", "Ngày ra", "Giấy chuyển tuyến",
                        "Số ngày điều trị", "PP Điều trị", "Kết quả DT", "Mã loại ra viện", "Ghi chú",
                        "Ngày thanh toán", "Tiền thuốc", "Tiền VTTY",
                        "Tổng chi BV", "Tổng chi BH", "Tiền BNTT",
                        "Tiền BNCTT", "Tiền BHTT", "Tiền Nguồn khác", "Tiền BHTT GDV",
                        "Năm QT", "Tháng QT", "Mã loại KCB",
                        "Mã Khoa", "Tên Khoa", "Mã CSKCB", "Mã khu vực",
                        "Cân nặng", "Cân nặng con", "Năm năm liên tục", "ngày tái khám", "Mã hồ sơ bệnh án", "Mã TTDV"
                    );


                var x1Codes = dmErrors.Where(e => e.error_file == "XML1").Select(e => e.error_code).ToList();
                var x1Errors = errors.Where(a => x1Codes.Contains(a.MA_LOI)).ToList();
                var x1Join = data1.Join(x1Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X01 = x, XER = e })
                                        .Distinct().ToList();

                foreach (var x in x1Join)
                {
                    AppendRow(xlSheet1, cellStyles, rows++,
                            x.X01.STT, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                            x.X01.MA_LK, x.X01.MA_BN, x.X01.HO_TEN, x.X01.SO_CCCD, GetNgayThang(x.X01.NGAY_SINH), x.X01.GIOI_TINH, x.X01.NHOM_MAU,
                            x.X01.MA_QUOCTICH, x.X01.MA_DANTOC, x.X01.MA_NGHE_NGHIEP, x.X01.DIA_CHI,
                            x.X01.MATINH_CU_TRU, x.X01.MAHUYEN_CU_TRU, x.X01.MAXA_CU_TRU, x.X01.DIEN_THOAI,
                            x.X01.MA_THE_BHYT, x.X01.MA_DKBD, x.X01.GT_THE_TU, x.X01.GT_THE_DEN, x.X01.NGAY_MIEN_CCT,
                            x.X01.LY_DO_VV, x.X01.LY_DO_VNT, x.X01.MA_LY_DO_VNT,
                            x.X01.CHAN_DOAN_VAO, x.X01.CHAN_DOAN_RV, x.X01.MA_BENH_CHINH, x.X01.MA_BENH_KT, x.X01.MA_BENH_YHCT,
                            x.X01.MA_PTTT_QT, x.X01.MA_DOITUONG_KCB, x.X01.MA_NOI_DI, x.X01.MA_NOI_DEN, x.X01.MA_TAI_NAN,
                            GetNgayThang(x.X01.NGAY_VAO), GetNgayThang(x.X01.NGAY_VAO_NOI_TRU), GetNgayThang(x.X01.NGAY_RA), x.X01.GIAY_CHUYEN_TUYEN,
                            x.X01.SO_NGAY_DTRI, x.X01.PP_DIEU_TRI, x.X01.KET_QUA_DTRI, x.X01.MA_LOAI_RV, x.X01.GHI_CHU,
                            GetNgayThang(x.X01.NGAY_TTOAN), Uti.GetMoneyVN(x.X01.T_THUOC), Uti.GetMoneyVN(x.X01.T_VTYT),
                            Uti.GetMoneyVN(x.X01.T_TONGCHI_BV), Uti.GetMoneyVN(x.X01.T_TONGCHI_BH), Uti.GetMoneyVN(x.X01.T_BNTT),
                            Uti.GetMoneyVN(x.X01.T_BNCCT), Uti.GetMoneyVN(x.X01.T_BHTT), Uti.GetMoneyVN(x.X01.T_NGUONKHAC), Uti.GetMoneyVN(x.X01.T_BHTT_GDV),
                            x.X01.NAM_QT, x.X01.THANG_QT, x.X01.MA_LOAI_KCB,
                            x.X01.MA_KHOA, GetFormular_TenKhoa(fmKhoa, x.X01.MA_KHOA), x.X01.MA_CSKCB, x.X01.MA_KHUVUC,
                            x.X01.CAN_NANG, x.X01.CAN_NANG_CON, x.X01.NAM_NAM_LIEN_TUC, x.X01.NGAY_TAI_KHAM, x.X01.MA_HSBA, x.X01.MA_TTDV
                    );
                }

                SetAutoFeatures(xlSheet1, cols, 2, -4, 12, -15);
                #endregion

                #region Sheet 4: DS Loi X02


                var x2Codes = dmErrors.Where(e => e.error_file == "XML2").Select(e => e.error_code).ToList();
                var x2Errors = errors.Where(a => x2Codes.Contains(a.MA_LOI)).ToList();
                var x2All = data1.Where(x => x2Errors.Any(e => e.XML01ID == x.XML01ID))
                                    .SelectMany(e => e.XML02s).ToList();
                var x2Join = x2All.Join(x2Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X02 = x, XER = e })
                                    .Distinct().ToList();

                ISheet xlSheet2 = workbook.CreateSheet("Lỗi XML02");
                rows = 2;
                cols = CreateHeader(xlSheet2, cellStyles,
                            "Danh sách lỗi của XML02", // Sheet title
                            "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                            "Thứ tự", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Mã Thẻ", "Ngày sinh", "Giới tính",
                            "Mã Khoa", "Tên Khoa",
                            "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh", "Mã BHXH", "Khoa y lệnh",
                            "Mã bệnh chính", "Mã bệnh kèm theo", "Chuẩn đoán ra viện",
                            "Mã Thuốc", "Mã Nhóm", "Tên Thuốc", "ĐVT", "Liều dùng", "Số Đăng ký","Cách dùng","Dạng bào chế","Mã Dịch Vụ", "Đường Dùng",
                            "TT Thầu", "Đơn giá", "Số lượng", "Mức hưởng", "Tỷ lệ TT", "Thành tiền",
                            "Ngày Y lệnh", "Ngày thực hiên Y Lệnh", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
                            "Tổng Chi");
            
                foreach (var x in x2Join)
                {
                    string? maBS = x.X02.MA_BAC_SI;
                    string[] ttBS = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu, fmBHXH }, ref maBS);

                    AppendRow(xlSheet2, cellStyles, rows++,
                            rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                            x.X02.STT, x.X02.XML01.MA_LK, x.X02.XML01.MA_BN, x.X02.XML01.HO_TEN,
                            x.X02.XML01.MA_THE_BHYT, GetNgayThang(x.X02.XML01.NGAY_SINH, true), GetType_GioiTinh(x.X02.XML01.GIOI_TINH),
                            x.X02.XML01.MA_KHOA, GetFormular_TenKhoa(fmKhoa, x.X02.XML01.MA_KHOA),
                            maBS, ttBS[0], ttBS[1], ttBS[2], x.X02.MA_KHOA,
                            x.X02.XML01.MA_BENH_CHINH, x.X02.XML01.MA_BENH_KT, x.X02.XML01.CHAN_DOAN_RV,
                            x.X02.MA_THUOC, x.X02.MA_NHOM, x.X02.TEN_THUOC, x.X02.DON_VI_TINH, x.X02.LIEU_DUNG, x.X02.SO_DANG_KY, x.X02.CACH_DUNG,x.X02.DANG_BAO_CHE,x.X02.MA_DICH_VU, x.X02.DUONG_DUNG,
                            x.X02.TT_THAU, x.X02.DON_GIA, x.X02.SO_LUONG, x.X02.MUC_HUONG, x.X02.TYLE_TT_BH, x.X02.THANH_TIEN_BV,
                            GetNgayThang(x.X02.NGAY_YL), GetNgayThang(x.X02.NGAY_TH_YL), GetNgayThang(x.X02.XML01.NGAY_VAO), GetNgayThang(x.X02.XML01.NGAY_RA), GetNgayThang(x.X02.XML01.NGAY_TTOAN),
                            string.Format("{0:##,##0.0#}", x.X02.XML01.T_TONGCHI_BV).Replace(".", "$").Replace(",", ".").Replace("$", ","));
                }

                SetAutoFeatures(xlSheet2, cols, 2, -4, 12, 14, 15, -19);
                #endregion

                #region Sheet 5: Loi XML3

                var x3Codes = dmErrors.Where(e => e.error_file == "XML3").Select(e => e.error_code).ToList();
                var x3Errors = errors.Where(a => x3Codes.Contains(a.MA_LOI)).ToList();
                var x3Join = data1.Where(x => x3Errors.Any(e => e.XML01ID == x.XML01ID))
                                    .SelectMany(e => e.XML03s)
                                    .Join(x3Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X03 = x, XER = e })
                                    .Distinct().ToList();

                ISheet xlSheet3 = workbook.CreateSheet("Lỗi XML03");
                rows = 2;
                cols = CreateHeader(xlSheet3, cellStyles,
                            "Danh sách lỗi của XML03", // Sheet title
                            "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                            "Thứ tự", "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Mã Thẻ", "Ngày sinh", "Giới tính",
                            "Mã Khoa", "Tên Khoa", "Phạm vi",
                            "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh", "Mã BHXH", "Khoa y lệnh", "Khoa Thực hiện", "Mã Người thực hiện", "Tên người thực hiện", "Mã BHXH",
                            "Mã Dịch vụ", "Tên Dịch vụ", "Mã Vật tư", "Tên Vật tư", "Mã nhóm", "Mã máy",
                            "Mã bệnh chính", "Mã bệnh kèm theo", "Chuẩn đoán ra viện", "Số lượng", "Mã giường",
                            "ĐVT", "TT Thầu", "Đơn giá", "Mức hưởng", "Tỷ lệ TT", "Thành tiền",
                            "Ngày Y lệnh", "Ngày thực hiện Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
                            "Tổng Chi");
            
                foreach (var x in x3Join)
                {
                    string? maBS = x.X03.MA_BAC_SI;
                    string? maBSTH = x.X03.NGUOI_THUC_HIEN;
                    string tenKTH = GetFormular_KhoaTH(fmKhoaTH, maBS);
                    string[] ttBS = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu, fmBHXH }, ref maBS);
                    string[] ttBStt = GetFormular_BacSi(new string[] { fmBacsi, fmBHXH }, ref maBSTH);
                    AppendRow(xlSheet3, cellStyles, rows++,
                            rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                            x.X03.STT, x.X03.XML01.MA_LK, x.X03.XML01.MA_BN, x.X03.XML01.HO_TEN,
                            x.X03.XML01.MA_THE_BHYT, GetNgayThang(x.X03.XML01.NGAY_SINH, true), GetType_GioiTinh(x.X03.XML01.GIOI_TINH),
                            x.X03.XML01.MA_KHOA, GetFormular_TenKhoa(fmKhoa, x.X03.XML01.MA_KHOA), x.X03.PHAM_VI,
                            maBS, ttBS[0], ttBS[1], ttBS[2], x.X03.MA_KHOA, tenKTH, maBSTH, ttBStt[0], ttBStt[1],
                            x.X03.MA_DICH_VU, x.X03.TEN_DICH_VU, x.X03.MA_VAT_TU, x.X03.TEN_VAT_TU, x.X03.MA_NHOM, x.X03.MA_MAY,
                            x.X03.XML01.MA_BENH_CHINH, x.X03.XML01.MA_BENH_KT, x.X03.XML01.CHAN_DOAN_RV, x.X03.SO_LUONG, x.X03.MA_GIUONG,
                            x.X03.DON_VI_TINH, x.X03.TT_THAU, x.X03.DON_GIA_BV, x.X03.MUC_HUONG, x.X03.TYLE_TT_BH, x.X03.THANH_TIEN_BV,
                            GetNgayThang(x.X03.NGAY_YL), GetNgayThang(x.X03.NGAY_TH_YL), GetNgayThang(x.X03.NGAY_KQ), GetNgayThang(x.X03.XML01.NGAY_VAO), GetNgayThang(x.X03.XML01.NGAY_RA), GetNgayThang(x.X03.XML01.NGAY_TTOAN),
                            string.Format("{0:##,##0.0#}", x.X03.XML01.T_TONGCHI_BV).Replace(".", "$").Replace(",", ".").Replace("$", ","));
                }

                SetAutoFeatures(xlSheet3, cols, 2, -4, 12, 14, 15, 17, -24);
                #endregion

                #region Sheet 6: Loi XML4

                var x4Codes = dmErrors.Where(e => e.error_file == "XML4").Select(e => e.error_code).ToList();
                var x4Errors = errors.Where(a => x4Codes.Contains(a.MA_LOI)).ToList();
                var x4Join = data1.Where(x => x4Errors.Any(e => e.XML01ID == x.XML01ID))
                                    .SelectMany(e => e.XML04s)
                                    .Join(x4Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X04 = x, XER = e })
                                    .Distinct().ToList();

                ISheet xlSheet4 = workbook.CreateSheet("Lỗi XML04");
                rows = 2;
                cols = CreateHeader(xlSheet4, cellStyles,
                            "Danh sách lỗi của XML04", // Sheet title
                            "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                            "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Mã Thẻ", "Ngày sinh", "Giới tính", "Mã Dịch vụ",
                            "Mã chỉ số", "Tên chỉ số", "Giá trị", "Đơn vị đo", "Mô tả", "Kết luận", "Ngày kết quả",
                            "Mã Bác sĩ đọc kết quả", "Tên bác sĩ đọc kết quả", "Chức Danh", "Mã BHXH",
                            "Mã Khoa", "Tên Khoa",
                            "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                        );
            
                foreach (var x in x4Join)
                {
                    string? maBS = x.X04.MA_BS_DOC_KQ;

                    string[] ttBS = GetFormular_BacSi(new string[] { fmMaNV, fmTenNV, fmChucNV, fmKhoaNV, fmTKNV, fmBHNV }, ref maBS);

                    AppendRow(xlSheet4, cellStyles, rows++,
                            rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                            x.X04.XML01.MA_LK, x.X04.XML01.MA_BN, x.X04.XML01.HO_TEN,
                            x.X04.XML01.MA_THE_BHYT, GetNgayThang(x.X04.XML01.NGAY_SINH, true), GetType_GioiTinh(x.X04.XML01.GIOI_TINH), x.X04.MA_DICH_VU,
                            x.X04.MA_CHI_SO, x.X04.TEN_CHI_SO, x.X04.GIA_TRI, x.X04.DON_VI_DO, x.X04.MO_TA, x.X04.KET_LUAN, x.X04.NGAY_KQ,
                            ttBS[0], ttBS[1], ttBS[2], ttBS[5], //maBS,
                            ttBS[3], ttBS[4],
                            GetNgayThang(x.X04.XML01.NGAY_VAO), GetNgayThang(x.X04.XML01.NGAY_RA), GetNgayThang(x.X04.XML01.NGAY_TTOAN)
                    );
                }

                SetAutoFeatures(xlSheet4, cols, 2, -4, 12, 14, 15, 17, -24);
                #endregion

                #region Sheet 7: Loi XML5

                var x5Codes = dmErrors.Where(e => e.error_file == "XML5").Select(e => e.error_code).ToList();
                var x5Errors = errors.Where(a => x5Codes.Contains(a.MA_LOI)).ToList();
                var x5Join = data1.Where(x => x5Errors.Any(e => e.XML01ID == x.XML01ID))
                                    .SelectMany(e => e.XML05s)
                                    .Join(x5Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X05 = x, XER = e })
                                    .Distinct().ToList();

                ISheet xlSheet5 = workbook.CreateSheet("Lỗi XML05");
                rows = 2;
                cols = CreateHeader(xlSheet5, cellStyles,
                            "Danh sách lỗi của XML05", // Sheet title
                            "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                            "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Diễn biến Lâm Sàng", "Giai đoạn bệnh", "Hội chẩn",
                            "Phẫu thuật", "Thời điểm diễn biến lâm sàng",
                            "Mã Người thực hiện", "Tên Người thực hiện", "Mã BHXH",
                            "Mã Khoa", "Tên Khoa",
                            "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                            );
            
                foreach (var x in x5Join)
                {
                    string? maBS = x.X05.NGUOI_THUC_HIEN;
                    string[] ttBS = GetFormular_BacSi(new string[] { fmMaNV, fmTenNV, fmKhoaNV, fmTKNV, fmBHNV }, ref maBS);

                    AppendRow(xlSheet5, cellStyles, rows++,
                            rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                            x.X05.XML01.MA_LK, x.X05.XML01.MA_BN, x.X05.XML01.HO_TEN,
                            x.X05.DIEN_BIEN_LS, x.X05.GIAI_DOAN_BENH, x.X05.HOI_CHAN,
                            x.X05.PHAU_THUAT, GetNgayThang(x.X05.THOI_DIEM_DBLS),
                            ttBS[0], ttBS[1], ttBS[4], //maBS, x.X05.NGUOI_THUC_HIEN,
                            ttBS[2], ttBS[3], 
                            GetNgayThang(x.X05.XML01.NGAY_VAO), GetNgayThang(x.X05.XML01.NGAY_RA), GetNgayThang(x.X05.XML01.NGAY_TTOAN)
                    );
                }

                SetAutoFeatures(xlSheet5, cols, 2, -4, 12, 14, 15, 17, -24);
                #endregion

                #region Báo cáo XML6

                var x6Codes = dmErrors.Where(e => e.error_file == "XML6").Select(e => e.error_code).ToList();
                var x6Errors = errors.Where(a => x6Codes.Contains(a.MA_LOI)).ToList();
                var x6Join = data1.Where(x => x6Errors.Any(e => e.XML01ID == x.XML01ID))
                                    .SelectMany(e => e.XML06s)
                                    .Join(x6Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X06 = x, XER = e })
                                    .Distinct().ToList();

                ISheet xlSheet6 = workbook.CreateSheet("Lỗi XML06");
                rows = 2;
                cols = CreateHeader(xlSheet6, cellStyles,
                            "Danh sách lỗi của XML06", // Sheet title
                            "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                            "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Thời điểm khẳng định HIV", "Thời điểm nhận thuốc ARV", "Mã phác đồ điều trị", "Mã bậc phác đồ điều trị", "Mã lý do đăng ký", "Mã loại điều trị lao", "Mã phác đồ điều trị bệnh lao",
                            "Mã Cơ sở lấy mẫu xét nghiệm", "Thời điểm bắt đầu điều trị lao", "Nơi làm kết quả xét nghiệm", "Thời điểm kết thúc điều trị lao", "Nơi đầu tiên nhận thuốc ARV", "Lý do chỉ định xét nghiệm đo tải lượng virus",
                            "Thời điểm lấy mẫu", "Kết quả xét nghiệm tải lượng", "Thời điểm kết quả xét nghiệm", "Mã đối tượng", "Mã tình trạng", "Phương pháp sàng lọc lao", "Mã lần thực hiện xét nghiệm PCR", "Ngày bệnh nhân thực hiện PCR",
                            "Ngày bệnh nhân có kết quả PCR", "Mã kết quả xét nghiệm PCR", "Mã kết quả điều trị Lao", "Thời điểm ghi nhận thông tin mang thai", "Thời điểm bắt đàu điều trị CTX", "Mã xử trí", "Ngày bắt đầu xử trí ARV", "Ngày kết thúc xử lý ARV",
                            "Mã phác đồ điều trị HIV", "Mã giai đoạn lâm sàng", "Mã bậc phác đồ điều trị", "Mã nhóm đối tượng", "Số ngày thuốc ARV", "Ngày chuyển phác đồ điều trị", "Mã lý do chuyển phác đồ",
                            "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                           );
            
                foreach (var x in x6Join)
                {
                    AppendRow(xlSheet6, cellStyles, rows++,
                            rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                            x.X06.XML01.MA_LK, x.X06.XML01.MA_BN, x.X06.XML01.HO_TEN,
                            GetNgayThang(x.X06.NGAYKD_HIV), x.X06.BDDT_ARV, x.X06.MA_PHAC_DO_DIEU_TRI_BD, x.X06.MA_BAC_PHAC_DO_BD, x.X06.MA_LYDO_DTRI, x.X06.LOAI_DTRI_LAO, x.X06.PHACDO_DTRI_LAO,
                            x.X06.NOI_LAY_MAU_XN, GetNgayThang(x.X06.NGAYBD_DTRI_LAO), x.X06.NOI_XN_KD, GetNgayThang(x.X06.NGAYKT_DTRI_LAO), x.X06.NOI_BDDT_ARV, x.X06.MA_LYDO_XNTL_VR,
                            GetNgayThang(x.X06.NGAY_XN_TLVR), x.X06.KQ_XNTL_VR, GetNgayThang(x.X06.NGAY_KQ_XN_TLVR), x.X06.MA_LOAI_BN, x.X06.MA_TINH_TRANG_DK, x.X06.SANG_LOC_LAO, x.X06.LAN_XN_PCR, GetNgayThang(x.X06.NGAY_XN_PCR),
                            GetNgayThang(x.X06.NGAY_KQ_XN_PCR), x.X06.MA_KQ_XN_PCR, x.X06.KQ_DTRI_LAO, GetNgayThang(x.X06.NGAY_NHAN_TT_MANG_THAI), GetNgayThang(x.X06.NGAY_BAT_DAU_DT_CTX), x.X06.MA_XU_TRI, GetNgayThang(x.X06.NGAY_BAT_DAU_XU_TRI), GetNgayThang(x.X06.NGAY_KET_THUC_XU_TRI),
                            x.X06.MA_PHAC_DO_DIEU_TRI, x.X06.GIAI_DOAN_LAM_SANG, x.X06.MA_BAC_PHAC_DO, x.X06.NHOM_DOI_TUONG, x.X06.SO_NGAY_CAP_THUOC_ARV, x.X06.NGAY_CHUYEN_PHAC_DO, x.X06.LY_DO_CHUYEN_PHAC_DO,
                            GetNgayThang(x.X06.XML01.NGAY_VAO), GetNgayThang(x.X06.XML01.NGAY_RA), GetNgayThang(x.X06.XML01.NGAY_TTOAN)
                            );
                }

                SetAutoFeatures(xlSheet6, cols, 2, -4, 12, 14, 15, 17, -24);
                #endregion

                #region Báo cáo XML7

                var x7Codes = dmErrors.Where(e => e.error_file == "XML7").Select(e => e.error_code).ToList();
                var x7Errors = errors.Where(a => x7Codes.Contains(a.MA_LOI)).ToList();
                var x7Join = data1.Where(x => x7Errors.Any(e => e.XML01ID == x.XML01ID))
                                    .SelectMany(e => e.XML07s)
                                    .Join(x7Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X07 = x, XER = e })
                                    .Distinct().ToList();

                ISheet xlSheet7 = workbook.CreateSheet("Lỗi XML07");
                rows = 2;
                cols = CreateHeader(xlSheet7, cellStyles,
                            "Danh sách lỗi của XML07", // Sheet title
                            "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                            "Số lưu trú", "Mã Y Tế", "Mã Khoa ra viện", "Ngày vào", "Ngày ra", "Mã đình chỉ thai", "Nguyên nhân đình chỉ thai", "Thời gian đình chỉ thai", "Tuổi thai", "Chẩn đoán ra viện",
                            "Phương pháp điều trị", "Ghi chú", "Mã TTDV", "Mã BS", "Tên BS", "Ngày Chứng từ", "Mã Cha", "Mã Mẹ", "Mã thẻ tạm", "Họ tên cha", "Họ tên mẹ", "Số ngày nghỉ", "Ngoại trú từ ngày", "Ngoại trú đến ngày",
                            "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                           );
            
                foreach (var x in x7Join)
                {
                    AppendRow(xlSheet7, cellStyles, rows++,
                            rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X07.XML01.MA_BN, x.X07.XML01.HO_TEN, x.X07.XML01.MA_LK, x.X07.XML01.MA_THE_BHYT,
                            x.X07.SO_LUU_TRU, x.X07.MA_YTE, x.X07.MA_KHOA_RV, GetNgayThang(x.X07.NGAY_VAO), GetNgayThang(x.X07.NGAY_RA), x.X07.MA_DINH_CHI_THAI, x.X07.NGUYENNHAN_DINHCHI, x.X07.THOIGIAN_DINHCHI, x.X07.TUOI_THAI, x.X07.CHAN_DOAN_RV,
                            x.X07.PP_DIEUTRI, x.X07.GHI_CHU, x.X07.MA_TTDV, x.X07.MA_BS, x.X07.TEN_BS, GetNgayThang(x.X07.NGAY_CT), x.X07.MA_CHA, x.X07.MA_ME, x.X07.MA_THE_TAM, x.X07.HO_TEN_CHA, x.X07.HO_TEN_ME, x.X07.SO_NGAY_NGHI, GetNgayThang(x.X07.NGOAITRU_TUNGAY), GetNgayThang(x.X07.NGOAITRU_DENNGAY),
                           GetNgayThang(x.X07.XML01.NGAY_VAO), GetNgayThang(x.X07.XML01.NGAY_RA), GetNgayThang(x.X07.XML01.NGAY_TTOAN)
                            );
                }

                SetAutoFeatures(xlSheet7, cols, 2, -4, 12, 14, 15, 17, -24);
                #endregion

                #region Báo cáo XML8

                var x8Codes = dmErrors.Where(e => e.error_file == "XML8").Select(e => e.error_code).ToList();
                var x8Errors = errors.Where(a => x8Codes.Contains(a.MA_LOI)).ToList();
                var x8Join = data1.Where(x => x8Errors.Any(e => e.XML01ID == x.XML01ID))
                                    .SelectMany(e => e.XML08s)
                                    .Join(x8Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X08 = x, XER = e })
                                    .Distinct().ToList();

                ISheet xlSheet8 = workbook.CreateSheet("Lỗi XML08");
                rows = 2;
                cols = CreateHeader(xlSheet8, cellStyles,
                            "Danh sách lỗi của XML08", // Sheet title
                            "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                            "Mã loại KCB", "Họ tên Cha", "Họ tên mẹ", "Người giám hộ", "Đơn vị", "Ngày vào", "Ngày ra", "Chẩn đoán vào", "Chẩn đoán ra viện", "Quá trình bệnh lý", "Tóm tắt KQ", "Phương pháp điều trị", "Ngày sinh con",
                            "Ngày con chết", "Số con chết", "Kết quả điều trị", "Ghi chú", "Mã TTDV", "Ngày chứng từ", "Mã thẻ tạm",
                            "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                           );
            
                foreach (var x in x8Join)
                {

                    AppendRow(xlSheet8, cellStyles, rows++,
                            rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X08.XML01.MA_BN, x.X08.XML01.HO_TEN, x.X08.XML01.MA_LK, x.X08.XML01.MA_THE_BHYT,
                            x.X08.MA_LOAI_KCB, x.X08.HO_TEN_CHA, x.X08.HO_TEN_ME, x.X08.NGUOI_GIAM_HO, x.X08.DON_VI, GetNgayThang(x.X08.NGAY_VAO), GetNgayThang(x.X08.NGAY_RA), x.X08.CHAN_DOAN_VAO, x.X08.CHAN_DOAN_RV, x.X08.QT_BENHLY, x.X08.TOMTAT_KQ, x.X08.PP_DIEUTRI, GetNgayThang(x.X08.NGAY_SINHCON),
                            GetNgayThang(x.X08.NGAY_CONCHET), x.X08.SO_CONCHET, x.X08.KET_QUA_DTRI, x.X08.GHI_CHU, x.X08.MA_TTDV, GetNgayThang(x.X08.NGAY_CT), x.X08.MA_THE_TAM,
                            GetNgayThang(x.X08.XML01.NGAY_VAO), GetNgayThang(x.X08.XML01.NGAY_RA), GetNgayThang(x.X08.XML01.NGAY_TTOAN)
                            );
                }

                SetAutoFeatures(xlSheet8, cols, 2, -4, 12, 14, 15, 17, -24);
                #endregion

                #region Báo cáo XML9

                var x9Codes = dmErrors.Where(e => e.error_file == "XML9").Select(e => e.error_code).ToList();
                var x9Errors = errors.Where(a => x9Codes.Contains(a.MA_LOI)).ToList();
                var x9Join = data1.Where(x => x9Errors.Any(e => e.XML01ID == x.XML01ID))
                                    .SelectMany(e => e.XML09s)
                                    .Join(x9Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X09 = x, XER = e })
                                    .Distinct().ToList();

                ISheet xlSheet9 = workbook.CreateSheet("Lỗi XML09");
                rows = 2;
                cols = CreateHeader(xlSheet9, cellStyles,
                            "Danh sách lỗi của XML09", // Sheet title
                            "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã loại KCB", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                            "Mã BHXH người nuôi dưỡng", "Mã thẻ NND", "Họ tên NND", "Ngày sinh NND", "Mã DT NND", "Số CCCD NND", "Ngày cấp CCCD NND", "Nơi cấp CCCD NND", "Nơi cư trú NND", "Mã QT",
                            "Mã Tỉnh", "Mã huyện", "Mã xã", "Họ tên cha", "Mã thẻ tạm", "Họ tên con", "Giới tính", "Số con", "Lần sinh", "Số con sống", "Cân nặng con", "Ngày sinh con", "Nơi sinh con",
                            "Trình trạng con", "Sinh con phẫu thuật", "Sinh con dưới 32 tuần", "Ghi chú", "Người đỡ đẻ", "Người ghi phiếu", "Ngày chứng từ", "Số Chứng từ", "Quyển số", "Mã TTDV",
                            "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                           );
            
                foreach (var x in x9Join)
                {
                    AppendRow(xlSheet9, cellStyles, rows++,
                            rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X09.XML01.MA_LOAI_KCB, x.X09.XML01.MA_BN, x.X09.XML01.HO_TEN, x.X09.XML01.MA_LK, x.X09.XML01.MA_THE_BHYT,
                            x.X09.MA_BHXH_NND, x.X09.MA_THE_NND, x.X09.HO_TEN_NND, GetNgayThang(x.X09.NGAYSINH_NND), x.X09.MA_DANTOC_NND, x.X09.SO_CCCD_NND, GetNgayThang(x.X09.NGAYCAP_CCCD_NND), x.X09.NOICAP_CCCD_NND, x.X09.NOI_CU_TRU_NND, x.X09.MA_QUOCTICH,
                            x.X09.MATINH_CU_TRU, x.X09.MAHUYEN_CU_TRU, x.X09.MAXA_CU_TRU, x.X09.HO_TEN_CHA, x.X09.MA_THE_TAM, x.X09.HO_TEN_CON, x.X09.GIOI_TINH_CON, x.X09.SO_CON, x.X09.LAN_SINH, x.X09.SO_CON_SONG, x.X09.CAN_NANG_CON, GetNgayThang(x.X09.NGAY_SINH_CON), x.X09.NOI_SINH_CON,
                            x.X09.TINH_TRANG_CON, x.X09.SINHCON_PHAUTHUAT, x.X09.SINHCON_DUOI32TUAN, x.X09.GHI_CHU, x.X09.NGUOI_DO_DE, x.X09.NGUOI_GHI_PHIEU, GetNgayThang(x.X09.NGAY_CT), x.X09.SO, x.X09.QUYEN_SO, x.X09.MA_TTDV,
                            GetNgayThang(x.X09.XML01.NGAY_VAO), GetNgayThang(x.X09.XML01.NGAY_RA), GetNgayThang(x.X09.XML01.NGAY_TTOAN)
                            );
                }

                SetAutoFeatures(xlSheet9, cols, 2, -4, 12, 14, 15, 17, -24);
                #endregion

                #region Báo cáo XML10

                var x10Codes = dmErrors.Where(e => e.error_file == "XML10").Select(e => e.error_code).ToList();
                var x10Errors = errors.Where(a => x10Codes.Contains(a.MA_LOI)).ToList();
                var x10Join = data1.Where(x => x10Errors.Any(e => e.XML01ID == x.XML01ID))
                                    .SelectMany(e => e.XML10s)
                                    .Join(x10Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X10 = x, XER = e })
                                    .Distinct().ToList();

                ISheet xlSheet10 = workbook.CreateSheet("Lỗi XML10");
                rows = 2;
                cols = CreateHeader(xlSheet10, cellStyles,
                            "Danh sách lỗi của XML10", // Sheet title
                            "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                            "Số Seri", "Số chứng từ", "Số ngày", "Đơn vị", "Chẩn đoán ra viện", "Từ ngày", "Đến ngày", "Mã TTDV", "Tên BS", "Mã BS", "Ngày chứng từ",
                            "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                           );
            
                foreach (var x in x10Join)
                {
                    AppendRow(xlSheet10, cellStyles, rows++,
                            rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X10.XML01.MA_BN, x.X10.XML01.HO_TEN, x.X10.XML01.MA_LK, x.X10.XML01.MA_THE_BHYT,
                            x.X10.SO_SERI, x.X10.SO_CT, x.X10.SO_NGAY, x.X10.DON_VI, x.X10.CHAN_DOAN_RV, GetNgayThang(x.X10.TU_NGAY), GetNgayThang(x.X10.DEN_NGAY), x.X10.MA_TTDV, x.X10.TEN_BS, x.X10.MA_BS, x.X10.NGAY_CT,
                            GetNgayThang(x.X10.XML01.NGAY_VAO), GetNgayThang(x.X10.XML01.NGAY_RA), GetNgayThang(x.X10.XML01.NGAY_TTOAN)
                            );
                }

                SetAutoFeatures(xlSheet10, cols, 2, -4, 12, 14, 15, 17, -24);

                #endregion

                #region Báo cáo XML11

                var x11Codes = dmErrors.Where(e => e.error_file == "XML11").Select(e => e.error_code).ToList();
                var x11Errors = errors.Where(a => x11Codes.Contains(a.MA_LOI)).ToList();
                var x11Join = data1.Where(x => x11Errors.Any(e => e.XML01ID == x.XML01ID))
                                    .SelectMany(e => e.XML11s)
                                    .Join(x11Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X11 = x, XER = e })
                                    .Distinct().ToList();

                ISheet xlSheet11 = workbook.CreateSheet("Lỗi XML11");
                rows = 2;
                cols = CreateHeader(xlSheet11, cellStyles,
                            "Danh sách lỗi của XML11", // Sheet title
                            "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                            "Số Chứng Từ", "Số Seri", "Số KCB", "Đơn vị", "Mã BHXH", "Mã thẻ BHYT", "Chẩn đoán ra viện", "Phương pháp điều trị", "Mã đình chỉ thai", "Nguyên nhân đình chỉ", "Tuổi thai", "Số ngày nghỉ",
                            "Từ ngày", "Đến ngày", "Họ tên Cha", "Họ tên mẹ", "Mã TTDV", "Mã BS", "Ngày CT", "Mã thẻ tạm", "Mẫu số",
                            "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                           );
            
                foreach (var x in x11Join)
                {
                    AppendRow(xlSheet11, cellStyles, rows++,
                            rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X11.XML01.MA_BN, x.X11.XML01.HO_TEN, x.X11.XML01.MA_LK, x.X11.XML01.MA_THE_BHYT,
                            x.X11.SO_CT, x.X11.SO_SERI, x.X11.SO_KCB, x.X11.DON_VI, x.X11.MA_BHXH, x.X11.MA_THE_BHYT, x.X11.CHAN_DOAN_RV, x.X11.PP_DIEUTRI, x.X11.MA_DINH_CHI_THAI, x.X11.NGUYENNHAN_DINHCHI, x.X11.TUOI_THAI, x.X11.SO_NGAY_NGHI,
                            GetNgayThang(x.X11.TU_NGAY), GetNgayThang(x.X11.DEN_NGAY), x.X11.HO_TEN_CHA, x.X11.HO_TEN_ME, x.X11.MA_TTDV, x.X11.MA_BS, GetNgayThang(x.X11.NGAY_CT), x.X11.MA_THE_TAM, x.X11.MAU_SO,
                            GetNgayThang(x.X11.XML01.NGAY_VAO), GetNgayThang(x.X11.XML01.NGAY_RA), GetNgayThang(x.X11.XML01.NGAY_TTOAN)
                            );
                }

                SetAutoFeatures(xlSheet11, cols, 2, -4, 12, 14, 15, 17, -24);


                #endregion

                #region Báo cáo XML13

                var x13Codes = dmErrors.Where(e => e.error_file == "XML13").Select(e => e.error_code).ToList();
                var x13Errors = errors.Where(a => x13Codes.Contains(a.MA_LOI)).ToList();
                var x13Join = data1.Where(x => x13Errors.Any(e => e.XML01ID == x.XML01ID))
                                    .SelectMany(e => e.XML13s)
                                    .Join(x13Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X13 = x, XER = e })
                                    .Distinct().ToList();

                ISheet xlSheet13 = workbook.CreateSheet("Lỗi XML13");
                rows = 2;
                cols = CreateHeader(xlSheet13, cellStyles,
                            "Danh sách lỗi của XML13", // Sheet title
                            "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                            "Số hồ sơ", "Số chuyển tuyến", "Giấy chuyển tuyến", "Mã CSKCB", "Mã nơi đi", "Mã nơi đến", "Họ Tên", "Ngày sinh", "Giới tính", "Mã quốc tịch", "Mã dân tộc", "Mã nghề nghiệp", "Địa chỉ",
                            "Mã thẻ BHYT", "Giá trị thẻ đến", "Ngày vào", "Ngày vào nội trú", "Ngày ra", "Dấu hiệu LS", "Chẩn đoán ra viện", "Quá trình bệnh lý", "Tóm tắt KQ", "Phương pháp điều trị",
                            "Mã bệnh chính", "Mã bệnh YHCT", "Tên dịch vụ", "Tên thuốc", "Phương pháp điều trị", "Mã loại ra viện", "Mã lý do chuyển tuyến", "Hướng điều trị", "Phương tiện vận chuyển", "Họ tên người hộ tống",
                            "Chức dang người hộ tống", "Mã bác sĩ", "Mã TTDV",
                            "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                           );
            
                foreach (var x in x13Join)
                {
                    AppendRow(xlSheet13, cellStyles, rows++,
                            rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X13.XML01.MA_BN, x.X13.XML01.HO_TEN, x.X13.XML01.MA_LK, x.X13.XML01.MA_THE_BHYT,
                            x.X13.SO_HOSO, x.X13.SO_CHUYENTUYEN, x.X13.GIAY_CHUYEN_TUYEN, x.X13.MA_CSKCB, x.X13.MA_NOI_DI, x.X13.MA_NOI_DEN, x.X13.HO_TEN, GetNgayThang(x.X13.NGAY_SINH), x.X13.GIOI_TINH, x.X13.MA_QUOCTICH, x.X13.MA_DANTOC, x.X13.MA_NGHE_NGHIEP, x.X13.DIA_CHI,
                            x.X13.MA_THE_BHYT, x.X13.GT_THE_DEN, GetNgayThang(x.X13.NGAY_VAO), GetNgayThang(x.X13.NGAY_VAO_NOI_TRU), GetNgayThang(x.X13.NGAY_RA), x.X13.DAU_HIEU_LS, x.X13.CHAN_DOAN_RV, x.X13.QT_BENHLY, x.X13.TOMTAT_KQ, x.X13.PP_DIEUTRI,
                            x.X13.MA_BENH_CHINH, x.X13.MA_BENH_YHCT, x.X13.TEN_DICH_VU, x.X13.TEN_THUOC, x.X13.PP_DIEU_TRI, x.X13.MA_LOAI_RV, x.X13.MA_LYDO_CT, x.X13.HUONG_DIEU_TRI, x.X13.PHUONGTIEN_VC, x.X13.HOTEN_NGUOI_HT,
                            x.X13.CHUCDANH_NGUOI_HT, x.X13.MA_BAC_SI, x.X13.MA_TTDV,
                            GetNgayThang(x.X13.XML01.NGAY_VAO), GetNgayThang(x.X13.XML01.NGAY_RA), GetNgayThang(x.X13.XML01.NGAY_TTOAN)
                            );
                }

                SetAutoFeatures(xlSheet13, cols, 2, -4, 12, 14, 15, 17, -24);


                #endregion

                #region Báo cáo XML14

                var x14Codes = dmErrors.Where(e => e.error_file == "XML14").Select(e => e.error_code).ToList();
                var x14Errors = errors.Where(a => x14Codes.Contains(a.MA_LOI)).ToList();
                var x14Join = data1.Where(x => x14Errors.Any(e => e.XML01ID == x.XML01ID))
                                    .SelectMany(e => e.XML14s)
                                    .Join(x14Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X14 = x, XER = e })
                                    .Distinct().ToList();

                ISheet xlSheet14 = workbook.CreateSheet("Lỗi XML14");
                rows = 2;
                cols = CreateHeader(xlSheet14, cellStyles,
                            "Danh sách lỗi của XML14", // Sheet title
                            "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                            "Số giấy hẹn khám lại", "Mã CSKCB", "Họ tên", "Ngày sinh", "Giới tính", "Địa chỉ", "Mã thẻ BHYT", "Giá trị thẻ đến", "Ngày vào", "ngày vào nội trú", "ngày ra", "ngày hẹn khám lại", "chẩn đoán ra viện",
                            "Mã bệnh chính", "Mã bệnh kèm theo", "Mã bệnh YHCT", "Mã đối tượng KCB", "Mã bác sĩ", "Mã TTDV", "Ngày chứng tử",
                            "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                           );
            
                foreach (var x in x14Join)
                {
                    AppendRow(xlSheet14, cellStyles, rows++,
                            rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X14.XML01.MA_BN, x.X14.XML01.HO_TEN, x.X14.XML01.MA_LK, x.X14.XML01.MA_THE_BHYT,
                            x.X14.SO_GIAYHEN_KL, x.X14.MA_CSKCB, x.X14.HO_TEN, GetNgayThang(x.X14.NGAY_SINH), x.X14.GIOI_TINH, x.X14.DIA_CHI, x.X14.MA_THE_BHYT, x.X14.GT_THE_DEN, GetNgayThang(x.X14.NGAY_VAO), GetNgayThang(x.X14.NGAY_VAO_NOI_TRU), GetNgayThang(x.X14.NGAY_RA), GetNgayThang(x.X14.NGAY_HEN_KL), x.X14.CHAN_DOAN_RV,
                            x.X14.MA_BENH_CHINH, x.X14.MA_BENH_KT, x.X14.MA_BENH_YHCT, x.X14.MA_DOITUONG_KCB, x.X14.MA_BAC_SI, x.X14.MA_TTDV, GetNgayThang(x.X14.NGAY_CT),
                            GetNgayThang(x.X14.XML01.NGAY_VAO), GetNgayThang(x.X14.XML01.NGAY_RA), GetNgayThang(x.X14.XML01.NGAY_TTOAN)
                            );
                }

                SetAutoFeatures(xlSheet14, cols, 2, -4, 12, 14, 15, 17, -24);


                #endregion

                #region Báo cáo XML15

                var x15Codes = dmErrors.Where(e => e.error_file == "XML15").Select(e => e.error_code).ToList();
                var x15Errors = errors.Where(a => x15Codes.Contains(a.MA_LOI)).ToList();
                var x15Join = data1.Where(x => x15Errors.Any(e => e.XML01ID == x.XML01ID))
                                    .SelectMany(e => e.XML15s)
                                    .Join(x15Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X15 = x, XER = e })
                                    .Distinct().ToList();

                ISheet xlSheet15 = workbook.CreateSheet("Lỗi XML15");
                rows = 2;
                cols = CreateHeader(xlSheet15, cellStyles,
                            "Danh sách lỗi của XML15", // Sheet title
                            "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                            "Mã bệnh nhân", "Họ tên", "Số CCCD", "PHân loại loai theo VT", "Phân loại lao theo TS", "Phân loại lao HIV", "Phân loại Lao VK", "Loại điều trị Lao", "Ngày bắt đầu điều trị lao",
                            "Phác đồ điều trị lao", "Ngày kết thúc điều trị lao", "Kết quả điều trị Lao", "Mã CSKCB", "Ngày khẳng định HIV", "Thời điểm nhận thuốc ARV", "Ngày bắt đầu điều trị CTX",
                            "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                           );
            
                foreach (var x in x15Join)
                {
                    AppendRow(xlSheet15, cellStyles, rows++,
                            rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X15.XML01.MA_BN, x.X15.XML01.HO_TEN, x.X15.XML01.MA_LK, x.X15.XML01.MA_THE_BHYT,
                            x.X15.MA_BN, x.X15.HO_TEN, x.X15.SO_CCCD, x.X15.PHANLOAI_LAO_VITRI, x.X15.PHANLOAI_LAO_TS, x.X15.PHANLOAI_LAO_HIV, x.X15.PHANLOAI_LAO_VK, GetNgayThang(x.X15.NGAYBD_DTRI_LAO),
                            x.X15.PHACDO_DTRI_LAO, GetNgayThang(x.X15.NGAYKT_DTRI_LAO), x.X15.KET_QUA_DTRI_LAO, x.X15.MA_CSKCB, x.X15.NGAYKD_HIV, GetNgayThang(x.X15.BDDT_ARV), GetNgayThang(x.X15.NGAY_BAT_DAU_DT_CTX),
                            GetNgayThang(x.X15.XML01.NGAY_VAO), GetNgayThang(x.X15.XML01.NGAY_RA), GetNgayThang(x.X15.XML01.NGAY_TTOAN)
                            );
                }

                SetAutoFeatures(xlSheet15, cols, 2, -4, 12, 14, 15, 17, -24);


                #endregion

                CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);
                workbook.Write(fs);
                res.Data = fs.ToArray();
            }
        }
        catch (Exception ex)
        {
            res = Fail(ex, nameof(BaoCao_TongHop));
        }

        return res;
    }
}
