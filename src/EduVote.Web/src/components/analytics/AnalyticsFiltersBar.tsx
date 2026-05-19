import type { EducationUnit } from '@/api/educationUnitApi'
import type { UserResponse } from '@/api/userApi'
import { inputClass, STATUS_CONFIG, VOTING_TYPES } from '@/components/voting/votingConstants'
import type { AnalyticsFilterState } from '@/types/analytics'
import type { VotingStatus } from '@/api/votingApi'
import { Filter, RotateCcw } from 'lucide-react'

interface AnalyticsFiltersBarProps {
    filters: AnalyticsFilterState
    onChange: (filters: AnalyticsFilterState) => void
    onApply: () => void
    onReset: () => void
    educationUnits: EducationUnit[]
    users: UserResponse[]
}

export default function AnalyticsFiltersBar({
    filters,
    onChange,
    onApply,
    onReset,
    educationUnits,
    users,
}: AnalyticsFiltersBarProps) {
    const set = <K extends keyof AnalyticsFilterState>(key: K, value: AnalyticsFilterState[K]) =>
        onChange({ ...filters, [key]: value })

    return (
        <div className="rounded-2xl border border-border bg-card p-5 shadow-sm">
            <div className="mb-4 flex items-center gap-2 text-sm font-semibold text-foreground">
                <Filter className="h-4 w-4 text-primary" />
                Фильтры
            </div>
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                <label className="block space-y-1.5">
                    <span className="text-xs font-medium text-muted-foreground">Дата с</span>
                    <input
                        type="date"
                        className={inputClass}
                        value={filters.dateFrom}
                        onChange={e => set('dateFrom', e.target.value)}
                    />
                </label>
                <label className="block space-y-1.5">
                    <span className="text-xs font-medium text-muted-foreground">Дата по</span>
                    <input
                        type="date"
                        className={inputClass}
                        value={filters.dateTo}
                        onChange={e => set('dateTo', e.target.value)}
                    />
                </label>
                <label className="block space-y-1.5">
                    <span className="text-xs font-medium text-muted-foreground">Подразделение</span>
                    <select
                        className={inputClass}
                        value={filters.educationUnitId}
                        onChange={e => set('educationUnitId', e.target.value)}
                    >
                        <option value="">Все</option>
                        {educationUnits.map(u => (
                            <option key={u.id} value={u.id}>{u.name}</option>
                        ))}
                    </select>
                </label>
                <label className="block space-y-1.5">
                    <span className="text-xs font-medium text-muted-foreground">Тип</span>
                    <select
                        className={inputClass}
                        value={filters.type}
                        onChange={e => set('type', e.target.value)}
                    >
                        <option value="">Все</option>
                        {VOTING_TYPES.map(t => (
                            <option key={t.value} value={t.value}>{t.label}</option>
                        ))}
                    </select>
                </label>
                <label className="block space-y-1.5">
                    <span className="text-xs font-medium text-muted-foreground">Статус</span>
                    <select
                        className={inputClass}
                        value={filters.status}
                        onChange={e => set('status', e.target.value)}
                    >
                        <option value="">Все</option>
                        {(Object.keys(STATUS_CONFIG) as VotingStatus[]).map(s => (
                            <option key={s} value={s}>{STATUS_CONFIG[s].label}</option>
                        ))}
                    </select>
                </label>
                <label className="block space-y-1.5">
                    <span className="text-xs font-medium text-muted-foreground">Создатель</span>
                    <select
                        className={inputClass}
                        value={filters.createdById}
                        onChange={e => set('createdById', e.target.value)}
                    >
                        <option value="">Все</option>
                        {users.map(u => (
                            <option key={u.id} value={u.id}>{u.name}</option>
                        ))}
                    </select>
                </label>
            </div>
            <div className="mt-4 flex flex-wrap gap-2">
                <button
                    type="button"
                    onClick={onApply}
                    className="rounded-xl bg-primary px-4 py-2.5 text-sm font-medium text-primary-foreground transition-colors duration-150 hover:bg-primary/90"
                >
                    Применить
                </button>
                <button
                    type="button"
                    onClick={onReset}
                    className="flex items-center gap-1.5 rounded-xl border border-border bg-background px-4 py-2.5 text-sm font-medium text-muted-foreground transition-colors duration-150 hover:bg-secondary hover:text-foreground"
                >
                    <RotateCcw className="h-4 w-4" />
                    Сбросить
                </button>
            </div>
        </div>
    )
}
