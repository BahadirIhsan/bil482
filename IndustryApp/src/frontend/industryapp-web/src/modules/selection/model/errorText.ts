/**
 * Hata nesnesinden kullanıcıya gösterilecek Türkçe metni alır. axios istemcisinin interceptor'ı
 * ProblemDetails.code'u Türkçe mesaja çevirip Error.message olarak fırlatır (T-11.4, NFR-U5);
 * teknik ayrıntı hiçbir zaman ekrana çıkmaz.
 */
export const errorText = (error: unknown, fallback = 'İşlem tamamlanamadı. Lütfen tekrar deneyin.'): string =>
  error instanceof Error && error.message ? error.message : fallback
