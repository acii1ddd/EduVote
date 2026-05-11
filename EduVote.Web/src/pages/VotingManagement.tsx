import { useEffect, useState } from 'react'
import { Plus, Vote } from 'lucide-react'
import {
    getVotings, createVoting, updateVoting,
    deleteVoting, startVoting, pauseVoting, finishVoting, approveVoting,
    type VotingResponse, type VotingFormPayload,
} from '@/api/votingApi'
import Modal from '@/components/ui/Modal'
import VotingCard from '@/components/voting/VotingCard'
import VotingForm, { type VotingFormData, EMPTY_VOTING_FORM } from '@/components/voting/VotingForm'
import CandidatesModal from '@/components/voting/CandidatesModal'
import VotingTargetsModal from '@/components/voting/VotingTargetsModal'
import { useAuth } from '@/context/AuthContext'

// ── Helpers ────────────────────────────────────────────────

const toIso = (local: string) => local ? new Date(local).toISOString() : ''

function formToPayload(f: VotingFormData): VotingFormPayload {
    return {
        title:           f.title,
        description:     f.description,
        type:            f.type,
        isAnonymous:     f.isAnonymous,
        allowVoteChange: f.allowVoteChange,
        startTime:       toIso(f.startTime),
        endTime:         toIso(f.endTime),
    }
}

function votingToForm(v: VotingResponse): VotingFormData {
    return {
        title:           v.title,
        description:     v.description,
        type:            v.type,
        isAnonymous:     v.isAnonymous,
        allowVoteChange: v.allowVoteChange,
        startTime:       v.startTime ? v.startTime.slice(0, 16) : '',
        endTime:         v.endTime   ? v.endTime.slice(0, 16)   : '',
    }
}

// ── Page ───────────────────────────────────────────────────

type ModalState =
    | { kind: 'none' }
    | { kind: 'create' }
    | { kind: 'edit';       voting: VotingResponse }
    | { kind: 'candidates'; voting: VotingResponse }
    | { kind: 'targets';    voting: VotingResponse }

export default function VotingManagement() {
    const { claims } = useAuth()
    const userRole = claims?.role ?? ''

    const [votings, setVotings] = useState<VotingResponse[]>([])
    const [loading, setLoading] = useState(true)
    const [error,   setError]   = useState<string | null>(null)
    const [busyId,  setBusyId]  = useState<string | null>(null)
    const [modal,   setModal]   = useState<ModalState>({ kind: 'none' })

    const [formData,    setFormData]    = useState<VotingFormData>(EMPTY_VOTING_FORM)
    const [formError,   setFormError]   = useState<string | null>(null)
    const [formBusy,    setFormBusy]    = useState(false)

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

    const closeModal = () => setModal({ kind: 'none' })

    // ── Create ──────────────────────────────────────────────
    const openCreate = () => {
        setFormData(EMPTY_VOTING_FORM)
        setFormError(null)
        setModal({ kind: 'create' })
    }

    const submitCreate = async (e: React.FormEvent) => {
        e.preventDefault()
        setFormError(null)
        if (!formData.title.trim()) { setFormError('Укажите название голосования'); return }
        setFormBusy(true)
        try {
            const created = await createVoting(formToPayload(formData))
            setVotings(prev => [created, ...prev])
            closeModal()
        } catch (err: any) {
            setFormError(err?.response?.data?.message ?? 'Не удалось создать голосование')
        } finally {
            setFormBusy(false)
        }
    }

    // ── Edit ────────────────────────────────────────────────
    const openEdit = (voting: VotingResponse) => {
        setFormData(votingToForm(voting))
        setFormError(null)
        setModal({ kind: 'edit', voting })
    }

    const submitEdit = async (e: React.FormEvent) => {
        e.preventDefault()
        setFormError(null)
        if (!formData.title.trim()) { setFormError('Укажите название голосования'); return }
        if (modal.kind !== 'edit') return
        setFormBusy(true)
        try {
            const updated = await updateVoting(modal.voting.id, formToPayload(formData))
            setVotings(prev => prev.map(v => v.id === modal.voting.id ? updated : v))
            closeModal()
        } catch (err: any) {
            setFormError(err?.response?.data?.message ?? 'Не удалось сохранить изменения')
        } finally {
            setFormBusy(false)
        }
    }

    // ── Lifecycle ────────────────────────────────────────────
    const runAction = async (voting: VotingResponse, action: (id: string) => Promise<void>) => {
        setBusyId(voting.id)
        try {
            await action(voting.id)
            await loadVotings()
        } catch (err) {
            console.error(err)
        } finally {
            setBusyId(null)
        }
    }

    const handleDelete = async (voting: VotingResponse) => {
        if (!window.confirm(`Удалить голосование «${voting.title}»?`)) return
        setBusyId(voting.id)
        try {
            await deleteVoting(voting.id)
            setVotings(prev => prev.filter(v => v.id !== voting.id))
        } catch (err) {
            console.error(err)
        } finally {
            setBusyId(null)
        }
    }

    // ── Render ───────────────────────────────────────────────
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

            {/* Content */}
            {loading ? (
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Vote className="h-4 w-4 animate-pulse text-primary" />
                    Загрузка...
                </div>
            ) : error ? (
                <div className="rounded-xl border border-destructive/20 bg-destructive/10 px-4 py-3 text-sm font-medium text-destructive">
                    {error}
                </div>
            ) : votings.length === 0 ? (
                <div className="flex flex-col items-center gap-3 rounded-2xl border border-dashed border-border bg-card py-24 text-center">
                    <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-muted">
                        <Vote className="h-7 w-7 text-muted-foreground" />
                    </div>
                    <p className="text-sm font-semibold text-foreground">Нет голосований</p>
                    <p className="text-xs text-muted-foreground">Создайте первое голосование, нажав кнопку выше</p>
                </div>
            ) : (
                <div className="grid gap-4 sm:grid-cols-2">
                    {votings.map(voting => (
                        <VotingCard
                            key={voting.id}
                            voting={voting}
                            userRole={userRole}
                            busy={busyId === voting.id}
                            onEdit={() => openEdit(voting)}
                            onCandidates={() => setModal({ kind: 'candidates', voting })}
                            onTargets={() => setModal({ kind: 'targets', voting })}
                            onStart={() => runAction(voting, startVoting)}
                            onPause={() => runAction(voting, pauseVoting)}
                            onFinish={() => runAction(voting, finishVoting)}
                            onDelete={() => handleDelete(voting)}
                            onApprove={() => runAction(voting, approveVoting)}
                        />
                    ))}
                </div>
            )}

            {/* Create modal */}
            {modal.kind === 'create' && (
                <Modal title="Новое голосование" onClose={closeModal}>
                    <VotingForm
                        form={formData}
                        onChange={setFormData}
                        onSubmit={submitCreate}
                        onCancel={closeModal}
                        error={formError}
                        submitting={formBusy}
                        submitLabel="Создать"
                    />
                </Modal>
            )}

            {/* Edit modal */}
            {modal.kind === 'edit' && (
                <Modal title="Редактировать голосование" onClose={closeModal}>
                    <VotingForm
                        form={formData}
                        onChange={setFormData}
                        onSubmit={submitEdit}
                        onCancel={closeModal}
                        error={formError}
                        submitting={formBusy}
                        submitLabel="Сохранить"
                    />
                </Modal>
            )}

            {/* Candidates modal */}
            {modal.kind === 'candidates' && (
                <CandidatesModal
                    votingId={modal.voting.id}
                    votingTitle={modal.voting.title}
                    onClose={closeModal}
                />
            )}

            {/* Targets modal */}
            {modal.kind === 'targets' && (
                <VotingTargetsModal
                    votingId={modal.voting.id}
                    votingTitle={modal.voting.title}
                    onClose={closeModal}
                />
            )}

        </div>
    )
}
