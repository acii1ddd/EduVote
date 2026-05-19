import { BarChart3 } from 'lucide-react'

export default function AnalyticsPageHeader() {
    return (
        <div className="flex items-start gap-4">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-primary/10">
                <BarChart3 className="h-5 w-5 text-primary" />
            </div>
            <div>
                <h1 className="text-2xl font-bold tracking-tight">Аналитика</h1>
                <p className="mt-0.5 text-sm text-muted-foreground">
                    Обзор голосований, активности и явки по выбранным фильтрам
                </p>
            </div>
        </div>
    )
}
