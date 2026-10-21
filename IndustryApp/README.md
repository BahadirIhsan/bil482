# IndustryApp

## Proje

IndustryApp, kaza geçirmiş araç sahiplerini sanayi ustalarıyla buluşturan bir ters açık artırma pazaryeridir. Müşteri hasarı fotoğraf ve adım adım kriterlerle raporlar. Sistem uygun ustaları olay tabanlı olarak bulup bilgilendirir. Ustalar kapalı teklif verir, müşteri birini seçer. İletişim bilgileri yalnızca eşleşme anında açılır ve seçim belirli bir süre içinde geri alınabilir.

- Ders: BIL 482 Tasarım Desenleri · Ekip: Ömer Efe Öztürk, Bahadır İhsan Herdem
- Mimari: Clean Architecture ile katmanlı modüler monolit (.NET 8 Web API + React/TypeScript). Modüller arasında in-process publish–subscribe olay veriyolu kullanılır.
- Use case'ler: UC-01 rol bazlı kayıt ve giriş, UC-02 kaza raporu, UC-03 iş bildirimi ve teklif, UC-04 teklif seçimi ve iletişim; destekleyici akış YA-01 usta onayı ve katalog
- Dokümanlar: `docs/PA1` (Project Definition, Project Plan, SRS, Architecture Overview, Pattern–Use Case Mapping)

Durum: **Faz 1 – Temel altyapı** (`v0.1`). Repo, 6 projelik solution, domain modeli ve desen iskeletleri, veritabanı şeması ve referans veri, olay altyapısı, Swagger ve `/health` içeren boş API hazır. Henüz hiçbir use case uçtan uca çalışmıyor.

## Gereksinimler

- .NET 8 SDK (sürüm `src/backend/global.json` ile 8.0.4xx'e sabitlenmiştir)
- Node 20 veya üzeri (frontend)
- Docker ve Docker Compose (yerel PostgreSQL 16)

## Yerel çalıştırma

Komutlar repo kökünden çalıştırılır.

**1. Veritabanı.** Yerelde yalnızca `db` konteyneri çalışır.

```bash
docker compose up -d db
```

Makinenizde 5432 portu doluysa (ör. Homebrew PostgreSQL), repo köküne bir `.env` dosyası koyup başka port verin. `.env` repoya girmez.

```bash
echo "DB_PORT=5433" > .env
docker compose up -d db
```

**2. Gizli ayarlar.** Bağlantı dizesi ve JWT anahtarı user-secrets'ta tutulur, repoya girmez. Portu 1. adımdaki değere göre yazın.

```bash
cd src/backend/IndustryApp.WebApi
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=industryapp;Username=industryapp;Password=localdev"
dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 64)"
cd ../../..
```

**3. Şema ve referans veri.** `dotnet-ef` aracı repoda yerel araç olarak tanımlıdır (`src/backend/.config/dotnet-tools.json`).

```bash
cd src/backend
dotnet tool restore
dotnet ef database update -p IndustryApp.Infrastructure -s IndustryApp.WebApi
cd ../..
docker compose exec -T db psql -U industryapp -d industryapp < database/reference-data.sql
```

Kontrol: `SELECT level, count(*) FROM damage_categories GROUP BY level;` üç seviyeyi döndürmeli, `roles` tablosunda 3 satır olmalı. Veritabanında seed yoktur. Kullanıcı, rapor ve teklif gibi işlem verileri yalnızca uygulama ekranlarından oluşur. İlk admin için `database/admin-bootstrap.sql` kullanılır.

**4. API.**

```bash
cd src/backend
dotnet build        # uyarılar hata sayılır (TreatWarningsAsErrors)
dotnet test
dotnet run --project IndustryApp.WebApi
```

- Swagger: http://localhost:5080/swagger
- Sağlık kontrolü (veritabanı bağlantısı dahil): http://localhost:5080/health
- Loglar: konsol ve `IndustryApp.WebApi/logs/api-YYYYMMDD.log`. Her yanıt `X-Correlation-Id` başlığı taşır.

**5. Frontend.** `src/frontend/industryapp-web` altında yer alır. Vite geliştirme sunucusu `/api` ve `/hubs` isteklerini `http://localhost:5080`'e yönlendirir.

```bash
cd src/frontend/industryapp-web
npm install
npm run dev         # http://localhost:5173
```

**Migration ekleme.** Şema değişikliği önce teknik tasarım dokümanında güncellenir, sonra koda geçer.

```bash
cd src/backend
dotnet ef migrations add <Ad> -p IndustryApp.Infrastructure -s IndustryApp.WebApi -o Persistence/Migrations
dotnet ef migrations script -p IndustryApp.Infrastructure -s IndustryApp.WebApi -o ../../database/initial.sql
```

Üretimde uygulama açılırken migration yapılmaz. Dağıtım adımında migrations bundle çalıştırılır (Faz 4).

## Klasör yapısı

```
IndustryApp/
├── database/
│   ├── reference-data.sql        → roller, araç türleri, 3 seviyeli hasar ağacı, sözleşmeler (tekrar çalıştırılabilir)
│   ├── admin-bootstrap.sql       → ilk admin'e rol atama
│   ├── initial.sql               → Initial migration'ın SQL çıktısı (DDL karşılaştırması için)
│   └── dev/                      → yalnızca yerel betikler (*.local.sql repoya girmez)
├── deploy/                       → Caddyfile ve üretim .env şablonu (Faz 4)
├── docs/
│   ├── PA1/  PA2/  PA3/          → ödev teslimleri
│   └── diagrams/
├── src/
│   ├── backend/
│   │   ├── IndustryApp.sln
│   │   ├── Directory.Build.props → net8.0, Nullable, TreatWarningsAsErrors
│   │   ├── IndustryApp.Domain/          → entity'ler, durum makinesi, desen sınıfları, domain olayları (hiçbir projeye bağlı değil)
│   │   ├── IndustryApp.Application/     → servis/repository/altyapı arayüzleri, Result, Options (→ Domain)
│   │   ├── IndustryApp.Infrastructure/  → EF Core + PostgreSQL, Identity, repository'ler, UnitOfWork, olay kuyruğu (→ Application)
│   │   ├── IndustryApp.WebApi/          → controller'lar, middleware, Program.cs (→ Application, Infrastructure yalnızca DI için)
│   │   └── tests/
│   │       ├── IndustryApp.UnitTests/         → veritabanısız (→ Domain, Application)
│   │       └── IndustryApp.IntegrationTests/  → gerçek PostgreSQL konteyneriyle (→ WebApi)
│   └── frontend/
│       └── industryapp-web/      → React 18 + TypeScript + Vite
├── docker-compose.yml            → Faz 1'de yalnızca db
├── .editorconfig
├── .gitignore
└── README.md
```

Her modülün (Accounts, Catalog, Reports, Matching, Offers, Contacts, Notifications, Admin) her katmanda aynı adlı bir klasörü vardır.

Desen sınıfları `// [UC-0n][PAT-0n.m] <Desen>` yorumuyla işaretlidir.

Çalışma kuralları:
- Branch: `uc-0n/<kısa-ad>` veya `infra/<kısa-ad>`
- Commit: `[UC-0n] <tür>: <açıklama>` (altyapı işlerinde `[INFRA]`)
- Her PR'ı yazmayan ekip üyesi inceler.
