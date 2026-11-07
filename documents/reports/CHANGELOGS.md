# CHANGELOGS — các quyết định cuối cùng của dự án

Mỗi dòng: `YYYY-MM-DD R-<id>: <quyết định>`. Mới nhất ở cuối. Quyết định bị thay thế thì gạch bỏ và trỏ sang quyết định mới (`~~...~~ -> thay bởi ...`).
Đây là nhật ký quyết định nội bộ; changelog sản phẩm (phát hành) ở `CHANGELOG.md` tại root.

2026-11-06 R-20261106-01: Port .NET 10 theo thứ tự Data->Core->Repositories->Services->Web; giữ SQL Server; Nullable bật; Newtonsoft->System.Text.Json (lowercase, case-insensitive); chỉ build không test
2026-11-07 R-20261107-01: BackgroundServices vào HIA.Services; Startup.cs giữ riêng; JWT loại trừ email theo config; OpenAPI thay Swagger; appsettings giữ nguyên
2026-11-07 R-20261107-02: S1-S4 xóa khỏi code+git; lowercase = chữ thường, bỏ Newtonsoft; Excel ngày text HH:mm:ss dd/MM/yyyy; #1 #26 #40 #45 #46 giữ nguyên
2026-11-07 R-20261107-03: email loại trừ từ appsettings (không phân biệt hoa thường); [Authorize] cho Report và DMKhoaBV.Import; lỗi thân thiện thay ex.ToString(); bỏ AutoMapper, map tay IMapable<T>; #53 #62 giữ nguyên
2026-11-07 R-20261107-03: Rule 5d - dùng lại agent theo dòng việc (SendMessage), chuỗi task giao một lần, reviewer/tester luôn mới
2026-11-07 R-20261107-04: #28 PublicBaseUrl mdinsight.vn; #48 CSP frame-ancestors từ appsettings; #50 #47 #55 #60 #67 sửa; #68 chấp nhận; Models: xóa Files/FileUploads cũ, Files130->Files, FileUploads130->FileUploads; T014 hướng 1 (chỉ license Z.EF)
