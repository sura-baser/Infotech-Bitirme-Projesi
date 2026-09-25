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

## Yapılandırma (gizli bilgiler)

Şifre ve anahtarlar repoda tutulmaz, ortam değişkeni olarak verilir (`__` ayracı iç içe ayarları temsil eder):

| Ortam değişkeni | Nerede | Açıklama |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Web, Api | PostgreSQL bağlantı cümlesi (geliştirmede `appsettings.Development.json` içinde yerel varsayılan var) |
| `Seed__AdminPassword` | Web | **İlk kurulumda** yönetici hesabını oluşturur. Verilmezse yönetici oluşturulmaz ve uygulama uyarı yazar. Hesap bir kez oluştuktan sonra şifre panelden değiştirilir, bu değişken bir daha kullanılmaz. |
| `Seed__AdminEmail` | Web | İsteğe bağlı, varsayılan `admin@pastane.com` |
| `Jwt__Key` | Api | Token imzalama anahtarı, en az 32 karakter. Canlıda mutlaka kendi değerinizi verin. |

Boş bir veritabanıyla ilk çalıştırma örneği:

```bash
Seed__AdminPassword='<güçlü-bir-şifre>' dotnet run --project src/PastaneApp.Web
```

## API

`src/PastaneApp.Api` — JWT ile korunan REST API (kategori ve ürün CRUD, kayıt/giriş).
Örnek istekler: `src/PastaneApp.Api/PastaneApp.Api.http`
