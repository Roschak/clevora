# PRD-02 — System Architecture
**Versi:** 1.0 | **Tanggal:** 24 November 2026 | **Produk:** Clevora

## 1. Keputusan Arsitektur Terkunci
| Komponen | Teknologi / Ketentuan |
|---|---|
| Nama Produk | **Clevora** (bukan Quicky) |
| Framework Baseline | **.NET 10 LTS** |
| Web Frontend | **Blazor** (Interactive Web App / Server + WebAssembly atau Interactive Server terpadu) |
| Web Backend | **ASP.NET Core Web API** |
| Web Hosting Model | **Blazor dan Web API dalam satu aplikasi/deployment (`Clevora.Web`)** |
| Mobile App | **.NET MAUI Blazor Hybrid (`Clevora.Mobile`)** |
| Server Database | **SQLite** pada `/home/data/app.db` (persistent managed disk) |
| Mobile Database | **SQLite lokal** terpisah di penyimpanan privat aplikasi |
| File Storage | **Azure Blob Storage** |
| AI Services | **Azure Cognitive Services / Azure AI** (`gpt-5.6-luna` utama, `gpt-5-mini` ringan) |
| Server Hosting & OS | **Azure Virtual Machine — Ubuntu Linux** |
| Reverse Proxy & Process | **Nginx (HTTPS)** -> **Kestrel loopback (`127.0.0.1:5000`)** via **systemd** |

## 2. Struktur Solusi (`Clevora.sln`)
```text
Clevora.sln
├── src/
│   ├── Clevora.Domain/          # Entitas, Value Objects, aturan bisnis murni, parser placeholder
│   ├── Clevora.Application/     # Use cases, CQRS/Services, DTOs, interfaces (IAiMessageService, IBlobStorageService)
│   ├── Clevora.Infrastructure/  # EF Core SQLite server, Azure AI client, Azure Blob client, Identity/Auth
│   ├── Clevora.Shared/          # Kontrak API, DTO bersama Web & Mobile, komponen UI Razor bersama
│   ├── Clevora.Web/             # Host terpadu Blazor UI + ASP.NET Core Web API endpoints
│   └── Clevora.Mobile/          # .NET MAUI Blazor Hybrid, SQLite lokal, Sync Engine
├── tests/
│   ├── Clevora.Domain.Tests/
│   ├── Clevora.Application.Tests/
│   ├── Clevora.Infrastructure.Tests/
│   ├── Clevora.Api.Tests/
│   └── Clevora.Mobile.Tests/
├── ops/
│   ├── nginx/
│   ├── systemd/
│   ├── scripts/
│   ├── backup/
│   └── monitoring/
├── docs/
└── .kiro/
```

## 3. Alur Data & Batas Keamanan
1. **Browser <-> `Clevora.Web`:** HTTPS melalui Nginx; Blazor UI dan REST API berbagi domain/origin yang sama.
2. **`Clevora.Mobile` <-> `Clevora.Web`:** Hanya berkomunikasi melalui HTTPS REST API dengan Bearer Token; mobile **tidak pernah** terhubung langsung ke `/home/data/app.db`.
3. **`Clevora.Web` <-> Azure AI & Azure Blob Storage:** Kredensial/API key disimpan hanya di konfigurasi server backend (environment variables / secret file dengan izin terbatas), tidak pernah dikirim ke klien.
