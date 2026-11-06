# PROJECT.md — profile của dự án
Điền bằng skill /init-project (phỏng vấn). Orchestrator đọc file này đầu mỗi phiên (sau latest_state.md). Sửa tay được; đổi `docs_profile` hoặc `modules` phải ghi vào report.

name: HIA
jira_key: -                     # chỉ dùng khi modules.jira = on, ví dụ APP

docs_profile: lean              # lean | standard | full  (xem documents/document-registry.md)
tdd_gate: off                   # giai đoạn 1 (port .NET 10): PO chọn chỉ build; bật lại từ giai đoạn 2
retry_limit: 10

modules:
  mobile: off                   # Flutter: source/mobile + agent mobile-coder
  identity: off                 # IdP độc lập: source/identity + ADR chọn auth
  jira: off                     # on: id = Jira key; off: id nội bộ F-xxx
  figma: off                    # on: agent designer + skill /design-to-code
  ai: port-only                 # port-only | off | <tên nhà cung cấp sau khi có ADR>

versions:                       # chốt lại khi khởi tạo; nguồn: trang chính thức
  dotnet: net10.0               # LTS, Microsoft hỗ trợ đến 11/2028 (.NET 8 và 9 hết hỗ trợ 10/11/2026)
  node: 24                      # LTS đang hoạt động theo nguồn tra cứu 10/2026; xác nhận khi khởi tạo
  sqlserver: hiện tại (giai đoạn 1); postgres: 16 từ giai đoạn 2

solution: source/backend/HIA/HIA.slnx
commands: documents/BUILD.md       # lệnh build/test đã chạy thử
git_ignored: [.claude, .running]   # latest_state.md chỉ nằm trên máy này: sao lưu nếu cần
