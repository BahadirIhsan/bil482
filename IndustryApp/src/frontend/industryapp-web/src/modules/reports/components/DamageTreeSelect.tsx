import { useMemo } from 'react'
import { Alert, Box, CircularProgress, IconButton, MenuItem, Paper, Stack, TextField, Typography } from '@mui/material'
import CloseIcon from '@mui/icons-material/Close'
import { SimpleTreeView } from '@mui/x-tree-view/SimpleTreeView'
import { TreeItem } from '@mui/x-tree-view/TreeItem'
import type { DamageCategoryNode } from '../../catalog/model/types'
import { severityLabels, type DamageSeverity } from '../model/types'

export interface SelectedDamage {
  categoryId: number
  label: string
  severity: DamageSeverity
}

interface DamageTreeSelectProps {
  tree: DamageCategoryNode[] | undefined
  loading: boolean
  value: SelectedDamage[]
  onChange: (value: SelectedDamage[]) => void
  error?: string
}

const severities: DamageSeverity[] = ['Light', 'Medium', 'Heavy']

/** "Kaporta › Çamurluk › Sol ön çamurluk" etiketleriyle düz liste (Composite ağacın her düğümü). */
function flatten(nodes: DamageCategoryNode[], parents: string[] = []): Map<string, string> {
  const map = new Map<string, string>()
  for (const node of nodes) {
    const path = [...parents, node.name]
    map.set(String(node.id), path.join(' › '))
    flatten(node.children, path).forEach((label, id) => map.set(id, label))
  }
  return map
}

function renderNodes(nodes: DamageCategoryNode[]) {
  return nodes.map((node) => (
    <TreeItem key={node.id} itemId={String(node.id)} label={node.name}>
      {node.children.length > 0 && renderNodes(node.children)}
    </TreeItem>
  ))
}

/**
 * Sihirbazın 2. adımı: hasar ağacından (PAT-02.2 Composite) bir veya daha çok bölge seçilir, her biri için şiddet
 * verilir (FR-02.2). Ağaç bir kez yüklenir ve önbellekte tutulur.
 */
export function DamageTreeSelect({ tree, loading, value, onChange, error }: DamageTreeSelectProps) {
  const labels = useMemo(() => flatten(tree ?? []), [tree])
  const selectedIds = value.map((d) => String(d.categoryId))

  const select = (ids: string[]) => {
    const kept = value.filter((d) => ids.includes(String(d.categoryId)))
    const added = ids
      .filter((id) => !kept.some((d) => String(d.categoryId) === id))
      .map<SelectedDamage>((id) => ({ categoryId: Number(id), label: labels.get(id) ?? id, severity: 'Medium' }))
    onChange([...kept, ...added])
  }

  if (loading) return <CircularProgress aria-label="Hasar ağacı yükleniyor" />
  if (!tree || tree.length === 0) return <Alert severity="error">Hasar bölgeleri yüklenemedi.</Alert>

  return (
    <Stack spacing={2}>
      <Typography color="text.secondary">Hasarlı bölgeleri işaretleyin; her biri için hasarın şiddetini seçin.</Typography>
      <Paper variant="outlined" sx={{ p: 1, maxHeight: 360, overflow: 'auto' }}>
        <SimpleTreeView
          checkboxSelection
          multiSelect
          selectedItems={selectedIds}
          onSelectedItemsChange={(_, ids) => select(ids)}
          aria-label="Hasar bölgeleri"
        >
          {renderNodes(tree)}
        </SimpleTreeView>
      </Paper>
      {error && <Alert severity="error">{error}</Alert>}

      {value.length > 0 && (
        <Stack spacing={1}>
          <Typography variant="subtitle2">Seçilen bölgeler ({value.length})</Typography>
          {value.map((damage) => (
            <Box key={damage.categoryId} sx={{ display: 'flex', gap: 1, alignItems: 'center', flexWrap: 'wrap' }}>
              <Typography sx={{ flex: '1 1 200px' }}>{damage.label}</Typography>
              <TextField
                select
                size="small"
                label="Şiddet"
                value={damage.severity}
                onChange={(e) =>
                  onChange(value.map((d) => (d.categoryId === damage.categoryId ? { ...d, severity: e.target.value as DamageSeverity } : d)))
                }
                sx={{ width: 130 }}
              >
                {severities.map((s) => (
                  <MenuItem key={s} value={s}>
                    {severityLabels[s]}
                  </MenuItem>
                ))}
              </TextField>
              <IconButton aria-label="Bölgeyi kaldır" onClick={() => onChange(value.filter((d) => d.categoryId !== damage.categoryId))}>
                <CloseIcon />
              </IconButton>
            </Box>
          ))}
        </Stack>
      )}
    </Stack>
  )
}
