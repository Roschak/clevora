# PRD-01 — Functional Requirements
**Versi:** 1.0 | **Tanggal:** 24 November 2026 | **Produk:** Clevora

## 1. Daftar Fitur Inti (MVP P0)
### FR-01: Manajemen Template Pesan
- Pengguna dapat membuat, melihat, mengedit, menduplikasi, mengarsipkan, dan menghapus (soft-delete) template pesan.
- Setiap template memiliki judul, isi pesan, kategori opsional, daftar placeholder yang terdeteksi otomatis, penanda favorit, serta metadata versi/waktu pembaruan.

### FR-02: Kategori & Favorit
- Pengguna dapat membuat, mengubah nama, memberi warna/ikon label, dan menghapus kategori template.
- Pengguna dapat menandai/membatalkan favorit (`IsFavorite`) dan memfilter template berdasarkan kategori, favorit, maupun pencarian kata kunci.

### FR-03: Placeholder Dinamis (`{{variable_name}}`)
- Sistem mendeteksi sintaks `{{nama_variabel}}` (huruf kecil/besar, angka, underscore) secara real-time.
- Composer menghasilkan input form otomatis untuk setiap placeholder unik.
- Validasi menampilkan peringatan eksplisit apabila masih ada placeholder yang belum terisi sebelum URL WhatsApp dibuat.

### FR-04: Message Composer & Live Preview
- Panel penyusunan pesan dengan pratinjau langsung hasil substitusi placeholder dan render visual formatting WhatsApp.
- Mendukung penyisipan template ke composer tanpa merusak template asli.

### FR-05: Formatting Pesan WhatsApp
- Toolbar formatting cepat untuk *Bold* (`*teks*`), _Italic_ (`_teks_`), ~Strikethrough~ (`~teks~`), dan `Monospace` (``` ```teks``` ```), serta daftar poin/penomoran.

### FR-06: Word & Character Counter
- Menghitung jumlah karakter (termasuk spasi), jumlah kata, dan estimasi paragraf secara real-time pada hasil pesan final (setelah substitusi placeholder).
- Memberikan indikator peringatan lembut jika panjang pesan mendekati/melewati batas praktis pesan WhatsApp (mis. 4.096 karakter / 65.536 karakter).

### FR-07: WhatsApp Click-to-Chat URL Generator
- Menerima nomor tujuan opsional (format internasional E.164, dinormalisasi dari `08...` ke `628...` bila sesuai aturan negara/kode negara) dan pesan terformat.
- Menghasilkan URL resmi `https://wa.me/{phone}?text={urlEncodedText}` atau `https://wa.me/?text={urlEncodedText}` (tanpa nomor).
- Menyediakan tombol **Salin Pesan**, **Salin Tautan**, dan **Buka di WhatsApp** (menyerahkan sepenuhnya aksi kirim ke aplikasi WhatsApp pengguna).

### FR-08: Asisten Penulisan AI
- Mode aksi AI:
  1. **Perbaiki Tata Bahasa & Ejaan (Fix):** Menggunakan model ringan (`gpt-5-mini`).
  2. **Tulis Ulang / Ubah Gaya Bahasa (Rewrite — Formal, Ramah, Singkat, Persuasif):** Menggunakan `gpt-5-mini` atau `gpt-5.6-luna` sesuai kompleksitas.
  3. **Susun Pesan dari Instruksi (Compose):** Menggunakan model utama (`gpt-5.6-luna`) dengan mempertahankan variabel `{{placeholder}}`.
- Seluruh hasil AI ditampilkan sebagai saran yang harus ditinjau/diterima pengguna sebelum menggantikan isi composer.

### FR-09: Penyimpanan Lampiran via Azure Blob Storage
- Pengguna dapat mengunggah file pendukung (gambar, PDF, dokumen) ke Azure Blob Storage melalui backend API terautentikasi.
- File dikaitkan dengan template atau draf pesan dan dapat disisipkan sebagai tautan unduhan aman (SAS URL berbatas waktu atau endpoint proxy terotorisasi) ke dalam teks pesan.

### FR-10: Mobile Offline-First & Sinkronisasi
- Seluruh operasi baca/tulis template, kategori, favorit, dan draf di `.NET MAUI Blazor Hybrid` bekerja langsung pada SQLite lokal tanpa membutuhkan koneksi internet.
- Saat koneksi tersedia, *Sync Engine* menyinkronkan perubahan lokal dan menarik pembaruan server melalui HTTPS API dengan *conflict resolution* deterministik.

## 2. Aturan Kualitas P0
- Tidak ada layar placeholder, tidak ada respons sukses palsu (*fake success*), dan tidak ada `TODO` pada fitur P0.
