import { useNavigate, useParams } from 'react-router-dom'
import { Alert, Box, Button, Card, CardContent, CircularProgress, Link, Stack, Typography } from '@mui/material'
import { labelOf } from '../../../shared/format'
import { errorText } from '../../selection/model/errorText'
import { useMatchContact } from '../controller/useMatchContact'
import { counterpartRoleLabels } from '../model/types'

/**
 * UC-04 · Eşleşme ekranı (route /match/:reportId, view #7). Müşteri kazanan ustayı, usta müşteriyi görür.
 * Gösterme kararı sunucudaki ContactInfoProxy'dedir (PAT-04.3); bu ekran yalnızca gelen değeri çizer:
 * isRevealed=false ise telefon zaten maskeli gelir.
 */
export function MatchView() {
  const { reportId = '' } = useParams()
  const navigate = useNavigate()
  const contact = useMatchContact(reportId)

  if (contact.isPending) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress aria-label="Yükleniyor" />
      </Box>
    )
  }

  return (
    <Stack spacing={3} sx={{ maxWidth: 560 }}>
      <Typography variant="h5" component="h1">
        Eşleşme
      </Typography>

      {contact.isError || !contact.data ? (
        <Alert severity="error">{errorText(contact.error, 'İletişim bilgisi yüklenemedi.')}</Alert>
      ) : (
        <>
          {!contact.data.isRevealed && (
            <Alert severity="info">
              İletişim bilgileri yalnızca müşteri ile seçilen usta arasında açılır. Bu rapor için size açık değil.
            </Alert>
          )}
          <Card variant="outlined" sx={contact.data.isRevealed ? { borderColor: 'success.main', borderWidth: 2 } : undefined}>
            <CardContent>
              <Typography variant="overline" color="text.secondary">
                {labelOf(counterpartRoleLabels, contact.data.counterpartRole)}
              </Typography>
              <Typography variant="h6">{contact.data.fullName || '-'}</Typography>
              {contact.data.workshopName && <Typography color="text.secondary">{contact.data.workshopName}</Typography>}
              <Stack spacing={1} sx={{ mt: 2 }}>
                <Typography>
                  <strong>Telefon:</strong>{' '}
                  {contact.data.isRevealed ? (
                    <Link href={`tel:${contact.data.phone}`}>{contact.data.phone}</Link>
                  ) : (
                    contact.data.phone || '-'
                  )}
                </Typography>
                <Typography>
                  <strong>E-posta:</strong>{' '}
                  {contact.data.isRevealed && contact.data.email !== '-' ? (
                    <Link href={`mailto:${contact.data.email}`}>{contact.data.email}</Link>
                  ) : (
                    '-'
                  )}
                </Typography>
                {contact.data.counterpartRole === 'Mechanic' && (
                  <Typography>
                    <strong>Adres:</strong> {contact.data.address || '-'}
                  </Typography>
                )}
              </Stack>
            </CardContent>
          </Card>
        </>
      )}

      <Box>
        <Button onClick={() => navigate(-1)}>Geri dön</Button>
      </Box>
    </Stack>
  )
}
