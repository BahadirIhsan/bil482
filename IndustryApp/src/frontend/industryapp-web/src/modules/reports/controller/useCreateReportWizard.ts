import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { findCity } from '../model/cities'
import { reportsApi } from '../model/reportsApi'
import { reportKeys, type CreateReportRequest } from '../model/types'
import { reportWizardSchema, stepFields, wizardDefaults, wizardSteps, type ReportWizardValues } from '../model/wizardSchema'
import { usePhotoUpload } from './usePhotoUpload'

/**
 * UC-02 · 5 adımlı rapor sihirbazının controller'ı. Her adım "İleri"den önce Zod ile doğrulanır (NFR-U1);
 * gönderimde rapor sunucuda Builder ile kurulur (PAT-02.1) ve müşteri eşleştirmeyi beklemeden rapor detayına gider.
 */
export function useCreateReportWizard() {
  const [step, setStep] = useState(0)
  const photos = usePhotoUpload()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const form = useForm<ReportWizardValues>({
    resolver: zodResolver(reportWizardSchema),
    defaultValues: wizardDefaults,
    mode: 'onTouched',
  })

  const submit = useMutation({
    mutationFn: (request: CreateReportRequest) => reportsApi.create(request),
    onSuccess: async (created) => {
      await queryClient.invalidateQueries({ queryKey: reportKeys.mine })
      navigate(`/customer/reports/${created.id}`, { replace: true })
    },
  })

  const photoStepValid = photos.uploadedCount > 0 && !photos.isUploading
  const isLast = step === wizardSteps.length - 1

  const next = async () => {
    if (step === 0) {
      if (photoStepValid) setStep(1)
      return
    }
    if (await form.trigger(stepFields[step])) setStep((s) => Math.min(s + 1, wizardSteps.length - 1))
  }

  const back = () => setStep((s) => Math.max(0, s - 1))

  const send = form.handleSubmit((values) => {
    // Koordinat cihazdan gelmediyse ilin merkezi yazılır; il listeden seçildiği için her zaman bulunur.
    const center = findCity(values.city)
    const latitude = values.isDeviceLocation && values.latitude != null ? values.latitude : center?.latitude
    const longitude = values.isDeviceLocation && values.longitude != null ? values.longitude : center?.longitude
    if (latitude == null || longitude == null) {
      setStep(3)
      return
    }

    submit.mutate({
      vehicleTypeId: values.vehicleTypeId,
      description: values.description.trim() || null,
      city: values.city,
      district: values.district.trim() || null,
      addressText: values.addressText.trim() || null,
      latitude,
      longitude,
      isDeviceLocation: values.isDeviceLocation,
      damages: values.damages.map((d) => ({ categoryId: d.categoryId, severity: d.severity })),
      photoKeys: photos.tempKeys,
    })
  })

  return { step, setStep, next, back, send, isLast, form, photos, photoStepValid, submit }
}
