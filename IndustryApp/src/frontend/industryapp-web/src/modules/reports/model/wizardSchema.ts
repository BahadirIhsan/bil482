import { z } from 'zod'
import { cityNames } from './cities'

// UC-02 · Rapor sihirbazının form şeması (NFR-U1). Kurallar sunucudaki CreateReportValidator ve Builder ile aynıdır;
// fotoğraflar form dışında usePhotoUpload'da tutulur.

const severity = z.enum(['Light', 'Medium', 'Heavy'])

export const reportWizardSchema = z
  .object({
    damages: z
      .array(z.object({ categoryId: z.number(), label: z.string(), severity }))
      .min(1, 'En az bir hasar bölgesi seçin.')
      .max(30, 'En fazla 30 hasar bölgesi seçilebilir.'),
    vehicleTypeId: z.number({ error: 'Araç türünü seçin.' }).int().positive('Araç türünü seçin.'),
    description: z.string().max(1000, 'Açıklama en fazla 1000 karakter olabilir.'),
    city: z.string().refine((value) => cityNames.includes(value), 'İl seçin.'),
    district: z.string().max(80, 'İlçe en fazla 80 karakter olabilir.'),
    addressText: z.string().max(300, 'Adres en fazla 300 karakter olabilir.'),
    isDeviceLocation: z.boolean(),
    latitude: z.number().min(-90).max(90).nullable(),
    longitude: z.number().min(-180).max(180).nullable(),
  })
  .superRefine((values, ctx) => {
    if (!values.isDeviceLocation && values.district.trim().length === 0) {
      ctx.addIssue({ code: 'custom', path: ['district'], message: 'Konum izni verilmediğinde ilçe zorunludur.' })
    }
  })

export type ReportWizardValues = z.infer<typeof reportWizardSchema>

export const wizardDefaults: ReportWizardValues = {
  damages: [],
  vehicleTypeId: 0,
  description: '',
  city: '',
  district: '',
  addressText: '',
  isDeviceLocation: false,
  latitude: null,
  longitude: null,
}

/** Her adımda doğrulanan alanlar; 1. adım (fotoğraflar) form dışındadır. */
export const stepFields: Record<number, (keyof ReportWizardValues)[]> = {
  0: [],
  1: ['damages'],
  2: ['vehicleTypeId', 'description'],
  3: ['city', 'district', 'addressText'],
  4: [],
}

export const wizardSteps = ['Fotoğraflar', 'Hasar bölgeleri', 'Araç ve açıklama', 'Konum', 'Özet'] as const
