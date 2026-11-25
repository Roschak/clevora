# PRD-10 — Azure VM Deployment
**Versi:** 1.0 | **Tanggal:** 24 November 2026 | **Produk:** Clevora

## 1. Topologi Infrastruktur Ubuntu Azure VM
- **OS:** Ubuntu Linux LTS (22.04 / 24.04) pada Azure Virtual Machine.
- **Persistent Managed Disk:** Dipasang (mounted) pada `/home/data` melalui `/etc/fstab` menggunakan UUID disk (`ext4`, opsi `defaults,nofail`).
- **Direktori Aplikasi & Data:**
  - `/opt/clevora/current` — Binary publish `.NET 10 LTS` (`Clevora.Web`).
  - `/opt/clevora/releases/{timestamp}` — Arsip rilis untuk rollback cepat.
  - `/home/data/app.db` — Database SQLite produksi (dimiliki oleh user sistem `clevora:clevora`).
  - `/home/data/backups` — Salinan backup harian terverifikasi.

## 2. Konfigurasi Reverse Proxy & Service
- **Kestrel:** Mendengarkan hanya `http://127.0.0.1:5000`.
- **Nginx:** Reverse proxy pada port `80` & `443` dengan sertifikat TLS (Let's Encrypt / Certbot), meneruskan header `X-Forwarded-For` dan `X-Forwarded-Proto`, serta WebSocket upgrade untuk Blazor.
- **systemd (`clevora.service`):**
  - Menjalankan `/opt/clevora/current/Clevora.Web` sebagai user non-root `clevora`.
  - Memiliki directive `RequiresMountsFor=/home/data` dan `ConditionPathIsMountPoint=/home/data` sebagai lapisan proteksi tambahan di level OS di samping validasi di dalam aplikasi.

## 3. Prosedur Deployment & Rollback
- **Deployment:** Build & test lulus -> unggah paket rilis ke `/opt/clevora/releases/{release_id}` -> backup `/home/data/app.db` -> arahkan symlink `/opt/clevora/current` -> `sudo systemctl restart clevora` -> verifikasi `curl -fsS https://localhost/health/ready`.
- **Rollback:** Arahkan kembali symlink `/opt/clevora/current` ke rilis stabil sebelumnya -> `sudo systemctl restart clevora` -> restore database dari backup pra-rilis apabila terdapat perubahan skema.
