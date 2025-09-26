# 🔑 ID Performance Tests (UUID, ULID, INT)

Bu proje, farklı **benzersiz kimlik stratejilerinin (identifier strategies)** veritabanı üzerindeki performanslarını karşılaştırmak amacıyla hazırlanmıştır.  
Testler **.NET 9 + EF Core 9 + SQL Server** kullanılarak yapılmıştır.  

## 📌 Amaç
Veritabanlarında sıkça kullanılan **GUID / UUID** yapıları, her zaman en iyi performansı vermez.  
Bu projede şu sorulara cevap arıyoruz:  
- UUID'ler neden veritabanı performansını olumsuz etkileyebilir?  
- Bu her projede kritik midir, yoksa sadece büyük veri setlerinde mi hissedilir?  
- UUID yerine **ULID** veya **UUIDv7** gibi modern alternatifler ne avantaj sağlar?  
- Projemizde hangi kimlik stratejisini seçmeliyiz?  

## 🚀 Desteklenen ID Stratejileri
Projede aşağıdaki veri modelleri tanımlanmıştır:

1. **INT IDENTITY** → (Artan sayılar, clustered PK)  
2. **GUID v4** → Rastgele GUID (default `Guid.NewGuid()`)  
3. **ULID String (26 char)** → Sıralanabilir, URL-friendly  
4. **ULID Binary (16 byte)** → Daha kompakt depolama  
5. **GUID v4 + Clustered CreatedOn** → Nonclustered PK, zaman bazlı clustered index  
6. **GUID v7 (RFC 9562, 2024)** → Zaman sıralı GUID  

## ⚙️ Kurulum
```bash
# Klasör oluştur
mkdir IdStrategiesDemo && cd IdStrategiesDemo

# Solution + Console App
dotnet new sln -n IdStrategiesDemo
dotnet new console -n IdStrategiesDemo
dotnet sln add IdStrategiesDemo/IdStrategiesDemo.csproj

# EF Core + SQL Server
dotnet add IdStrategiesDemo package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add IdStrategiesDemo package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.0
dotnet add IdStrategiesDemo package Microsoft.EntityFrameworkCore.Tools --version 9.0.0

# ULID desteği
dotnet add IdStrategiesDemo package NUlid --version 1.7.3
```bash

## 🧪 Benchmark Ayarları

appsettings.json dosyasında test parametrelerini değiştirebilirsiniz:

```bash
"Benchmark": {
  "InsertCount": 1000,      
  "BatchSize": 100,        
  "PageSkip": 500,          
  "PageTake": 100          
}
```

# 📊 UUID, ULID ve INT Performans Karşılaştırması

## 🚀 Insert Performansı
- **GUIDv4, ULID** → Küçük datasetlerde hızlı (0.10–0.13s).
- **INT** → Beklenenden biraz yavaş (0.21s) ama büyük datasetlerde en stabil yöntem.
- **GUIDv7** → İlk testlerde 0.75s → EF Core batch overhead olabilir.

## 🔍 Query Performansı
- Küçük datasetlerde (1000 kayıt) tüm stratejiler **0.00–0.01s**.
- Fark milyonlarca kayıt olduğunda ortaya çıkar:
  - **GUIDv4** → %99 fragmentation → random erişim → yavaş
  - **ULID & GUIDv7** → sıralı → düşük fragmentation → hızlı
  - **INT** → her zaman en stabil ve hızlı

## 📷 Grafikler
*(Grafikler eklenecek)*

## 📝 Notlar
- **RFC 9562 (2024)** ile tanımlanan **UUIDv7** çok yakında **.NET 9** içerisinde native olarak desteklenecek.
- **ULID**, string formatının okunabilirliği ve URL uyumluluğu ile pratik bir alternatif.
- Gerçek hayat kullanımı, dataset büyüklüğüne ve sistemin dağıtık olup olmamasına göre değişir.

---

👩‍💻 **Yazar:** Kardel Rüveyda Çetin  
📚 **Medium:** *UUID vs ULID vs INT: Modern Identifier Strategies*

