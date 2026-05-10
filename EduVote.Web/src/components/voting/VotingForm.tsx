import type { VotingType } from '@/api/votingApi'
import { Field, CheckboxField } from '@/components/ui/Field'
import { inputClass, VOTING_TYPES } from './votingConstants'

export type VotingFormData = {
    title: string
    description: string
    type: VotingType
    isAnonymous: boolean
    allowVoteChange: boolean
    startTime: string
    endTime: string
}

export const EMPTY_VOTING_FORM: VotingFormData = {
    title: '',
    description: '',
    type: 'SingleChoice',
    isAnonymous: false,
    allowVoteChange: false,
    startTime: '',
    endTime: '',
}

interface Props {
    form: VotingFormData
    onChange: (f: VotingFormData) => void
    onSubmit: (e: React.FormEvent) => void
    onCancel: () => void
    error: string | null
    submitting: boolean
    submitLabel: string
}

export default function VotingForm({ form, onChange, onSubmit, onCancel, error, submitting, submitLabel }: Props) {
    const set = <K extends keyof VotingFormData>(key: K, value: VotingFormData[K]) =>
        onChange({ ...form, [key]: value })

    return (
        <form onSubmit={onSubmit} className="space-y-4">

            <Field label="Название *">
                <input
                    className={inputClass}
                    placeholder="Выборы старосты группы"
                    value={form.title}
                    onChange={e => set('title', e.target.value)}
                />
            </Field>

            <Field label="Описание">
                <textarea
                    className={inputClass + ' resize-none'}
                    rows={3}
                    placeholder="Краткое описание голосования..."
                    value={form.description}
                    onChange={e => set('description', e.target.value)}
                />
            </Field>

            <Field label="Тип">
                <select
                    className={inputClass}
                    value={form.type}
                    onChange={e => set('type', e.target.value as VotingType)}
                >
                    {VOTING_TYPES.map(t => (
                        <option key={t.value} value={t.value}>{t.label}</option>
                    ))}
                </select>
            </Field>

            <div className="grid grid-cols-2 gap-4">
                <Field label="Начало">
                    <input
                        type="datetime-local"
                        className={inputClass}
                        value={form.startTime}
                        onChange={e => set('startTime', e.target.value)}
                    />
                </Field>
                <Field label="Конец">
                    <input
                        type="datetime-local"
                        className={inputClass}
                        value={form.endTime}
                        onChange={e => set('endTime', e.target.value)}
                    />
                </Field>
            </div>

            <div className="flex flex-col gap-3">
                <CheckboxField
                    label="Анонимное голосование"
                    checked={form.isAnonymous}
                    onChange={v => set('isAnonymous', v)}
                />
                <CheckboxField
                    label="Разрешить изменение голоса"
                    checked={form.allowVoteChange}
                    onChange={v => set('allowVoteChange', v)}
                />
            </div>

            {error && (
                <div className="rounded-xl border border-destructive/20 bg-destructive/10 px-3.5 py-2.5 text-sm font-medium text-destructive">
                    {error}
                </div>
            )}

            <div className="flex justify-end gap-2 pt-1">
                <button
                    type="button"
                    onClick={onCancel}
                    className="rounded-xl border border-border px-4 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
                >
                    Отмена
                </button>
                <button
                    type="submit"
                    disabled={submitting}
                    className="flex items-center gap-2 rounded-xl bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/20 transition-all hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-50"
                >
                    {submitting ? 'Сохраняем...' : submitLabel}
                </button>
            </div>

        </form>
    )
}
