import { useEffect, useState } from 'react'

/**
 * Verilen son ana kalan süreyi milisaniye olarak döner ve saniyede bir günceller.
 * Son an yoksa veya geçtiyse 0 döner; sayaç sıfıra inince durur.
 */
export function useCountdown(deadline: string | null | undefined): number {
  const target = deadline ? new Date(deadline).getTime() : Number.NaN
  const [now, setNow] = useState(() => Date.now())

  useEffect(() => {
    if (Number.isNaN(target) || target <= Date.now()) return
    const timer = window.setInterval(() => {
      const current = Date.now()
      setNow(current)
      if (current >= target) window.clearInterval(timer)
    }, 1000)
    return () => window.clearInterval(timer)
  }, [target])

  return Number.isNaN(target) ? 0 : Math.max(0, target - now)
}

/** 512000 ms → "08:32" */
export const formatRemaining = (ms: number): string => {
  const totalSeconds = Math.ceil(ms / 1000)
  const minutes = Math.floor(totalSeconds / 60)
  const seconds = totalSeconds % 60
  return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`
}
