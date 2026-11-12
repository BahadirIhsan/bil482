// İstemci tarafı ön kontroller; asıl karar sunucudadır (Photos ayarları, dosya imzası).

export const MAX_PHOTOS = 8
export const MAX_PHOTO_MB = 10
export const ACCEPTED_PHOTO_TYPES = ['image/jpeg', 'image/png', 'image/webp']
export const PHOTO_INPUT_ACCEPT = ACCEPTED_PHOTO_TYPES.join(',')

/** Dosya yüklenmeden önce reddedilecekse Türkçe nedeni, değilse null. */
export function photoRejectionReason(file: File): string | null {
  if (!ACCEPTED_PHOTO_TYPES.includes(file.type)) return 'Yalnızca JPEG, PNG veya WebP fotoğraf yüklenebilir.'
  if (file.size > MAX_PHOTO_MB * 1024 * 1024) return `Fotoğraf en fazla ${MAX_PHOTO_MB} MB olabilir.`
  return null
}
