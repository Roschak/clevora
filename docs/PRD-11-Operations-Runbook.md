# PRD-11 — Operations Runbook
**Versi:** 1.0 | **Tanggal:** 24 November 2026 | **Produk:** Clevora

## 1. Pemeriksaan Kesehatan Harian
- Status service: `systemctl status clevora nginx`
- Status mount persistent disk: `mountpoint -q /home/data && df -h /home/data`
- Health endpoint: `curl -fsS https://<domain-clevora>/health/ready`
- Log aplikasi: `journalctl -u clevora -n 100 --no-pager`

## 2. Backup & Restore SQLite (`/home/data/app.db`)
- **Backup Terjadwal (Cron Harian):**
  - Menggunakan perintah aman SQLite online backup: `sqlite3 /home/data/app.db ".backup '/home/data/backups/app-$(date +%Y%m%d-%H%M%S).db'"` (jangan menyalin file `.db` mentah dengan `cp` saat transaksi WAL aktif).
  - Verifikasi integritas setiap hasil backup dengan `sqlite3 <backup_file> "PRAGMA integrity_check;"` dan unggah salinan terenkripsi ke container backup terpisah di Azure Blob Storage.
- **Prosedur Restore Teruji:**
  1. Hentikan service: `sudo systemctl stop clevora`
  2. Amankan file lama: `mv /home/data/app.db /home/data/app.db.corrupt.$(date +%s)` (beserta `-wal` dan `-shm` jika ada).
  3. Pulihkan file backup terverifikasi ke `/home/data/app.db` dan atur izin `chown clevora:clevora /home/data/app.db`.
  4. Jalankan ulang: `sudo systemctl start clevora` dan verifikasi `/health/ready` serta login uji.

## 3. Penanganan Insiden Umum
- **Aplikasi gagal start karena `/home/data` tidak ter-mount:** Periksa `lsblk` dan status disk di Azure Portal, jalankan `sudo mount -a`, pastikan `mountpoint /home/data` valid, lalu `sudo systemctl start clevora`.
- **Error Azure AI (429 / 404 deployment):** Periksa kuota dan nama deployment pada `/etc/clevora/clevora.env`; bila `gpt-5.6-luna` mengalami pembatasan kuota sementara, alihkan konfigurasi router fallback ke `gpt-5-mini` tanpa mengubah kode aplikasi.
