import { MenuItem, TextField } from '@mui/material'
import { offerSortLabels, type OfferSortKey } from '../model/types'

interface SortSelectProps {
  value: OfferSortKey
  onChange: (value: OfferSortKey) => void
}

const sortKeys = Object.keys(offerSortLabels) as OfferSortKey[]

/** FR-04.1 · Sıralama seçimi; seçilen anahtar sunucudaki strateji anahtarıdır (PAT-04.4). */
export function SortSelect({ value, onChange }: SortSelectProps) {
  return (
    <TextField
      select
      size="small"
      label="Sırala"
      value={value}
      onChange={(event) => onChange(event.target.value as OfferSortKey)}
      sx={{ minWidth: 180 }}
    >
      {sortKeys.map((key) => (
        <MenuItem key={key} value={key}>
          {offerSortLabels[key]}
        </MenuItem>
      ))}
    </TextField>
  )
}
