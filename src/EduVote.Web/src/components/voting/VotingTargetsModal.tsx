import { useEffect, useState } from 'react'
import { Info, Plus, Trash2 } from 'lucide-react'
import Modal from '@/components/ui/Modal'
import { Field } from '@/components/ui/Field'
import { getTargets, addTarget, deleteTarget, type VotingTargetResponse } from '@/api/votingTargetApi'
import { getEducationUnits, type EducationUnit } from '@/api/educationUnitApi'
import { inputClass } from './votingConstants'

interface Props {
    votingId: string
    votingTitle: string
    onClose: () => void
}

export default function VotingTargetsModal({ votingId, votingTitle, onClose }: Props) {
    const [targets,        setTargets]        = useState<VotingTargetResponse[]>([])
    const [units,          setUnits]          = useState<EducationUnit[]>([])
    const [loading,        setLoading]        = useState(true)
    const [selectedUnitId, setSelectedUnitId] = useState('')
    const [adding,         setAdding]         = useState(false)
    const [error,          setError]          = useState<string | null>(null)

    useEffect(() => {
        Promise.all([
            getTargets(votingId),
            getEducationUnits(),
        ]).then(([targetsData, unitsData]) => {
            setTargets(targetsData.targets ?? [])
            setUnits(unitsData.educationUnits ?? [])
        }).finally(() => setLoading(false))
    }, [votingId])

    const unitById = (id: string) => units.find(u => u.id === id)

    const availableUnits = units.filter(u => !targets.some(t => t.educationUnitId === u.id))

    const handleAdd = async (e: React.FormEvent) => {
        e.preventDefault()
        setError(null)
        if (!selectedUnitId) { setError('Выберите учебную единицу'); return }
        setAdding(true)
        try {
            const created = await addTarget(votingId, selectedUnitId)
            setTargets(prev => [...prev, created])
            setSelectedUnitId('')
        } catch (err: any) {
            setError(err?.response?.data?.message ?? 'Не удалось добавить таргет')
        } finally {
            setAdding(false)
        }
    }

    const handleDelete = async (educationUnitId: string) => {
        try {
            await deleteTarget(votingId, educationUnitId)
            setTargets(prev => prev.filter(t => t.educationUnitId !== educationUnitId))
        } catch (err) {
            console.error(err)
        }
    }

    return (
        <Modal title={`Таргетинг — ${votingTitle}`} onClose={onClose}>
            <div className="space-y-5">

                {/* Info banner */}
                <div className="flex gap-2.5 rounded-xl border border-border bg-muted/40 px-4 py-3 text-sm text-muted-foreground">
                    <Info className="h-4 w-4 shrink-0 mt-0.5" />
                    <span>
                        Если таргеты не заданы — голосование доступно всем пользователям системы.
                        Таргет задаёт учебную единицу, участники которой смогут проголосовать.
                    </span>
                </div>

                {loading ? (
                    <p className="text-sm text-muted-foreground">Загрузка...</p>
                ) : targets.length === 0 ? (
                    <div className="rounded-xl border border-dashed border-border py-8 text-center text-sm text-muted-foreground">
                        Таргеты не заданы — голосование публичное
                    </div>
                ) : (
                    <ul className="space-y-2">
                        {targets.map(t => {
                            const unit = unitById(t.educationUnitId)
                            return (
                                <li
                                    key={t.educationUnitId}
                                    className="flex items-center justify-between gap-3 rounded-xl border border-border bg-background px-4 py-3"
                                >
                                    <div className="min-w-0">
                                        <p className="text-sm font-medium text-foreground truncate">
                                            {unit?.name ?? t.educationUnitId}
                                        </p>
                                        {unit?.type && (
                                            <p className="text-xs text-muted-foreground mt-0.5">{unit.type}</p>
                                        )}
                                    </div>
                                    <button
                                        onClick={() => handleDelete(t.educationUnitId)}
                                        className="shrink-0 flex h-7 w-7 items-center justify-center rounded-lg text-muted-foreground transition-colors hover:bg-destructive/10 hover:text-destructive"
                                        title="Удалить таргет"
                                    >
                                        <Trash2 className="h-3.5 w-3.5" />
                                    </button>
                                </li>
                            )
                        })}
                    </ul>
                )}

                {/* Add target form */}
                <form onSubmit={handleAdd} className="space-y-3 border-t border-border pt-4">
                    <p className="text-sm font-medium text-foreground">Добавить таргет</p>

                    <Field label="Учебная единица">
                        <select
                            className={inputClass}
                            value={selectedUnitId}
                            onChange={e => setSelectedUnitId(e.target.value)}
                            disabled={loading || availableUnits.length === 0}
                        >
                            <option value="">
                                {availableUnits.length === 0
                                    ? 'Все единицы уже добавлены'
                                    : 'Выберите учебную единицу...'
                                }
                            </option>
                            {availableUnits.map(u => (
                                <option key={u.id} value={u.id}>
                                    {u.name} ({u.type})
                                </option>
                            ))}
                        </select>
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
                            disabled={adding || availableUnits.length === 0}
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
