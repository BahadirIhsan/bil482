import { useCallback, useState } from 'react'

export type GeolocationStatus = 'idle' | 'locating' | 'granted' | 'denied' | 'unsupported'

export interface Coordinates {
  latitude: number
  longitude: number
}

/** Tarayıcının konum API'si. İzin verilmezse sihirbazda il ve ilçe zorunlu olur (UC-02 5a). */
export function useGeolocation() {
  const [status, setStatus] = useState<GeolocationStatus>('idle')
  const [coords, setCoords] = useState<Coordinates | null>(null)

  const request = useCallback(
    () =>
      new Promise<Coordinates | null>((resolve) => {
        if (!('geolocation' in navigator)) {
          setStatus('unsupported')
          resolve(null)
          return
        }
        setStatus('locating')
        navigator.geolocation.getCurrentPosition(
          (position) => {
            const value = {
              latitude: Number(position.coords.latitude.toFixed(6)),
              longitude: Number(position.coords.longitude.toFixed(6)),
            }
            setCoords(value)
            setStatus('granted')
            resolve(value)
          },
          () => {
            setCoords(null)
            setStatus('denied')
            resolve(null)
          },
          { enableHighAccuracy: true, timeout: 10_000, maximumAge: 60_000 },
        )
      }),
    [],
  )

  return { status, coords, request }
}
