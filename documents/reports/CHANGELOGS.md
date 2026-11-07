# CHANGELOGS — các quyết định cuối cùng của dự án

Mỗi dòng: `YYYY-MM-DD R-<id>: <quyết định>`. Mới nhất ở cuối. Quyết định bị thay thế thì gạch bỏ và trỏ sang quyết định mới (`~~...~~ -> thay bởi ...`).
Đây là nhật ký quyết định nội bộ; changelog sản phẩm (phát hành) ở `CHANGELOG.md` tại root.

2026-11-06 R-20261106-01: Port .NET 10 theo thứ tự Data->Core->Repositories->Services->Web; giữ SQL Server; Nullable bật; Newtonsoft->System.Text.Json (lowercase, case-insensitive); chỉ build không test
2026-11-07 R-20261107-01: BackgroundServices vào HIA.Services; Startup.cs giữ riêng; JWT loại trừ email theo config; OpenAPI thay Swagger; appsettings giữ nguyên
