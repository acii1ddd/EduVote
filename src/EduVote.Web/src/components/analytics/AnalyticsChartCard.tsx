import type { ReactNode } from 'react'
import AnalyticsEmptyState from './AnalyticsEmptyState'

interface AnalyticsChartCardProps {
    title: string
    isEmpty?: boolean
    emptyTitle?: string
    children: ReactNode
}

export default function AnalyticsChartCard({
    title,
    isEmpty = false,
    emptyTitle,
    children,
}: AnalyticsChartCardProps) {
    return (
        <div className="rounded-2xl border border-border bg-card p-5 shadow-sm transition-shadow duration-150 hover:shadow-md">
            <h3 className="mb-4 text-sm font-semibold text-foreground">{title}</h3>
            {isEmpty ? (
                <AnalyticsEmptyState
                    title={emptyTitle ?? 'Нет данных для графика'}
                    description="За выбранный период записей не найдено."
                />
            ) : (
                children
            )}
        </div>
    )
}
