# PORT-ISSUES — Cảnh báo và ghi nhận trong quá trình port .NET 10

> Một nơi duy nhất cho mọi cảnh báo, lệch hành vi, nợ kỹ thuật phát sinh khi port. Xử lý từng mục SAU KHI convert xong cả 5 project (PO yêu cầu 2026-11-06). Cập nhật liên tục; mỗi mục: mức độ · nguồn · mô tả · đề xuất · trạng thái.
> Mức độ: **Cao** = bảo mật/mất dữ liệu · **TB** = lệch hành vi hoặc cần PO quyết · **Thấp** = dọn dẹp.

## Bảo mật và phụ thuộc
| # | Mức | Nơi | Mô tả | Đề xuất | Trạng thái |
|---|---|---|---|---|---|
| 1 | Cao | Core/Base/Uti.cs | Mật khẩu mã hóa đảo ngược được (PasswordEncrypt/Decrypt), key và salt hardcode, PBKDF2-SHA1 1000 vòng, IV suy ra từ key. Có từ bản cũ. Bản port cho kết quả GIỐNG HỆT bản cũ (reviewer đã so sánh), nên dữ liệu cũ vẫn giải mã được. Cảnh báo tự động của security-guidance gắn cờ HIGH | CR riêng: băm một chiều (PasswordHasher hoặc PBKDF2-SHA256 >=100k vòng, salt ngẫu nhiên mỗi user), băm lại khi user đăng nhập, key đưa ra secret/IConfiguration | Mở, chờ PO |
| 2 | TB | Core csproj | AutoMapper 14.0.0 báo NU1903 (GHSA-rvv3-g6hj-g44x); bản vá từ v15 cần license thương mại | Giữ + chấp nhận, hoặc thay Mapperly/map tay. Đo mức dùng AutoMapper ở Services/Web rồi quyết | Mở, chờ PO |
| 3 | Thấp | Data csproj | Pin System.Security.Cryptography.Pkcs 10.0.* để vá NU1902/NU1903 từ gói kéo theo 9.0.11 | Gỡ pin khi EF Core cập nhật | Mở |

## Lệch hành vi so với bản cũ (đã chấp nhận tạm, cần PO xác nhận)
| # | Mức | Nơi | Mô tả | Đề xuất | Trạng thái |
|---|---|---|---|---|---|
| 4 | TB | Core/Models/Diagram/AIStatsModel | `DataLines` và `DataBars` cùng `[JsonProperty("dataLines")]` ở bản cũ (System.Text.Json ném lỗi khi trùng tên). Bản mới đặt `DataBars` = `"dataBars"` | Kiểm tra giao diện/JS đọc tên nào khi làm Web | Mở |
| 5 | Thấp | Core/HTML/CategoryImporter | Chỗ trước ném NullReferenceException khi thiếu dữ liệu nay trả về/bỏ qua dòng | Chấp nhận | Đã chấp nhận |
| 6 | Thấp | Core/Base/FormatConverter | Là `JsonConverter<int>`; `Write` vẫn ném NotImplementedException như bản cũ. Đọc số thập phân (1.5 -> 15) giữ như cũ | Xem lại khi dùng thật | Mở |
| 7 | Thấp | Core/Base/Constant | Serialize bằng System.Text.Json thường, không dùng lowercase policy, để giữ tên `sEmptyTable`, `oPaginate`, `fileSize` | Xác nhận tên phía client | Mở |
| 8 | TB | JSON toàn dự án | "lowercase" được hiểu là `name.ToLowerInvariant()` (ví dụ `MA_BN` -> `ma_bn`), vì .NET không có chính sách lowercase sẵn. Kèm PropertyNameCaseInsensitive = true | PO xác nhận không muốn camelCase | Chờ PO |
| 9 | Thấp | Core/Base/PredicateExpresion | Dùng `!.Value` (cố ý) cho int?/double?/DateTime? để SQL không COALESCE (blocker B1 đã sửa); thêm ràng buộc `where T : class` | Giữ | Đã xử lý |
| 10 | Thấp | Core/Startup | Dùng `new SerilogLoggerFactory(Log.Logger)` thay `AddSerilog()`, tương đương | Giữ | Đã xử lý |

## BÍ MẬT ĐÃ NẰM TRÊN GITHUB (phát hiện 2026-11-06, ưu tiên cao nhất)
`source/resources/` (bản code cũ) đã được push lên GitHub (commit initial trên main và nhánh feature) và chứa các bí mật dưới đây. Chúng nằm trong lịch sử git nên xóa file ở commit sau KHÔNG đủ. Báo cáo này cố ý KHÔNG chép giá trị, chỉ ghi vị trí.
| # | Mức | Nơi | Loại | Đề xuất |
|---|---|---|---|---|
| S1 | Cao | resources/HIA.Web/HIA.DatabaseModel/Entites/HIADbContext.cs (dòng ~100, OnConfiguring) | Chuỗi kết nối SQL Server thật (máy chủ nội bộ, user và mật khẩu) | Đổi mật khẩu user đó |
| S2 | Cao | resources/HIA.Web/HIA.ImportExportApp/appsettings.json | Chuỗi kết nối tài khoản `sa` và khóa ký JWT | Đổi mật khẩu sa, đổi khóa JWT (mọi token cũ mất hiệu lực) |
| S3 | TB | resources/HIA.Web/HIA.DatabaseModel/readme-build.md | Mật khẩu `sa` trong lệnh scaffold | Như S2 |
| S4 | TB | resources/HIA.Web/HIA.Repositories/Base/BaseRepository.cs (dòng ~32) | Khóa license Z.EntityFramework hardcode | Xin cấp lại khóa; code mới không dùng gói này |
| S5 | TB | HIA.Core/Base/Uti.cs (mục 1) | Salt/key mã hóa mật khẩu hardcode | Như mục 1 |
Cách xử lý đề xuất (PO quyết): (a) đổi toàn bộ mật khẩu/khóa trên; (b) để repo private; (c) muốn xóa khỏi lịch sử: git filter-repo rồi force push (phá hủy, mọi bản clone khác phải clone lại); (d) code mới lấy bí mật từ user-secrets/biến môi trường. Chưa quét hết HIA.Web/appsettings và các project khác trong resources: sẽ quét ở bước Web.

## Cấu trúc dữ liệu / entity
| # | Mức | Nơi | Mô tả | Đề xuất | Trạng thái |
|---|---|---|---|---|---|
| 11 | TB | Data/HIADbContext | Chỉ có constructor rỗng (đã bỏ OnConfiguring và chuỗi kết nối cứng chứa mật khẩu `sa`). Cần constructor `DbContextOptions` khi đăng ký DI | Làm ở bước Web (DI, connection string trong cấu hình/secret) | Mở |
| 12 | Thấp | Data | Đã bỏ 7 entity: CheckChanges_DEL, XMLCheckIn_DEL, XMLState_DEL, XMLVerify_DEL, DMBacSi_OLD, UserKhoa, XMLResult (3 file Core cũng bỏ: 2 file "- Copy.cs", XMLCheckinInfo). Mọi nơi dùng đều đã grep | Giữ | Đã xử lý |
| 13 | Thấp | Data/DMCSKCB, DMThietBi | ICategoryData.MA_CSKCB implement tường minh trả `""` thay null | Dọn khi sửa interface | Mở |
| 14 | Thấp | Core/Models | Một số thuộc tính XML để `string?` thay `string.Empty` để serialize vẫn bỏ qua phần tử vắng; `Export()` gán `string?` vào entity có thể gây cảnh báo | Xem lại khi test thật | Mở |
| 15 | Thấp | Core/Models/FileUploads130 | `EvalHelper.Cast/ToDouble/ToDateSql` được gọi với `(x ?? string.Empty)`; có thể bỏ nếu EvalHelper nhận `string?` | Dọn | Mở |

## Repositories (T003, 2026-11-06)
| # | Mức | Nơi | Mô tả | Đề xuất | Trạng thái |
|---|---|---|---|---|---|
| 18 | TB | Repositories/Base | `SingleInsertAsync`/`SingleDelete` (Z.EF, ghi DB ngay) đổi thành `AddAsync`/`Remove` (chờ SaveChanges/Commit). Nơi gọi phải Commit mới ghi | Kiểm tra từng nơi gọi ở Services; nếu cần ghi ngay thì gọi SaveChanges | Mở |
| 19 | TB | Repositories/Base | Đã bỏ gói Z.EntityFramework.Plus + dòng AddLicense. IncludeOptimized/BulkInsert chỉ còn trong comment nên không cần thay | Nếu sau này cần bulk: EFCore.BulkExtensions hoặc AddRange | Đã xử lý |
| 20 | Thấp | Repositories | Bỏ 12 file chỉ gồm comment và 3 hàm không ai gọi (GetAllListXmlReferenceHaveXML23, GetXML1HaveXML4ErrorsOnly, GetMiddleMaCSKCB; hàm cuối còn dùng ThenByDescendingDynamic không tồn tại trong Core) | Giữ | Đã xử lý |
| 21 | TB | Repositories/Files/Xml01Repository | 9 hàm stub trả `null`; kiểu trả về đổi thành nullable. Services sẽ báo warning nullable khi port | Xem hàm nào thật sự dùng ở Services; xóa hoặc cài đặt | Mở |
| 22 | Thấp | Repositories/Base | `IBaseRepository`: GetById/GetItem/GetItemOrderBy trả `T?`; tham số filter/include nullable; GetMinMax bỏ `?? default` | Theo dõi warning ở Services | Mở |
| 23 | Thấp | Repositories/Base | Filter null: bản cũ ném ngoại lệ, nay bỏ qua filter; HttpContext null: CurrentUserId = null (cũ NRE); Xml01.GetAllListXmlReference ném InvalidOperationException khi thiếu claim MA_CSKCB (cũ NRE) | Chấp nhận | Đã chấp nhận |
| 24 | Thấp | Repositories/HDSD | Tìm kiếm: `(x.COL ?? "").Contains(...)` thay `x.COL.Contains(...)` cho cột nullable (tương đương SQL) | Chấp nhận | Đã chấp nhận |
| 25 | TB | Repositories/Startup, DbFactory | `DbFactory` (Core) gọi `Func<HIADbContext>`; chưa ai đăng ký `Func<HIADbContext>`/DbContext trong DI, HIADbContext chưa có constructor options (liên quan mục 11) | Làm ở Services/Web: AddDbContext + đăng ký Func | Mở |
| 26 | Thấp | Core/Base/RequestLimit | Danh sách IP cho phép hardcode trong code (có địa chỉ hạ tầng nội bộ). Có từ bản cũ | Chuyển ra cấu hình (appsettings/secret) | Mở |

## Quy trình
| # | Mức | Mô tả | Đề xuất |
|---|---|---|---|
| 16 | Thấp | Agent T002e3 báo "Base không còn cảnh báo" nhưng build `--no-incremental` vẫn còn 53 cảnh báo; orchestrator phải tự build lại. Với việc chạy song song, luôn kiểm bằng `--no-incremental` sau khi gộp | Đã áp dụng trong Rule 5b (gộp rồi build một lần) |
| 17 | Thấp | Chưa có test nào (PO chọn chỉ build trong giai đoạn 1) | Giai đoạn 2 bật lại tdd_gate, bổ sung test cho phần JSON/Evaluation/PredicateExpresion |

## Sẽ bổ sung ở các bước sau
Repositories (thay Z.EntityFramework.Plus), Services (Nancy, NPOI, MailKit...), Web (Swashbuckle, JWT, view).
