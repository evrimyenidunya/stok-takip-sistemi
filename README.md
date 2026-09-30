# Stok Takip Sistemi

Bu proje, C# Windows Forms ile hazırlanmış masaüstü stok takip uygulamasıdır. Veritabanı olarak SQLite kullanır. Program ilk açıldığında `stok_takip.db` adlı veritabanı dosyasını otomatik oluşturur.

## İçerik

- Kullanıcı giriş ekranı
- Admin ve Personel rol ayrımı
- Ürün ekleme, silme, güncelleme ve listeleme
- Kategori ekleme, silme, güncelleme ve listeleme
- Stok giriş işlemi
- Stok çıkış işlemi
- Mevcut stok listesi
- Ürün adına göre arama
- Kategoriye göre filtreleme
- Stok miktarına göre sıralama
- Genel stok ve kategori bazlı raporlama

## Varsayılan Kullanıcılar

Program ilk çalıştığında iki kullanıcı otomatik oluşturulur.

| Rol | Kullanıcı adı | Şifre |
| --- | --- | --- |
| Admin | `admin` | `admin123` |
| Personel | `personel` | `personel123` |

Admin kullanıcısı bütün ekranlara erişebilir. Personel kullanıcısı stok giriş, stok çıkış ve mevcut stok listesi ekranlarını kullanabilir.

## Visual Studio ile Çalıştırma

1. Visual Studio'yu aç.
2. `Open a project or solution` seçeneğine tıkla.
3. Bu dosyayı aç:
   `StokTakipSistemi.csproj`
4. Üst kısımda yeşil çalıştırma düğmesine bas.
5. Giriş ekranında `admin / admin123` bilgileriyle giriş yap.

## Komut Satırı ile Çalıştırma

PowerShell açıp proje klasörüne gel:

```powershell
cd "C:\Users\yenid\Documents\Codex\2026-05-26\sana-bir-proje-konusu-ataca-m\StokTakipSistemi"
```

Projeyi derle:

```powershell
dotnet build
```

Projeyi çalıştır:

```powershell
dotnet run
```

## Veritabanı

Uygulama SQLite kullanır. Veritabanı dosyası program çalışınca otomatik oluşur:

```text
bin\Debug\net8.0-windows\stok_takip.db
```

Tablo yapısı ayrıca `database_schema.sql` dosyasında verilmiştir.

Temel tablolar:

- `Users`: kullanıcı adı, şifre ve rol bilgileri
- `Categories`: kategori bilgileri
- `Products`: ürün bilgileri ve mevcut stok miktarı
- `StockMovements`: stok giriş ve çıkış hareketleri

`Products.CategoryId` alanı `Categories.Id` alanına bağlıdır. Böylece her ürün bir kategoriyle ilişkilidir.

## Ekranların Kullanımı

### Ürün Tanımlama

Bu ekran sadece Admin rolünde görünür. Ürün kodu, ürün adı, kategori, marka/model, stok miktarı, lokasyon ve açıklama bilgileri girilir.

İşlemler:

- `Ekle`: yeni ürün kaydeder
- `Güncelle`: listeden seçilen ürünü günceller
- `Sil`: listeden seçilen ürünü siler
- `Temizle`: form alanlarını boşaltır

### Kategori Tanımlama

Bu ekran sadece Admin rolünde görünür. Kategori adı ve açıklama bilgisi girilir.

Bir kategoriye bağlı ürün varsa kategori silinemez. Bu, veri bütünlüğünü korumak için yapılmıştır.

### Stok Giriş

Firmaya yeni ürün geldiğinde kullanılır. Ürün seçilir, giriş miktarı ve tarih girilir. Kaydedildiğinde ürünün mevcut stok miktarı otomatik artar.

### Stok Çıkış

Üretim, proje veya servis için kullanılan malzeme stoktan düşülür. Çıkış miktarı mevcut stoktan fazlaysa program uyarı verir ve işleme izin vermez.

### Mevcut Stok Listesi

Tüm ürünlerin güncel stok durumu görüntülenir. Bu ekranda ürün adına göre arama, kategori filtresi ve stok miktarına göre sıralama vardır.

### Raporlama

Bu ekran sadece Admin rolünde görünür. Toplam ürün çeşidi, toplam stok miktarı ve kategori bazlı stok bilgileri listelenir.

## Proje Dosyaları

- `Program.cs`: uygulamanın başlangıç noktası
- `LoginForm.cs`: kullanıcı giriş ekranı
- `MainForm.cs`: ana ekran ve bütün işlem sekmeleri
- `Database.cs`: SQLite bağlantısı, tablo oluşturma ve veritabanı işlemleri
- `UserSession.cs`: giriş yapan kullanıcı bilgisi
- `database_schema.sql`: veritabanı tablo yapısı

## Not

Bu proje eğitim/ödev amacıyla hazırlanmıştır. Şifreler örnek olarak düz metin tutulmuştur. Gerçek bir ticari uygulamada şifrelerin hash yöntemiyle saklanması gerekir.
