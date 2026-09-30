# Kısa Kullanım Açıklaması

## Proje Adı

Stok Takip Sistemi

## Kullanılan Teknolojiler

- C#
- Windows Forms
- SQLite veritabanı
- .NET 8

## Programı Çalıştırma

Visual Studio ile çalıştırmak için:

1. `StokTakipSistemi.csproj` dosyasını Visual Studio ile açın.
2. Üst menüdeki yeşil çalıştırma düğmesine basın.
3. Giriş ekranı açıldığında kullanıcı bilgileriyle giriş yapın.

Hazır uygulama dosyasından çalıştırmak için:

1. `Calisan_Uygulama` klasörünü açın.
2. `StokTakipSistemi.exe` dosyasına çift tıklayın.

## Varsayılan Kullanıcılar

Admin kullanıcısı:

```text
Kullanıcı adı: admin
Şifre: admin123
```

Personel kullanıcısı:

```text
Kullanıcı adı: personel
Şifre: personel123
```

## Yetkiler

Admin kullanıcısı ürün ekleme, güncelleme, silme, kategori işlemleri, stok giriş-çıkış ve rapor görüntüleme işlemlerini yapabilir.

Personel kullanıcısı stok giriş, stok çıkış ve mevcut stok görüntüleme ekranlarını kullanabilir.

## Veritabanı

Program SQLite veritabanı kullanır. Veritabanı dosyası program ilk çalıştırıldığında otomatik olarak oluşturulur.

SQL tablo oluşturma kodları `database_schema.sql` dosyasında verilmiştir.

## Temel Ekranlar

- Kullanıcı giriş ekranı
- Ürün tanımlama ekranı
- Kategori tanımlama ekranı
- Stok giriş ekranı
- Stok çıkış ekranı
- Mevcut stok listesi ekranı
- Raporlama ekranı
