import {
    getAnalyticsOverview,
    getAnalyticsVotings,
} from '@/api/analyticsApi'
import type {
    AnalyticsFilterState,
    AnalyticsFilters,
    AnalyticsOverview,
    AnalyticsVotingsResponse,
} from '@/types/analytics'
import { DEFAULT_PAGE_SIZE } from '@/types/analytics'

const toIsoStart = (localDate: string): string | undefined => {
    if (!localDate) return undefined
    return new Date(`${localDate}T00:00:00`).toISOString()
}

const toIsoEndInclusive = (localDate: string): string | undefined => {
    if (!localDate) return undefined
    return new Date(`${localDate}T23:59:59.999`).toISOString()
}

const toApiFilters = (
    state: AnalyticsFilterState,
    page = 1,
    pageSize = DEFAULT_PAGE_SIZE,
): AnalyticsFilters => ({
    dateFrom: toIsoStart(state.dateFrom),
    dateTo: toIsoEndInclusive(state.dateTo),
    educationUnitId: state.educationUnitId || undefined,
    type: state.type || undefined,
    status: state.status || undefined,
    createdById: state.createdById || undefined,
    page,
    pageSize,
})

export const buildOverviewFilters = (state: AnalyticsFilterState): AnalyticsFilters => ({
    dateFrom: toIsoStart(state.dateFrom),
    dateTo: toIsoEndInclusive(state.dateTo),
    educationUnitId: state.educationUnitId || undefined,
    type: state.type || undefined,
    status: state.status || undefined,
    createdById: state.createdById || undefined,
})

export const buildVotingsFilters = (
    state: AnalyticsFilterState,
    page: number,
    pageSize = DEFAULT_PAGE_SIZE,
): AnalyticsFilters => toApiFilters(state, page, pageSize)

export const fetchOverview = (state: AnalyticsFilterState): Promise<AnalyticsOverview> =>
    getAnalyticsOverview(buildOverviewFilters(state))

export const fetchVotings = (
    state: AnalyticsFilterState,
    page: number,
    pageSize = DEFAULT_PAGE_SIZE,
): Promise<AnalyticsVotingsResponse> =>
    getAnalyticsVotings(buildVotingsFilters(state, page, pageSize))

export const fetchDashboard = (
    state: AnalyticsFilterState,
    page = 1,
    pageSize = DEFAULT_PAGE_SIZE,
): Promise<{ overview: AnalyticsOverview; votings: AnalyticsVotingsResponse }> =>
    Promise.all([
        fetchOverview(state),
        fetchVotings(state, page, pageSize),
    ]).then(([overview, votings]) => ({ overview, votings }))

/** Format ISO week period (e.g. 2025-W12) for chart labels. */
export const formatWeekPeriod = (period: string): string => {
    const match = /^(\d{4})-W(\d{2})$/.exec(period)
    if (!match) return period
    return `Нед. ${Number(match[2])}, ${match[1]}`
}
