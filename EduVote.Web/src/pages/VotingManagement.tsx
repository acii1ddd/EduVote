import { useEffect, useState } from 'react'
import {
    getVotings,
    createVoting,
    updateVoting,
    type VotingResponse,
    type VotingType,
    type VotingStatus,
    type VotingFormPayload,
} from '@/api/votingApi'
import {
    getCandidates,
    createCandidate,
    deleteCandidate,
    type CandidateResponse,
} from '@/api/candidateApi'
import { Vote, Plus, X, FileText, CheckCircle, PauseCircle, Clock, Users, Trash2 } from 'lucide-react'

const inputClass = `
    w-full rounded-xl border border-border bg-background
    px-3.5 py-2.5 text-sm text-foreground placeholder:text-muted-foreground
    focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20
    transition-colors
`

const VOTING_TYPES: { value: VotingType; label: string }[] = [
    { value: 'SingleChoice',   label: 'Один вариант' },
    { value: 'MultipleChoice', label: 'Несколько вариантов' },
    { value: 'Rating',         label: 'Оценка' },
    { value: 'OpenAnswer',     label: 'Открытый ответ' },
]

const STATUS_CONFIG: Record<VotingStatus, { label: string; className: string; icon: React.ReactNode }> = {
    Draft:    { label: 'Черновик',       className: 'bg-muted text-muted-foreground',                                              icon: <FileText className="h-3 w-3" /> },
    Active:   { label: 'Активно',        className: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-400',   icon: <CheckCircle className="h-3 w-3" /> },
    Paused:   { label: 'Приостановлено', className: 'bg-amber-100 text-amber-700 dark:bg-amber-950 dark:text-amber-400',           icon: <PauseCircle className="h-3 w-3" /> },
    Finished: { label: 'Завершено',      className: 'bg-secondary text-secondary-foreground',                                      icon: <Clock className="h-3 w-3" /> },
}

const TYPE_LABELS: Record<VotingType, string> = {
    SingleChoice:   'Один вариант',
    MultipleChoice: 'Несколько вариантов',
    Rating:         'Оценка',
    OpenAnswer:     'Открытый ответ',
}

type FormData = {
    title: string
    description: string
    type: VotingType
    isAnonymous: boolean
    allowVoteChange: boolean
    startTime: string
    endTime: string
}

const EMPTY_FORM: FormData = {
    title: '',
    description: '',
    type: 'SingleChoice',
    isAnonymous: false,
    allowVoteChange: false,
    startTime: '',
    endTime: '',
}

function toDateTimeLocal(iso: string): string {
    if (!iso) return ''
    return iso.slice(0, 16)
}

function toIso(local: string): string {
    if (!local) return ''
    return new Date(local).toISOString()
}

function formToPayload(form: FormData): VotingFormPayload {
    return {
        title: form.title,
        description: form.description,
        type: form.type,
        isAnonymous: form.isAnonymous,
        allowVoteChange: form.allowVoteChange,
        startTime: toIso(form.startTime),
        endTime: toIso(form.endTime),
    }
}

function votingToForm(voting: VotingResponse): FormData {
    return {
        title: voting.title,
        description: voting.description,
        type: voting.type,
        isAnonymous: voting.isAnonymous,
        allowVoteChange: voting.allowVoteChange,
        startTime: toDateTimeLocal(voting.startTime),
        endTime: toDateTimeLocal(voting.endTime),
    }
}

export default function VotingManagement() {
    const [votings, setVotings] = useState<VotingResponse[]>([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)

    // Create modal
    const [isCreateOpen, setIsCreateOpen] = useState(false)
    const [createForm, setCreateForm] = useState<FormData>(EMPTY_FORM)
    const [createError, setCreateError] = useState<string | null>(null)
    const [isSubmitting, setIsSubmitting] = useState(false)

    // Edit modal
    const [isEditOpen, setIsEditOpen] = useState(false)
    const [editingId, setEditingId] = useState<string | null>(null)
    const [editForm, setEditForm] = useState<FormData>(EMPTY_FORM)
    const [editError, setEditError] = useState<string | null>(null)
    const [isSaving, setIsSaving] = useState(false)

    // Candidates modal
    const [candidatesVoting, setCandidatesVoting] = useState<VotingResponse | null>(null)
    const [candidates, setCandidates] = useState<CandidateResponse[]>([])
    const [candidatesLoading, setCandidatesLoading] = useState(false)
    const [newName, setNewName] = useState('')
    const [newDesc, setNewDesc] = useState('')
    const [addingCandidate, setAddingCandidate] = useState(false)
    const [addError, setAddError] = useState<string | null>(null)

    useEffect(() => { loadVotings() }, [])

    const loadVotings = async () => {
        try {
            setLoading(true)
            const data = await getVotings()
            setVotings(data.votings ?? [])
        } catch (err: any) {
            setError(err?.response?.data?.message ?? 'Не удалось загрузить голосования')
        } finally {
            setLoading(false)
        }
    }

    // ── Create ─────────────────────────────────────────────
    const openCreate = () => {
        setCreateForm(EMPTY_FORM)
        setCreateError(null)
        setIsCreateOpen(true)
    }

    const submitCreate = async (e: React.FormEvent) => {
        e.preventDefault()
        setCreateError(null)
        if (!createForm.title.trim()) { setCreateError('Укажите название голосования'); return }
        setIsSubmitting(true)
        try {
            const created = await createVoting(formToPayload(createForm))
            setVotings(prev => [created, ...prev])
            setIsCreateOpen(false)
        } catch (err: any) {
            setCreateError(err?.response?.data?.message ?? 'Не удалось создать голосование')
        } finally {
            setIsSubmitting(false)
        }
    }

    // ── Edit ───────────────────────────────────────────────
    const openEdit = (voting: VotingResponse) => {
        setEditingId(voting.id)
        setEditForm(votingToForm(voting))
        setEditError(null)
        setIsEditOpen(true)
    }

    const submitEdit = async (e: React.FormEvent) => {
        e.preventDefault()
        setEditError(null)
        if (!editForm.title.trim()) { setEditError('Укажите название голосования'); return }
        if (!editingId) return
        setIsSaving(true)
        try {
            const updated = await updateVoting(editingId, formToPayload(editForm))
            setVotings(prev => prev.map(v => v.id === editingId ? updated : v))
            setIsEditOpen(false)
        } catch (err: any) {
            setEditError(err?.response?.data?.message ?? 'Не удалось сохранить изменения')
        } finally {
            setIsSaving(false)
        }
    }

    // ── Candidates ─────────────────────────────────────────
    const openCandidates = async (voting: VotingResponse) => {
        setCandidatesVoting(voting)
        setCandidates([])
        setNewName('')
        setNewDesc('')
        setAddError(null)
        setCandidatesLoading(true)
        try {
            const data = await getCandidates(voting.id)
            setCandidates(data.candidates ?? [])
        } finally {
            setCandidatesLoading(false)
        }
    }

    const closeCandidates = () => setCandidatesVoting(null)

    const submitAddCandidate = async (e: React.FormEvent) => {
        e.preventDefault()
        setAddError(null)
        if (!newName.trim()) { setAddError('Укажите имя кандидата'); return }
        if (!candidatesVoting) return
        setAddingCandidate(true)
        try {
            const created = await createCandidate(candidatesVoting.id, newName.trim(), newDesc.trim())
            setCandidates(prev => [...prev, created])
            setNewName('')
            setNewDesc('')
        } catch (err: any) {
            setAddError(err?.response?.data?.message ?? 'Не удалось добавить кандидата')
        } finally {
            setAddingCandidate(false)
        }
    }

    const handleDeleteCandidate = async (candidateId: string) => {
        if (!candidatesVoting) return
        try {
            await deleteCandidate(candidatesVoting.id, candidateId)
            setCandidates(prev => prev.filter(c => c.id !== candidateId))
        } catch (err) {
            console.error(err)
        }
    }

    return (
        <div className="space-y-8">

            {/* Header */}
            <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                    <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary/10 text-primary">
                        <Vote className="h-5 w-5" />
                    </div>
                    <div>
                        <h1 className="text-2xl font-bold tracking-tight text-foreground">
                            Управление голосованиями
                        </h1>
                        <p className="text-sm text-muted-foreground">
                            Создание и редактирование голосований
                        </p>
                    </div>
                </div>
                <button
                    onClick={openCreate}
                    className="flex items-center gap-2 rounded-xl bg-primary px-4 py-2.5 text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/20 transition-all hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2"
                >
                    <Plus className="h-4 w-4" />
                    Создать
                </button>
            </div>

            {/* Table */}
            <section className="rounded-2xl border border-border bg-card shadow-sm overflow-hidden">
                <div className="border-b border-border px-6 py-4 flex items-center justify-between">
                    <h2 className="text-base font-semibold text-card-foreground">Голосования</h2>
                    {!loading && (
                        <span className="rounded-full bg-secondary px-2.5 py-0.5 text-xs font-semibold text-secondary-foreground">
                            {votings.length}
                        </span>
                    )}
                </div>

                {loading ? (
                    <div className="px-6 py-10 text-sm text-muted-foreground">Загрузка...</div>
                ) : error ? (
                    <div className="px-6 py-10 text-sm text-destructive">{error}</div>
                ) : votings.length === 0 ? (
                    <div className="flex flex-col items-center gap-3 py-20 text-center">
                        <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-muted">
                            <Vote className="h-7 w-7 text-muted-foreground" />
                        </div>
                        <p className="text-sm font-semibold text-foreground">Нет голосований</p>
                        <p className="text-xs text-muted-foreground">Создайте первое голосование, нажав кнопку выше</p>
                    </div>
                ) : (
                    <div className="overflow-x-auto">
                        <table className="min-w-full">
                            <thead className="bg-muted/40">
                                <tr>
                                    {['Название', 'Тип', 'Статус', 'Период', ''].map(h => (
                                        <th key={h} className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                                            {h}
                                        </th>
                                    ))}
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-border">
                                {votings.map(voting => {
                                    const status = STATUS_CONFIG[voting.status] ?? STATUS_CONFIG.Draft
                                    const hasCandidates = voting.type !== 'OpenAnswer'
                                    return (
                                        <tr key={voting.id} className="hover:bg-muted/20 transition-colors">
                                            <td className="px-5 py-3.5 text-sm font-medium text-foreground max-w-xs truncate">
                                                {voting.title}
                                            </td>
                                            <td className="px-5 py-3.5 text-sm text-muted-foreground whitespace-nowrap">
                                                {TYPE_LABELS[voting.type] ?? voting.type}
                                            </td>
                                            <td className="px-5 py-3.5">
                                                <span className={`flex w-fit items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold ${status.className}`}>
                                                    {status.icon}
                                                    {status.label}
                                                </span>
                                            </td>
                                            <td className="px-5 py-3.5 text-sm text-muted-foreground whitespace-nowrap">
                                                {voting.startTime
                                                    ? `${new Date(voting.startTime).toLocaleDateString('ru-RU')} — ${new Date(voting.endTime).toLocaleDateString('ru-RU')}`
                                                    : '—'
                                                }
                                            </td>
                                            <td className="px-5 py-3.5 text-right">
                                                <div className="flex items-center justify-end gap-2">
                                                    {hasCandidates && (
                                                        <button
                                                            onClick={() => openCandidates(voting)}
                                                            className="flex items-center gap-1.5 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-foreground transition-colors hover:bg-secondary hover:border-primary/30"
                                                        >
                                                            <Users className="h-3.5 w-3.5" />
                                                            Кандидаты
                                                        </button>
                                                    )}
                                                    <button
                                                        onClick={() => openEdit(voting)}
                                                        className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-foreground transition-colors hover:bg-secondary hover:border-primary/30"
                                                    >
                                                        Изменить
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    )
                                })}
                            </tbody>
                        </table>
                    </div>
                )}
            </section>

            {/* Create modal */}
            {isCreateOpen && (
                <Modal title="Новое голосование" onClose={() => setIsCreateOpen(false)}>
                    <VotingForm
                        form={createForm}
                        onChange={setCreateForm}
                        onSubmit={submitCreate}
                        onCancel={() => setIsCreateOpen(false)}
                        error={createError}
                        submitting={isSubmitting}
                        submitLabel="Создать"
                    />
                </Modal>
            )}

            {/* Edit modal */}
            {isEditOpen && (
                <Modal title="Редактировать голосование" onClose={() => setIsEditOpen(false)}>
                    <VotingForm
                        form={editForm}
                        onChange={setEditForm}
                        onSubmit={submitEdit}
                        onCancel={() => setIsEditOpen(false)}
                        error={editError}
                        submitting={isSaving}
                        submitLabel="Сохранить"
                    />
                </Modal>
            )}

            {/* Candidates modal */}
            {candidatesVoting && (
                <Modal
                    title={`Кандидаты — ${candidatesVoting.title}`}
                    onClose={closeCandidates}
                >
                    <div className="space-y-5">

                        {/* Candidate list */}
                        {candidatesLoading ? (
                            <div className="text-sm text-muted-foreground">Загрузка...</div>
                        ) : candidates.length === 0 ? (
                            <div className="rounded-xl border border-dashed border-border py-8 text-center text-sm text-muted-foreground">
                                Кандидатов пока нет
                            </div>
                        ) : (
                            <ul className="space-y-2">
                                {candidates.map(c => (
                                    <li
                                        key={c.id}
                                        className="flex items-center justify-between gap-3 rounded-xl border border-border bg-background px-4 py-3"
                                    >
                                        <div className="min-w-0">
                                            <p className="text-sm font-medium text-foreground truncate">{c.name}</p>
                                            {c.description && (
                                                <p className="text-xs text-muted-foreground truncate mt-0.5">{c.description}</p>
                                            )}
                                        </div>
                                        <button
                                            onClick={() => handleDeleteCandidate(c.id)}
                                            className="shrink-0 flex h-7 w-7 items-center justify-center rounded-lg text-muted-foreground transition-colors hover:bg-destructive/10 hover:text-destructive"
                                            title="Удалить"
                                        >
                                            <Trash2 className="h-3.5 w-3.5" />
                                        </button>
                                    </li>
                                ))}
                            </ul>
                        )}

                        {/* Add candidate form */}
                        <form onSubmit={submitAddCandidate} className="space-y-3 border-t border-border pt-4">
                            <p className="text-sm font-medium text-foreground">Добавить кандидата</p>
                            <Field label="Имя *">
                                <input
                                    className={inputClass}
                                    placeholder="Иван Иванов"
                                    value={newName}
                                    onChange={e => setNewName(e.target.value)}
                                />
                            </Field>
                            <Field label="Описание">
                                <input
                                    className={inputClass}
                                    placeholder="Краткое описание..."
                                    value={newDesc}
                                    onChange={e => setNewDesc(e.target.value)}
                                />
                            </Field>

                            {addError && (
                                <div className="rounded-xl border border-destructive/20 bg-destructive/10 px-3.5 py-2.5 text-sm font-medium text-destructive">
                                    {addError}
                                </div>
                            )}

                            <div className="flex justify-end gap-2">
                                <button
                                    type="button"
                                    onClick={closeCandidates}
                                    className="rounded-xl border border-border px-4 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
                                >
                                    Закрыть
                                </button>
                                <button
                                    type="submit"
                                    disabled={addingCandidate}
                                    className="flex items-center gap-2 rounded-xl bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/20 transition-all hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-50"
                                >
                                    <Plus className="h-3.5 w-3.5" />
                                    {addingCandidate ? 'Добавляем...' : 'Добавить'}
                                </button>
                            </div>
                        </form>

                    </div>
                </Modal>
            )}

        </div>
    )
}

// ── Shared voting form ─────────────────────────────────────

function VotingForm({
    form, onChange, onSubmit, onCancel, error, submitting, submitLabel,
}: {
    form: FormData
    onChange: (f: FormData) => void
    onSubmit: (e: React.FormEvent) => void
    onCancel: () => void
    error: string | null
    submitting: boolean
    submitLabel: string
}) {
    const set = <K extends keyof FormData>(key: K, value: FormData[K]) =>
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

// ── UI helpers ─────────────────────────────────────────────

function Modal({ title, onClose, children }: {
    title: string
    onClose: () => void
    children: React.ReactNode
}) {
    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/20 backdrop-blur-sm">
            <div className="w-full max-w-lg rounded-2xl border border-border bg-card p-6 shadow-xl max-h-[90vh] overflow-y-auto">
                <div className="mb-5 flex items-center justify-between">
                    <h2 className="text-lg font-semibold text-card-foreground">{title}</h2>
                    <button
                        onClick={onClose}
                        className="flex h-7 w-7 items-center justify-center rounded-lg text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
                    >
                        <X className="h-4 w-4" />
                    </button>
                </div>
                {children}
            </div>
        </div>
    )
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
    return (
        <div className="space-y-1.5">
            <label className="block text-sm font-medium text-foreground">{label}</label>
            {children}
        </div>
    )
}

function CheckboxField({ label, checked, onChange }: {
    label: string
    checked: boolean
    onChange: (v: boolean) => void
}) {
    return (
        <label className="flex items-center gap-2.5 cursor-pointer select-none">
            <input
                type="checkbox"
                checked={checked}
                onChange={e => onChange(e.target.checked)}
                className="h-4 w-4 rounded border-border text-primary accent-primary cursor-pointer"
            />
            <span className="text-sm text-foreground">{label}</span>
        </label>
    )
}
