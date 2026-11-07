# PORT-ISSUES — Cảnh báo và ghi nhận trong quá trình port .NET 10

> Một nơi duy nhất cho mọi cảnh báo, lệch hành vi, nợ kỹ thuật phát sinh khi port. Xử lý từng mục SAU KHI convert xong cả 5 project (PO yêu cầu 2026-11-06). Cập nhật liên tục; mỗi mục: mức độ · nguồn · mô tả · đề xuất · trạng thái.
> Mức độ: **Cao** = bảo mật/mất dữ liệu · **TB** = lệch hành vi hoặc cần PO quyết · **Thấp** = dọn dẹp.

## Bảo mật và phụ thuộc
| # | Mức | Nơi | Mô tả | Đề xuất | Trạng thái |
|---|---|---|---|---|---|
| 1 | Cao | Core/Base/Uti.cs | Mật khẩu mã hóa đảo ngược được (PasswordEncrypt/Decrypt), key và salt hardcode, PBKDF2-SHA1 1000 vòng, IV suy ra từ key. Có từ bản cũ. Bản port cho kết quả GIỐNG HỆT bản cũ (reviewer đã so sánh), nên dữ liệu cũ vẫn giải mã được. Cảnh báo tự động của security-guidance gắn cờ HIGH | CR riêng: băm một chiều (PasswordHasher hoặc PBKDF2-SHA256 >=100k vòng, salt ngẫu nhiên mỗi user), băm lại khi user đăng nhập, key đưa ra secret/IConfiguration | Giữ nguyên theo PO (2026-11-07) |
| 2 | TB | Core csproj | AutoMapper 14.0.0 báo NU1903 (GHSA-rvv3-g6hj-g44x); bản vá từ v15 cần license thương mại | Giữ + chấp nhận, hoặc thay Mapperly/map tay. Đo mức dùng AutoMapper ở Services/Web rồi quyết | Đã xử lý (2026-11-07): bỏ hoàn toàn AutoMapper, map tay IMapable |
| 3 | Thấp | Data csproj | Pin System.Security.Cryptography.Pkcs 10.0.* để vá NU1902/NU1903 từ gói kéo theo 9.0.11 | Gỡ pin khi EF Core cập nhật | Mở |

## Lệch hành vi so với bản cũ (đã chấp nhận tạm, cần PO xác nhận)
| # | Mức | Nơi | Mô tả | Đề xuất | Trạng thái |
|---|---|---|---|---|---|
| 4 | TB | Core/Models/Diagram/AIStatsModel | `DataLines` và `DataBars` cùng `[JsonProperty("dataLines")]` ở bản cũ (System.Text.Json ném lỗi khi trùng tên). Bản mới đặt `DataBars` = `"dataBars"` | Kiểm tra giao diện/JS đọc tên nào khi làm Web | Đã xử lý (2026-11-07) |
| 5 | Thấp | Core/HTML/CategoryImporter | Chỗ trước ném NullReferenceException khi thiếu dữ liệu nay trả về/bỏ qua dòng | Chấp nhận | Đã chấp nhận |
| 6 | Thấp | Core/Base/FormatConverter | Là `JsonConverter<int>`; `Write` vẫn ném NotImplementedException như bản cũ. Đọc số thập phân (1.5 -> 15) giữ như cũ | Xem lại khi dùng thật | Đã xử lý (2026-11-07) |
| 7 | Thấp | Core/Base/Constant | Serialize bằng System.Text.Json thường, không dùng lowercase policy, để giữ tên `sEmptyTable`, `oPaginate`, `fileSize` | Xác nhận tên phía client | Đã xử lý (2026-11-07) |
| 8 | TB | JSON toàn dự án | "lowercase" được hiểu là `name.ToLowerInvariant()` (ví dụ `MA_BN` -> `ma_bn`), vì .NET không có chính sách lowercase sẵn. Kèm PropertyNameCaseInsensitive = true | PO xác nhận không muốn camelCase | Đã xử lý (2026-11-07): lowercase = ToLowerInvariant, MVC và JsonDefaults cùng chính sách |
| 9 | Thấp | Core/Base/PredicateExpresion | Dùng `!.Value` (cố ý) cho int?/double?/DateTime? để SQL không COALESCE (blocker B1 đã sửa); thêm ràng buộc `where T : class` | Giữ | Đã xử lý |
| 10 | Thấp | Core/Startup | Dùng `new SerilogLoggerFactory(Log.Logger)` thay `AddSerilog()`, tương đương | Giữ | Đã xử lý |

## BÍ MẬT ĐÃ NẰM TRÊN GITHUB (phát hiện 2026-11-06, ưu tiên cao nhất)
`source/resources/` (bản code cũ) đã được push lên GitHub (commit initial trên main và nhánh feature) và chứa các bí mật dưới đây. Chúng nằm trong lịch sử git nên xóa file ở commit sau KHÔNG đủ. Báo cáo này cố ý KHÔNG chép giá trị, chỉ ghi vị trí.
| # | Mức | Nơi | Loại | Đề xuất |
|---|---|---|---|---|
| S1 | Cao | resources/HIA.Web/HIA.DatabaseModel/Entites/HIADbContext.cs (dòng ~100, OnConfiguring) | Chuỗi kết nối SQL Server thật (máy chủ nội bộ, user và mật khẩu) | Đã xóa khỏi working tree (2026-11-07); lịch sử git chờ T014 |
| S2 | Cao | resources/HIA.Web/HIA.ImportExportApp/appsettings.json | Chuỗi kết nối tài khoản `sa` và khóa ký JWT | Đã xóa khỏi working tree (2026-11-07); lịch sử git chờ T014 |
| S3 | TB | resources/HIA.Web/HIA.DatabaseModel/readme-build.md | Mật khẩu `sa` trong lệnh scaffold | Đã xóa khỏi working tree (2026-11-07); lịch sử git chờ T014 |
| S4 | TB | resources/HIA.Web/HIA.Repositories/Base/BaseRepository.cs (dòng ~32) | Khóa license Z.EntityFramework hardcode | Đã xóa khỏi working tree (2026-11-07); lịch sử git chờ T014 |
| S5 | TB | HIA.Core/Base/Uti.cs (mục 1) | Salt/key mã hóa mật khẩu hardcode | Như mục 1 |
Cách xử lý đề xuất (PO quyết): (a) đổi toàn bộ mật khẩu/khóa trên; (b) để repo private; (c) muốn xóa khỏi lịch sử: git filter-repo rồi force push (phá hủy, mọi bản clone khác phải clone lại); (d) code mới lấy bí mật từ user-secrets/biến môi trường. Chưa quét hết HIA.Web/appsettings và các project khác trong resources: sẽ quét ở bước Web.

## Bảo mật trong Services (cảnh báo tự động security-guidance, 2026-11-06)
Hai mục dưới nằm trong code Services đang port dở (agent T004 chưa xong). Cần đối chiếu bản cũ: nhiều khả năng là hành vi có sẵn từ trước, port chỉ giữ nguyên. Xử lý trong CR bảo mật sau khi convert.
| # | Mức | Nơi | Mô tả | Đề xuất | Trạng thái |
|---|---|---|---|---|---|
| 27 | Cao | Services/Files/FilesService130.cs | Lọc theo `request.HospitalId` do client gửi, không ép theo cơ sở KCB của người đăng nhập: có nguy cơ xem dữ liệu của cơ sở khác (IDOR) | Lấy MA_CSKCB từ claim của user và luôn áp dụng; chỉ cho đổi HospitalId với vai trò MASTER/BOYTE sau khi kiểm tra quyền | Mở, chờ PO |
| 28 | TB | Services/Auth/AuthService.cs | Link đặt mật khẩu/tài khoản mới dựng từ host của request (`Uti.GetFullDomainUrl`): có thể bị làm giả bằng header Host | Dùng URL gốc cấu hình sẵn (appsettings/secret), hoặc ép AllowedHosts | Mở, chờ PO |
| 29 | Cao | Services/Categories/DMKhoaBVService | Cập nhật khoa theo `request.MA_CSKCB` do client gửi, không kiểm tra bản ghi thuộc cơ sở của người dùng: sửa dữ liệu cơ sở khác (IDOR) | Nạp bản ghi hiện có, ép theo claim MA_CSKCB của user, chỉ copy MA_KHOA/TEN_KHOA, bỏ qua MA_CSKCB từ request | Mở, chờ PO |
| 30 | Cao | Services/Categories/DMKhoaBVService (xóa) | Xóa theo `id` không kiểm tra `item.MA_CSKCB` có khớp cơ sở của user | Kiểm tra claim trước khi xóa, cùng kiểm tra vai trò như Save/DeleteItem | Mở, chờ PO |
| 31 | Cao | Services/HDSDManagement/ChiDinhDongThoiService | UpdateItem/GetItemById lấy bản ghi theo id; ADMIN còn bị ghi đè `item.MA_CSKCB = currentCSKCB` thay vì bị chặn khi khác cơ sở | ADMIN: trả 403 nếu `item.MA_CSKCB != currentCSKCB`, kiểm tra trước khi SaveRuleError; thêm kiểm tra vai trò + sở hữu cho GetItemById | Mở, chờ PO |
| 32 | Cao | Services/HDSDManagement/HDSDDichVuService | Như #31 | Như #31 | Mở, chờ PO |
| 33 | Cao | Services/HDSDManagement/HDSDThuocService | Như #31 | Như #31 | Mở, chờ PO |
| 34 | TB | Services/Files/FilesService130 | Trả `ex.ToString()` (stack trace, đường dẫn) cho client trong thông báo lỗi upload và trong `res.Message` của GetXml1ErrorItem(s)ByFileName, GetXmlErrorItems: lộ thông tin nội bộ | Log phía server, trả thông báo chung (hoặc mã tham chiếu). Có thể có sẵn từ bản cũ, cần đối chiếu | Đã xử lý (2026-11-07): thông báo thân thiện, lỗi ghi log |
| 35 | Cao | Services/Report/ReportHandleService.TongHop, TrungDichVu | Bộ lọc báo cáo giải mã từ chuỗi do client gửi; chỉ gán MaCSKCB từ claim khi client để trống, nên client truyền mã cơ sở khác sẽ xem được báo cáo của cơ sở đó (IDOR). Cùng gốc với #27, #29–#33 | Luôn gán MaCSKCB từ claim; chỉ cho đổi với vai trò đặc quyền sau kiểm tra. Làm cơ chế chung trong CR bảo mật | Mở, chờ PO |

Nhận xét: #27, #29–#33 cùng một lỗi gốc là thiếu ràng buộc dữ liệu theo cơ sở KCB (multi-tenant). Nên xử lý một lần bằng cơ chế chung (filter theo claim ở repository/service) thay vì vá từng chỗ. Đối chiếu bản cũ để xác nhận có sẵn từ trước.

## Services (T004, reviewer T004-R, 2026-11-06)
| # | Mức | Nơi | Mô tả | Đề xuất | Trạng thái |
|---|---|---|---|---|---|
| 36 | TB | Services/Base/HandleFile130CommonService | UploadExcel/DownloadExcel bỏ DataTable + Newtonsoft, dùng reflection. Cột/null/ô rỗng khớp bản cũ; xử lý ngày chưa chứng minh (Newtonsoft mặc định đổi chuỗi ISO thành DateTime, bản mới giữ chuỗi thô). Không có test | Thử upload/download Excel thật với cột ngày; viết test tương đương ở giai đoạn 2; hoặc PO ghi nhận là thay đổi chấp nhận | Đã xử lý (2026-11-07): export mọi ngày thành text `HH:mm:ss dd/MM/yyyy`; import nhận 9 định dạng + ô ngày gốc (HIA.Core/Base/ExcelDate.cs). Ưu tiên khi mơ hồ: dd/MM trước MM/dd, yyyyMMdd trước ddMMyyyy. Cột chuỗi nhận ô ngày gốc cũng lưu dạng này |
| 37 | TB | Services (nhiều nơi) | Không có test cho các hành vi đã đổi: BuildQuery_XML01 bỏ nhánh Status và trim TypeKCB, ReplaceCategoryFile (EF.Property), DetachError, TotalFile* khoảng [from,to), AttachInnerXML/LoadInner (PO chọn chỉ build ở giai đoạn 1) | Viết characterization test khi bật lại tdd_gate (giai đoạn 2) | Mở |
| 38 | Thấp | Services/Report/ReportHandleService.cs:294-297 | TypeKCB được trim trước khi so sánh (bản cũ không trim) | PO ghi nhận quyết định | Mở |
| 39 | Thấp | Services/Categories/ReplaceCategoryFile | Các nhánh DMThuoc/DMVTYT/DMBacSi/DMKhoaBV không đổi so với bản cũ: nạp lại file sẽ chèn trùng bản ghi | Sửa khi làm nghiệp vụ, không thuộc port | Mở |
| 40 | TB | Services/Auth/AuthService.cs:90, :232 | Email loại trừ hardcode trong code (địa chỉ cá nhân) | Chuyển ra cấu hình | Đã xử lý (2026-11-07): đọc từ AppSettings:ExcludedEmails |
| 41 | Thấp | Services/Report, Auth, UserManagement, HospitalManagement | Còn 14 cảnh báo CS: 8 x CS8619 (Include nav `Hospital?` nullable bên HIA.Data), 6 x CS8602/CS8604 ở Report (sửa sẽ đổi hành vi khi null) | PO xử lý nullable của entity Data; Report xem lại cùng test | Mở |
| 42 | Thấp | Services/Validate130 | ValidateXML03 có 159 lời gọi `NullGuard.Req()` và 2 helper `StrField` ở ValidateXML01/02 vì Uti.CodeMatch/CodeContain, EvalHelper nhận `string` không nullable | Đổi các hàm Core sang `string?` để bỏ lớp phụ (~100 chỗ `?? string.Empty`) | Mở |
| 43 | Thấp | Services/Files | `ValidateHoso` hardcode `{"25001"}`; `ValidateHosoFrom4210` chỉ còn là stub trả Task.CompletedTask; `FileUploadAsync`/`FileUploadAPI`/`DownloadBaoCao` trả nullable (Web phải xử lý null) | Xem khi port Web | Mở |
| 44 | Thấp | Services/Categories | `ImportFromAPI` đổi cách map DataTable sang entity (`ToEntities`); lỗi cũ trong Scrape_* (kiểm tra nhầm biến content thay vì html) đã sửa | Thử với dữ liệu thật | Mở |
| 45 | Cao | Web/appsettings*.json | Bí mật thật (khóa JWT, mật khẩu email, AiKey/AuthKey/StatKey, chuỗi kết nối dùng sa) nằm trong repo; trái CLAUDE.md "Secrets are never committed". PO quyết giữ nguyên vì repo private (2026-11-07 12:11) | Trước khi repo công khai hoặc triển khai: xoay toàn bộ, chuyển sang biến môi trường/user-secrets, dùng login SQL ít quyền (liên quan S1-S5) | Chấp nhận có điều kiện |
| 46 | TB | Web/Helper/ExcludedEmailValidator.cs, Startup.cs | Danh sách email loại trừ so sánh phân biệt hoa thường (Ordinal) và đối chiếu claim FULL_NAME, giữ đúng bản cũ | Đổi sang OrdinalIgnoreCase + trim, đọc claim email thật; cần PO duyệt vì đổi hành vi | Đã xử lý (2026-11-07): OrdinalIgnoreCase + trim, JWT đọc đúng claim email |
| 47 | TB | Web/wwwroot/js/tagslist.js | Thư viện JS cũ dùng innerHTML với dữ liệu người dùng (nguy cơ XSS), sao chép nguyên từ bản cũ | Sửa sang textContent khi dọn front-end | Mở |
| 48 | TB | Web/Startup.cs | AddAntiforgery SuppressXFrameOptionsHeader = true (giữ như cũ) làm trang nhúng được vào iframe (clickjacking); có thể cố ý | PO xác nhận có cần nhúng iframe không; nếu không, bỏ cờ này hoặc đặt frame-ancestors | Mở, chờ PO |
| 49 | Thấp | Web | Chưa port: web.config, compilerconfig.json, .config/dotnet-tools.json (dotnet-ef), HIA.Web.csproj.user; EF.Tools bị bỏ khỏi Web; Models/ErrorViewModel.cs mồ côi; OpenAPI đổi path sang /openapi/v1.json | Xử lý ở giai đoạn 2/3 | Mở |
| 50 | Cao | Web/Controllers Files130, FilesAI, FileDownload130 | Tải file ghép `fileName` từ client vào đường dẫn rồi PhysicalFile, không chặn `..` hay dấu phân cách (path traversal). Có sẵn ở bản cũ (đã đối chiếu), port giữ nguyên | Dùng Path.GetFileName + GetFullPath và kiểm tra nằm trong thư mục gốc của CSKCB; sửa sau port (CR) | Mở, chờ PO |
| 51 | Cao | Web/Controllers DichVuMaMay (và các DM* nhận maCSKCB từ client) | Tham số maCSKCB do client gửi, không ép về claim với user không phải MASTER (IDOR đa cơ sở). Cùng nhóm CR multi-tenant #27-35 | Gộp vào CR bảo mật multi-tenant | Mở, chờ PO |
| 52 | TB | Web/Controllers/DMKhoaBVController.Import | Action Import (gọi ImportFromAPI) thiếu [Authorize] theo vai trò, bất đối xứng với các action sửa khác; bản cũ cũng vậy | Thêm [Authorize(Roles=...)] sau khi PO chọn vai trò | Đã xử lý (2026-11-07): [Authorize(Roles=ADMIN)] |
| 53 | TB | Web/Controllers/AIDataController | Endpoint [AllowAnonymous] nhận khóa qua query string `key`, so sánh chuỗi thường; bản cũ cũng vậy | Chuyển sang header, so sánh FixedTimeEquals | Giữ nguyên theo PO (2026-11-07) |
| 54 | TB | Web/Controllers/AIDataController (~dòng 301) | Che SO_CCCD ngược điều kiện: chỉ che khi xml06s rỗng, còn khi có dữ liệu thì trả nguyên số CCCD; bản cũ cũng vậy, có thể là lỗi | PO xác nhận ý định rồi sửa | Mở, chờ PO |
| 55 | Thấp | Web/Controllers/AIMessageController | Trả `respBody` của dịch vụ GPT cho client trong lỗi, và khóa gptApiKey gửi trong body JSON; bản cũ cũng vậy | Ghi log phía server, trả thông báo chung, gửi khóa qua header | Mở |
| 56 | Cao | Web/Controllers/HospitalController (EditSpec, Edit, Delete) | Quyền chỉ MASTER_and_ADMIN, nhận Id/MA_CSKCB/MaxFilePerDate từ client, không ép theo cơ sở của user (IDOR đa cơ sở, trường đặc quyền). Bản cũ giữ nguyên | Gộp vào CR bảo mật multi-tenant #27-35 và #51 | Mở, chờ PO |
| 57 | Cao | Web/Controllers/ReportController (Index, Totalize, Summary, Download) | Không có [Authorize] ở cả controller lẫn action; thống kê toàn bộ cơ sở trả cho bất kỳ ai gọi được. Bản cũ cũng không có (đã đối chiếu) | PO xác nhận trang báo cáo có chủ ý công khai không; nếu không thì thêm [Authorize] và lọc theo cơ sở được phép | Đã xử lý (2026-11-07): [Authorize] ALL; Totalize/Summary MASTER_and_ADMIN |
| 58 | Thấp | Web (nhiều controller) | Các điểm cũ ném NullReference/500 nay trả 404/400/EmptyResult/danh sách rỗng hoặc InvalidOperationException (GetItems các DM*, Download*, claim thiếu, AIData.IsAuthorized→401, FileDownload130.Index→SignOut). Kết cục chủ yếu vẫn là lỗi/từ chối | PO ghi nhận là thay đổi chấp nhận | Mở |
| 59 | Thấp | Web/Controllers | AuthController.SignOutAction trỏ nameof(UserController.SignIn) không tồn tại ở bản cũ, nay nameof(AuthController.SignIn); ApiController giữ 2 endpoint GetXMLVerify/SendXMLResult chỉ là stub (luôn NotFound / Ok(true)); bỏ 5 controller toàn comment (HoSo, DMError, Files, FileUpload, FileDownload) còn view mồ côi tương ứng | Dọn view mồ côi; PO quyết có bỏ 2 endpoint stub không | Mở |
| 60 | Thấp | Web | JsonDefaults dùng UnsafeRelaxedJsonEscaping trong @Html.Raw (Home/Index.cshtml, Report/Index.cshtml): rủi ro `</script>` trong nhãn; Views/AIMessage/index.cshtml chữ thường nhưng PartialView("Index") (lỗi trên Linux, giai đoạn 3); csproj JwtBearer/OpenApi dùng 10.0.* chưa pin; /openapi/v1.json không xác thực; NPOI cần AcceptNPOIOSMFLicense | Xử lý ở giai đoạn 2/3 | Mở |
| 61 | Cao | Web/Controllers/ApiController (Insert danh mục DMThuoc, DMDichVu, DMVTYT, DMThietBi, DMKhoaBV, DMDVGiuong, DMMaGiuong, DMBacSi, DMCSKCB) | Bind trực tiếp entity EF từ client rồi InsertListAsync, không ép MA_CSKCB theo cơ sở của token và không bỏ khóa chính do client gửi (ghi chéo cơ sở / mass assignment). Bản cũ cũng vậy | Gộp CR multi-tenant #27-35, #51; ép MA_CSKCB, dùng DTO riêng | Mở, chờ PO |
| 62 | TB | Web/Controllers/ApiController.CheckToken | [AllowAnonymous] nhận token qua query string (lộ trong log/URL). Bản cũ cũng vậy; đổi sang body/header sẽ làm gãy client hiện có | Đổi khi có kế hoạch cập nhật client | Giữ nguyên theo PO (2026-11-07) |
| 63 | Cao | Web/Controllers/DMKhoaController (Create, Edit, DeleteAction, Upload) | Chỉ [Authorize(Roles=ALL)] cho mọi action ghi (mọi vai trò sửa/xóa/nạp danh mục khoa), bất đối xứng với DMKhoaBVController. Bản cũ cũng vậy | Gộp CR multi-tenant/phân quyền (#52); PO chọn vai trò | Mở, chờ PO |
| 64 | Cao | Web/Controllers/Files130Controller (GetXML02, GetXmlError, RecheckHoso, các GetXmlNN) | Nhận xml01ID từ client, không kiểm tra hồ sơ thuộc MA_CSKCB của user (IDOR đọc/recheck hồ sơ cơ sở khác). Cùng nhóm #27 | Gộp CR multi-tenant | Mở, chờ PO |
| 65 | TB | Web/Controllers/AIMessageController.Send | SanitizeInput chỉ áp cho chuỗi ghi log (clearedMessage); payload gửi dịch vụ GPT vẫn dùng dto.message gốc (sanitizer không có tác dụng với prompt); controller không có [Authorize], xác thực bằng kiểm tra claim thủ công. Bản cũ cũng vậy | Gửi giá trị đã làm sạch hoặc từ chối khi vi phạm; thêm [Authorize] | Mở, chờ PO |
| 66 | Thấp | Core/Models, Core/Base/Uti | Dọn code chết sau bỏ AutoMapper: MapFrom/MapTo của XML1/2/3InfoModel (không có nơi gọi), ToNewUserModel, ModifyUserModel.MapTo; tham số `noTime` của GetNgayThang không còn tác dụng | Xóa khi dọn code | Mở |
| 67 | TB | Core/Models/Categories/DMVTYTInfoModel (và Modify) | `(int?)item.DINH_MUC` cắt phần thập phân; chưa kiểm chứng AutoMapper cũ làm tròn hay cắt | PO xác nhận cách làm tròn | Mở, chờ PO |
| 68 | TB | Repositories/Base `_cateRepos.Update` | Update ghi đè CreatedBy/CreatedDate/IsDeleted vì MapTo tạo entity mới (có sẵn từ bản cũ) | Chỉ cập nhật các cột cần thiết khi sửa nghiệp vụ | Mở |
| 69 | Thấp | Core/Models Files | Map mới NgayLAP<->NGAY_LAP trong XML1InfoModel (AutoMapper cũ không có); chuỗi ngày sai định dạng trả null thay vì ném lỗi; string null -> "" khi MapFrom; DVTCID đã nối đúng (trước đây DVID luôn 0 làm nút sửa/xóa DMDVThuCong hỏng) | Chấp nhận | Đã chấp nhận |

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
| 26 | Thấp | Core/Base/RequestLimit | Danh sách IP cho phép hardcode trong code (có địa chỉ hạ tầng nội bộ). Có từ bản cũ | Chuyển ra cấu hình (appsettings/secret) | Giữ nguyên theo PO (2026-11-07) |

## Quy trình
| # | Mức | Mô tả | Đề xuất |
|---|---|---|---|
| 16 | Thấp | Agent T002e3 báo "Base không còn cảnh báo" nhưng build `--no-incremental` vẫn còn 53 cảnh báo; orchestrator phải tự build lại. Với việc chạy song song, luôn kiểm bằng `--no-incremental` sau khi gộp | Đã áp dụng trong Rule 5b (gộp rồi build một lần) |
| 17 | Thấp | Chưa có test nào (PO chọn chỉ build trong giai đoạn 1) | Giai đoạn 2 bật lại tdd_gate, bổ sung test cho phần JSON/Evaluation/PredicateExpresion |

## Sẽ bổ sung ở các bước sau
Repositories (thay Z.EntityFramework.Plus), Services (Nancy, NPOI, MailKit...), Web (Swashbuckle, JWT, view).
