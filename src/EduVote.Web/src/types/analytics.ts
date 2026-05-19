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

/** UI filter state before mapping to API params. */
export interface AnalyticsFilterState {
    dateFrom: string
    dateTo: string
    educationUnitId: string
    type: string
    status: string
    createdById: string
}

export const EMPTY_ANALYTICS_FILTER_STATE: AnalyticsFilterState = {
    dateFrom: '',
    dateTo: '',
    educationUnitId: '',
    type: '',
    status: '',
    createdById: '',
}

export const DEFAULT_PAGE_SIZE = 20
