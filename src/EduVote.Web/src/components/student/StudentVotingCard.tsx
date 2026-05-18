import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { BarChart2, CheckCircle, ChevronDown, ChevronUp, Clock, FileText, Lock, MapPin, PauseCircle, ShieldAlert, Unlock, User, Vote } from 'lucide-react'
import type { VotingResponse, VotingStatus } from '@/api/votingApi'
import type { EducationUnit } from '@/api/educationUnitApi'
import { getTargets } from '@/api/votingTargetApi'

// ── Status config ──────────────────────────────────────────

const STATUS_CONFIG: Record<VotingStatus, { label: string; className: string; icon: React.ReactNode }> = {
    Draft: {
        label: 'Черновик',
        className: 'bg-muted text-muted-foreground',
        icon: <FileText className="h-3 w-3" />,
    },
    Active: {
        label: 'Активно',
        className: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-400',
        icon: <CheckCircle className="h-3 w-3" />,
    },
    Paused: {
        label: 'Приостановлено',
        className: 'bg-amber-100 text-amber-700 dark:bg-amber-950 dark:text-amber-400',
        icon: <PauseCircle className="h-3 w-3" />,
    },
    Finished: {
        label: 'Завершено',
        className: 'bg-secondary text-secondary-foreground',
        icon: <Clock className="h-3 w-3" />,
    },
    PendingApproval: {
        label: 'На модерации',
        className: 'bg-violet-100 text-violet-700 dark:bg-violet-950 dark:text-violet-400',
        icon: <ShieldAlert className="h-3 w-3" />,
    },
}

const TYPE_LABELS: Record<string, string> = {
    SingleChoice:   'Один вариант',
    MultipleChoice: 'Несколько вариантов',
    Rating:         'Оценка',
    OpenAnswer:     'Открытый ответ',
}

// ── Props ──────────────────────────────────────────────────

interface Props {
    voting: VotingResponse
    allUnits: EducationUnit[]
    createdByName?: string
    hasVoted?: boolean
}

// ── Component ──────────────────────────────────────────────

export default function StudentVotingCard({ voting, allUnits, createdByName, hasVoted }: Props) {
    const navigate = useNavigate()
    const status = STATUS_CONFIG[voting.status] ?? STATUS_CONFIG.Draft

    const [open,    setOpen]    = useState(false)
    const [unitIds, setUnitIds] = useState<string[] | null>(null)
    const [loading, setLoading] = useState(false)

    const toggleTargets = async () => {
        if (!open && unitIds === null) {
            setLoading(true)
            try {
                const data = await getTargets(voting.id)
                setUnitIds((data.targets ?? []).map(t => t.educationUnitId))
            } catch {
                setUnitIds([])
            } finally {
                setLoading(false)
            }
        }
        setOpen(prev => !prev)
    }

    const targetUnits = unitIds
        ?.map(id => allUnits.find(u => u.id === id))
        .filter(Boolean) as EducationUnit[] | undefined

    const fmt = (iso: string) => iso ? new Date(iso).toLocaleDateString('ru-RU') : '—'

    const isFinished = voting.status === 'Finished'

    return (
        <div className={`flex flex-col rounded-2xl border border-border bg-card shadow-sm transition-all ${isFinished ? 'opacity-80 hover:border-primary/30 hover:shadow-md hover:opacity-100' : 'hover:border-primary/30 hover:shadow-md'}`}>

            {/* Main content */}
            <div
                className="flex flex-col gap-3 p-5 cursor-pointer"
                onClick={() => navigate(isFinished ? `/votings/${voting.id}/results` : `/votings/${voting.id}`)}
            >

                <div className="flex items-start justify-between gap-3">
                    <h3 className="text-base font-semibold leading-snug text-card-foreground">
                        {voting.title}
                    </h3>
                    <div className="flex shrink-0 flex-col items-end gap-1.5">
                        <span className={`flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold ${status.className}`}>
                            {status.icon}
                            {status.label}
                        </span>
                        {isFinished && (
                            <span className="flex items-center gap-1 rounded-full bg-primary/10 px-2.5 py-1 text-xs font-semibold text-primary">
                                <BarChart2 className="h-3 w-3" />
                                Результаты доступны
                            </span>
                        )}
                        {hasVoted && !isFinished && (
                            <span className="flex items-center gap-1 rounded-full bg-emerald-100 px-2.5 py-1 text-xs font-semibold text-emerald-700 dark:bg-emerald-950 dark:text-emerald-400">
                                <Vote className="h-3 w-3" />
                                Вы проголосовали
                            </span>
                        )}
                    </div>
                </div>

                {createdByName && (
                    <div className="flex items-center gap-1 text-xs text-muted-foreground">
                        <User className="h-3 w-3 shrink-0" />
                        <span>{createdByName}</span>
                    </div>
                )}

                {voting.description && (
                    <p className="text-sm leading-relaxed text-muted-foreground line-clamp-2">
                        {voting.description}
                    </p>
                )}

                <div className="flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
                    <span className="rounded-md bg-secondary px-2 py-0.5 text-secondary-foreground font-medium">
                        {TYPE_LABELS[voting.type] ?? voting.type}
                    </span>
                    {voting.isAnonymous && (
                        <span className="rounded-md bg-secondary px-2 py-0.5 text-secondary-foreground font-medium">
                            Анонимное
                        </span>
                    )}
                    {voting.allowVoteChange ? (
                        <span className="flex items-center gap-1 rounded-md bg-secondary px-2 py-0.5 text-secondary-foreground font-medium">
                            <Unlock className="h-3 w-3" />
                            Смена голоса доступна
                        </span>
                    ) : (
                        <span className="flex items-center gap-1 rounded-md bg-secondary px-2 py-0.5 text-secondary-foreground font-medium">
                            <Lock className="h-3 w-3" />
                            Смена голоса недоступна
                        </span>
                    )}
                    <span className="ml-auto whitespace-nowrap">
                        {fmt(voting.startTime)} — {fmt(voting.endTime)}
                    </span>
                </div>

            </div>

            {/* Targeting toggle */}
            <button
                onClick={toggleTargets}
                className="flex items-center justify-between border-t border-border px-5 py-2.5 text-xs font-medium text-muted-foreground transition-colors hover:bg-muted/30 hover:text-foreground"
            >
                <span className="flex items-center gap-1.5">
                    <MapPin className="h-3.5 w-3.5" />
                    Доступные группы
                </span>
                {open
                    ? <ChevronUp className="h-3.5 w-3.5" />
                    : <ChevronDown className="h-3.5 w-3.5" />
                }
            </button>

            {/* Targeting panel */}
            {open && (
                <div className="border-t border-border px-5 py-3">
                    {loading ? (
                        <p className="text-xs text-muted-foreground">Загрузка...</p>
                    ) : !targetUnits || targetUnits.length === 0 ? (
                        <p className="text-xs text-muted-foreground italic">
                            Публичное — доступно всем пользователям
                        </p>
                    ) : (
                        <div className="flex flex-wrap gap-1.5">
                            {targetUnits.map(u => (
                                <span
                                    key={u.id}
                                    className="rounded-md border border-border bg-muted/50 px-2 py-0.5 text-xs text-foreground"
                                >
                                    {u.name}
                                    <span className="ml-1 text-muted-foreground">({u.type})</span>
                                </span>
                            ))}
                        </div>
                    )}
                </div>
            )}

        </div>
    )
}
