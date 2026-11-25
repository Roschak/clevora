# Agent Execution Rules — Clevora
- **Pola Kerja:** Inspect repo -> cek Git & SDK/workloads -> baca steering & specs -> kerjakan satu *vertical slice* -> build & test -> perbaiki kegagalan -> update `docs/IMPLEMENTATION-LOG.md` -> lanjut ke task berikutnya yang tidak terblokir.
- **Gerbang Persetujuan Wajib:** Minta persetujuan pemilik proyek sebelum:
  1. Membuat resource Azure yang menimbulkan biaya.
  2. Menghapus data atau file.
  3. Menjalankan migrasi database yang bersifat destruktif.
  4. Mengubah keputusan arsitektur yang sudah dikunci (`PRD-12`).
  5. Melakukan deployment ke environment produksi.
  6. Mengubah konfigurasi keamanan atau hak akses berisiko.
- **Kejujuran Progres:** Jangan pernah mengarang status build, hasil test, atau isi file yang belum diverifikasi.
