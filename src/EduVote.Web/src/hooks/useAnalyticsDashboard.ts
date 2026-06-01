import { useCallback, useEffect, useState } from 'react'
import { getEducationUnits, type EducationUnit } from '@/api/educationUnitApi'
import { getUsers, type UserResponse } from '@/api/userApi'
import { fetchDashboard, fetchVotings } from '@/services/analyticsService'
import type {
    AnalyticsFilterState,
    AnalyticsOverview,
    AnalyticsVotingsResponse,
} from '@/types/analytics'
import {
    DEFAULT_PAGE_SIZE,
    EMPTY_ANALYTICS_FILTER_STATE,
} from '@/types/analytics'

export function useAnalyticsDashboard() {
    const [draftFilters, setDraftFilters] = useState<AnalyticsFilterState>(
        EMPTY_ANALYTICS_FILTER_STATE,
    )
    const [appliedFilters, setAppliedFilters] = useState<AnalyticsFilterState>(
        EMPTY_ANALYTICS_FILTER_STATE,
    )
    const [page, setPage] = useState(1)

    const [overview, setOverview] = useState<AnalyticsOverview | null>(null)
    const [votings, setVotings] = useState<AnalyticsVotingsResponse | null>(null)
    const [loading, setLoading] = useState(true)
    const [tableLoading, setTableLoading] = useState(false)
    const [error, setError] = useState<string | null>(null)

    const [educationUnits, setEducationUnits] = useState<EducationUnit[]>([])
    const [users, setUsers] = useState<UserResponse[]>([])

    const userMap: Record<string, UserResponse> = {}
    for (const u of users) userMap[u.id] = u

    const loadDashboard = useCallback(async (
        filters: AnalyticsFilterState,
        targetPage: number,
        options?: { tableOnly?: boolean },
    ) => {
        const tableOnly = options?.tableOnly ?? false
        try {
            if (tableOnly) setTableLoading(true)
            else {
                setLoading(true)
                setError(null)
            }

            if (tableOnly) {
                const data = await fetchVotings(filters, targetPage)
                setVotings(data)
            } else {
                const data = await fetchDashboard(filters, targetPage)
                setOverview(data.overview)
                setVotings(data.votings)
            }
        } catch (err: unknown) {
            const msg = (err as { response?: { data?: { message?: string } } })
                ?.response?.data?.message
            setError(msg ?? 'Не удалось загрузить аналитику')
        } finally {
            setLoading(false)
            setTableLoading(false)
        }
    }, [])

    useEffect(() => {
        let cancelled = false

        void Promise.all([getEducationUnits(), getUsers()])
            .then(([unitsData, usersData]) => {
                if (cancelled) return
                setEducationUnits(unitsData.educationUnits ?? [])
                setUsers(usersData.users ?? [])
            })
            .catch(() => {})

        void fetchDashboard(EMPTY_ANALYTICS_FILTER_STATE, 1)
            .then(data => {
                if (cancelled) return
                setOverview(data.overview)
                setVotings(data.votings)
                setError(null)
            })
            .catch((err: unknown) => {
                if (cancelled) return
                const msg = (err as { response?: { data?: { message?: string } } })
                    ?.response?.data?.message
                setError(msg ?? 'Не удалось загрузить аналитику')
            })
            .finally(() => {
                if (!cancelled) setLoading(false)
            })

        return () => { cancelled = true }
    }, [])

    const applyFilters = () => {
        setAppliedFilters(draftFilters)
        setPage(1)
        void loadDashboard(draftFilters, 1)
    }

    const resetFilters = () => {
        setDraftFilters(EMPTY_ANALYTICS_FILTER_STATE)
        setAppliedFilters(EMPTY_ANALYTICS_FILTER_STATE)
        setPage(1)
        void loadDashboard(EMPTY_ANALYTICS_FILTER_STATE, 1)
    }

    const goToPage = (nextPage: number) => {
        setPage(nextPage)
        void loadDashboard(appliedFilters, nextPage, { tableOnly: true })
    }

    const retry = () => void loadDashboard(appliedFilters, page)

    const totalPages = votings
        ? Math.max(1, Math.ceil(votings.totalCount / (votings.pageSize || DEFAULT_PAGE_SIZE)))
        : 1

    return {
        draftFilters,
        setDraftFilters,
        overview,
        votings,
        loading,
        tableLoading,
        error,
        educationUnits,
        users,
        userMap,
        page,
        totalPages,
        applyFilters,
        resetFilters,
        goToPage,
        retry,
    }
}
