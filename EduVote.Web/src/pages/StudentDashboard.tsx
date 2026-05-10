import { useEffect, useState } from 'react'
import { getVotingsForUser, type VotingResponse, type VotingStatus } from '@/api/votingApi'
import { useAuth } from '@/context/AuthContext'
import { Vote, Clock, CheckCircle, PauseCircle, FileText } from 'lucide-react'

const STATUS_CONFIG: Record<VotingStatus, { label: string; className: string; dotClass: string; icon: React.ReactNode }> = {
    Draft: {
        label: 'Черновик',
        className: 'bg-muted text-muted-foreground',
        dotClass: 'bg-muted-foreground',
        icon: <FileText className="h-3 w-3" />,
    },
    Active: {
        label: 'Активно',
        className: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-400',
        dotClass: 'bg-emerald-500',
        icon: <CheckCircle className="h-3 w-3" />,
    },
    Paused: {
        label: 'Приостановлено',
        className: 'bg-amber-100 text-amber-700 dark:bg-amber-950 dark:text-amber-400',
        dotClass: 'bg-amber-500',
        icon: <PauseCircle className="h-3 w-3" />,
    },
    Finished: {
        label: 'Завершено',
        className: 'bg-secondary text-secondary-foreground',
        dotClass: 'bg-primary/40',
        icon: <Clock className="h-3 w-3" />,
    },
}

const TYPE_LABELS: Record<string, string> = {
    SingleChoice:   'Один вариант',
    MultipleChoice: 'Несколько вариантов',
    Rating:         'Оценка',
    OpenAnswer:     'Открытый ответ',
}

function VotingCard({ voting }: { voting: VotingResponse }) {
    const status = STATUS_CONFIG[voting.status] ?? STATUS_CONFIG.Draft

    return (
        <div className="group flex flex-col gap-4 rounded-2xl border border-border bg-card p-5 shadow-sm transition-all hover:border-primary/30 hover:shadow-md">

            <div className="flex items-start justify-between gap-3">
                <h3 className="text-base font-semibold leading-snug text-card-foreground">
                    {voting.title}
                </h3>
                <span className={`flex shrink-0 items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold ${status.className}`}>
                    {status.icon}
                    {status.label}
                </span>
            </div>

            {voting.description && (
                <p className="text-sm leading-relaxed text-muted-foreground line-clamp-2">
                    {voting.description}
                </p>
            )}

            <div className="flex flex-wrap items-center gap-2 border-t border-border pt-3 text-xs text-muted-foreground mt-auto">
                <span className="rounded-md bg-secondary px-2 py-0.5 text-secondary-foreground font-medium">
                    {TYPE_LABELS[voting.type] ?? voting.type}
                </span>
                {voting.isAnonymous && (
                    <span className="rounded-md bg-secondary px-2 py-0.5 text-secondary-foreground font-medium">
                        Анонимное
                    </span>
                )}
                <span className="ml-auto">
                    {new Date(voting.startTime).toLocaleDateString('ru-RU')}
                    {' — '}
                    {new Date(voting.endTime).toLocaleDateString('ru-RU')}
                </span>
            </div>

        </div>
    )
}

export default function StudentDashboard() {
    const { claims } = useAuth()
    const [votings, setVotings] = useState<VotingResponse[]>([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)

    useEffect(() => {
        if (!claims?.nameid) return

        getVotingsForUser(claims.nameid)
            .then(data => setVotings(data.votings ?? []))
            .catch(err => setError(err?.response?.data?.message ?? 'Не удалось загрузить голосования'))
            .finally(() => setLoading(false))
    }, [claims?.nameid])

    const active = votings.filter(v => v.status === 'Active')
    const other  = votings.filter(v => v.status !== 'Active')

    return (
        <div className="space-y-8">

            <div>
                <h1 className="text-2xl font-bold tracking-tight text-foreground">
                    Мои голосования
                </h1>
                <p className="mt-1 text-sm text-muted-foreground">
                    Голосования, доступные для вашей учебной группы
                </p>
            </div>

            {loading && (
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Vote className="h-4 w-4 animate-pulse text-primary" />
                    Загрузка голосований...
                </div>
            )}

            {error && (
                <div className="rounded-xl border border-destructive/20 bg-destructive/10 px-4 py-3 text-sm font-medium text-destructive">
                    {error}
                </div>
            )}

            {!loading && !error && votings.length === 0 && (
                <div className="flex flex-col items-center gap-3 rounded-2xl border border-dashed border-border bg-card py-20 text-center">
                    <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-muted">
                        <Vote className="h-7 w-7 text-muted-foreground" />
                    </div>
                    <p className="text-sm font-semibold text-foreground">Нет доступных голосований</p>
                    <p className="text-xs text-muted-foreground max-w-xs">
                        Голосования появятся, когда администратор их создаст для вашей группы
                    </p>
                </div>
            )}

            {active.length > 0 && (
                <section className="space-y-3">
                    <h2 className="flex items-center gap-2 text-xs font-semibold uppercase tracking-widest text-muted-foreground">
                        <span className="h-2 w-2 rounded-full bg-emerald-500 animate-pulse" />
                        Активные
                    </h2>
                    <div className="grid gap-4 sm:grid-cols-2">
                        {active.map(v => <VotingCard key={v.id} voting={v} />)}
                    </div>
                </section>
            )}

            {other.length > 0 && (
                <section className="space-y-3">
                    <h2 className="text-xs font-semibold uppercase tracking-widest text-muted-foreground">
                        Остальные
                    </h2>
                    <div className="grid gap-4 sm:grid-cols-2">
                        {other.map(v => <VotingCard key={v.id} voting={v} />)}
                    </div>
                </section>
            )}

        </div>
    )
}
