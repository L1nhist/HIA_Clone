# Total report — dành cho PO đọc và kiểm soát

> Cập nhật mỗi khi hoàn thành một plan. Chi tiết: `documents/reports/`. Quyết định cuối: `documents/reports/CHANGELOGS.md`.

**Cập nhật:** 2026-11-07 · **Profile:** lean (PROJECT.md)

## Tiến độ theo plan
| Plan | Trạng thái | Hoàn thành | Báo cáo |
|---|---|---|---|
| P-20261106-01..04 (Data, Core, Repositories, Services) | Done | 4/4 project | R-20261106-02 |
| P-20261107-01 (Web) | Done | 7/7 | R-20261107-01 |

## Đã hoàn thành đáng chú ý
- Giai đoạn 1: cả 5 project (Data, Core, Repositories, Services, Web) đã port sang net10; HIA.slnx build 0 lỗi, 32 cảnh báo. Web chạy thử: /Auth/SignIn và /openapi/v1.json trả 200. Chưa test với SQL Server thật; không có test (PO chọn).
- BackgroundServices gộp vào HIA.Services; OpenAPI thay Swagger; email loại trừ JWT theo cấu hình.

## Cần PO quyết
- Commit + push toàn bộ giai đoạn 1 (chưa commit Services, Web).
- Bí mật S1-S5 + appsettings (#45); AutoMapper NU1903; JSON lowercase (#8); Excel ngày (#36).
- Bảo mật có sẵn ở bản cũ: #46 email so sánh, #48 iframe, #50 path traversal, #51/#56/#57 multi-tenant + Report không Authorize, #54 CCCD che ngược.

## Rủi ro
- Chưa chạy với dữ liệu thật; nhiều thay đổi hành vi nhỏ (PORT-ISSUES #36-44, #58-60) không có test.

## Dự kiến plan tiếp theo
- Xử lý PORT-ISSUES theo ưu tiên PO, rồi giai đoạn 2 (PostgreSQL + SQL Server, migration, bật TDD) và 3 (Docker, standalone).
