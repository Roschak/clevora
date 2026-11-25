# PRD-09 — Testing & QA
**Versi:** 1.0 | **Tanggal:** 24 November 2026 | **Produk:** Clevora

## 1. Strategi Piramida Pengujian
1. **`Clevora.Domain.Tests` (Unit Tests):**
   - Ekstraksi & substitusi `{{placeholder}}`, deteksi placeholder kosong, formatting teks WhatsApp, perhitungan kata/karakter, dan generasi URL `https://wa.me/...` (termasuk encoding karakter khusus & emoji).
2. **`Clevora.Application.Tests` (Unit & Service Tests):**
   - Use case CRUD template/kategori/favorit, routing model AI (`gpt-5-mini` vs `gpt-5.6-luna`) melalui mock `IAiMessageService`, validasi ukuran/MIME lampiran, dan logika *conflict resolution*.
3. **`Clevora.Infrastructure.Tests` (Integration Tests):**
   - Pengujian EF Core SQLite pada file temporer, verifikasi gagal startup saat direktori mount `/home/data` tidak tersedia/tidak writable, serta kontrak adapter Blob & AI.
4. **`Clevora.Api.Tests` (End-to-End API Tests):**
   - Menggunakan `WebApplicationFactory<Program>` untuk menguji alur Auth, isolasi data antar pengguna (IDOR prevention), CRUD endpoint, preview composer, AI endpoint, dan `/api/v1/sync/push` + `pull`.
5. **`Clevora.Mobile.Tests`:**
   - Pengujian penyimpanan SQLite lokal, antrean `SyncOutbox` saat offline, pemulihan saat online kembali, dan skenario konflik pembaruan.

## 2. Quality Gates Sebelum Rilis
- `dotnet test` lulus 100% tanpa test yang di-skip untuk fitur P0.
- `dotnet publish` untuk `Clevora.Web` (Linux x64) berhasil tanpa error.
- Build **Android Release** `Clevora.Mobile` berhasil dan terverifikasi skenario offline-first + sync.
- Tidak ada klaim selesai tanpa bukti log build dan hasil eksekusi pengujian nyata.
