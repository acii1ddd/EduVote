import { useEffect, useState } from 'react'
import { Plus, Trash2 } from 'lucide-react'
import Modal from '@/components/ui/Modal'
import { Field } from '@/components/ui/Field'
import { getCandidates, createCandidate, deleteCandidate, type CandidateResponse } from '@/api/candidateApi'
import { inputClass } from './votingConstants'

interface Props {
    votingId: string
    votingTitle: string
    onClose: () => void
}

export default function CandidatesModal({ votingId, votingTitle, onClose }: Props) {
    const [candidates, setCandidates] = useState<CandidateResponse[]>([])
    const [loading, setLoading]       = useState(true)
    const [newName, setNewName]       = useState('')
    const [newDesc, setNewDesc]       = useState('')
    const [adding,  setAdding]        = useState(false)
    const [error,   setError]         = useState<string | null>(null)

    useEffect(() => {
        getCandidates(votingId)
            .then(d => setCandidates(d.candidates ?? []))
            .finally(() => setLoading(false))
    }, [votingId])

    const handleAdd = async (e: React.FormEvent) => {
        e.preventDefault()
        setError(null)
        if (!newName.trim()) { setError('Укажите имя кандидата'); return }
        setAdding(true)
        try {
            const created = await createCandidate(votingId, newName.trim(), newDesc.trim())
            setCandidates(prev => [...prev, created])
            setNewName('')
            setNewDesc('')
        } catch (err: any) {
            setError(err?.response?.data?.message ?? 'Не удалось добавить кандидата')
        } finally {
            setAdding(false)
        }
    }

    const handleDelete = async (id: string) => {
        try {
            await deleteCandidate(votingId, id)
            setCandidates(prev => prev.filter(c => c.id !== id))
        } catch (err) {
            console.error(err)
        }
    }

    return (
        <Modal title={`Кандидаты — ${votingTitle}`} onClose={onClose}>
            <div className="space-y-5">

                {loading ? (
                    <p className="text-sm text-muted-foreground">Загрузка...</p>
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
                                    onClick={() => handleDelete(c.id)}
                                    className="shrink-0 flex h-7 w-7 items-center justify-center rounded-lg text-muted-foreground transition-colors hover:bg-destructive/10 hover:text-destructive"
                                    title="Удалить"
                                >
                                    <Trash2 className="h-3.5 w-3.5" />
                                </button>
                            </li>
                        ))}
                    </ul>
                )}

                <form onSubmit={handleAdd} className="space-y-3 border-t border-border pt-4">
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

                    {error && (
                        <div className="rounded-xl border border-destructive/20 bg-destructive/10 px-3.5 py-2.5 text-sm font-medium text-destructive">
                            {error}
                        </div>
                    )}

                    <div className="flex justify-end gap-2">
                        <button
                            type="button"
                            onClick={onClose}
                            className="rounded-xl border border-border px-4 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
                        >
                            Закрыть
                        </button>
                        <button
                            type="submit"
                            disabled={adding}
                            className="flex items-center gap-2 rounded-xl bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/20 transition-all hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-50"
                        >
                            <Plus className="h-3.5 w-3.5" />
                            {adding ? 'Добавляем...' : 'Добавить'}
                        </button>
                    </div>
                </form>

            </div>
        </Modal>
    )
}
