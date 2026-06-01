import { Vote } from 'lucide-react'
import AnalyticsFiltersBar from '@/components/analytics/AnalyticsFiltersBar'
import AnalyticsPageHeader from '@/components/analytics/AnalyticsPageHeader'
import AnalyticsSkeleton from '@/components/analytics/AnalyticsSkeleton'
import AnalyticsVotingsTable from '@/components/analytics/AnalyticsVotingsTable'
import KpiStatGrid from '@/components/analytics/KpiStatGrid'
import PendingApprovalQueue from '@/components/analytics/PendingApprovalQueue'
import StatusDistributionChart from '@/components/analytics/StatusDistributionChart'
import TimeSeriesChart from '@/components/analytics/TimeSeriesChart'
import TypeDistributionChart from '@/components/analytics/TypeDistributionChart'
import { useAnalyticsDashboard } from '@/hooks/useAnalyticsDashboard'
import { usePrefersDark } from '@/hooks/usePrefersDark'

export default function AdminAnalyticsPage() {
    const isDark = usePrefersDark()
    const {
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
    } = useAnalyticsDashboard()

    if (loading && !overview) {
        return (
            <div className="space-y-8">
                <AnalyticsPageHeader />
                <AnalyticsSkeleton />
            </div>
        )
    }

    return (
        <div className="space-y-8">
            <AnalyticsPageHeader />

            <AnalyticsFiltersBar
                filters={draftFilters}
                onChange={setDraftFilters}
                onApply={applyFilters}
                onReset={resetFilters}
                educationUnits={educationUnits}
                users={users}
            />

            {error && (
                <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between rounded-xl border border-destructive/20 bg-destructive/10 px-4 py-3">
                    <p className="text-sm font-medium text-destructive">{error}</p>
                    <button
                        type="button"
                        onClick={retry}
                        className="shrink-0 rounded-lg bg-destructive/10 px-3 py-1.5 text-sm font-medium text-destructive transition-colors hover:bg-destructive/20"
                    >
                        Повторить
                    </button>
                </div>
            )}

            {loading && overview && (
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Vote className="h-4 w-4 animate-pulse text-primary" />
                    Обновление данных...
                </div>
            )}

            {overview && (
                <>
                    <KpiStatGrid overview={overview} />

                    <div className="grid gap-4 lg:grid-cols-2">
                        <StatusDistributionChart
                            data={overview.statusCounts}
                            isDark={isDark}
                        />
                        <TypeDistributionChart
                            data={overview.typeCounts}
                            isDark={isDark}
                        />
                    </div>

                    <div className="grid gap-4 lg:grid-cols-2">
                        <TimeSeriesChart
                            title="Созданные голосования по неделям"
                            chartId="votings-created"
                            data={overview.votingsCreatedSeries}
                            isDark={isDark}
                            color="#4F46E5"
                        />
                        <TimeSeriesChart
                            title="Отданные голоса по неделям"
                            chartId="votes-cast"
                            data={overview.votesCastSeries}
                            isDark={isDark}
                            color="#10B981"
                        />
                    </div>

                    <PendingApprovalQueue
                        queue={overview.pendingApprovalQueue}
                        userMap={userMap}
                    />
                </>
            )}

            {votings && (
                <AnalyticsVotingsTable
                    votings={votings}
                    userMap={userMap}
                    page={page}
                    totalPages={totalPages}
                    loading={tableLoading}
                    onPageChange={goToPage}
                />
            )}
        </div>
    )
}
