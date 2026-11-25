# Clevora MVP — Requirements Specification
## Ringkasan Kebutuhan Terukur
- **REQ-01 (Template & Kategori):** Pengguna dapat membuat, mengubah, mencari, memfavoritkan, dan mengarsipkan template pesan beserta kategorinya.
- **REQ-02 (Placeholder Engine):** Sistem mendeteksi `{{variable}}`, merender input dinamis, memvalidasi variabel kosong, dan menggantinya secara akurat pada preview.
- **REQ-03 (Composer, Formatting & Counter):** Composer mendukung formatting WhatsApp (`*`, `_`, `~`, `` ``` ``), pratinjau langsung, serta penghitungan kata & karakter real-time.
- **REQ-04 (Click-to-Chat URL):** Menghasilkan tautan `https://wa.me/...` yang valid dan ter-encode dengan benar untuk dikirim manual oleh pengguna.
- **REQ-05 (AI Writing Assistant):** Mendukung Fix, Rewrite (`gpt-5-mini`), dan Compose (`gpt-5.6-luna`) melalui abstraksi backend yang aman.
- **REQ-06 (Azure Blob Attachments):** Mendukung unggah, daftar, hapus, dan pembuatan tautan unduhan aman untuk lampiran pesan.
- **REQ-07 (Mobile Offline-First & Sync):** Aplikasi `.NET MAUI Blazor Hybrid` menyimpan data di SQLite lokal, bekerja offline, menyinkronkan mutasi saat online, dan menyelesaikan konflik tanpa kehilangan data.
- **REQ-08 (Server Hardening & Ops):** Server memvalidasi mount `/home/data`, menjalankan SQLite `/home/data/app.db`, melayani HTTPS via Nginx -> Kestrel loopback (`systemd`), dan memiliki mekanisme backup/restore teruji.
