import { Link } from 'react-router-dom'
import { ChevronLeft, ChevronRight, List } from 'lucide-react'
import { STATUS_CONFIG, TYPE_LABELS } from '@/components/voting/votingConstants'
import type { VotingStatus, VotingType } from '@/api/votingApi'
import type { UserResponse } from '@/api/userApi'
import type { AnalyticsVotingsResponse } from '@/types/analytics'

interface AnalyticsVotingsTableProps {
    votings: AnalyticsVotingsResponse
    userMap: Record<string, UserResponse>
    page: number
    totalPages: number
    loading?: boolean
    onPageChange: (page: number) => void
}

const fmtDate = (iso: string) =>
    iso ? new Date(iso).toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—'

export default function AnalyticsVotingsTable({
    votings,
    userMap,
    page,
    totalPages,
    loading = false,
    onPageChange,
}: AnalyticsVotingsTableProps) {
    return (
        <div className="rounded-2xl border border-border bg-card shadow-sm transition-shadow duration-150 hover:shadow-md">
            <div className="flex items-center gap-2 border-b border-border px-5 py-4">
                <List className="h-4 w-4 text-primary" />
                <h3 className="text-sm font-semibold text-foreground">Голосования</h3>
                <span className="ml-auto text-xs text-muted-foreground">
                    Всего: {votings.totalCount}
                </span>
            </div>

            {votings.items.length === 0 ? (
                <p className="px-5 py-12 text-center text-sm text-muted-foreground">
                    Голосования не найдены
                </p>
            ) : (
                <div className={`overflow-x-auto ${loading ? 'opacity-50 pointer-events-none' : ''}`}>
                    <table className="w-full min-w-[640px] text-sm">
                        <thead>
                            <tr className="border-b border-border text-left text-xs text-muted-foreground">
                                <th className="px-5 py-3 font-medium">Название</th>
                                <th className="px-5 py-3 font-medium">Тип</th>
                                <th className="px-5 py-3 font-medium">Статус</th>
                                <th className="px-5 py-3 font-medium">Период</th>
                                <th className="px-5 py-3 font-medium">Голоса</th>
                                <th className="px-5 py-3 font-medium">Явка</th>
                                <th className="px-5 py-3 font-medium">Создатель</th>
                            </tr>
                        </thead>
                        <tbody>
                            {votings.items.map(row => {
                                const statusCfg = STATUS_CONFIG[row.status as VotingStatus]
                                return (
                                    <tr
                                        key={row.id}
                                        className="border-b border-border/60 transition-colors last:border-0 hover:bg-secondary/50"
                                    >
                                        <td className="max-w-[200px] px-5 py-3.5">
                                            {row.status === 'Finished' ? (
                                                <Link
                                                    to={`/votings/${row.id}/results`}
                                                    className="font-medium text-primary transition-colors hover:text-primary/80"
                                                >
                                                    {row.title}
                                                </Link>
                                            ) : (
                                                <span className="font-medium text-foreground">{row.title}</span>
                                            )}
                                        </td>
                                        <td className="px-5 py-3.5 text-muted-foreground">
                                            {TYPE_LABELS[row.type as VotingType] ?? row.type}
                                        </td>
                                        <td className="px-5 py-3.5">
                                            {statusCfg ? (
                                                <span className={`inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium ${statusCfg.badgeClass}`}>
                                                    {statusCfg.icon}
                                                    {statusCfg.label}
                                                </span>
                                            ) : (
                                                row.status
                                            )}
                                        </td>
                                        <td className="whitespace-nowrap px-5 py-3.5 text-muted-foreground">
                                            {fmtDate(row.startTime)} – {fmtDate(row.endTime)}
                                        </td>
                                        <td className="px-5 py-3.5 tabular-nums text-muted-foreground">
                                            {row.totalVotes} / {row.eligibleCount}
                                        </td>
                                        <td className="px-5 py-3.5 tabular-nums text-muted-foreground">
                                            {row.turnoutPercent.toFixed(1)}%
                                        </td>
                                        <td className="px-5 py-3.5 text-muted-foreground">
                                            {userMap[row.createdById]?.name ?? '—'}
                                        </td>
                                    </tr>
                                )
                            })}
                        </tbody>
                    </table>
                </div>
            )}

            {votings.totalCount > 0 && (
                <div className="flex items-center justify-between border-t border-border px-5 py-3">
                    <button
                        type="button"
                        disabled={page <= 1 || loading}
                        onClick={() => onPageChange(page - 1)}
                        className="flex items-center gap-1 rounded-lg px-3 py-1.5 text-sm font-medium text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground disabled:opacity-40"
                    >
                        <ChevronLeft className="h-4 w-4" />
                        Назад
                    </button>
                    <span className="text-xs text-muted-foreground">
                        Стр. {page} из {totalPages}
                    </span>
                    <button
                        type="button"
                        disabled={page >= totalPages || loading}
                        onClick={() => onPageChange(page + 1)}
                        className="flex items-center gap-1 rounded-lg px-3 py-1.5 text-sm font-medium text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground disabled:opacity-40"
                    >
                        Вперёд
                        <ChevronRight className="h-4 w-4" />
                    </button>
                </div>
            )}
        </div>
    )
}
