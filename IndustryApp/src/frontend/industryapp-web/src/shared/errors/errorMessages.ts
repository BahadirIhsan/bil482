/**
 * ProblemDetails.code → kullanıcıya gösterilecek Türkçe mesaj (T-11.4, NFR-U5). Kodlar backend'deki
 * ErrorCodes ile aynıdır. Karşılığı olmayan kodda genel mesaj gösterilir; teknik ayrıntı ekrana çıkmaz.
 */
export const errorMessages: Record<string, string> = {
  // Genel
  VALIDATION_FAILED: 'Gönderilen bilgiler geçersiz. Lütfen alanları kontrol edin.',
  UNIQUE_VIOLATION: 'Bu işlem aynı anda başka bir işlemle çakıştı. Sayfayı yenileyip tekrar deneyin.',
  CONCURRENCY_CONFLICT: 'Kayıt siz işlem yaparken değişti. Sayfayı yenileyip tekrar deneyin.',
  UNEXPECTED_ERROR: 'Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.',
  UNKNOWN_ROLE: 'Geçersiz kullanıcı rolü.',
  TOO_MANY_REQUESTS: 'Çok fazla deneme yapıldı. Lütfen biraz sonra tekrar deneyin.',
  // Hesaplar (UC-01)
  INVALID_CREDENTIALS: 'E-posta veya şifre hatalı.',
  ACCOUNT_LOCKED: 'Çok fazla hatalı deneme yapıldı. Lütfen 15 dakika sonra tekrar deneyin.',
  TOKEN_EXPIRED: 'Oturumunuzun süresi doldu. Lütfen tekrar giriş yapın.',
  EMAIL_ALREADY_USED: 'Bu e-posta adresi zaten kayıtlı.',
  TAX_NUMBER_ALREADY_USED: 'Bu vergi numarası zaten kayıtlı.',
  CONSENT_REQUIRED: 'Zorunlu sözleşmeler onaylanmalıdır.',
  INVALID_SPECIALTY: 'Uzmanlıklar geçerli ana hasar kategorilerinden seçilmelidir.',
  MECHANIC_NOT_APPROVED: 'Hesabınız henüz onaylanmadı.',
  MECHANIC_SPECIALTY_REQUIRED: 'En az bir uzmanlık seçilmelidir.',
  INVALID_LOCATION: 'Konum koordinatları geçersiz.',
  // Raporlar ve teklifler
  REPORT_NOT_FOUND: 'Rapor bulunamadı.',
  REPORT_INVALID_STATE: 'Rapor bu işlem için uygun durumda değil.',
  NOT_REPORT_OWNER: 'Bu rapor size ait değil.',
  NOT_MATCHED: 'Bu raporla eşleşmeniz yok.',
  OFFER_NOT_FOUND: 'Teklif bulunamadı.',
  OFFER_INVALID_STATE: 'Teklif bu işlem için uygun durumda değil.',
  // Seçim ve iletişim (UC-04)
  OFFER_NOT_AVAILABLE: 'Teklif artık seçilebilir durumda değil.',
  UNDO_WINDOW_EXPIRED: 'Seçimi geri alma süresi doldu.',
  NO_ACTIVE_SELECTION: 'Bu raporda henüz bir teklif seçilmedi.',
  CONTACT_NOT_FOUND: 'İletişim bilgisi bulunamadı.',
  // Bildirimler
  NOTIFICATION_NOT_FOUND: 'Bildirim bulunamadı.',
}

export const GENERIC_ERROR = 'İşlem tamamlanamadı. Lütfen tekrar deneyin.'
export const NETWORK_ERROR = 'Sunucuya ulaşılamadı. İnternet bağlantınızı kontrol edin.'

export const messageFor = (code: string | null | undefined, fallback = GENERIC_ERROR): string =>
  (code && errorMessages[code]) || fallback
