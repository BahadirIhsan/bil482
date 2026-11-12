import { useEffect, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { photosApi } from '../model/photosApi'
import { photoKeys } from '../model/types'

/** Fotoğrafı Authorization başlığıyla blob olarak çeker ve ekranda gösterilecek geçici URL'yi döner (SDD 10). */
export function usePhotoBlob(photoId: string) {
  const blob = useQuery({
    queryKey: photoKeys.blob(photoId),
    queryFn: () => photosApi.getBlob(photoId),
    staleTime: 60 * 60_000,
  })
  const [url, setUrl] = useState<string | null>(null)

  useEffect(() => {
    if (!blob.data) return
    const objectUrl = URL.createObjectURL(blob.data)
    setUrl(objectUrl)
    return () => URL.revokeObjectURL(objectUrl)
  }, [blob.data])

  return { url, isPending: blob.isPending, isError: blob.isError }
}
