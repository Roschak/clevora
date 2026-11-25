# PRD-07 — Azure AI & Blob Storage
**Versi:** 1.0 | **Tanggal:** 24 November 2026 | **Produk:** Clevora

## 1. Abstraksi Layanan AI (`IAiMessageService`)
- Seluruh logika bisnis di `Clevora.Application` bergantung hanya pada antarmuka `IAiMessageService` dan `IAiModelRouter`, sehingga penyedia atau nama deployment model dapat diganti melalui konfigurasi tanpa mengubah kode domain/application.
- **Routing Model:**
  - **LLM Ringan (`gpt-5-mini`):** Koreksi ejaan/tata bahasa (*Fix*), penyesuaian nada singkat (*Rewrite*), dan ringkasan cepat.
  - **LLM Utama (`gpt-5.6-luna`):** Penyusunan pesan kompleks dari poin-poin instruksi (*Compose*), penulisan ulang bernuansa tinggi, dan optimasi struktur komunikasi bisnis.
- **Verifikasi Pra-Produksi Azure AI:**
  - Karena *Model ID* dan *Azure Deployment Name* dapat berbeda pada resource Azure AI / Azure OpenAI, tim/operator wajib memverifikasi ketersediaan model, endpoint region, kuota TPM/RPM, dan versi API pada subscription Azure sebelum deployment produksi.
- **Keamanan Kredensial AI:**
  - API key / Entra ID credentials Azure AI **hanya** disimpan di backend (`Clevora.Web` via environment variables / secret file). Dilarang keras menyertakan kredensial AI di `Clevora.Mobile` maupun aset statis Blazor.

## 2. Integrasi Azure Blob Storage (`IBlobStorageService`)
- **Struktur Container:** Container privat `clevora-attachments` dengan pola path `{userId}/{year}/{month}/{attachmentId}-{sanitizedFileName}`.
- **Validasi Upload:**
  - Batas ukuran file maksimal `10 MB` per lampiran.
  - Allowlist tipe MIME aman (`image/jpeg`, `image/png`, `image/webp`, `application/pdf`, `application/vnd.openxmlformats-officedocument.wordprocessingml.document`, `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`).
- **Akses Berbagi:** Menghasilkan *User Delegation SAS* atau *Account SAS* berizin `Read-Only` dengan masa berlaku terkonfigurasi (default 7 hari) saat pengguna menyisipkan tautan file ke pesan WhatsApp.
