# Pastane E-Ticaret Projesi

Infotech Academy MCSD Yazılım Uzmanlığı bitirme projesi. El yapımı pasta/tatlı satışı yapan bir pastane için e-ticaret sitesi.

## Proje Yapısı

Solution katmanlı mimari ile kurgulanmıştır:

```
PastaneApp.slnx
└── src/
    ├── PastaneApp.Core     # Entity'ler ve interface'ler (IGenericRepository, IUnitOfWork vb.)
    ├── PastaneApp.Data     # EF Core (PostgreSQL), Generic Repository implementasyonu, migration'lar
    ├── PastaneApp.Web      # ASP.NET Core MVC — müşteri arayüzü + admin paneli + Identity
    └── PastaneApp.Api      # ASP.NET Core Web API — REST endpoint'leri
```

- **Veritabanı:** PostgreSQL (Npgsql EF Core provider)
- **Mimari:** SOLID prensipleri, Generic Repository Pattern
- **Kimlik doğrulama:** ASP.NET Core Identity

## Geliştirme

```bash
dotnet build PastaneApp.slnx
```
