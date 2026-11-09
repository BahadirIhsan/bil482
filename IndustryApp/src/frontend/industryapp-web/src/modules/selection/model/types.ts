import type { ReportStatus } from '../../reports/model/types'

// UC-04 · Teklif seçimi (endpoint 29, 30, 31).

export interface SelectionResponse {
  reportId: string
  offerId: string
  selectedAt: string
  undoDeadline: string
}

export interface SelectionStatus {
  reportId: string
  status: ReportStatus
  selectedOfferId: string | null
  selectedAt: string | null
  undoDeadline: string | null
  canUndo: boolean
}

/** SignalR report.statusChanged yükü (T-12). */
export interface ReportStatusChangedPayload {
  reportId: string
  status: ReportStatus
  selectedOfferId: string | null
}

export const selectionKeys = {
  status: (reportId: string) => ['reports', reportId, 'selection'] as const,
}
