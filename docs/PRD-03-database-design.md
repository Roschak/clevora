# PRD-03 — Database Design
**Versi:** 1.0 | **Tanggal:** 24 November 2026 | **Produk:** Clevora

## 1. Arsitektur Dual-Database
### 1.1 Server Database (SQLite di Ubuntu Azure VM)
- **Lokasi Wajib:** `/home/data/app.db`
- **Media Penyimpanan:** Persistent Managed Disk Azure VM yang di-mount ke `/home/data`.
- **Larangan Keras:**
  - Dilarang menyimpan database produksi di disk sementara/ephemeral VM.
  - Dilarang meletakkan file SQLite di Azure Files / SMB / NFS (risiko korupsi locking SQLite).
- **Fail-Safe Startup Check:** Sebelum host ASP.NET Core mulai melayani trafik, `DatabaseMountHealthValidator` memverifikasi bahwa `/home/data` merupakan mount point aktif dan dapat ditulis (*writable*). Jika gagal, proses aplikasi wajib menghentikan startup dengan exit code non-zero dan log eksplisit.
- **Konfigurasi SQLite Produksi:** `Journal Mode=WAL`, `Synchronous=NORMAL`, `Foreign Keys=ON`, `Busy Timeout=5000`.

### 1.2 Mobile Database (SQLite Lokal)
- Disimpan pada `FileSystem.AppDataDirectory` di perangkat mobile.
- Terisolasi penuh dari database server; memiliki tabel tambahan untuk pelacakan status sinkronisasi (`SyncState`, `LocalUpdatedAtUtc`, `ServerVersion`, `IsDeleted`).

## 2. Skema Tabel Utama
1. **`Users`**: `Id (UUID)`, `Email (UNIQUE)`, `PasswordHash`, `DisplayName`, `Role`, `IsActive`, `CreatedAtUtc`, `UpdatedAtUtc`.
2. **`Categories`**: `Id (UUID)`, `UserId (FK)`, `Name`, `ColorHex`, `SortOrder`, `Version (INT)`, `UpdatedAtUtc`, `DeletedAtUtc`.
3. **`Templates`**: `Id (UUID)`, `UserId (FK)`, `CategoryId (FK NULL)`, `Title`, `Body`, `IsFavorite (BOOL)`, `UsageCount (INT)`, `Version (INT)`, `CreatedAtUtc`, `UpdatedAtUtc`, `DeletedAtUtc`.
4. **`TemplateAttachments`**: `Id (UUID)`, `TemplateId (FK)`, `UserId (FK)`, `FileName`, `ContentType`, `SizeBytes`, `BlobPath`, `Version (INT)`, `CreatedAtUtc`, `DeletedAtUtc`.
5. **`DraftMessages`**: `Id (UUID)`, `UserId (FK)`, `TemplateId (FK NULL)`, `RecipientPhone`, `RenderedBody`, `PlaceholderValuesJson`, `Version (INT)`, `UpdatedAtUtc`, `DeletedAtUtc`.
6. **`AiUsageLogs`**: `Id (UUID)`, `UserId (FK)`, `ActionType`, `ModelDeployment`, `PromptTokens`, `CompletionTokens`, `CreatedAtUtc`.
7. **`SyncCheckpoints`** *(server & mobile)*: `DeviceId`, `UserId`, `LastPulledVersion`, `LastSyncedAtUtc`.

## 3. Indeks & Integritas
- Indeks komposit pada `(UserId, DeletedAtUtc, UpdatedAtUtc)` di `Categories`, `Templates`, dan `DraftMessages` untuk mempercepat query daftar dan *delta sync*.
- Seluruh perubahan skema dikelola melalui EF Core Migrations dengan validasi migrasi sebelum dijalankan.
