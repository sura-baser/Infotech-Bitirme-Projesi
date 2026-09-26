# Atelier Sura

Infotech Academy MCSD Yazılım Uzmanlığı bitirme projem. Evde el yapımı tatlı yaptığım küçük pastanem "Atelier Sura" için gerçek bir e-ticaret sitesi hazırladım. Ürün fotoğraflarının hepsi bana aittir.

## Neden bu proje?

Hazır bir konu seçmek yerine kendi işim için bir site yapmak istedim. Menüdeki tatlıları, fiyatları ve alerjen bilgilerini gerçek verilerle girdim. Amacım yalnızca projeyi tamamlamak değil, ileride bu siteyle gerçekten sipariş almaktır.

## Sitede neler var?

**Müşteri tarafı**
- Ürünleri kategoriye göre listeleme ve isimle arama.
- Alerjen filtresi: örneğin "gluten içermeyenleri göster" diyerek listeyi daraltabilirsiniz.
- Ürün sayfasında bitmiş ürün fotoğrafı, malzemelerin fotoğrafı, dokununca açılan "oluşum" fotoğrafı ve alerjen bilgisi.
- Parti Kutusu: küçük, orta ya da büyük kutuyu seçip içine istediğiniz tatlılardan istediğiniz adette koyabilirsiniz.
- Üye olma, giriş yapma ve çıkış yapma.
- Sepet, sipariş verme ve ödeme.
- Siparişlerim sayfası ve sipariş detayı.
- Profilim sayfası: ad, telefon ve adres bilgilerini güncelleme, şifre değiştirme.
- Gizlilik (KVKK), iade ve mesafeli satış sayfaları.
- Telefonda ve tablette de düzgün görünür.

**Yönetici paneli** (yalnızca admin girebilir)
- Kategori ekleme, düzenleme, silme.
- Ürün ekleme, düzenleme, silme. Ürünün fotoğraflarını, malzemelerini ve alerjenlerini de buradan yönetiyorum.
- Siparişleri görme ve durumunu değiştirme (beklemede, hazırlanıyor, yolda, teslim edildi, iptal edildi).
- Kullanıcı ekleme, düzenleme ve silme.

**Kurallar**
- Her şeyi tek tek elle hazırladığım için bir üründen tek seferde en fazla 2 adet sipariş verilebilir.
- Stoktan fazla ürün sipariş edilemez. Ödeme alınınca stok kendiliğinden düşer.

## Kullandığım teknolojiler

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core ve PostgreSQL
- ASP.NET Core Identity (üyelik ve roller: Admin, Customer)
- ASP.NET Core Web API (JWT ile giriş)
- iyzico (ödeme)
- Docker (veritabanını çalıştırmak için)

## Proje klasörleri

Projeyi derslerde öğrendiğimiz gibi katmanlara böldüm:

- **PastaneApp.Core:** Tabloların karşılığı olan sınıflar (ürün, sipariş, sepet gibi) ve arayüzler. Başka hiçbir katmana bağlı değildir.
- **PastaneApp.Data:** Veritabanı işleri. Generic Repository, UnitOfWork ve migration dosyaları burada.
- **PastaneApp.Services:** Sepet, sipariş ve parti kutusu işlerinin mantığı. Controller'lar bu işleri kendileri yapmaz, buradaki servislere yaptırır.
- **PastaneApp.Web:** Sitenin kendisi. Müşteri sayfaları ve yönetici paneli burada.
- **PastaneApp.Api:** Web API. Kategori ve ürünleri dışarıya açar.

Kodu yazarken SOLID prensiplerine dikkat ettim. Her servisin tek bir işi vardır ve controller'lar somut sınıflara değil arayüzlere bağlıdır.

## Nasıl çalıştırılır?

Bilgisayarınızda .NET 10 SDK ve Docker kurulu olmalıdır.

1. Önce veritabanını başlatın:

```bash
docker compose up -d
```

2. Ardından siteyi çalıştırın. İlk çalıştırmada admin hesabı oluşsun diye kendi belirleyeceğiniz bir şifre vermeniz gerekir:

```bash
Seed__AdminPassword='kendi-sifreniz' dotnet run --project src/PastaneApp.Web --launch-profile http
```

3. Tarayıcıdan `http://localhost:5164` adresini açın.

Tabloları elle oluşturmanız gerekmez, site açılırken veritabanı kendiliğinden hazırlanır. Admin kullanıcısının e-postası varsayılan olarak `admin@pastane.com` olur. Admin hesabı bir kez oluştuktan sonra şifreyi yönetici panelinden değiştirebilirsiniz.

Şifre gibi gizli bilgileri kodun içine ya da repoya yazmadım, hepsi dışarıdan verilir.

## Ödeme nasıl çalışıyor?

Ödeme için iyzico entegrasyonunu yaptım. Sitede kart bilgisi girilmez ve saklanmaz, kart bilgileri iyzico'nun kendi güvenli formunda girilir. Ödeme bitince sonucu iyzico'dan tekrar sorup doğruluyorum. Ödeme başarılıysa sipariş "ödendi" olur.

İyzico anahtarlarım henüz girilmediği için geliştirme sırasında **demo ödeme** çalışır: "Demo Ödemeyi Tamamla" düğmesine basınca ödeme alınmış sayılır ve gerçek para çekilmez. Demo yalnızca geliştirme ortamında ve anahtar girilmemişse devreye girer, yayında asla çalışmaz. İyzico test anahtarları girildiğinde demo kapanır ve gerçek iyzico test ortamı kullanılır.

## Web API

Kategori ve ürünler için listeleme, tek kayıt getirme, ekleme, güncelleme ve silme işlemlerini içeren bir API hazırladım. Ayrıca kayıt olma ve giriş yapma işlemleri de vardır. Listeleme herkese açıktır, ekleme, güncelleme ve silme yalnızca admin girişi (token) ile yapılabilir.

Başlıca adresler:
- `POST /api/auth/register` ve `POST /api/auth/login`
- `/api/categories` ve `/api/categories/{id}`
- `/api/products` ve `/api/products/{id}`

API, geliştirme ortamında `http://localhost:5214` adresinde çalışır. Örnek istekler `src/PastaneApp.Api/PastaneApp.Api.http` dosyasındadır.

## Ayarlar

Gizli bilgiler ortam değişkeni olarak verilir (iç içe ayarlarda `__` kullanılır):

- `ConnectionStrings__DefaultConnection`: PostgreSQL bağlantısı. Geliştirmede hazır bir varsayılanı vardır.
- `Seed__AdminPassword`: İlk admin hesabının şifresi.
- `Seed__AdminEmail`: İsteğe bağlı, admin e-postası.
- `Jwt__Key`: API'nin token imzalama anahtarı, en az 32 karakter. Geliştirmede hazır bir değeri vardır, yayında kendi değerinizi vermelisiniz.

## Sonraki adımlar

Proje teslimi için gereken her şey tamamlandı. Bundan sonra yapmak istediklerim:
- İyzico test hesabıyla gerçek bir deneme ödemesi yapmak.
- Siteyi internette yayına almak.
- Şirketimi kurup gerçek satışa başlamak.
