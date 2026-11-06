# BUILD.md — build, test, chạy local (STUB, chưa xác minh)

> **Trạng thái:** Draft · Phiên khởi tạo (/init-project) phải chạy thử TỪNG lệnh dưới đây trên máy thật rồi sửa file này theo kết quả. Không dùng lệnh nào chưa chạy thử. Phiên bản công cụ thực tế ghi ở mục cuối.

## Phiên bản (nguồn: documents/PROJECT.md)
.NET: net10.0 · Node: 24 · PostgreSQL: 16

## Lệnh dự kiến (chưa chạy thử), chạy từ thư mục gốc repo
| Việc | Lệnh | Đã chạy thử |
|---|---|---|
| Dựng PostgreSQL + Seq | `docker compose -f source/deploys/docker-compose.yml up -d` | chưa |
| Build backend | `dotnet build source/backend/App.sln -c Release -warnaserror` | chưa |
| Test backend (+ coverage) | `dotnet test source/backend/App.sln -c Release --collect:"XPlat Code Coverage"` | chưa |
| Tạo migration | `dotnet ef migrations add <Name> -p source/backend/Infrastructure -s source/backend/Api` | chưa |
| Cài frontend | `npm ci` (trong source/frontend) | chưa |
| Lint / test / build frontend | `npm run lint` · `npm run test -- --run` · `npm run build` (trong source/frontend) | chưa |
| E2E | `npx playwright test` (trong source/tests/e2e) | chưa |

Ghi chú cấu trúc: test .NET ở `source/backend/tests/Backend.*.Tests` (tham chiếu từ `source/backend/App.sln`); test Vitest ở `source/frontend/tests` (vitest `include` trỏ tới đó); E2E, test tải, test API ngoài ở `source/tests`.

## Phiên bản công cụ thực tế
<!-- dotnet --version, node --version, docker --version: điền sau khi chạy -->
