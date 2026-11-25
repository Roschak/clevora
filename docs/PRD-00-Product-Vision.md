# PRD-00 — Product Vision
**Versi:** 1.0 | **Tanggal:** 24 November 2026 | **Produk:** Clevora | **Status:** Locked

## 1. Visi & Misi Produk
**Clevora** adalah platform produktivitas penyiapan pesan WhatsApp lintas platform (Web & Mobile Offline-First) yang membantu profesional, pelaku UMKM, tim penjualan, dan layanan pelanggan menyusun, mempersonalisasi, memformat, dan meninjau pesan WhatsApp secara cepat, konsisten, dan aman sebelum dikirim secara manual oleh pengguna.

## 2. Masalah yang Diselesaikan
- Penyusunan pesan berulang yang memakan waktu dan rawan salah ketik nama/nominal/detail pelanggan.
- Kesulitan menjaga konsistensi format WhatsApp (*bold*, _italic_, ~strikethrough~, ```monospace```) dan batasan panjang pesan.
- Kebutuhan akses cepat saat kondisi sinyal tidak stabil (mobile offline-first) tanpa kehilangan draf atau perubahan template.

## 3. Target Pengguna
1. **Admin / Customer Support:** Menjawab pertanyaan pelanggan dengan template terstruktur dan variabel dinamis.
2. **Sales / Account Executive:** Mengirim penawaran, tindak lanjut, dan lampiran brosur/dokumen melalui tautan resmi `wa.me` / `api.whatsapp.com`.
3. **Freelancer & Pemilik Bisnis:** Mengelola katalog pesan siap pakai di desktop maupun ponsel.

## 4. Batasan Produk Keras (Hard Non-Goals)
**Clevora BUKAN aplikasi WhatsApp dan BUKAN pengganti WhatsApp.**
Dilarang keras merancang atau mengimplementasikan:
- WhatsApp inbox atau tampilan daftar percakapan masuk/keluar WhatsApp.
- Pembacaan chat, riwayat pesan, atau daftar kontak dari akun WhatsApp pengguna.
- Pengiriman pesan otomatis (*auto-send* / background scheduler kirim pesan).
- *Bulk messaging* / *broadcast* otomatis ke banyak nomor.
- Integrasi, API tidak resmi, scraping WhatsApp Web, atau automasi sesi WhatsApp.

**Prinsip Human-in-the-Loop:** Pengguna wajib meninjau isi pesan di Clevora lalu membuka WhatsApp secara eksplisit untuk menekan tombol kirim sendiri.

## 5. Kriteria Keberhasilan (Success Criteria)
- **Web:** Blazor + ASP.NET Core Web API berjalan stabil dalam satu deployment di `.NET 10 LTS`.
- **Mobile:** `.NET MAUI Blazor Hybrid` berfungsi penuh secara offline-first dengan sinkronisasi & *conflict resolution*, serta paket **Android Release** teruji.
- **Cloud & Ops:** Berjalan di Ubuntu Azure VM (Nginx + Kestrel loopback + systemd + HTTPS), SQLite persistent disk (`/home/data/app.db`), Azure Blob Storage, Azure AI (`gpt-5.6-luna` & `gpt-5-mini`), serta backup & restore teruji.
