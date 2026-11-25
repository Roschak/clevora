# PRD-08 — Security & Privacy
**Versi:** 1.0 | **Tanggal:** 24 November 2026 | **Produk:** Clevora

## 1. Autentikasi & Otorisasi
- Kata sandi di-hash menggunakan `ASP.NET Core Identity` (PBKDF2 / Argon2id dengan salt unik per pengguna).
- Kebijakan otorisasi berbasis kepemilikan sumber daya (*Resource-Based Authorization*): setiap query dan mutasi pada `Categories`, `Templates`, `DraftMessages`, dan `Attachments` wajib memfilter `UserId == CurrentUser.Id`.
- Rate limiting diterapkan pada endpoint `/api/v1/auth/*` dan `/api/v1/ai/*` untuk mencegah brute-force dan penyalahgunaan kuota AI.

## 2. Manajemen Rahasia (Secrets Management)
- **Zero Hardcoded Secrets:** Tidak ada API key, connection string produksi, atau signing key JWT di dalam repository Git.
- Konfigurasi produksi disuntikkan melalui file environment khusus systemd (`/etc/clevora/clevora.env`) dengan kepemilikan `root:clevora` dan izin `0640`.

## 3. Keamanan Transport & Web
- Nginx memaksa pengalihan HTTP (80) ke HTTPS (443) dengan TLS 1.2/1.3 modern dan header keamanan (`Strict-Transport-Security`, `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: strict-origin-when-cross-origin`).
- Kestrel hanya mendengarkan antarmuka loopback (`http://127.0.0.1:5000`) dan tidak pernah terekspos langsung ke jaringan publik.

## 4. Privasi & Kepatuhan Produk
- Clevora tidak meminta izin akses kontak ponsel (`READ_CONTACTS`) maupun izin aksesibilitas/notifikasi untuk membaca pesan WhatsApp.
- Prompt yang dikirim ke Azure AI hanya berisi teks pesan yang secara sadar dipilih pengguna untuk diproses.
