import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { ArrowLeft, Calendar, UserRound, Vote } from 'lucide-react'
import { getVotingById, type VotingResponse } from '@/api/votingApi'
import { getCandidates, type CandidateResponse } from '@/api/candidateApi'
import { STATUS_CONFIG, TYPE_LABELS } from '@/components/voting/votingConstants'

export default function VotingDetail() {
    const { id } = useParams<{ id: string }>()
    const navigate = useNavigate()

    const [voting, setVoting]       = useState<VotingResponse | null>(null)
    const [candidates, setCandidates] = useState<CandidateResponse[]>([])
    const [loading, setLoading]     = useState(true)
    const [error, setError]         = useState<string | null>(null)

    // Selection state for all voting types
    const [singleId, setSingleId]   = useState<string | null>(null)
    const [multiIds, setMultiIds]   = useState<Set<string>>(new Set())
    const [ratings, setRatings]     = useState<Record<string, number>>({})
    const [openText, setOpenText]   = useState('')

    useEffect(() => {
        if (!id) return
        Promise.all([getVotingById(id), getCandidates(id)])
            .then(([v, c]) => {
                setVoting(v)
                setCandidates(c.candidates ?? [])
            })
            .catch(err => {
                const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
                setError(msg ?? 'Не удалось загрузить голосование')
            })
            .finally(() => setLoading(false))
    }, [id])

    const toggleMulti = (candidateId: string) =>
        setMultiIds(prev => {
            const next = new Set(prev)
            if (next.has(candidateId)) next.delete(candidateId)
            else next.add(candidateId)
            return next
        })

    const setStars = (candidateId: string, stars: number) =>
        setRatings(prev => ({ ...prev, [candidateId]: stars }))

    const canVote = (() => {
        if (!voting) return false
        switch (voting.type) {
            case 'SingleChoice':   return singleId !== null
            case 'MultipleChoice': return multiIds.size > 0
            case 'Rating':         return candidates.length > 0 && candidates.every(c => (ratings[c.id] ?? 0) > 0)
            case 'OpenAnswer':     return openText.trim().length > 0
            default:               return false
        }
    })()

    const handleVote = () => {
        if (!voting) return
        switch (voting.type) {
            case 'SingleChoice':
                console.log('Vote:', { votingId: voting.id, type: 'SingleChoice', candidateId: singleId })
                break
            case 'MultipleChoice':
                console.log('Vote:', { votingId: voting.id, type: 'MultipleChoice', candidateIds: [...multiIds] })
                break
            case 'Rating':
                console.log('Vote:', { votingId: voting.id, type: 'Rating', ratings })
                break
            case 'OpenAnswer':
                console.log('Vote:', { votingId: voting.id, type: 'OpenAnswer', answer: openText })
                break
        }
    }

    if (loading) return (
        <div className="flex items-center gap-2 text-sm text-muted-foreground py-16 justify-center">
            <Vote className="h-4 w-4 animate-pulse text-primary" />
            Загрузка...
        </div>
    )

    if (error || !voting) return (
        <div className="space-y-4">
            <button onClick={() => navigate(-1)} className="flex items-center gap-1.5 text-sm text-muted-foreground transition-colors hover:text-foreground">
                <ArrowLeft className="h-4 w-4" />
                Назад
            </button>
            <div className="rounded-xl border border-destructive/20 bg-destructive/10 px-4 py-3 text-sm font-medium text-destructive">
                {error ?? 'Голосование не найдено'}
            </div>
        </div>
    )

    const cfg = STATUS_CONFIG[voting.status] ?? STATUS_CONFIG.Draft
    const fmt = (iso: string) => iso ? new Date(iso).toLocaleDateString('ru-RU') : '—'

    return (
        <div className="space-y-8">

            {/* Back */}
            <button
                onClick={() => navigate(-1)}
                className="flex items-center gap-1.5 text-sm font-medium text-muted-foreground transition-colors hover:text-foreground"
            >
                <ArrowLeft className="h-4 w-4" />
                Назад
            </button>

            {/* Header */}
            <div className="space-y-4">
                <div className="flex flex-wrap items-start gap-3">
                    <h1 className="flex-1 text-2xl font-bold tracking-tight text-foreground min-w-0">
                        {voting.title}
                    </h1>
                    <span className={`flex shrink-0 items-center gap-1.5 rounded-full px-3 py-1.5 text-xs font-semibold ${cfg.badgeClass}`}>
                        {cfg.icon}
                        {cfg.label}
                    </span>
                </div>

                {voting.description && (
                    <p className="text-sm leading-relaxed text-muted-foreground">
                        {voting.description}
                    </p>
                )}

                <div className="flex flex-wrap gap-2 text-xs">
                    <span className="rounded-md bg-secondary px-2.5 py-1 font-medium text-secondary-foreground">
                        {TYPE_LABELS[voting.type]}
                    </span>
                    {voting.isAnonymous && (
                        <span className="rounded-md bg-secondary px-2.5 py-1 font-medium text-secondary-foreground">
                            Анонимное
                        </span>
                    )}
                    {(voting.startTime || voting.endTime) && (
                        <span className="flex items-center gap-1.5 text-muted-foreground">
                            <Calendar className="h-3.5 w-3.5" />
                            {fmt(voting.startTime)} — {fmt(voting.endTime)}
                        </span>
                    )}
                </div>
            </div>

            {/* Voting area */}
            {voting.type === 'OpenAnswer' ? (
                <OpenAnswerSection text={openText} onChange={setOpenText} />
            ) : (
                <CandidatesSection
                    type={voting.type}
                    candidates={candidates}
                    singleId={singleId}
                    multiIds={multiIds}
                    ratings={ratings}
                    onSelectSingle={setSingleId}
                    onToggleMulti={toggleMulti}
                    onSetStars={setStars}
                />
            )}

            {/* Vote button */}
            <div className="flex justify-end pt-2">
                <button
                    onClick={handleVote}
                    disabled={!canVote}
                    className="flex items-center gap-2 rounded-xl bg-primary px-6 py-2.5 text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/20 transition-all hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-40"
                >
                    <Vote className="h-4 w-4" />
                    Проголосовать
                </button>
            </div>

        </div>
    )
}

// ── Open answer ────────────────────────────────────────────

function OpenAnswerSection({ text, onChange }: { text: string; onChange: (v: string) => void }) {
    return (
        <div className="space-y-2">
            <h2 className="text-sm font-semibold text-foreground">Ваш ответ</h2>
            <textarea
                className="w-full resize-none rounded-xl border border-border bg-background px-4 py-3 text-sm text-foreground placeholder:text-muted-foreground transition-colors focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                rows={5}
                placeholder="Введите ваш ответ..."
                value={text}
                onChange={e => onChange(e.target.value)}
            />
        </div>
    )
}

// ── Candidates section ─────────────────────────────────────

interface CandidatesSectionProps {
    type: 'SingleChoice' | 'MultipleChoice' | 'Rating'
    candidates: CandidateResponse[]
    singleId: string | null
    multiIds: Set<string>
    ratings: Record<string, number>
    onSelectSingle: (id: string) => void
    onToggleMulti: (id: string) => void
    onSetStars: (id: string, stars: number) => void
}

function CandidatesSection({
    type, candidates, singleId, multiIds, ratings,
    onSelectSingle, onToggleMulti, onSetStars,
}: CandidatesSectionProps) {
    const hint =
        type === 'SingleChoice'   ? 'Выберите одного кандидата' :
        type === 'MultipleChoice' ? 'Выберите одного или нескольких кандидатов' :
                                    'Оцените каждого кандидата'

    if (candidates.length === 0) return (
        <div className="rounded-2xl border border-dashed border-border py-16 text-center text-sm text-muted-foreground">
            Кандидатов ещё нет
        </div>
    )

    return (
        <div className="space-y-4">
            <h2 className="text-sm font-semibold text-foreground">{hint}</h2>
            <div className="space-y-3">
                {candidates.map(c => (
                    <CandidateCard
                        key={c.id}
                        candidate={c}
                        type={type}
                        selected={type === 'SingleChoice' ? singleId === c.id : multiIds.has(c.id)}
                        stars={ratings[c.id] ?? 0}
                        onSelect={() =>
                            type === 'SingleChoice' ? onSelectSingle(c.id) : onToggleMulti(c.id)
                        }
                        onSetStars={stars => onSetStars(c.id, stars)}
                    />
                ))}
            </div>
        </div>
    )
}

// ── Single candidate card ──────────────────────────────────

interface CandidateCardProps {
    candidate: CandidateResponse
    type: 'SingleChoice' | 'MultipleChoice' | 'Rating'
    selected: boolean
    stars: number
    onSelect: () => void
    onSetStars: (stars: number) => void
}

function CandidateCard({ candidate: c, type, selected, stars, onSelect, onSetStars }: CandidateCardProps) {
    const isClickable = type === 'SingleChoice' || type === 'MultipleChoice'

    return (
        <div
            onClick={isClickable ? onSelect : undefined}
            className={[
                'flex flex-col sm:flex-row gap-4 rounded-2xl border-2 p-4 transition-all',
                isClickable ? 'cursor-pointer' : '',
                selected
                    ? 'border-primary bg-primary/5'
                    : isClickable
                        ? 'border-border bg-card hover:border-primary/30 hover:bg-primary/[0.02]'
                        : 'border-border bg-card',
            ].join(' ')}
        >
            {/* Photo */}
            <div className="mx-auto sm:mx-0 h-36 w-36 shrink-0 overflow-hidden rounded-xl bg-secondary">
                {c.photoUrl ? (
                    <img src={c.photoUrl} alt={c.name} className="h-full w-full object-cover" />
                ) : (
                    <div className="flex h-full w-full items-center justify-center">
                        <UserRound className="h-12 w-12 text-muted-foreground" />
                    </div>
                )}
            </div>

            {/* Info */}
            <div className="flex flex-1 flex-col justify-center gap-2">

                {/* Name with selection indicator */}
                <div className="flex items-center gap-2.5">
                    {type === 'SingleChoice' && (
                        <RadioIndicator selected={selected} />
                    )}
                    {type === 'MultipleChoice' && (
                        <CheckboxIndicator selected={selected} />
                    )}
                    <p className="text-base font-semibold text-foreground">{c.name}</p>
                </div>

                {c.description && (
                    <p className="text-sm leading-relaxed text-muted-foreground">{c.description}</p>
                )}

                {type === 'Rating' && (
                    <StarRating stars={stars} onRate={onSetStars} />
                )}
            </div>
        </div>
    )
}

// ── Sub-components ─────────────────────────────────────────

function RadioIndicator({ selected }: { selected: boolean }) {
    return (
        <div className={`flex h-4 w-4 shrink-0 items-center justify-center rounded-full border-2 transition-colors ${selected ? 'border-primary bg-primary' : 'border-muted-foreground/40'}`}>
            {selected && <div className="h-1.5 w-1.5 rounded-full bg-primary-foreground" />}
        </div>
    )
}

function CheckboxIndicator({ selected }: { selected: boolean }) {
    return (
        <div className={`flex h-4 w-4 shrink-0 items-center justify-center rounded border-2 transition-colors ${selected ? 'border-primary bg-primary' : 'border-muted-foreground/40'}`}>
            {selected && <span className="text-[10px] font-bold leading-none text-primary-foreground">✓</span>}
        </div>
    )
}

function StarRating({ stars, onRate }: { stars: number; onRate: (n: number) => void }) {
    return (
        <div className="flex items-center gap-1 pt-1">
            {[1, 2, 3, 4, 5].map(n => (
                <button
                    key={n}
                    type="button"
                    onClick={() => onRate(n)}
                    className={`text-2xl leading-none transition-all hover:scale-110 focus:outline-none ${stars >= n ? 'text-amber-400' : 'text-muted-foreground/25 hover:text-amber-300'}`}
                >
                    ★
                </button>
            ))}
            {stars > 0 && (
                <span className="ml-1.5 text-sm text-muted-foreground">{stars}/5</span>
            )}
        </div>
    )
}
