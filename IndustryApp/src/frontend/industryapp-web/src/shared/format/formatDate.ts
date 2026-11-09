/** dd/MM/yyyy veya dd/MM/yyyy HH:mm (T-14.5). Boş veya geçersiz değer '-'. Tarihler tarayıcının yerel saatindedir. */
export const formatDate = (value?: string | null, withTime = false): string => {
  if (!value) return '-'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '-'
  const p = (n: number) => String(n).padStart(2, '0')
  const date = `${p(d.getDate())}/${p(d.getMonth() + 1)}/${d.getFullYear()}`
  return withTime ? `${date} ${p(d.getHours())}:${p(d.getMinutes())}` : date
}
