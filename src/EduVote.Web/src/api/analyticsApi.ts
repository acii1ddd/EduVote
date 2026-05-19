import api from './axiosClient'

export interface AnalyticsOverview {
    totalVotings: number
    activeVotings: number
    pendingApprovalVotings: number
    finishedVotings: number
    draftVotings: number
    pausedVotings: number
    totalVotesCast: number
    statusCounts: { status: string; count: number }[]
    typeCounts: { type: string; count: number }[]
    votingsCreatedSeries: { period: string; count: number }[]
    votesCastSeries: { period: string; count: number }[]
    pendingApprovalQueue: {
        votingId: string
        title: string
        createdById: string
        createdAt: string
    }[]
}

export interface AnalyticsVotingRow {
    id: string
    title: string
    type: string
    status: string
    startTime: string
    endTime: string
    createdAt: string
    totalVotes: number
    eligibleCount: number
    turnoutPercent: number
    createdById: string
}

export interface AnalyticsVotingsResponse {
    items: AnalyticsVotingRow[]
    totalCount: number
    page: number
    pageSize: number
}

export interface AnalyticsFilters {
    dateFrom?: string
    dateTo?: string
    educationUnitId?: string
    type?: string
    status?: string
    createdById?: string
    page?: number
    pageSize?: number
}

const buildParams = (filters: AnalyticsFilters) => {
    const params = new URLSearchParams()
    if (filters.dateFrom) params.set('dateFrom', filters.dateFrom)
    if (filters.dateTo) params.set('dateTo', filters.dateTo)
    if (filters.educationUnitId) params.set('educationUnitId', filters.educationUnitId)
    if (filters.type) params.set('type', filters.type)
    if (filters.status) params.set('status', filters.status)
    if (filters.createdById) params.set('createdById', filters.createdById)
    if (filters.page) params.set('page', String(filters.page))
    if (filters.pageSize) params.set('pageSize', String(filters.pageSize))
    return params
}

export const getAnalyticsOverview = async (filters: AnalyticsFilters = {}): Promise<AnalyticsOverview> => {
    const response = await api.get<AnalyticsOverview>('/analytics/overview', {
        params: buildParams(filters),
    })
    return response.data
}

export const getAnalyticsVotings = async (filters: AnalyticsFilters = {}): Promise<AnalyticsVotingsResponse> => {
    const response = await api.get<AnalyticsVotingsResponse>('/analytics/votings', {
        params: buildParams(filters),
    })
    return response.data
}

const downloadPdf = async (url: string, fallbackName: string) => {
    const response = await api.get(url, { responseType: 'blob' })
    const disposition = response.headers['content-disposition'] as string | undefined
    const match = disposition?.match(/filename\*=UTF-8''([^;]+)|filename="([^"]+)"/)
    const fileName = decodeURIComponent(match?.[1] ?? match?.[2] ?? fallbackName)
    const blob = new Blob([response.data], { type: 'application/pdf' })
    const link = document.createElement('a')
    link.href = URL.createObjectURL(blob)
    link.download = fileName
    link.click()
    URL.revokeObjectURL(link.href)
}

/** PDF отчёт по одному голосованию (генерируется на сервере, QuestPDF). */
export const downloadVotingReportPdf = (votingId: string) =>
    downloadPdf(`/analytics/votings/${votingId}/report.pdf`, `voting-${votingId}.pdf`)

/** Сводный PDF по фильтрам обзора (генерируется на сервере, QuestPDF). */
export const downloadOverviewReportPdf = (filters: AnalyticsFilters = {}) => {
    const qs = buildParams(filters).toString()
    const suffix = qs ? `?${qs}` : ''
    return downloadPdf(`/analytics/overview/report.pdf${suffix}`, 'eduvote-analytics.pdf')
}
