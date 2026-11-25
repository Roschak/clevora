# PRD-06 — Mobile Offline & Sync
**Versi:** 1.0 | **Tanggal:** 24 November 2026 | **Produk:** Clevora

## 1. Prinsip Offline-First (`.NET MAUI Blazor Hybrid`)
- Aplikasi mobile tidak pernah memblokir interaksi pengguna saat jaringan terputus.
- Semua operasi CRUD pada `Templates`, `Categories`, `Favorites`, dan `DraftMessages` menulis langsung ke **SQLite lokal** di perangkat dan mencatat entri pada tabel lokal `SyncOutbox`.
- Fitur yang membutuhkan layanan cloud secara inheren (**AI Assist** dan **Upload Lampiran Baru ke Azure Blob**) menampilkan status *Membutuhkan Koneksi Internet* secara jelas saat offline tanpa crash atau respons sukses palsu.

## 2. Mekanisme Sync Engine
1. **Trigger Sinkronisasi:** Saat aplikasi dibuka, saat status konektivitas berubah dari *Offline* ke *Online* (`Connectivity.ConnectivityChanged`), secara periodik di latar depan, dan melalui tombol **Sinkronkan Sekarang**.
2. **Fase Push (`POST /api/v1/sync/push`):**
   - Mengirim batch entri `SyncOutbox` berurutan berdasarkan `SequenceId`.
   - Setiap item menyertakan `EntityType`, `EntityId`, `Operation`, `BaseVersion`, `ClientUpdatedAtUtc`, dan `PayloadJson`.
3. **Fase Pull (`GET /api/v1/sync/pull`):**
   - Mengambil seluruh perubahan server dengan `ServerVersion > LastPulledVersion` dan menerapkannya ke SQLite lokal dalam satu transaksi atomik.

## 3. Conflict Resolution
- **Deteksi Konflik:** Terjadi jika `BaseVersion` yang dikirim klien lebih kecil dari `CurrentServerVersion` untuk `EntityId` yang sama.
- **Aturan Resolusi Deterministik:**
  1. **Delete vs Update:** Penghapusan eksplisit dengan versi lebih baru menandai arsip/soft-delete, namun salinan teks yang bertentangan disimpan sebagai draf pemulihan lokal agar tidak ada tulisan pengguna yang hilang diam-diam.
  2. **Update vs Update (Template/Draft):** Server mempertahankan versi server yang lebih baru sebagai record utama **dan** mengembalikan konflik ke klien; klien menyimpan versi lokal pengguna sebagai salinan duplikat bertanda `"[Konflik Lokal] {Judul}"` serta memberi notifikasi di pusat sinkronisasi sehingga pengguna dapat membandingkan atau menggabungkannya.
  3. **Toggle Favorit:** *Last-Write-Wins* berdasarkan `ClientUpdatedAtUtc`.

## 4. Target Build Mobile
- Target utama wajib: **Android Release** (`.aab` / `.apk` signed, diuji pada emulator/perangkat fisik dalam mode pesawat dan jaringan aktif).
- **iOS:** Tidak diasumsikan sebagai target wajib sebelum macOS build host, Apple Developer provisioning, dan kebutuhan platform dikonfirmasi pemilik proyek.
