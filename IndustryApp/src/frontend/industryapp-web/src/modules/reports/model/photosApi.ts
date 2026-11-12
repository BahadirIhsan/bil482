import { client } from '../../../shared/http/axiosClient'
import type { TemporaryPhoto } from './types'

// UC-02 · Fotoğraflar (endpoint 14, 15). Fotoğraflar herkese açık URL ile değil, token'lı istekle blob olarak okunur.
export const photosApi = {
  /** Tek fotoğraf yükler; onProgress 0–100 arası yükleme yüzdesini verir. */
  uploadTemporary: async (file: File, onProgress?: (percent: number) => void): Promise<TemporaryPhoto> => {
    const form = new FormData()
    form.append('file', file)
    const { data } = await client.post<TemporaryPhoto>('/photos/temporary', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (event) => {
        if (onProgress && event.total) onProgress(Math.round((event.loaded / event.total) * 100))
      },
    })
    return data
  },

  getBlob: async (photoId: string): Promise<Blob> =>
    (await client.get<Blob>(`/photos/${photoId}`, { responseType: 'blob' })).data,
}
