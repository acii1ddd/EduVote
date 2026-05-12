import { useEffect, useState } from 'react'
import { BookOpen, Plus, Vote } from 'lucide-react'
import ProfileCard from '@/components/ProfileCard'
import {
    getVotingsCreatedByUser, getVotingsForUser, createVoting, updateVoting,
    deleteVoting, startVoting, pauseVoting, finishVoting,
    sortVotingsFinishedLast,
    type VotingResponse, type VotingFormPayload,
} from '@/api/votingApi'
import { getUsers, type UserResponse } from '@/api/userApi'
import { getEducationUnits, type EducationUnit } from '@/api/educationUnitApi'
import { useAuth } from '@/context/AuthContext'
import VotingCard from '@/components/voting/VotingCard'
import StudentVotingCard from '@/components/student/StudentVotingCard'
import Modal from '@/components/ui/Modal'
import VotingForm, { type VotingFormData, EMPTY_VOTING_FORM } from '@/components/voting/VotingForm'
import CandidatesModal from '@/components/voting/CandidatesModal'
import VotingTargetsModal from '@/components/voting/VotingTargetsModal'

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

// ── Modal state ────────────────────────────────────────────

type ModalState =
    | { kind: 'none' }
    | { kind: 'create' }
    | { kind: 'edit';       voting: VotingResponse }
    | { kind: 'candidates'; voting: VotingResponse }
    | { kind: 'targets';    voting: VotingResponse }

// ── Component ──────────────────────────────────────────────

export default function TeacherDashboard() {
    const { claims } = useAuth()

    const [myVotings, setMyVotings]               = useState<VotingResponse[]>([])
    const [availableVotings, setAvailableVotings] = useState<VotingResponse[]>([])
    const [allUnits, setAllUnits]                 = useState<EducationUnit[]>([])
    const [userMap, setUserMap]                   = useState<Record<string, UserResponse>>({})
    const [facultyName, setFacultyName]           = useState<string | null>(null)
    const [loading, setLoading]                   = useState(true)
    const [error, setError]                       = useState<string | null>(null)

    const [busyId, setBusyId]         = useState<string | null>(null)
    const [modal, setModal]           = useState<ModalState>({ kind: 'none' })
    const [formData, setFormData]     = useState<VotingFormData>(EMPTY_VOTING_FORM)
    const [formError, setFormError]   = useState<string | null>(null)
    const [formBusy, setFormBusy]     = useState(false)

    const reloadMyVotings = async () => {
        try {
            const data = await getVotingsCreatedByUser()
            setMyVotings(sortVotingsFinishedLast(data.votings ?? []))
        } catch (err) {
            console.error(err)
        }
    }

    useEffect(() => {
        if (!claims?.nameid) return
        const userId = claims.nameid
        Promise.all([
            getVotingsCreatedByUser(),
            getVotingsForUser(userId),
            getUsers(),
            getEducationUnits(),
        ])
            .then(([myData, availableData, usersData, unitsData]) => {
                setMyVotings(sortVotingsFinishedLast(myData.votings ?? []))
                setAvailableVotings(sortVotingsFinishedLast(availableData.votings ?? []))
                setAllUnits(unitsData.educationUnits ?? [])
                const map: Record<string, UserResponse> = {}
                for (const u of usersData.users) map[u.id] = u
                setUserMap(map)
                const me = usersData.users.find(u => u.id === userId)
                if (me?.educationUnitName) setFacultyName(me.educationUnitName)
            })
            .catch(err => {
                const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
                setError(msg ?? 'Не удалось загрузить данные')
            })
            .finally(() => setLoading(false))
    }, [claims?.nameid])

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
            setMyVotings(prev => [created, ...prev])
            closeModal()
        } catch (err: unknown) {
            const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
            setFormError(msg ?? 'Не удалось создать голосование')
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
            setMyVotings(prev => prev.map(v => v.id === modal.voting.id ? updated : v))
            closeModal()
        } catch (err: unknown) {
            const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
            setFormError(msg ?? 'Не удалось сохранить изменения')
        } finally {
            setFormBusy(false)
        }
    }

    // ── Lifecycle ────────────────────────────────────────────
    const runAction = async (voting: VotingResponse, action: (id: string) => Promise<void>) => {
        setBusyId(voting.id)
        try {
            await action(voting.id)
            await reloadMyVotings()
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
            setMyVotings(prev => prev.filter(v => v.id !== voting.id))
            setAvailableVotings(prev => prev.filter(v => v.id !== voting.id))
        } catch (err) {
            console.error(err)
        } finally {
            setBusyId(null)
        }
    }

    // ── Render ───────────────────────────────────────────────
    return (
        <div className="space-y-10">

            {/* Header */}
            <div className="flex items-start justify-between gap-4">
                <div className="space-y-3">
                    <div>
                        <h1 className="text-2xl font-bold tracking-tight text-foreground">
                            Панель преподавателя
                        </h1>
                        {facultyName && (
                            <p className="mt-1 text-sm text-muted-foreground">
                                <BookOpen className="inline h-3.5 w-3.5 mr-1 align-text-bottom" />
                                Факультет{' '}
                                <span className="font-semibold text-foreground">{facultyName}</span>
                            </p>
                        )}
                    </div>

                    {claims && (
                        <ProfileCard fields={[
                            { label: 'Имя преподавателя', value: userMap[claims.nameid]?.name ?? '—' },
                            { label: 'Email',             value: userMap[claims.nameid]?.email ?? '—' },
                            { label: 'Ваша роль',         value: claims.role, highlight: true },
                        ]} />
                    )}
                </div>

                <button
                    onClick={openCreate}
                    className="flex shrink-0 items-center gap-2 rounded-xl bg-primary px-4 py-2.5 text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/20 transition-all hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2"
                >
                    <Plus className="h-4 w-4" />
                    Создать
                </button>
            </div>

            {/* Loading */}
            {loading && (
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Vote className="h-4 w-4 animate-pulse text-primary" />
                    Загрузка...
                </div>
            )}

            {/* Error */}
            {error && (
                <div className="rounded-xl border border-destructive/20 bg-destructive/10 px-4 py-3 text-sm font-medium text-destructive">
                    {error}
                </div>
            )}

            {/* My votings */}
            {!loading && !error && (
                <section className="space-y-3">
                    <h2 className="text-xs font-semibold uppercase tracking-widest text-muted-foreground">
                        Мои голосования
                    </h2>
                    {myVotings.length === 0 ? (
                        <div className="flex flex-col items-center gap-3 rounded-2xl border border-dashed border-border bg-card py-16 text-center">
                            <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-muted">
                                <Vote className="h-6 w-6 text-muted-foreground" />
                            </div>
                            <p className="text-sm font-semibold text-foreground">Нет голосований</p>
                            <p className="text-xs text-muted-foreground">Создайте первое голосование, нажав кнопку выше</p>
                        </div>
                    ) : (
                        <div className="grid gap-4 sm:grid-cols-2">
                            {myVotings.map(v => (
                                <VotingCard
                                    key={v.id}
                                    voting={v}
                                    userRole="Teacher"
                                    busy={busyId === v.id}
                                    createdByName={userMap[v.createdById]?.name ?? v.createdById}
                                    onEdit={() => openEdit(v)}
                                    onCandidates={() => setModal({ kind: 'candidates', voting: v })}
                                    onTargets={() => setModal({ kind: 'targets', voting: v })}
                                    onStart={() => runAction(v, startVoting)}
                                    onPause={() => runAction(v, pauseVoting)}
                                    onFinish={() => runAction(v, finishVoting)}
                                    onDelete={() => handleDelete(v)}
                                    onApprove={() => {}}
                                />
                            ))}
                        </div>
                    )}
                </section>
            )}

            {/* Available votings */}
            {!loading && !error && availableVotings.length > 0 && (
                <section className="space-y-3">
                    <h2 className="text-xs font-semibold uppercase tracking-widest text-muted-foreground">
                        Доступные голосования
                    </h2>
                    <div className="grid gap-4 sm:grid-cols-2">
                        {availableVotings.map(v => (
                            <StudentVotingCard
                                key={v.id}
                                voting={v}
                                allUnits={allUnits}
                                createdByName={userMap[v.createdById]?.name ?? v.createdById}
                            />
                        ))}
                    </div>
                </section>
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
