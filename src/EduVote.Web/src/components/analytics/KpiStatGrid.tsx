import {
    CheckCircle,
    Clock,
    FileText,
    PauseCircle,
    ShieldAlert,
    Users,
    Vote,
} from 'lucide-react'
import type { AnalyticsOverview } from '@/types/analytics'
import KpiStatCard from './KpiStatCard'

interface KpiStatGridProps {
    overview: AnalyticsOverview
}

export default function KpiStatGrid({ overview }: KpiStatGridProps) {
    const items = [
        { icon: <Vote className="h-4 w-4" />, label: 'Всего голосований', value: overview.totalVotings },
        { icon: <CheckCircle className="h-4 w-4" />, label: 'Активные', value: overview.activeVotings },
        { icon: <ShieldAlert className="h-4 w-4" />, label: 'На модерации', value: overview.pendingApprovalVotings },
        { icon: <Clock className="h-4 w-4" />, label: 'Завершённые', value: overview.finishedVotings },
        { icon: <FileText className="h-4 w-4" />, label: 'Черновики', value: overview.draftVotings },
        { icon: <PauseCircle className="h-4 w-4" />, label: 'Приостановленные', value: overview.pausedVotings },
        { icon: <Users className="h-4 w-4" />, label: 'Всего голосов', value: overview.totalVotesCast },
    ]

    return (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {items.map(item => (
                <KpiStatCard
                    key={item.label}
                    icon={item.icon}
                    label={item.label}
                    value={item.value}
                />
            ))}
        </div>
    )
}
