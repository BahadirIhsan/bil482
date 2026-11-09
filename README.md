# bil482 – IndustryApp

BIL 482 Tasarım Desenleri dönem projesi · Ekip: Ömer Efe Öztürk, Bahadır İhsan Herdem

IndustryApp, kaza geçirmiş araç sahiplerini sanayi ustalarıyla buluşturan bir ters açık artırma pazaryeridir. Proje .NET 8 Web API ve React/TypeScript ile yazılmıştır.

## İnceleyen için

| Ne | Nerede |
|----|--------|
| Proje, kurulum ve çalıştırma | [`IndustryApp/README.md`](IndustryApp/README.md) |
| Kaynak kod | [`IndustryApp/src/`](IndustryApp/src) |
| PA1 dokümanları | [`IndustryApp/docs/PA1/`](IndustryApp/docs/PA1) |
| PA2 Software Design Document | [`IndustryApp/docs/PA2/SoftwareDesignDocument.pdf`](IndustryApp/docs/PA2/SoftwareDesignDocument.pdf) |
| PA2 görev matrisi | [`IndustryApp/docs/PA2/TASK_MATRIX.md`](IndustryApp/docs/PA2/TASK_MATRIX.md) |

**PA2'de seçilen use case: UC-04, Teklif Seçimi ve İletişim Bilgisinin Açılması.** Uygulanan desenler:

| Kod | Desen |
|-----|-------|
| PAT-04.1 | State |
| PAT-04.2 | Command (geri alınabilir seçim) |
| PAT-04.3 | Protection Proxy (iletişim bilgisi) |
| PAT-04.4 | Strategy (teklif sıralama) |

Desen sınıfları kodda `// [UC-04][PAT-04.n] <Desen>` yorumuyla işaretlidir. İzlenebilirlik zinciri: `UC-04 → FR-04.n → PAT-04.n → kod → TC-04.n.k → commit [UC-04]`.

Testleri çalıştırmak için `cd IndustryApp/src/backend && dotnet test` yeterlidir. Entegrasyon testleri için Docker açık olmalıdır.
