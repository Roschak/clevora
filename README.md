# 🟢 Clevora

> **Smart WhatsApp Message Composer & Template Management Platform**  
> *Dibuat oleh **Gravicode Studios** dipimpin oleh **Kang Fadhil**.*

[![.NET 10 LTS](https://img.shields.io/badge/.NET-10.0%20LTS-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Web-Blazor%20Server-0078D4?logo=blazor)](https://blazor.net)
[![MAUI](https://img.shields.io/badge/Mobile-.NET%20MAUI%20Blazor-512BD4)](https://dotnet.microsoft.com/apps/maui)
[![Database](https://img.shields.io/badge/Database-SQLite%20WAL-003B57?logo=sqlite)](https://www.sqlite.org/)
[![License](https://img.shields.io/badge/License-Proprietary-green)](#)

---

## 🌐 Bahasa Indonesia

### 📌 Tentang Clevora
**Clevora** adalah platform produktivitas penyiapan dan personalisasi pesan WhatsApp lintas platform (Web & Mobile Offline-First) yang dirancang untuk membantu profesional, pelaku UMKM, tim penjualan (*sales*), dan layanan pelanggan (*customer support*) menyusun, mempersonalisasi, memformat, dan meninjau pesan WhatsApp secara cepat dan akurat.

> ⚠️ **Prinsip Human-in-the-Loop:**  
> Clevora **BUKAN** aplikasi WhatsApp dan **BUKAN** bot blast/spam otomatis. Clevora membantu merangkai pesan secara aman. Pengguna selalu meninjau pesan di layar preview lalu mengirimkannya secara manual melalui tautan resmi `wa.me/` ke aplikasi WhatsApp pengguna.

---

### ✨ Fitur Unggulan

1. **📁 Manajemen Template & Kategori:** Simpan dan kelompokkan template pesan dengan penanda favorit dan pencarian cepat.
2. **🪄 Placeholder Dinamis (`{{variabel}}`):** Ekstraksi otomatis variabel seperti `{{nama}}`, `{{produk}}`, atau `{{nominal}}` menjadi form isian instan tanpa merusak teks asli.
3. **👁️ WhatsApp Live Preview Realistik:** Pratinjau langsung bubble chat WhatsApp lengkap dengan format (*tebal*, _miring_, ~coret~, ```monospace```), jam pengiriman, dan centang ganda.
4. **📊 Metrik Teks Real-time:** Penghitung karakter, kata, dan peringatan dini jika pesan mendekati batas maksimal.
5. **🔗 WhatsApp Click-to-Chat Generator:** Normalisasi otomatis nomor tujuan (misal `0812...` menjadi `62812...`) dan pembuatan link resmi `https://wa.me/...`.
6. **🤖 Asisten Penulisan AI (Azure OpenAI):**
   - **Perbaiki (Fix):** Memperbaiki typo dan tata bahasa (`gpt-5-mini`).
   - **Tulis Ulang (Rewrite):** Mengubah gaya bahasa menjadi lebih profesional, santun, atau persuasif.
   - **Compose AI:** Menyusun pesan utuh berdasarkan instruksi singkat (`gpt-5.6-luna`).
7. **📎 Manajemen Lampiran (Azure Blob Storage):** Unggah brosur, katalog PDF, atau gambar dan sisipkan tautan unduh aman berbatas waktu (SAS) ke dalam teks pesan.
8. **📱 Mobile Offline-First (.NET MAUI Blazor):** Bekerja lancar tanpa koneksi internet di ponsel, dengan sinkronisasi dua arah otomatis (*Sync Engine & Conflict Resolution*) saat online kembali.
9. **🔒 Keamanan & API Integrasi:** Autentikasi ganda (JWT Bearer Token dan `X-API-Key`) dengan dokumentasi Swagger UI interaktif.

---

### 🏗️ Arsitektur Proyek (Clean Architecture)

```
clevora/
├── docs/                        # Spesifikasi produk lengkap (PRD-00 s.d. PRD-12)
├── ops/                         # Skrip deployment, Nginx, Systemd, & Backup SQLite
├── src/
│   ├── Clevora.Domain/          # Logika murni: Entitas, Parser Placeholder, Formatter WA
│   ├── Clevora.Application/     # Kontrak antarmuka: Template, Composer, Sync, AI, Blob
│   ├── Clevora.Infrastructure/  # Implementasi: SQLite WAL, Identity, JWT, Azure Services
│   ├── Clevora.Shared/          # DTO Kontrak Sync, Template, Auth, dan Lampiran
│   ├── Clevora.Web/             # Host Blazor Server 3-Panel Workspace & Minimal API + Swagger
│   └── Clevora.Mobile/          # Aplikasi .NET MAUI Blazor Hybrid (Offline-first)
├── tests/                       # Unit & Integration Tests (Domain, Api, Infra, Mobile)
├── PLAN.md                      # Peta jalan pengembangan
├── Progress.md                  # Pelacakan status pengerjaan fitur
└── Clevora.slnx                 # Solusi Visual Studio / .NET
```

---

### 🚀 Cara Menjalankan Aplikasi

#### 1. Prasyarat
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQLite3

#### 2. Menjalankan Aplikasi Web & API
```bash
# Masuk ke direktori web
cd src/Clevora.Web

# Jalankan aplikasi
dotnet run
```
Buka peramban di:
- **Aplikasi Web Blazor:** `https://localhost:5001` atau `http://localhost:5000`
- **Dokumentasi Swagger API:** `https://localhost:5001/swagger`

#### 3. Menjalankan Pengujian Otomatis
```bash
dotnet test Clevora.slnx
```

#### 4. Menginstal Aplikasi Mobile Android (APK)
Paket APK Release bertandatangan (*Signed APK*) siap pakai telah berhasil dibuat:
- **Lokasi APK:** `src/Clevora.Mobile/bin/Release/net10.0-android/publish/com.gravicode.clevora-Signed.apk`
- **Ukuran File:** ~26.6 MB
- **Desain UI:** WhatsApp Otentik (Header WhatsApp Green `#008069`, 4-Tab Community/Chats/Status/Calls, Chat Room wallpaper, speech bubble hijau & putih, double blue checkmarks, offline SQLite cache).

Pasang langsung ke ponsel Android atau emulator:
```bash
adb install src/Clevora.Mobile/bin/Release/net10.0-android/publish/com.gravicode.clevora-Signed.apk
```

---

## 🇬🇧 English

### 📌 About Clevora
**Clevora** is a cross-platform (Web & Mobile Offline-First) WhatsApp message preparation and personalization productivity platform designed to help professionals, SMEs, sales teams, and customer support representatives compose, personalize, format, and preview WhatsApp messages quickly and accurately.

> ⚠️ **Human-in-the-Loop Principle:**  
> Clevora is **NOT** a WhatsApp client or automated spamming tool. It assists in composing messages safely. The user always inspects the message on the live preview screen and explicitly triggers message dispatch through official `wa.me/` links.

---

### ✨ Key Features
- **Template & Category Management:** Organized, taggable, and favorite message library.
- **Dynamic Variable Placeholder Engine (`{{variable}}`):** Automatic form generation for dynamic fields.
- **Realistic WhatsApp Live Preview:** Accurate styling for bold, italic, strikethrough, monospace, and chat bubbles.
- **Real-time Text Metrics:** Character and word counts with length threshold alerts.
- **WhatsApp Click-to-Chat Generator:** E.164 international phone normalization and link generation.
- **Azure OpenAI Assistant:** Grammatical fixes (`gpt-5-mini`), tone rewrites, and full prompt composition (`gpt-5.6-luna`).
- **Secure File Attachments (Azure Blob Storage):** Auto-generated expiring SAS download links.
- **Mobile Offline-First (.NET MAUI Blazor):** Local device SQLite caching with bidirectional push/pull outbox sync.
- **Dual Authentication:** JWT Bearer and `X-API-Key` headers with Swagger OpenAPI exploration.

---

### 👨‍💻 Credits & Attribution
- **Creator / Studio:** Gravicode Studios  
- **Lead / Project Owner:** Kang Fadhil  
- **Architecture:** Clean Architecture & Modular Monolith on .NET 10 LTS  
- **Copyright:** © 2026 Gravicode Studios. All rights reserved.
