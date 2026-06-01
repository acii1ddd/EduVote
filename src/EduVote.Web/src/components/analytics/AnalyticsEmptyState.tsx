import { BarChart2 } from 'lucide-react'

interface AnalyticsEmptyStateProps {
    title?: string
    description?: string
}

export default function AnalyticsEmptyState({
    title = 'Нет данных',
    description = 'Попробуйте изменить фильтры или период.',
}: AnalyticsEmptyStateProps) {
    return (
        <div className="flex flex-col items-center justify-center rounded-2xl border border-dashed border-border bg-card py-16 text-center">
            <div className="mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-muted">
                <BarChart2 className="h-7 w-7 text-muted-foreground" />
            </div>
            <p className="text-sm font-medium text-foreground">{title}</p>
            <p className="mt-1 max-w-xs text-sm text-muted-foreground">{description}</p>
        </div>
    )
}
