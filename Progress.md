# 📈 Clevora — Development Progress Tracker

> **Catatan Progres Pengembangan Aplikasi Clevora**  
> Dibuat oleh: **Gravicode Studios** (Lead: **Kang Fadhil**)  
> Terakhir Diperbarui: **25 November 2026**

---

## 📊 Status Ringkasan per Komponen

| Komponen | Status | Cakupan yang Dikerjakan |
| :--- | :---: | :--- |
| **`Clevora.Domain`** | ✅ 100% | Entitas inti, Versioning, Soft-delete, Parser Placeholder, Formatter WA, Url Builder, Metrics |
| **`Clevora.Shared`** | ✅ 100% | DTO Kontrak Sync (`SyncPushRequest`, `SyncPullResponse`, `SyncConflictItem`), Template, Category, Attachment, Auth |
| **`Clevora.Application`**| ✅ 100% | Antarmuka `ITemplateService`, `ICategoryService`, `IComposerService`, `IAiMessageService`, `IBlobStorageService`, `IAttachmentService`, `ISyncService` |
| **`Clevora.Infrastructure`**| ✅ 100% | SQLite WAL `/home/data/app.db`, `MountPointValidator`, Identity Core, JWT Token, `SyncService`, `AttachmentService`, Azure AI, Azure Blob |
| **`Clevora.Web`** | ✅ 100% | Blazor Server 3-Panel Workspace, Minimal API (Auth, Categories, Templates, Composer, Attachments, Sync, Health), ApiKey Middleware, Swagger UI |
| **`Clevora.Mobile`** | ✅ 100% | .NET MAUI Blazor Hybrid, `LocalDatabaseService`, `MobileSyncOrchestrator`, Halaman Composer Offline, Templates, Sync Center |
| **`ops/` (DevOps)** | ✅ 100% | Nginx HTTPS config, Systemd service unit, SQLite WAL backup/restore script, Deploy script, Monitoring health probe |
| **`tests/`** | ✅ 100% | Domain Tests, Api Tests, Infrastructure Tests, Mobile Tests |
| **Dokumentasi** | ✅ 100% | PRD-00 s.d. PRD-12, `PLAN.md`, `Progress.md`, `README.md` (Bilingual) |

---

## 📝 Catatan Tugas (Tasks T-00 s/d T-10)

- [x] **T-00: Audit & Spesifikasi Awal**
  - Penyusunan `PRD-00` sampai `PRD-12` dan panduan steering `.kiro/`.
- [x] **T-01: Verifikasi Environment & Tools**
  - Solusi `.NET 10 LTS` dan multi-target platform.
- [x] **T-02: Scaffolding Proyek**
  - Solusi `Clevora.slnx` memuat 6 proyek utama dan 4 proyek pengujian.
- [x] **T-03: Implementasi Domain & Logic**
  - Logika parsing, WhatsApp Markdown rendering, perhitungan statistik, dan normalisasi nomor.
  - Penambahan kolom `Version` dan `IsDeleted` pada semua entitas.
- [x] **T-04: Implementasi Infrastruktur Database & Auth**
  - Database SQLite WAL di `/home/data/app.db`, verifikasi akses mount point, Identity, dan JWT Service.
- [x] **T-05: Implementasi Web Minimal API**
  - Endpoint Auth (Register, Login, Refresh, Logout, Me), Templates, Categories, Composer, Attachments, Mobile Sync, dan Health Checks.
  - Penambahan otentikasi via `X-API-Key` melalui middleware.
- [x] **T-06: Antarmuka Web Blazor Modern**
  - Workspace 3-Panel responsif: Panel Pustaka, Panel Editor & AI, Panel WhatsApp Phone Mockup.
  - Pencantuman identitas resmi Gravicode Studios & Kang Fadhil.
- [x] **T-07: Layanan Azure AI & Blob Attachments**
  - Abstraksi `AzureAiMessageService` (routing `gpt-5-mini` dan `gpt-5.6-luna`), `AttachmentService` dengan SAS download link.
- [x] **T-08: Aplikasi Mobile Offline-First (.NET MAUI Blazor)**
  - Penyimpanan lokal ponsel, pencatatan mutasi outbox saat offline, push/pull engine dengan resolusi konflik, halaman mobile UI.
- [x] **T-09: Artefak DevOps Ubuntu Azure VM**
  - Konfigurasi Nginx, unit systemd, backup/restore SQLite WAL, skrip deployment, dan pemantauan otomatis.
- [x] **T-10: Pengujian Otomatis & Dokumentasi Proyek**
  - Implementasi unit & contract tests di semua proyek test.
  - Penyusunan `PLAN.md`, `Progress.md`, dan `README.md` bilingual.
