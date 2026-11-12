import { Controller } from 'react-hook-form'
import { Alert, Autocomplete, Box, Button, Chip, CircularProgress, Divider, MenuItem, Paper, Stack, Step, StepLabel, Stepper, TextField, Typography } from '@mui/material'
import MyLocationIcon from '@mui/icons-material/MyLocation'
import { useDamageTree, useVehicleTypes } from '../../catalog/controller/useCatalog'
import { errorText } from '../../selection/model/errorText'
import { DamageTreeSelect } from '../components/DamageTreeSelect'
import { PhotoUploader } from '../components/PhotoUploader'
import { useCreateReportWizard } from '../controller/useCreateReportWizard'
import { useGeolocation } from '../controller/useGeolocation'
import { cityNames, nearestCity } from '../model/cities'
import { severityLabels } from '../model/types'
import { wizardSteps } from '../model/wizardSchema'

/**
 * UC-02 · Kaza raporu sihirbazı (route /customer/reports/new, view #11). 5 adım: fotoğraflar, hasar bölgeleri,
 * araç ve açıklama, konum, özet. Telefon genişliğinde kullanılabilir; "İleri" adım geçerli olana kadar pasiftir.
 */
export function CreateReportWizardView() {
  const { step, setStep, next, back, send, isLast, form, photos, photoStepValid, submit } = useCreateReportWizard()
  const tree = useDamageTree()
  const vehicleTypes = useVehicleTypes()
  const geo = useGeolocation()
  const { control, register, watch, setValue, formState } = form
  const { errors } = formState
  const values = watch()

  const takeDeviceLocation = async () => {
    const coords = await geo.request()
    if (!coords) {
      setValue('isDeviceLocation', false)
      setValue('latitude', null)
      setValue('longitude', null)
      return
    }
    setValue('isDeviceLocation', true)
    setValue('latitude', coords.latitude)
    setValue('longitude', coords.longitude)
    if (!values.city) setValue('city', nearestCity(coords.latitude, coords.longitude).name, { shouldValidate: true })
    void form.trigger('district')
  }

  const vehicleName = vehicleTypes.data?.find((v) => v.id === values.vehicleTypeId)?.name ?? '-'
  const nextDisabled = step === 0 && !photoStepValid

  return (
    <Stack spacing={3} sx={{ maxWidth: 820 }}>
      <Typography variant="h5" component="h1">
        Yeni kaza raporu
      </Typography>

      <Stepper activeStep={step} alternativeLabel sx={{ '& .MuiStepLabel-label': { display: { xs: 'none', sm: 'block' } } }}>
        {wizardSteps.map((label) => (
          <Step key={label}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>
      <Typography variant="subtitle1" sx={{ display: { sm: 'none' } }}>
        Adım {step + 1}/{wizardSteps.length}: {wizardSteps[step]}
      </Typography>

      <Paper variant="outlined" sx={{ p: { xs: 2, sm: 3 } }}>
        {step === 0 && <PhotoUploader state={photos} />}

        {step === 1 && (
          <Controller
            control={control}
            name="damages"
            render={({ field }) => (
              <DamageTreeSelect
                tree={tree.data}
                loading={tree.isPending}
                value={field.value}
                onChange={field.onChange}
                error={errors.damages?.message}
              />
            )}
          />
        )}

        {step === 2 && (
          <Stack spacing={2}>
            <Controller
              control={control}
              name="vehicleTypeId"
              render={({ field }) => (
                <TextField
                  select
                  label="Araç türü"
                  value={field.value || ''}
                  onChange={(e) => field.onChange(Number(e.target.value))}
                  onBlur={field.onBlur}
                  error={Boolean(errors.vehicleTypeId)}
                  helperText={errors.vehicleTypeId?.message ?? (vehicleTypes.isError ? 'Araç türleri yüklenemedi.' : ' ')}
                >
                  {(vehicleTypes.data ?? []).map((v) => (
                    <MenuItem key={v.id} value={v.id}>
                      {v.name}
                    </MenuItem>
                  ))}
                </TextField>
              )}
            />
            <TextField
              label="Açıklama (isteğe bağlı)"
              multiline
              minRows={3}
              {...register('description')}
              error={Boolean(errors.description)}
              helperText={errors.description?.message ?? `${values.description.length}/1000`}
            />
          </Stack>
        )}

        {step === 3 && (
          <Stack spacing={2}>
            <Box>
              <Button
                variant="outlined"
                startIcon={geo.status === 'locating' ? <CircularProgress size={18} /> : <MyLocationIcon />}
                disabled={geo.status === 'locating'}
                onClick={() => void takeDeviceLocation()}
              >
                Konumumu kullan
              </Button>
            </Box>
            {values.isDeviceLocation && <Alert severity="success">Cihaz konumu alındı. İli kontrol edin; ilçe isteğe bağlıdır.</Alert>}
            {(geo.status === 'denied' || geo.status === 'unsupported') && (
              <Alert severity="warning">Konum alınamadı. İl ve ilçeyi seçmeniz gerekiyor.</Alert>
            )}
            <Controller
              control={control}
              name="city"
              render={({ field }) => (
                <Autocomplete
                  options={cityNames}
                  value={field.value || null}
                  onChange={(_, city) => field.onChange(city ?? '')}
                  onBlur={field.onBlur}
                  renderInput={(params) => (
                    <TextField {...params} label="İl" error={Boolean(errors.city)} helperText={errors.city?.message ?? ' '} />
                  )}
                />
              )}
            />
            <TextField
              label={values.isDeviceLocation ? 'İlçe (isteğe bağlı)' : 'İlçe'}
              {...register('district')}
              error={Boolean(errors.district)}
              helperText={errors.district?.message ?? ' '}
            />
            <TextField
              label="Adres tarifi (isteğe bağlı)"
              {...register('addressText')}
              error={Boolean(errors.addressText)}
              helperText={errors.addressText?.message ?? ' '}
            />
          </Stack>
        )}

        {step === 4 && (
          <Stack spacing={1.5}>
            <Typography variant="subtitle2">Fotoğraflar ({photos.uploadedCount})</Typography>
            <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
              {photos.items
                .filter((p) => p.status === 'done')
                .map((p) => (
                  <Box key={p.id} component="img" src={p.previewUrl} alt={p.file.name} sx={{ width: 96, height: 72, objectFit: 'cover', borderRadius: 1 }} />
                ))}
            </Box>
            {photos.hasErrors && <Alert severity="warning">Yüklenemeyen fotoğraflar rapora eklenmeyecek.</Alert>}
            <Divider />
            <Typography variant="subtitle2">Hasar bölgeleri</Typography>
            <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
              {values.damages.map((d) => (
                <Chip key={d.categoryId} variant="outlined" label={`${d.label} · ${severityLabels[d.severity]}`} />
              ))}
            </Stack>
            <Divider />
            <Typography>
              <strong>Araç türü:</strong> {vehicleName}
            </Typography>
            <Typography>
              <strong>Açıklama:</strong> {values.description.trim() || '-'}
            </Typography>
            <Typography>
              <strong>Konum:</strong> {values.city || '-'}
              {values.district.trim() ? ` / ${values.district.trim()}` : ''}
              {values.isDeviceLocation ? ' (cihaz konumu)' : ''}
            </Typography>
            <Alert severity="info">
              Rapor gönderildikten sonra uygun ustalara otomatik bildirim gider. Teklifler rapor ekranına anlık düşer.
            </Alert>
            {submit.isError && <Alert severity="error">{errorText(submit.error, 'Rapor gönderilemedi.')}</Alert>}
          </Stack>
        )}
      </Paper>

      <Stack direction="row" justifyContent="space-between">
        <Button disabled={step === 0 || submit.isPending} onClick={back}>
          Geri
        </Button>
        {isLast ? (
          <Stack direction="row" spacing={1}>
            <Button onClick={() => setStep(0)} disabled={submit.isPending}>
              Fotoğrafları düzenle
            </Button>
            <Button variant="contained" disabled={submit.isPending || photos.uploadedCount === 0} onClick={() => void send()}>
              {submit.isPending ? 'Gönderiliyor…' : 'Raporu gönder'}
            </Button>
          </Stack>
        ) : (
          <Button variant="contained" disabled={nextDisabled} onClick={() => void next()}>
            İleri
          </Button>
        )}
      </Stack>
    </Stack>
  )
}
