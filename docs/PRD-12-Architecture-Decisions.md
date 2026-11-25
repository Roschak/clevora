# PRD-12 — Architecture Decisions (ADR)
**Versi:** 1.0 | **Tanggal:** 24 November 2026 | **Produk:** Clevora

- **ADR-001 — Nama Produk & Isolasi Aset Lama:** Nama resmi produk adalah **Clevora**. Folder workspace `QUICKY` dan aset referensi `quicky-html-chat-template` tidak dihapus/ditimpa secara destruktif.
- **ADR-002 — Framework Baseline `.NET 10 LTS`:** Seluruh proyek server, shared, mobile, dan test menggunakan target `.NET 10 LTS`.
- **ADR-003 — Unified Web Host (`Clevora.Web`):** Blazor frontend dan ASP.NET Core Web API digabung dalam satu aplikasi dan satu unit deployment untuk menyederhanakan operasi di Azure VM.
- **ADR-004 — Dual SQLite Architecture:** Server menggunakan SQLite di `/home/data/app.db` pada persistent managed disk; mobile menggunakan SQLite lokal terpisah dan berkomunikasi hanya lewat HTTPS API.
- **ADR-005 — Fail-Safe Mount Validation & Larangan NFS/Ephemeral:** SQLite produksi dilarang berada di ephemeral disk atau Azure Files/NFS. Aplikasi wajib menggagalkan startup secara aman jika `/home/data` tidak tersedia.
- **ADR-006 — Abstraksi Layanan AI & Dua Tingkat Model:** Menggunakan `IAiMessageService` di backend dengan `gpt-5.6-luna` (utama) dan `gpt-5-mini` (ringan); kredensial AI hanya berada di server.
- **ADR-007 — Azure Blob Storage untuk Lampiran:** File pendukung disimpan di Azure Blob Storage dengan tautan akses terotorisasi/SAS berbatas waktu.
- **ADR-008 — Batasan Non-WhatsApp Client:** Tidak ada inbox, pembacaan chat/kontak, auto-send, bulk messaging, atau API WhatsApp tidak resmi.
- **ADR-009 — Mobile Offline-First & Target Android Release:** `.NET MAUI Blazor Hybrid` wajib mendukung offline-first, sync engine, dan conflict resolution dengan build **Android Release** teruji; iOS tidak diasumsikan wajib sebelum lingkungan build dan kebutuhan platform dikonfirmasi.
- **ADR-010 — Topologi Keamanan VM:** Ubuntu Azure VM dengan Nginx HTTPS reverse proxy, Kestrel hanya listen di `127.0.0.1`, dikelola oleh `systemd`.
