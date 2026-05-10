import type { VotingStatus, VotingType } from '@/api/votingApi'
import { CheckCircle, Clock, FileText, PauseCircle } from 'lucide-react'
import { createElement } from 'react'

export const inputClass = `
    w-full rounded-xl border border-border bg-background
    px-3.5 py-2.5 text-sm text-foreground placeholder:text-muted-foreground
    focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20
    transition-colors
`

export const VOTING_TYPES: { value: VotingType; label: string }[] = [
    { value: 'SingleChoice',   label: 'Один вариант' },
    { value: 'MultipleChoice', label: 'Несколько вариантов' },
    { value: 'Rating',         label: 'Оценка' },
    { value: 'OpenAnswer',     label: 'Открытый ответ' },
]

export const TYPE_LABELS: Record<VotingType, string> = {
    SingleChoice:   'Один вариант',
    MultipleChoice: 'Несколько вариантов',
    Rating:         'Оценка',
    OpenAnswer:     'Открытый ответ',
}

export const STATUS_CONFIG: Record<VotingStatus, {
    label: string
    cardClass: string
    badgeClass: string
    icon: React.ReactNode
}> = {
    Draft: {
        label:     'Черновик',
        cardClass: 'border-border',
        badgeClass: 'bg-muted text-muted-foreground',
        icon: createElement(FileText, { className: 'h-3 w-3' }),
    },
    Active: {
        label:     'Активно',
        cardClass: 'border-emerald-300 dark:border-emerald-800',
        badgeClass: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-400',
        icon: createElement(CheckCircle, { className: 'h-3 w-3' }),
    },
    Paused: {
        label:     'Приостановлено',
        cardClass: 'border-amber-300 dark:border-amber-800',
        badgeClass: 'bg-amber-100 text-amber-700 dark:bg-amber-950 dark:text-amber-400',
        icon: createElement(PauseCircle, { className: 'h-3 w-3' }),
    },
    Finished: {
        label:     'Завершено',
        cardClass: 'border-border opacity-75',
        badgeClass: 'bg-secondary text-secondary-foreground',
        icon: createElement(Clock, { className: 'h-3 w-3' }),
    },
}
