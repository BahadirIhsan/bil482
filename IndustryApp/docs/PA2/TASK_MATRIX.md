# Görev Matrisi – PA2 (UC-04 Teklif Seçimi ve İletişim Bilgisinin Açılması)

BIL 482 Tasarım Desenleri · Ekip: **Ömer Efe Öztürk** (GitHub: `omerefeozturk`), **Bahadır İhsan Herdem** (GitHub: `BahadirIhsan`)

Bu dosya PA2'deki iş bölümünü gösterir: kod, test ve doküman parçalarını kimin yazdığını ve kimin incelediğini listeler. Her üye kendi parçasını kendi GitHub hesabıyla gönderdi. Ömer'in kodu PR #4 ile, Bahadır'ın kodu PR #5 ile `main`'e girdi.

İzlenebilirlik zinciri PA1'deki numaralarla aynıdır: `UC-04 → FR-04.n → PAT-04.n → kod yorumu // [UC-04][PAT-04.n] → test TC-04.n.k → commit [UC-04] <tür>: <açıklama>`.

## 1. Kod

| # | İş | Konum | Desen / izlenebilirlik | Yazan | İnceleyen |
|---|----|-------|------------------------|-------|-----------|
| K1 | Seçim komutu, snapshot ve geri yükleme (`IReportCommand`, `SelectOfferCommand`, `IReportCommandFactory`, `ReportCommandFactory`, `ReportCommandNames`) | `IndustryApp.Application/Offers/Commands/` | PAT-04.2 Command · FR-04.2, FR-04.3 | Ömer Efe Öztürk | Bahadır İhsan Herdem |
| K2 | Seçim, geri alma ve seçim durumu servisi (`OfferSelectionService`, sözleşmeler) | `IndustryApp.Application/Selection/` | PAT-04.1 State + PAT-04.2 · FR-04.2, FR-04.3 | Ömer Efe Öztürk | Bahadır İhsan Herdem |
| K3 | İletişim bilgisi servisi (`ContactService`, `MatchContactResponse`) | `IndustryApp.Application/Contacts/` | PAT-04.3 Proxy · FR-04.4 | Ömer Efe Öztürk | Bahadır İhsan Herdem |
| K4 | Rapor detayı, tamamlama ve iptal (`ReportService`, `ReportDetailResponse`) | `IndustryApp.Application/Reports/` | PAT-04.1 State · FR-04.5 | Ömer Efe Öztürk | Bahadır İhsan Herdem |
| K5 | `OfferSelectionController`, `ContactsController`, `ReportsController` (endpoint 29–32) | `IndustryApp.WebApi/Controllers/` | FR-04.2 – FR-04.5 | Ömer Efe Öztürk | Bahadır İhsan Herdem |
| K6 | UC-04 hata kodları (`NO_ACTIVE_SELECTION`, `CONTACT_NOT_FOUND` vb.) ve DI kayıtları | `Domain/Common/ErrorCodes.cs`, `Application/Common/Errors.cs`, `Application/DependencyInjection.cs` | – | Ömer Efe Öztürk | Bahadır İhsan Herdem |
| K7 | Teklif sıralama stratejileri (`IOfferSortStrategy`, `PriceAscendingSort`, `FastestCompletionSort`, `NewestFirstSort`), `OfferService`, `OffersController` (endpoint 27) | `IndustryApp.Application/Offers/Sorting/`, `WebApi/Controllers/` | PAT-04.4 Strategy · FR-04.1 | Bahadır İhsan Herdem | Ömer Efe Öztürk |
| K8 | Kimlik: JWT, refresh token, `CurrentUser`, `IdentityService`, yetki politikaları, rate limiting | `Infrastructure/Identity/`, `WebApi/Extensions/` | UC-04'ün sahiplik ve rol kontrolleri | Bahadır İhsan Herdem | Ömer Efe Öztürk |
| K9 | Kayıt ve giriş (`IUserProfileFactory`, `CustomerProfileFactory`, `MechanicProfileFactory`, `AuthService`, `AuthController`, katalog uçları) | `Application/Accounts/`, `Application/Catalog/`, `WebApi/Controllers/` | PAT-01.1 Factory Method · UC-01 (UC-04'ün ön koşulu) | Bahadır İhsan Herdem | Ömer Efe Öztürk |
| K10 | Bildirimler ve anlık güncelleme (`NotificationService`, UC-04 olay handler'ları, `ReportStatusRealtimeHandler`, `NotificationHub`, `SignalRRealtimePublisher`, `NotificationsController`) | `Application/Notifications/`, `WebApi/Realtime/` | PAT-03.1 Publish–Subscribe · FR-04.2 bildirim adımı | Bahadır İhsan Herdem | Ömer Efe Öztürk |
| K11 | UC-04 ekranları: `CustomerReportDetailView`, `OfferList`, `SortSelect`, `PriceBreakdown`, `ConfirmSelectDialog`, `SelectionBanner`, `ReportActionDialog`, `MatchView` (model, controller, view) | `src/frontend/industryapp-web/src/modules/{reports,offers,selection,contacts}/` | FR-04.1 – FR-04.5 | Ömer Efe Öztürk | Bahadır İhsan Herdem |
| K12 | Frontend ortak katmanı (`shared/http`, `shared/auth`, `shared/errors`, `shared/format`, `shared/realtime`), router, layout'lar, giriş ve kayıt ekranları, `NotificationBell` | `src/frontend/industryapp-web/src/shared/`, `src/app/`, `src/modules/{auth,account,catalog,notifications,system}/` | UC-01, UC-04 bildirimleri | Bahadır İhsan Herdem | Ömer Efe Öztürk |
| K13 | UC-04 yerel test verisi betiği | `database/dev/uc04-dev-data.sql` | – | Ömer Efe Öztürk | Bahadır İhsan Herdem |
| K14 | Desen yorum şablonu (Niyet, Neden, Kazanım, Metrik): her üye kendi desen sınıfına ekledi | K1, K7, K9 ve PA1'deki desen sınıfları | Tüm PAT-04.n | Her üye kendi parçası | Diğer üye |

## 2. Testler

| Kod | Test | Konum | Yazan |
|-----|------|-------|-------|
| TC-04.1.1 | `SelectOffer_NotOwner_ReturnsForbidden` | `UnitTests/Application/Selection/OfferSelectionServiceTests.cs` | Ömer Efe Öztürk |
| TC-04.1.2 – 4 | Her sıralama stratejisi için bir test | `UnitTests/Application/Offers/OfferSortStrategyTests.cs` | Bahadır İhsan Herdem |
| TC-04.2.1, TC-04.2.2 | State geçiş testleri | `UnitTests/Domain/ReportStateTests.cs` | Bahadır İhsan Herdem (PA1) |
| TC-04.2.3 | `ConcurrentSelect_SecondRequestGets409` | `IntegrationTests/Uc04/OfferSelectionIntegrationTests.cs` | Ömer Efe Öztürk |
| TC-04.3.1, TC-04.3.2 | Geri alma, süre içinde ve süre dışında | `UnitTests/Application/Offers/SelectOfferCommandTests.cs` | Ömer Efe Öztürk |
| TC-04.4.1 | `ContactProxy_NotMatched_ReturnsMaskedPhone` | `UnitTests/Domain/ContactInfoProxyTests.cs` | Bahadır İhsan Herdem (PA1) |
| TC-04.4.2 | `Contact_NotMatchedMechanic_Gets403OrMasked` | `IntegrationTests/Uc04/OfferSelectionIntegrationTests.cs` | Ömer Efe Öztürk |
| – | Seçim, iletişim ve rapor servisi testleri, Testcontainers altyapısı | `UnitTests/Application/{Selection,Contacts,Reports}/`, `IntegrationTests/Infrastructure/` | Ömer Efe Öztürk |
| – | Kayıt, bildirim ve teklif servisi testleri | `UnitTests/Application/{Accounts,Notifications,Offers}/`, `IntegrationTests/Uc01/` | Bahadır İhsan Herdem |
| E2E-04, 05, 07 | Elle uçtan uca senaryolar (SDD bölüm 14) | – | Birlikte |

## 3. Dokümanlar

Software Design Document (`docs/PA2/SoftwareDesignDocument.pdf`). Dokümanın kendi görev matrisi ilk sayfadadır; özeti aşağıdadır.

| # | İş | Sorumlu | İnceleyen |
|---|----|---------|-----------|
| T1 | Bölüm 1–4: genel bakış, bağlam, özellikler, varsayımlar | Ömer Efe Öztürk | Bahadır İhsan Herdem |
| T2 | Bölüm 5–6: mimari, bileşenler, diyagramlar | Bahadır İhsan Herdem | Ömer Efe Öztürk |
| T3 | Bölüm 7: ER diyagramı, veri akışı, doğrulama kuralları | Bahadır İhsan Herdem | Ömer Efe Öztürk |
| T4 | Bölüm 8: desenler, sınıf ve sıra diyagramları | Ömer Efe Öztürk | Bahadır İhsan Herdem |
| T5 | Bölüm 9–11: uygulama notları, arayüz, dış arayüzler | Ömer Efe Öztürk | Bahadır İhsan Herdem |
| T6 | Bölüm 12–15: performans, hata yönetimi, test edilebilirlik, kurulum | Bahadır İhsan Herdem | Ömer Efe Öztürk |
| T7 | Bölüm 16–17, son kontrol, PDF çıktısı, GitHub'a yükleme | Ömer Efe Öztürk | Bahadır İhsan Herdem |

Repo dosyaları:

| İş | Sorumlu |
|----|---------|
| `IndustryApp/README.md` (Faz 2 kurulum ve UC-04 deneme adımları) | Bahadır İhsan Herdem |
| `docs/PA2/TASK_MATRIX.md`, kök `README.md` | Ömer Efe Öztürk |

## 4. Önceki fazdan gelen parçalar (v0.1, PA1)

UC-04 bu parçaların üzerine kuruldu.

| İş | Yazan |
|----|-------|
| 6 projeli çözüm, domain modeli, State ve Proxy iskeletleri, veritabanı şeması (kısmi tekil indeks, xmin), olay veriyolu, `UnitOfWork` | Ömer Efe Öztürk |
| PA1 dokümanları, domain birim testleri (State, Builder, Proxy, fiyat), frontend iskeleti | Bahadır İhsan Herdem |
