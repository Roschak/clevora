# PRD-04 — API Specification
**Versi:** 1.0 | **Tanggal:** 24 November 2026 | **Produk:** Clevora

## 1. Konvensi Umum
- **Base Path:** `/api/v1`
- **Format:** JSON (`application/json`), UTF-8, standar error menggunakan RFC 7807 `ProblemDetails`.
- **Autentikasi:** Cookie Auth (untuk sesi Blazor Web) dan JWT Bearer Token + Refresh Token (untuk `.NET MAUI Blazor Hybrid` dan klien API).
- **Versioning & Concurrency:** Header `If-Match` / field `version` untuk deteksi konflik pembaruan.

## 2. Daftar Endpoint Utama
### 2.1 Auth (`/api/v1/auth`)
- `POST /api/v1/auth/register` — Registrasi akun pengguna.
- `POST /api/v1/auth/login` — Login, mengembalikan token akses + refresh token.
- `POST /api/v1/auth/refresh` — Pembaruan access token.
- `POST /api/v1/auth/logout` — Pencabutan sesi/refresh token.
- `GET /api/v1/auth/me` — Profil pengguna terautentikasi.

### 2.2 Categories (`/api/v1/categories`)
- `GET /api/v1/categories` — Daftar kategori aktif milik pengguna.
- `POST /api/v1/categories` — Buat kategori baru.
- `PUT /api/v1/categories/{id}` — Perbarui kategori (dengan pemeriksaan `version`).
- `DELETE /api/v1/categories/{id}` — Soft-delete kategori.

### 2.3 Templates (`/api/v1/templates`)
- `GET /api/v1/templates?categoryId=&favoriteOnly=&search=` — Daftar template terfilter.
- `GET /api/v1/templates/{id}` — Detail template beserta daftar placeholder terdeteksi dan lampiran.
- `POST /api/v1/templates` — Buat template baru.
- `PUT /api/v1/templates/{id}` — Perbarui template.
- `PATCH /api/v1/templates/{id}/favorite` — Toggle status favorit.
- `DELETE /api/v1/templates/{id}` — Soft-delete template.

### 2.4 Composer & Click-to-Chat (`/api/v1/composer`)
- `POST /api/v1/composer/preview` — Validasi placeholder, hitung kata/karakter, dan buat URL `https://wa.me/...`.

### 2.5 AI Writing Assistant (`/api/v1/ai`)
- `POST /api/v1/ai/assist`
  - Request: `{ "action": "fix" | "rewrite" | "compose", "text": "...", "instruction": "...", "tone": "formal|friendly|concise" }`
  - Response: `{ "suggestedText": "...", "modelUsed": "gpt-5-mini|gpt-5.6-luna", "preservedPlaceholders": ["customer_name"] }`

### 2.6 Attachments (`/api/v1/attachments`)
- `POST /api/v1/attachments/upload` (`multipart/form-data`) — Validasi MIME/ukuran, unggah ke Azure Blob Storage, simpan metadata.
- `GET /api/v1/attachments/{id}/link` — Hasilkan tautan akses berbatas waktu (SAS read-only) untuk disisipkan ke pesan.
- `DELETE /api/v1/attachments/{id}` — Hapus blob dan tandai soft-delete.

### 2.7 Mobile Sync (`/api/v1/sync`)
- `POST /api/v1/sync/push` — Mengirim batch mutasi lokal (`Create`, `Update`, `Delete`) dari perangkat mobile. Mengembalikan daftar item diterima dan daftar konflik (`409` item-level) beserta versi server terbaru.
- `GET /api/v1/sync/pull?sinceVersion={long}&deviceId={string}` — Mengambil perubahan delta sejak checkpoint terakhir.

### 2.8 Health (`/health/live`, `/health/ready`)
- `/health/live` — Liveness proses Kestrel.
- `/health/ready` — Memverifikasi mount `/home/data`, koneksi SQLite `/home/data/app.db`, serta konfigurasi layanan inti.
