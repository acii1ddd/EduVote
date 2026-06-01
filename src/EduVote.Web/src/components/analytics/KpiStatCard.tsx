import type { ReactNode } from 'react'

interface KpiStatCardProps {
    icon: ReactNode
    label: string
    value: number | string
}

export default function KpiStatCard({ icon, label, value }: KpiStatCardProps) {
    return (
        <div className="rounded-2xl border border-border bg-card p-4 shadow-sm transition-shadow duration-150 hover:shadow-md">
            <div className="mb-3 flex h-9 w-9 items-center justify-center rounded-xl bg-primary/10 text-primary">
                {icon}
            </div>
            <p className="text-2xl font-bold tabular-nums text-foreground">{value}</p>
            <p className="mt-0.5 text-xs font-medium text-muted-foreground">{label}</p>
        </div>
    )
}
