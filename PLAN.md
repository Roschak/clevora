# 🗺️ Clevora — Roadmap & Development Plan

> **Produk:** Clevora  
> **Inisiator:** Dibuat oleh **Gravicode Studios** dipimpin oleh **Kang Fadhil**  
> **Teknologi:** .NET 10 LTS, Blazor Server, .NET MAUI Blazor Hybrid, SQLite (WAL), Azure OpenAI & Blob Storage

---

## 🎯 Visi & Tujuan
Clevora adalah platform produktivitas penyiapan dan personalisasi pesan WhatsApp lintas platform (*Web & Mobile Offline-First*) yang memegang teguh prinsip **Human-in-the-Loop**. Sistem membantu pengguna menyusun pesan, mengisi placeholder dinamis, dan merapikan format teks, sementara pengiriman pesan tetap dilakukan secara manual oleh pengguna melalui aplikasi WhatsApp resmi.

---

## 📅 Rencana Tahapan Rilis (Milestone & Roadmap)

### 🟢 Fase 1: Fondasi Arsitektur & Domain Core (Target: Selesai)
- [x] Inisialisasi arsitektur Clean Architecture pada .NET 10 LTS (`Clevora.Domain`, `Clevora.Application`, `Clevora.Infrastructure`, `Clevora.Web`, `Clevora.Mobile`, `Clevora.Shared`).
- [x] Implementasi mesin Domain murni:
  - `PlaceholderParser`: Ekstraksi & resolusi `{{variabel}}`.
  - `WhatsAppFormatter`: Konversi markdown WhatsApp (`*tebal*`, `_miring_`, `~coret~`, ````monospace````) ke HTML preview.
  - `TextMetricsCalculator`: Penghitung karakter, kata, dan estimasi batas panjang pesan.
  - `ClickToChatUrlBuilder`: Normalisasi nomor internasional E.164 dan perakitan URL `https://wa.me/...`.
- [x] Implementasi unit test domain (`Clevora.Domain.Tests`).

### 🟡 Fase 2: Backend API & Layanan Cloud (Target: Selesai)
- [x] Database SQLite server dengan Write-Ahead Logging (WAL) dan validasi mount volume `/home/data/app.db`.
- [x] Sistem Autentikasi: ASP.NET Core Identity + JWT Bearer Token + API-Key Middleware (`X-API-Key`).
- [x] Minimal API Endpoints:
  - Auth: Register, Login, Refresh, Logout, Me.
  - Categories: CRUD dengan soft-delete & tracking versi.
  - Templates: CRUD, pencarian, filter kategori, toggle favorit.
  - Composer: Preview & AI Assist (Fix, Rewrite, Compose).
  - Attachments: Upload Azure Blob Storage & SAS link generator.
  - Mobile Sync: Push outbox & Pull delta changes.
  - Health checks: `/health/live` & `/health/ready`.
- [x] Dokumentasi API interaktif Swagger / OpenAPI v1.

### 🟣 Fase 3: Antarmuka Web Modern (Blazor Interactive Server) (Target: Selesai)
- [x] Desain workspace 3-panel responsif:
  1. *Panel Pustaka:* Pemilih template cepat & penyisip link lampiran.
  2. *Panel Editor:* Formatting toolbar, dynamic placeholder form, penghitung metrik teks, dan tombol aksi AI.
  3. *Panel WhatsApp Preview:* Frame HP realistik dengan bubble chat hijau, waktu pengiriman, centang ganda, dan tombol direct link `wa.me/`.
- [x] Pencantuman identitas resmi: *"Dibuat oleh Gravicode Studios dipimpin oleh Kang Fadhil"*.

### 🔵 Fase 4: Aplikasi Mobile Offline-First (.NET MAUI Blazor) (Target: Selesai)
- [x] Arsitektur penyimpanan lokal SQLite (`LocalDatabaseService`).
- [x] Mesin sinkronisasi dua arah (`MobileSyncOrchestrator`):
  - Pencatatan mutasi lokal ke `SyncOutboxRecord` saat offline.
  - Push batch mutasi ke server saat online.
  - Pull perubahan delta dari server dan merge otomatis.
  - Deteksi dan resolusi konflik versi.
- [x] Antarmuka mobile: Halaman Composer, Katalog Template Offline, dan Pusat Sinkronisasi.

### 🟠 Fase 5: Operasional, DevOps & Pengujian (Target: Selesai)
- [x] Artefak server Ubuntu Azure VM di folder `ops/`:
  - `ops/nginx/clevora.conf`: Reverse proxy HTTPS & SSL termination.
  - `ops/systemd/clevora.service`: Daemon pengelola proses latar belakang Kestrel.
  - `ops/scripts/backup-sqlite.sh`: Backup berkas SQLite online yang aman (WAL-safe).
  - `ops/scripts/restore-sqlite.sh`: Skrip pemulihan database.
  - `ops/scripts/deploy.sh`: Skrip build dan deployment otomatis.
  - `ops/monitoring/health-probe.sh`: Pemeriksaan kondisi kesehatan aplikasi.
- [x] Paket pengujian otomatis: `Clevora.Domain.Tests`, `Clevora.Api.Tests`, `Clevora.Infrastructure.Tests`, `Clevora.Mobile.Tests`.

### 🚀 Fase 6: Pengemasan APK Android WhatsApp & Rilis (Target: Selesai)
- [x] Pengemasan dan penandatanganan paket APK Android Release (`com.gravicode.clevora-Signed.apk`).
- [x] Desain antarmuka mobile identik WhatsApp: WhatsApp Green theme, 4 tab (Community, Chat, Status, Calls), bubble chat realistik, centang biru ganda, form kontak baru, offline SQLite storage dan live simulated response.
- [x] Rangkaian 45 unit & integration tests lulus 100% (Domain, Infrastructure, API, Mobile).
- [ ] Integrasi perpustakaan sains & visualisasi Gravicode (*Visual.Net, GravicodeScience, RLNet*) untuk analisis tren efektivitas pesan.
- [ ] Dukungan multisandi suara / audio note WhatsApp menggunakan Azure Cognitive Services TTS.
