
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
    public async Task<ResponseModel<byte[]>> BaoCao_TrungThuocDV(string filterString)
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
            };

            filterModel.MaCSKCB = maCSKCB;
            var queryable = BuildQuery_XML01(filterModel);
            var data1 = await _hosoRepository.GetListAsync(queryable.FilterQueryable, x => x.OrderByDescending(x1 => x1.XML01ID));
            await AttachInnerXML(data1, "2;3;4;5");

            var dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == maCSKCB);
            var dmKhoas = await _dmKhoaRepository.GetListAsync(k => k.MA_CSKCB == maCSKCB);

            IWorkbook workbook = CreateWorkBook(out ICellStyle[] cellStyles);

            string fmBacsi = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:B{dmBacsis.Count() + 5},2,FALSE),\"\")";
            string fmChucvu = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:C{dmBacsis.Count() + 5},3,FALSE),\"\")";
            string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{dmKhoas.Count() + 5},2,FALSE),\"\")";

            var data2 = data1.SelectMany(x => x.XML02s);
            var data3 = data1.SelectMany(x => x.XML03s);
            var data4 = data1.SelectMany(x => x.XML04s);
            var data5 = data1.SelectMany(x => x.XML05s);

            using (var fs = new MemoryStream())
            {
                #region Sheet 1: Thuoc X02
                ISheet xlSheet2 = workbook.CreateSheet("DS Thuốc");
                int rows = 2;
                int cols = CreateHeader(xlSheet2, cellStyles,
                            "Danh sách tổng hợp Thuốc XML02", // Sheet title
                            "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Mã Thẻ", "Ngày sinh", "Giới tính","Ngày vào nội trú",
                            "Mã Khoa", "Tên Khoa", "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh",
                            "Chẩn đoán chính", "Chẩn đoán kèm theo", "Chẩn đoán ra viện",
                            "Mã Thuốc", "Mã Nhóm", "Tên Thuốc", "ĐVT", "Liều dùng",
                            "Số Đăng ký", "Cách dùng", "Mã dịch vụ", "TT Thầu", "Đơn giá", "Số lượng",
                            "Tỷ lệ TT", "Tiền BHTT", "Tiền BNCCT", "Thành tiền",
                            "Ngày Y lệnh", "Ngày TH Y lệnh", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

                foreach (var x2 in data2)
                {
                    string? maBS = x2.MA_BAC_SI;
                    string[] ttBS = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS);

                    AppendRow(xlSheet2, cellStyles, rows++,
                            rows - 2, x2.XML01.MA_LK, x2.XML01.MA_BN, x2.XML01.HO_TEN,
                            x2.XML01.MA_THE_BHYT, GetNgayThang(x2.XML01.NGAY_SINH, true), GetType_GioiTinh(x2.XML01.GIOI_TINH),GetNgayThang(x2.XML01.NGAY_VAO_NOI_TRU),
                            x2.MA_KHOA, string.Format(fmKhoa, x2.MA_KHOA), maBS, ttBS[0], ttBS[1],
                            x2.XML01.MA_BENH_CHINH, x2.XML01.MA_BENH_KT, x2.XML01.CHAN_DOAN_RV,
                            x2.MA_THUOC, x2.MA_NHOM, x2.TEN_THUOC, x2.DON_VI_TINH, x2.LIEU_DUNG,
                            x2.SO_DANG_KY, x2.CACH_DUNG,x2.MA_DICH_VU, x2.TT_THAU, x2.DON_GIA, x2.SO_LUONG,
                            x2.TYLE_TT_BH, x2.T_BHTT, x2.T_BNCCT, x2.THANH_TIEN_BV,
                            GetNgayThang(x2.NGAY_YL), GetNgayThang(x2.NGAY_TH_YL), GetNgayThang(x2.XML01.NGAY_VAO), GetNgayThang(x2.XML01.NGAY_RA), GetNgayThang(x2.XML01.NGAY_TTOAN));
                }

                SetAutoFeatures(xlSheet2, cols, 8, 10, 11, -14);
                #endregion

                #region Sheet 2: Dich vu XML3
                var data3DVs = data3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && !Uti.NumExists(x.MA_NHOM, 10, 11));

                ISheet xlSheetDV = workbook.CreateSheet("DS Dịch vụ");
                rows = 2;
                cols = CreateHeader(xlSheetDV, cellStyles,
                            "Danh sách tổng hợp Dịch vụ XML3", // Sheet caption
                            "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Mã Thẻ", "Ngày sinh", "Giới tính", "Ngày vào nội trú",
                            "Mã Khoa", "Tên Khoa", "Mã Bác Y lệnh", "Tên Bác sĩ", "Chức danh",
                            "Mã Bác Sĩ Thực hiện", "Tên Bác sĩ thực hiện", "Chức danh",
                            "Chẩn đoán chính", "Chẩn đoán kèm theo", "Chẩn đoán ra viện",
                            "Mã Dịch vụ", "Mã nhóm", "Tên Dịch vụ", "Mã giường", "Mã máy",
                            "ĐVT", "TT Thầu", "Đơn giá", "Tỷ lệ TT", "Tiền BHTT", "Tiền BNCCT", "Thành tiền",
                            "Ngày Y lệnh", "Ngày thực hiện Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

                foreach (var x3 in data3DVs)
                {
                    string? maBS = x3.MA_BAC_SI;
                    string? mabsTH = x3.NGUOI_THUC_HIEN;
                    string[] ttBS = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS);
                    string[] ttbsTH = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref mabsTH);

                    AppendRow(xlSheetDV, cellStyles, rows++,
                            rows - 2, x3.XML01.MA_LK, x3.XML01.MA_BN, x3.XML01.HO_TEN,
                            x3.XML01.MA_THE_BHYT, GetNgayThang(x3.XML01.NGAY_SINH, true), GetType_GioiTinh(x3.XML01.GIOI_TINH),GetNgayThang(x3.XML01.NGAY_VAO_NOI_TRU),
                            x3.MA_KHOA, string.Format(fmKhoa, x3.MA_KHOA), maBS, ttBS[0], ttBS[1], mabsTH, ttbsTH[0], ttbsTH[1],
                            x3.XML01.MA_BENH_CHINH, x3.XML01.MA_BENH_KT, x3.XML01.CHAN_DOAN_RV,
                            x3.MA_DICH_VU, x3.MA_NHOM, x3.TEN_DICH_VU, x3.MA_GIUONG, x3.MA_MAY,
                            x3.DON_VI_TINH, x3.TT_THAU, x3.DON_GIA_BV, x3.TYLE_TT_BH, x3.T_BHTT, x3.T_BNCCT, x3.THANH_TIEN_BV,
                            GetNgayThang(x3.NGAY_YL), GetNgayThang(x3.NGAY_TH_YL), GetNgayThang(x3.NGAY_KQ), GetNgayThang(x3.XML01.NGAY_VAO), GetNgayThang(x3.XML01.NGAY_RA), GetNgayThang(x3.XML01.NGAY_TTOAN));
                }
                SetAutoFeatures(xlSheetDV, cols, 8, 10, 11, -14);
                #endregion

                #region Sheet 3: Vat tu XML3
                var data3VTs = data3.Where(x => !string.IsNullOrEmpty(x.MA_VAT_TU) && Uti.NumExists(x.MA_NHOM, 10, 11));

                ISheet xlSheetVT = workbook.CreateSheet("DS VTYT");
                rows = 2;
                cols = CreateHeader(xlSheetVT, cellStyles, "Danh sách tổng hợp Vật tư XML3",
                            "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Mã Thẻ", "Ngày sinh", "Giới tính","Ngày vào nội trú",
                            "Mã Khoa", "Tên Khoa", "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh",
                            "Chẩn đoán chính", "Chẩn đoán kèm theo", "Chẩn đoán ra viện",
                            "Mã Vật tư", "Tên Vật tư", "ĐVT", "TT Thầu", "Đơn giá",
                            "Tỷ lệ TT", "Tiền BHTT", "Tiền BNCCT", "Thành tiền",
                            "Ngày Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

                foreach (var x3 in data3VTs)
                {
                    string? maBS = x3.MA_BAC_SI;
                    string[] ttBS = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS);

                    AppendRow(xlSheetVT, cellStyles, rows++,
                            rows - 2, x3.XML01.MA_LK, x3.XML01.MA_BN, x3.XML01.HO_TEN,
                            x3.XML01.MA_THE_BHYT, GetNgayThang(x3.XML01.NGAY_SINH, true), GetType_GioiTinh(x3.XML01.GIOI_TINH),GetNgayThang(x3.XML01.NGAY_VAO_NOI_TRU),
                            x3.MA_KHOA, string.Format(fmKhoa, x3.MA_KHOA), maBS, ttBS[0], ttBS[1],
                            x3.XML01.MA_BENH_CHINH, x3.XML01.MA_BENH_KT, x3.XML01.CHAN_DOAN_RV,
                            x3.MA_VAT_TU, x3.TEN_VAT_TU, x3.DON_VI_TINH, x3.TT_THAU, x3.DON_GIA_BV,
                            x3.TYLE_TT_BH, x3.T_BHTT, x3.T_BNCCT, x3.THANH_TIEN_BV,
                            GetNgayThang(x3.NGAY_YL), GetNgayThang(x3.NGAY_KQ), GetNgayThang(x3.XML01.NGAY_VAO), GetNgayThang(x3.XML01.NGAY_RA), GetNgayThang(x3.XML01.NGAY_TTOAN));
                }

                SetAutoFeatures(xlSheetVT, cols, 8, 10, 11, -14);
                #endregion

                #region Sheet 4: XML4
                ISheet xlSheetXML4 = workbook.CreateSheet("DS XML4");
                rows = 2;
                cols = CreateHeader(xlSheetXML4, cellStyles, "Danh sách tổng hợp XML4",
                            "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Mã Thẻ", "Ngày sinh", "Giới tính", "Ngày vào nội trú",
                            "Mã dịch vụ", "Mã chỉ số", "Tên chỉ số", "Giá Trị",
                            "Đơn vị đo", "Mô tả", "Kết luận", "Ngày kết quả",
                            "Mã BS Đọc kết quả", "Tên BS Đọc KQ", "Chức danh");

                foreach (var x4 in data4)
                {
                    string? maBS = x4.MA_BS_DOC_KQ;
                    string[] ttBS = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS);

                    AppendRow(xlSheetXML4, cellStyles, rows++,
                            rows - 2, x4.XML01.MA_LK, x4.XML01.MA_BN, x4.XML01.HO_TEN,
                            x4.XML01.MA_THE_BHYT, GetNgayThang(x4.XML01.NGAY_SINH, true), GetType_GioiTinh(x4.XML01.GIOI_TINH),GetNgayThang(x4.XML01.NGAY_VAO_NOI_TRU),
                            x4.MA_DICH_VU, x4.MA_CHI_SO, x4.TEN_CHI_SO, x4.GIA_TRI,
                            x4.DON_VI_DO, x4.MO_TA, x4.KET_LUAN, GetNgayThang(x4.NGAY_KQ),
                            maBS, ttBS[0], ttBS[1]
                        );
                }

                SetAutoFeatures(xlSheetXML4, cols, 8, 10, 11, -14);
                #endregion

                #region Sheet 5: XML5
                ISheet xlSheetXML5 = workbook.CreateSheet("DS XML5");
                rows = 2;
                cols = CreateHeader(xlSheetXML5, cellStyles, "Danh sách tổng hợp XML5",
                            "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Mã Thẻ", "Ngày sinh", "Giới tính","Ngày vào nội trú",
                            "Diễn biến LS", "Giai đoạn bệnh", "Hội chẩn", "Phẫu thuật", "Thời điểm diễn biến lâm sàng", "Mã người thực hiện", "Tên người thực hiện", "Chức danh"
                            );

                foreach (var x5 in data5)
                {
                    string? maBS = x5.NGUOI_THUC_HIEN;
                    string[] ttBS = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS);

                    AppendRow(xlSheetXML5, cellStyles, rows++,
                            rows - 2, x5.XML01.MA_LK, x5.XML01.MA_BN, x5.XML01.HO_TEN,
                            x5.XML01.MA_THE_BHYT, GetNgayThang(x5.XML01.NGAY_SINH, true), GetType_GioiTinh(x5.XML01.GIOI_TINH), GetNgayThang(x5.XML01.NGAY_VAO_NOI_TRU),
                            x5.DIEN_BIEN_LS, x5.GIAI_DOAN_BENH, x5.HOI_CHAN, x5.PHAU_THUAT, GetNgayThang(x5.THOI_DIEM_DBLS), maBS, ttBS[0], ttBS[1]
                            );

                }

                SetAutoFeatures(xlSheetXML5, cols, 8, 10, 11, -14);
                #endregion

                #region Sheet 6: DS Tien BHXH
                ISheet xlSheet6 = workbook.CreateSheet("Danh sách tiền BHXH");
                rows = 2;
                cols = CreateHeader(xlSheet6, cellStyles,
                        "Danh sách tiền BHXH", // Sheet title
                        "STT", "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân", "Tiền Bệnh nhân", "Tiền BHTT"
                    );

                foreach (var x in data1)
                {
                    AppendRow(xlSheet6, cellStyles, rows++,
                            x.STT, x.MA_LK, x.MA_BN, x.HO_TEN, Uti.GetMoneyVN(x.T_BNCCT), Uti.GetMoneyVN(x.T_BHTT)
                    );
                }

                SetAutoFeatures(xlSheet6, cols);
                #endregion

                CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);
                workbook.Write(fs);
                res.Data = fs.ToArray();
            }
        }
        catch (Exception ex)
        {
            res = Fail(ex, nameof(BaoCao_TrungThuocDV));
        }

        return res;
    }
}
