# Clevora — Implementation Log

## 2026-11-24 — Step 1 & Step 2 (Audit Awal & Specification Package)
- **Audit Arsip `Clevora.zip`:** Ditemukan `Clevora/Clevora.txt`, `Clevora/mcp.json`, dan direktori referensi UI `Clevora/quicky-html-chat-template/`. Belum terdapat file `.sln` maupun kode sumber `.NET` di dalam arsip.
- **Paket Spesifikasi Dihasilkan:** Menyusun lengkap `PRD-00` s.d. `PRD-12`, `.kiro/steering/00` s.d. `03`, `.kiro/specs/clevora-mvp/requirements.md`, `design.md`, `tasks.md`, dan `docs/IMPLEMENTATION-LOG.md`.

---

## 2026-11-25 — T-01: Verifikasi Lingkungan
- **.NET 10.0.401** SDK terverifikasi.
- **MAUI workload** berhasil diinstall (`dotnet workload install maui`).
- **JDK 21** (Microsoft OpenJDK 21.0.13.1) terinstall via winget.
- **Android SDK** terinstall di `%LOCALAPPDATA%\Android\Sdk`:
  - `platforms/android-36` ✅
  - `build-tools/35.0.0` ✅
  - `platform-tools/adb.exe` ✅
- **Git repo** diinisialisasi dan push ke `https://github.com/Roschak/clevora.git`.
- **Status:** T-01 ✅ SELESAI

---

## 2026-11-25 — T-02: Scaffold Solution
- `Clevora.sln` dibuat dengan 10 proyek:
  - `src/`: Domain, Application, Infrastructure, Shared, Web, Mobile
  - `tests/`: Domain.Tests, Infrastructure.Tests, Api.Tests, Mobile.Tests
- Project references antar layer dikonfigurasi sesuai arsitektur.
- NuGet packages: EF Core 10, Identity, JWT, Azure Blob, Azure AI OpenAI, Swashbuckle, sqlite-net-pcl.
- **Status:** T-02 ✅ SELESAI

---

## 2026-11-25 — T-03: Clevora.Domain
- **Entitas:** `MessageTemplate`, `TemplateCategory`, `AttachmentItem`, `MessageDraft` — termasuk field `Version`, `IsDeleted` untuk sync.
- **Services:**
  - `PlaceholderParser` — ekstrak & render `{{variable}}`
  - `WhatsAppFormatter` — markdown WA → HTML (`*bold*`, `_italic_`, `~strike~`, ` ```code``` `)
  - `TextMetricsCalculator` — char count, word count, estimated message parts
  - `ClickToChatUrlBuilder` — build & validasi `https://wa.me/...`
- **Tests:** 30/30 passed ✅
- **Status:** T-03 ✅ SELESAI

---

## 2026-11-25 — T-04: Clevora.Infrastructure
- `ClevoraDbContext` (EF Core + SQLite WAL, Identity)
- `MountPointValidator` — validasi `/home/data` writable saat startup
- `JwtTokenService` — generate JWT HS256
- `CategoryService`, `TemplateService` — CRUD + archive + favorite
- `ComposerService` — preview rendered message
- `AzureAiMessageService` — Fix/Rewrite (gpt-5-mini) & Compose (gpt-5.6-luna)
- `AzureBlobStorageService` — upload/list/delete/SAS URL
- `SyncService` — push/pull sync dengan conflict resolution
- `AttachmentService` — upload lampiran ke Azure Blob + simpan ke DB
- `DependencyInjection` extension method
- **Tests:** 4/4 passed ✅
- **Status:** T-04 ✅ SELESAI

---

## 2026-11-25 — T-05: Clevora.Web (API)
- **Auth endpoints:** POST `/api/v1/auth/register`, `/api/v1/auth/login`
- **Category endpoints:** GET/POST/PUT/POST archive
- **Template endpoints:** GET/search/favorites/by-category/POST/PUT/toggle-favorite/archive
- **Composer endpoints:** POST preview, POST AI
- **Attachment endpoints:** POST upload, GET list, GET link, DELETE
- **Sync endpoints:** POST push, GET pull
- **ApiKeyMiddleware** untuk akses via API key
- **Swagger/OpenAPI** dengan Bearer auth definition
- **Health checks:** `/health/live`, `/health/ready`
- **Tests:** 4/4 passed ✅
- **Status:** T-05 ✅ SELESAI

---

## 2026-11-25 — T-06: Clevora.Web (Blazor UI)
- `Home.razor` — dashboard dengan quick-nav cards
- `Templates.razor` — grid template + CRUD form + search + favorit
- `Categories.razor` — list kategori + modal form
- `Composer.razor` — 2-panel (input + preview WA), placeholder inputs, AI actions, click-to-chat URL
- `Favorites.razor` — list template favorit
- `NavMenu.razor` — navigasi sidebar dengan branding Clevora
- `app.css` — custom styling WhatsApp-inspired
- **Status:** T-06 ✅ SELESAI

---

## 2026-11-25 — T-07: Azure AI & Blob (oleh rekan — Iruzzz67)
- `IAiMessageService`, `IBlobStorageService`, `IAttachmentService`, `ISyncService` interfaces
- Implementasi `AzureAiMessageService`, `AzureBlobStorageService`, `AttachmentService`, `SyncService`
- Shared DTOs: `TemplateDtos`, `AuthDtos`, `AttachmentDtos`, `SyncModels`
- **Status:** T-07 ✅ SELESAI

---

## 2026-11-25 — T-08: Clevora.Mobile (MAUI Blazor Hybrid)
- `LocalDatabaseService` — SQLite persistent via `sqlite-net-pcl` (offline-first)
- `MobileSyncOrchestrator` — push/pull sync ke server saat online
- **Pages:** Home, Composer, Templates, SyncCenter
- **Build Android Debug:** ✅ 0 Error (8 warnings — SQLite library lama)
- **Tests:** 4/4 passed ✅
- **Keystore:** `keystore/clevora-release.keystore` dibuat untuk Play Store signing
- **Release APK:** sedang di-build...
- **Status:** T-08 🔄 IN PROGRESS

---

## 2026-11-25 — T-09: Ops (oleh rekan — Iruzzz67)
- `ops/nginx/clevora.conf` — Nginx reverse proxy config
- `ops/systemd/clevora.service` — systemd unit file
- `ops/scripts/deploy.sh`, `backup-sqlite.sh`, `restore-sqlite.sh`
- `ops/monitoring/health-probe.sh`
- **Status:** T-09 ✅ SELESAI

---

## Test Summary (semua suite)
| Suite | Passed | Failed |
|-------|--------|--------|
| Clevora.Domain.Tests | 30 | 0 |
| Clevora.Infrastructure.Tests | 4 | 0 |
| Clevora.Api.Tests | 4 | 0 |
| Clevora.Mobile.Tests | 4 | 0 |
| **Total** | **42** | **0** |
