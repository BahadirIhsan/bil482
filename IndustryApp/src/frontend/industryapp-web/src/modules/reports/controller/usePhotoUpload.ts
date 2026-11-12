import { useCallback, useEffect, useRef, useState } from 'react'
import { errorText } from '../../selection/model/errorText'
import { photosApi } from '../model/photosApi'
import { MAX_PHOTOS, photoRejectionReason } from '../model/photoRules'

export type UploadStatus = 'uploading' | 'done' | 'error'

export interface UploadItem {
  id: string
  file: File
  previewUrl: string
  progress: number
  status: UploadStatus
  error: string | null
  tempKey: string | null
}

/**
 * UC-02 · Sihirbazın 1. adımı. Her fotoğraf seçilir seçilmez ayrı istekle yüklenir ve sunucuda işlenir (K-04);
 * her birinin kendi ilerleme çubuğu, hatası ve "tekrar dene"si vardır. Reddedilen fotoğraf diğerlerini etkilemez
 * (UC-02 2a).
 */
export function usePhotoUpload(max = MAX_PHOTOS) {
  const [items, setItems] = useState<UploadItem[]>([])
  const itemsRef = useRef(items)
  itemsRef.current = items

  const update = useCallback((id: string, patch: Partial<UploadItem>) => {
    setItems((prev) => prev.map((item) => (item.id === id ? { ...item, ...patch } : item)))
  }, [])

  const upload = useCallback(
    async (item: UploadItem) => {
      try {
        const result = await photosApi.uploadTemporary(item.file, (progress) => update(item.id, { progress }))
        update(item.id, { status: 'done', progress: 100, tempKey: result.tempKey, error: null })
      } catch (error) {
        update(item.id, { status: 'error', error: errorText(error, 'Fotoğraf yüklenemedi.') })
      }
    },
    [update],
  )

  const add = useCallback(
    (files: FileList | File[]) => {
      const free = Math.max(0, max - itemsRef.current.length)
      const created = Array.from(files)
        .slice(0, free)
        .map<UploadItem>((file) => {
          const reason = photoRejectionReason(file)
          return {
            id: crypto.randomUUID(),
            file,
            previewUrl: URL.createObjectURL(file),
            progress: 0,
            status: reason ? 'error' : 'uploading',
            error: reason,
            tempKey: null,
          }
        })
      setItems((prev) => [...prev, ...created])
      created.filter((item) => item.status === 'uploading').forEach((item) => void upload(item))
    },
    [max, upload],
  )

  const retry = useCallback(
    (id: string) => {
      const item = itemsRef.current.find((i) => i.id === id)
      if (!item || photoRejectionReason(item.file)) return
      update(id, { status: 'uploading', progress: 0, error: null })
      void upload(item)
    },
    [update, upload],
  )

  const remove = useCallback((id: string) => {
    setItems((prev) => {
      const item = prev.find((i) => i.id === id)
      if (item) URL.revokeObjectURL(item.previewUrl)
      return prev.filter((i) => i.id !== id)
    })
  }, [])

  useEffect(() => () => itemsRef.current.forEach((item) => URL.revokeObjectURL(item.previewUrl)), [])

  const uploaded = items.filter((i) => i.status === 'done' && i.tempKey)
  return {
    items,
    add,
    retry,
    remove,
    /** Rapora gönderilecek geçici anahtarlar, seçim sırasıyla. */
    tempKeys: uploaded.map((i) => i.tempKey as string),
    uploadedCount: uploaded.length,
    isUploading: items.some((i) => i.status === 'uploading'),
    hasErrors: items.some((i) => i.status === 'error'),
    canAddMore: items.length < max,
    max,
  }
}
