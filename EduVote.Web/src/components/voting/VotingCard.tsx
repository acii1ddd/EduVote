import { Calendar, FlagTriangleRight, Pause, Pencil, Play, Target, Trash2, Users } from 'lucide-react'
import type { VotingResponse } from '@/api/votingApi'
import { STATUS_CONFIG, TYPE_LABELS } from './votingConstants'

interface Props {
    voting: VotingResponse
    busy: boolean
    onEdit: () => void
    onCandidates: () => void
    onTargets: () => void
    onStart: () => void
    onPause: () => void
    onFinish: () => void
    onDelete: () => void
}

export default function VotingCard({
    voting, busy,
    onEdit, onCandidates, onTargets,
    onStart, onPause, onFinish, onDelete,
}: Props) {
    const cfg = STATUS_CONFIG[voting.status] ?? STATUS_CONFIG.Draft
    const hasCandidates = voting.type !== 'OpenAnswer'

    const canStart  = voting.status === 'Draft'  || voting.status === 'Paused'
    const canPause  = voting.status === 'Active'
    const canFinish = voting.status === 'Active'  || voting.status === 'Paused'

    const fmt = (iso: string) => iso ? new Date(iso).toLocaleDateString('ru-RU') : '—'

    return (
        <div className={`flex flex-col rounded-2xl border-2 bg-card shadow-sm transition-all ${cfg.cardClass}`}>

            {/* Header */}
            <div className="flex items-start justify-between gap-3 px-5 pt-4 pb-3">
                <h3 className="text-base font-semibold leading-snug text-card-foreground line-clamp-2">
                    {voting.title}
                </h3>
                <span className={`flex shrink-0 items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold ${cfg.badgeClass}`}>
                    {cfg.icon}
                    {cfg.label}
                </span>
            </div>

            {/* Body */}
            <div className="flex flex-col gap-3 px-5 pb-4 flex-1">
                {voting.description && (
                    <p className="text-sm leading-relaxed text-muted-foreground line-clamp-2">
                        {voting.description}
                    </p>
                )}

                <div className="flex flex-wrap gap-1.5 text-xs">
                    <span className="rounded-md bg-secondary px-2 py-0.5 text-secondary-foreground font-medium">
                        {TYPE_LABELS[voting.type]}
                    </span>
                    {voting.isAnonymous && (
                        <span className="rounded-md bg-secondary px-2 py-0.5 text-secondary-foreground font-medium">
                            Анонимное
                        </span>
                    )}
                    {voting.allowVoteChange && (
                        <span className="rounded-md bg-secondary px-2 py-0.5 text-secondary-foreground font-medium">
                            Изм. голоса
                        </span>
                    )}
                </div>

                {(voting.startTime || voting.endTime) && (
                    <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
                        <Calendar className="h-3.5 w-3.5 shrink-0" />
                        <span>{fmt(voting.startTime)} — {fmt(voting.endTime)}</span>
                    </div>
                )}
            </div>

            {/* Actions */}
            <div className="flex items-center justify-between gap-2 border-t border-border px-5 py-3">

                {/* Lifecycle */}
                <div className="flex items-center gap-1.5 flex-wrap">
                    {canStart && (
                        <ActionBtn
                            disabled={busy}
                            onClick={onStart}
                            className="text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-950/40 border-emerald-200 dark:border-emerald-800"
                        >
                            <Play className="h-3.5 w-3.5" />
                            {voting.status === 'Paused' ? 'Возобновить' : 'Запустить'}
                        </ActionBtn>
                    )}
                    {canPause && (
                        <ActionBtn
                            disabled={busy}
                            onClick={onPause}
                            className="text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-950/40 border-amber-200 dark:border-amber-800"
                        >
                            <Pause className="h-3.5 w-3.5" />
                            Пауза
                        </ActionBtn>
                    )}
                    {canFinish && (
                        <ActionBtn
                            disabled={busy}
                            onClick={onFinish}
                            className="text-muted-foreground hover:bg-secondary border-border"
                        >
                            <FlagTriangleRight className="h-3.5 w-3.5" />
                            Завершить
                        </ActionBtn>
                    )}
                </div>

                {/* Management */}
                <div className="flex items-center gap-1 shrink-0">
                    <IconBtn onClick={onTargets} title="Таргетинг" disabled={busy}>
                        <Target className="h-4 w-4" />
                    </IconBtn>
                    {hasCandidates && (
                        <IconBtn onClick={onCandidates} title="Кандидаты" disabled={busy}>
                            <Users className="h-4 w-4" />
                        </IconBtn>
                    )}
                    <IconBtn onClick={onEdit} title="Редактировать" disabled={busy}>
                        <Pencil className="h-4 w-4" />
                    </IconBtn>
                    <IconBtn
                        onClick={onDelete}
                        title="Удалить"
                        disabled={busy}
                        className="hover:bg-destructive/10 hover:text-destructive hover:border-destructive/30"
                    >
                        <Trash2 className="h-4 w-4" />
                    </IconBtn>
                </div>

            </div>
        </div>
    )
}

function ActionBtn({ children, onClick, disabled, className = '' }: {
    children: React.ReactNode
    onClick: () => void
    disabled?: boolean
    className?: string
}) {
    return (
        <button
            onClick={onClick}
            disabled={disabled}
            className={`flex items-center gap-1.5 rounded-lg border px-2.5 py-1.5 text-xs font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-40 ${className}`}
        >
            {children}
        </button>
    )
}

function IconBtn({ children, onClick, title, disabled, className = '' }: {
    children: React.ReactNode
    onClick: () => void
    title: string
    disabled?: boolean
    className?: string
}) {
    return (
        <button
            onClick={onClick}
            disabled={disabled}
            title={title}
            className={`flex h-8 w-8 items-center justify-center rounded-lg border border-border text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground disabled:cursor-not-allowed disabled:opacity-40 ${className}`}
        >
            {children}
        </button>
    )
}
