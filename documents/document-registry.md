# Document registry — thư mục, profile tài liệu, ID, quy tắc đặt tên

> **Trạng thái:** Approved · **Version:** 1.1. Chọn profile ở `documents/PROJECT.md` (`docs_profile`). Nguyên tắc: một tài liệu chỉ tồn tại nếu có người đọc nó để ra quyết định.

## 1. Bản đồ `documents/`
| Thư mục / file | Nội dung | Ai ghi |
|---|---|---|
| `designs/` | tài liệu thiết kế và tham chiếu: `features/`, `adr/`, `ui/`, và theo profile `srs.md`, `frs/`, `erd.md`, `api/openapi.yaml`, `testing/` (test plan, test case), `requirements/raw/`, `charter/`, `traceability-matrix.md` | docs (ui: designer) |
| `plans/` | kế hoạch: `master.md` (tổng), `W-<yyyy>-W<ww>.md` (tuần), `P-<yyyyMMdd>-<nn>.md` (ngày/phiên), `wbs.md` và `sprint-<NN>/` (full) | orchestrator |
| `reports/` | `R-<yyyyMMdd>-<nn>.md` (một báo cáo cho một plan), `INCIDENT-*.md`, `CHANGELOGS.md` (quyết định cuối cùng) | orchestrator |
| `changes/` | `cr-<###>-<slug>.md`: yêu cầu thay đổi kèm phân tích lợi/hại; được duyệt thì sinh plan mới | orchestrator |
| `total_report.md` | báo cáo tổng cho PO, cập nhật khi xong một plan | orchestrator |
| `PROJECT.md`, `BUILD.md`, `document-registry.md` | profile; lệnh build/test; file này | orchestrator; docs (BUILD.md) |

## 2. Ba profile (chọn mức thấp nhất đủ dùng; nâng cấp sau được)
| Profile | Tài liệu bắt buộc trong `designs/` | Skill dùng | Phù hợp |
|---|---|---|---|
| **lean** | `features/F-xxx-*.md` (mục tiêu, AC, dữ liệu/API) · `adr/` cho quyết định lớn | /new-feature, /tdd, /review-code, /update-changelog, /promote-stable | một PO + nhóm nhỏ, yêu cầu đổi nhanh |
| **standard** | lean + `srs.md` · `frs/frs-<module>.md` (thay feature spec cho module lớn) · `erd.md` · `api/openapi.yaml` · `testing/test-plan.md` + `testing/test-cases/tc-<module>.md` | + /analyze-requirements, /design-module, /gen-testcases | nhiều module, cần đặc tả rõ |
| **full** | standard + `charter/` · `plans/wbs.md` · `traceability-matrix.md` · quy trình CR đầy đủ · đồng bộ Jira nếu bật | + /breakdown, /impact-analysis, /sync-trace | có bên ngoài nghiệm thu hoặc cần truy vết đầy đủ |

Nâng cấp lean -> standard: giữ id `F-xxx`, thêm cột map `SRS-F-xxx`/`FRS-...` vào spec khi viết SRS/FRS; không đánh lại số. Mọi profile đều có `plans/`, `reports/`, `changes/`, `total_report.md`.

## 3. ID (không bao giờ tái sử dụng)
`F-xxx` hạng mục (hoặc Jira key khi bật Jira) · `SRS-F-xxx`, `SRS-NF-xxx` · `FRS-<MODULE>-xxx` · `TC-<MODULE>-xxx` · `ADR-xxx` · `CR-xxx` · task `T012` (.running) · kế hoạch `P-yyyyMMdd-nn`, báo cáo `R-yyyyMMdd-nn`. Test và commit mang id hạng mục: `F012_Login_WithExpiredToken_Returns401`, `feat(auth): ... [F-012]`.

## 4. Đặt tên: SỐNG và SNAPSHOT
| Họ | Đặc điểm | Quy tắc tên | Ví dụ |
|---|---|---|---|
| SỐNG | được tài liệu khác trỏ tới, cập nhật có kiểm soát | tên cố định; version ở header + git | `frs-auth.md`, `F-012-login.md` |
| SNAPSHOT | đóng băng sau một sự kiện (test report, biên bản, bản bàn giao) | `<yyyyMMdd>-<tên>-v<X-Y>.<ext>` | `20261030-test-report-v1-0.md` |

Plan (`P-`), report (`R-`), incident, CR có quy tắc tên riêng ở mục 1 và 3. Mỗi tài liệu thiết kế có header (Trạng thái: Draft -> In Review -> Approved -> Superseded; Version `MAJOR.MINOR`) và bảng Lịch sử thay đổi. Tài liệu lỗi thời không xóa: đánh dấu Superseded và trỏ tới bản thay thế. Tài liệu Approved chỉ đổi khi PO đồng ý.

## 5. Version thống nhất
git là hệ thống version duy nhất cho `source/` và `documents/`: tag `stable-<yyyyMMdd>-<nn>` (PO xác nhận) và tag release `vX.Y.Z` (SemVer, CI) đóng băng cặp code + tài liệu. `.claude/` và `.running/` không nằm trong git. Schema: mỗi migration EF Core bất biến. API: `/api/v1`, chỉ mở `/v2` khi breaking.
