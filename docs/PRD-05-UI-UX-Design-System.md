# PRD-05 — UI/UX Design System
**Versi:** 1.0 | **Tanggal:** 24 November 2026 | **Produk:** Clevora

## 1. Identitas Visual & Rebranding dari Template Eksisting
- **Nama & Branding:** Seluruh tampilan menggunakan identitas **Clevora** (logo, favicon, judul halaman, tema).
- **Pemanfaatan Aset `quicky-html-chat-template`:**
  - Mengadopsi fondasi tipografi **Inter** (`woff2`/`woff`), palet tema **Light Skin** & **Dark Skin**, sistem utilitas Bootstrap, serta set ikon **Heroicons** (`outline` & `solid`) dari folder template tanpa menghapus file asli di `quicky-html-chat-template/`.
  - **Penyesuaian Struktur Layout:** Mengubah layout 3-kolom *chat messenger* menjadi 3-panel *Message Preparation Workspace*:
    1. **Panel Kiri (Navigation & Library):** Filter Favorit, Kategori, Daftar Template, dan Tombol `+ Template Baru`.
    2. **Panel Tengah (Smart Message Composer):** Editor teks dengan toolbar formatting (`*B*`, `_I_`, `~S~`, `Code`), form input dinamis untuk `{{placeholder}}`, tombol **AI Assist** (Fix / Rewrite / Compose), dan pengelola lampiran Blob.
    3. **Panel Kanan (Live WhatsApp Preview & Launch):** Pratinjau gelembung pesan WhatsApp secara real-time, indikator jumlah kata & karakter, input nomor tujuan opsional, serta tombol **Salin Pesan** & **Buka di WhatsApp**.

## 2. Responsivitas & Aksesibilitas
- **Desktop (>= 1200px):** Tampilan 3-panel simultan.
- **Tablet (768px – 1199px):** Library dapat dilipat (drawer) sementara Composer & Preview berdampingan.
- **Mobile Web & .NET MAUI Blazor Hybrid (< 768px):** Navigasi tab bawah (`Template`, `Composer`, `Preview`, `Sync/Akun`) dengan target sentuh minimal `44x44px` dan indikator status *Online / Offline / Syncing* yang selalu terlihat.
