import { useEffect, useRef, useState } from 'react'
import { Camera, Loader2, Plus, Trash2, UserRound, X } from 'lucide-react'
import Modal from '@/components/ui/Modal'
import { Field } from '@/components/ui/Field'
import {
    getCandidates,
    createCandidate,
    deleteCandidate,
    uploadCandidatePhoto,
    deleteCandidatePhoto,
    type CandidateResponse,
} from '@/api/candidateApi'
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
    const [photoLoading, setPhotoLoading] = useState<string | null>(null)

    const fileInputRefs = useRef<Record<string, HTMLInputElement | null>>({})

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

    const handleUploadPhoto = async (candidateId: string, file: File) => {
        setPhotoLoading(candidateId)
        try {
            const { photo_url } = await uploadCandidatePhoto(candidateId, file)
            setCandidates(prev =>
                prev.map(c => c.id === candidateId ? { ...c, photoUrl: photo_url } : c)
            )
        } catch (err) {
            console.error(err)
        } finally {
            setPhotoLoading(null)
        }
    }

    const handleDeletePhoto = async (candidateId: string) => {
        setPhotoLoading(candidateId)
        try {
            await deleteCandidatePhoto(candidateId)
            setCandidates(prev =>
                prev.map(c => c.id === candidateId ? { ...c, photoUrl: '' } : c)
            )
        } catch (err) {
            console.error(err)
        } finally {
            setPhotoLoading(null)
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
                        {candidates.map(c => {
                            const isPhotoLoading = photoLoading === c.id
                            const hasPhoto = Boolean(c.photoUrl)

                            return (
                                <li
                                    key={c.id}
                                    className="flex items-center gap-3 rounded-xl border border-border bg-background px-4 py-3"
                                >
                                    {/* Photo thumbnail */}
                                    <div className="relative shrink-0">
                                        {hasPhoto ? (
                                            <img
                                                src={c.photoUrl}
                                                alt={c.name}
                                                className="h-10 w-10 rounded-lg object-cover"
                                            />
                                        ) : (
                                            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-secondary">
                                                <UserRound className="h-5 w-5 text-muted-foreground" />
                                            </div>
                                        )}
                                        {isPhotoLoading && (
                                            <div className="absolute inset-0 flex items-center justify-center rounded-lg bg-background/70">
                                                <Loader2 className="h-4 w-4 animate-spin text-primary" />
                                            </div>
                                        )}
                                    </div>

                                    {/* Name + description */}
                                    <div className="min-w-0 flex-1">
                                        <p className="truncate text-sm font-medium text-foreground">{c.name}</p>
                                        {c.description && (
                                            <p className="mt-0.5 truncate text-xs text-muted-foreground">{c.description}</p>
                                        )}
                                    </div>

                                    {/* Actions */}
                                    <div className="flex shrink-0 items-center gap-1">
                                        {/* Upload photo */}
                                        <label
                                            title="Загрузить фото"
                                            className="flex h-7 w-7 cursor-pointer items-center justify-center rounded-lg text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
                                        >
                                            <Camera className="h-3.5 w-3.5" />
                                            <input
                                                type="file"
                                                accept="image/*"
                                                className="hidden"
                                                disabled={isPhotoLoading}
                                                ref={el => { fileInputRefs.current[c.id] = el }}
                                                onChange={e => {
                                                    const file = e.target.files?.[0]
                                                    if (file) handleUploadPhoto(c.id, file)
                                                    e.target.value = ''
                                                }}
                                            />
                                        </label>

                                        {/* Delete photo */}
                                        {hasPhoto && (
                                            <button
                                                onClick={() => handleDeletePhoto(c.id)}
                                                disabled={isPhotoLoading}
                                                title="Удалить фото"
                                                className="flex h-7 w-7 items-center justify-center rounded-lg text-muted-foreground transition-colors hover:bg-destructive/10 hover:text-destructive disabled:opacity-40"
                                            >
                                                <X className="h-3.5 w-3.5" />
                                            </button>
                                        )}

                                        {/* Delete candidate */}
                                        <button
                                            onClick={() => handleDelete(c.id)}
                                            disabled={isPhotoLoading}
                                            title="Удалить кандидата"
                                            className="flex h-7 w-7 items-center justify-center rounded-lg text-muted-foreground transition-colors hover:bg-destructive/10 hover:text-destructive disabled:opacity-40"
                                        >
                                            <Trash2 className="h-3.5 w-3.5" />
                                        </button>
                                    </div>
                                </li>
                            )
                        })}
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
