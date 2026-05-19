import { Link } from 'react-router-dom'
import { ShieldAlert } from 'lucide-react'
import type { AnalyticsOverview } from '@/types/analytics'
import type { UserResponse } from '@/api/userApi'

interface PendingApprovalQueueProps {
    queue: AnalyticsOverview['pendingApprovalQueue']
    userMap: Record<string, UserResponse>
}

export default function PendingApprovalQueue({ queue, userMap }: PendingApprovalQueueProps) {
    return (
        <div className="rounded-2xl border border-border bg-card shadow-sm transition-shadow duration-150 hover:shadow-md">
            <div className="flex items-center justify-between border-b border-border px-5 py-4">
                <div className="flex items-center gap-2">
                    <ShieldAlert className="h-4 w-4 text-violet-600 dark:text-violet-400" />
                    <h3 className="text-sm font-semibold text-foreground">Очередь модерации</h3>
                </div>
                <Link
                    to="/votings"
                    className="text-xs font-medium text-primary transition-colors hover:text-primary/80"
                >
                    Управление голосованиями →
                </Link>
            </div>
            {queue.length === 0 ? (
                <p className="px-5 py-8 text-center text-sm text-muted-foreground">
                    Нет голосований на модерации
                </p>
            ) : (
                <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                        <thead>
                            <tr className="border-b border-border text-left text-xs text-muted-foreground">
                                <th className="px-5 py-3 font-medium">Название</th>
                                <th className="px-5 py-3 font-medium">Создатель</th>
                                <th className="px-5 py-3 font-medium">Создано</th>
                            </tr>
                        </thead>
                        <tbody>
                            {queue.map(row => (
                                <tr
                                    key={row.votingId}
                                    className="border-b border-border/60 transition-colors last:border-0 hover:bg-secondary/50"
                                >
                                    <td className="px-5 py-3.5 font-medium text-foreground">{row.title}</td>
                                    <td className="px-5 py-3.5 text-muted-foreground">
                                        {userMap[row.createdById]?.name ?? row.createdById.slice(0, 8) + '…'}
                                    </td>
                                    <td className="px-5 py-3.5 text-muted-foreground">
                                        {row.createdAt
                                            ? new Date(row.createdAt).toLocaleString('ru-RU')
                                            : '—'}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    )
}
