# Engineering Standards — Clevora
- **Stack Terkunci:** `.NET 10 LTS`, `Clevora.Web` (Blazor + ASP.NET Core Web API dalam satu deployment), `Clevora.Mobile` (`.NET MAUI Blazor Hybrid`), SQLite server di `/home/data/app.db`, SQLite lokal terpisah di mobile, Azure Blob Storage, Azure AI (`gpt-5.6-luna` & `gpt-5-mini`).
- **Arsitektur Layer:** Pisahkan tanggung jawab secara tegas ke `Clevora.Domain`, `Clevora.Application`, `Clevora.Infrastructure`, `Clevora.Shared`, `Clevora.Web`, dan `Clevora.Mobile`.
- **Database Safety:** Pastikan validasi mount `/home/data` berjalan saat startup server; jangan gunakan Azure Files/NFS atau disk ephemeral untuk `/home/data/app.db`.
- **AI & Secrets:** Bungkus integrasi AI di balik abstraksi service backend; jangan pernah menaruh API key di client/mobile/frontend atau hardcode di source code.
