# Clevora MVP — Technical Design
## 1. Pemetaan Komponen
- **`Clevora.Domain`:** `MessageTemplate`, `TemplateCategory`, `MessageDraft`, `AttachmentItem`, `PlaceholderParser`, `WhatsAppFormatter`, `ClickToChatUrlBuilder`, `TextMetricsCalculator`.
- **`Clevora.Application`:** `ITemplateService`, `ICategoryService`, `IComposerService`, `IAiMessageService`, `IBlobStorageService`, `ISyncService`, `ICurrentUserService`.
- **`Clevora.Infrastructure`:** `ClevoraDbContext` (SQLite `/home/data/app.db`), `MountPointValidator`, `AzureAiMessageService` (`gpt-5.6-luna` & `gpt-5-mini`), `AzureBlobStorageService`, `JwtTokenService`.
- **`Clevora.Shared`:** DTO kontrak API, model sinkronisasi (`SyncPushRequest`, `SyncPullResponse`, `SyncConflictItem`), dan komponen Blazor UI bersama (`TemplateEditor`, `MessageComposer`, `WhatsAppPreviewCard`).
- **`Clevora.Web`:** Host Blazor + Minimal API / Controllers `/api/v1/*`, `/health/live`, `/health/ready`.
- **`Clevora.Mobile`:** MAUI Blazor Hybrid host, `LocalClevoraDbContext` (SQLite lokal), `ConnectivitySyncOrchestrator`, `ConflictResolver`.

## 2. Alur Eksekusi Vertical Slice
1. **Slice 1:** Domain core (Placeholder, Formatter, Counter, Click-to-Chat URL) + `Clevora.Domain.Tests`.
2. **Slice 2:** Auth + Categories + Templates persistence di SQLite + `MountPointValidator` + API endpoints + `Clevora.Api.Tests`.
3. **Slice 3:** Blazor Web Workspace (Library + Composer + Live Preview + Rebranding Clevora).
4. **Slice 4:** AI Writing Assistant (`IAiMessageService` router `gpt-5-mini` & `gpt-5.6-luna`) + Azure Blob Attachments.
5. **Slice 5:** `.NET MAUI Blazor Hybrid` offline SQLite + Sync Engine + Conflict Resolution + Android Release build.
6. **Slice 6:** Konfigurasi `ops/` (Nginx, systemd, backup/restore scripts) & deployment staging/production.
