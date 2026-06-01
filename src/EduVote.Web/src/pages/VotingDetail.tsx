import { useEffect, useState } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { ArrowLeft, BarChart2, Calendar, Check, Copy, Lock, Unlock, UserRound, Vote } from 'lucide-react'
import { getVotingById, castVote, getVotedVotingIds, type VotingResponse } from '@/api/votingApi'
import { getCandidates, type CandidateResponse } from '@/api/candidateApi'
import { STATUS_CONFIG, TYPE_LABELS } from '@/components/voting/votingConstants'

export default function VotingDetail() {
    const { id } = useParams<{ id: string }>()
    const navigate = useNavigate()

    const [voting, setVoting]       = useState<VotingResponse | null>(null)
    const [candidates, setCandidates] = useState<CandidateResponse[]>([])
    const [loading, setLoading]         = useState(true)
    const [error, setError]             = useState<string | null>(null)
    const [submitting, setSubmitting]         = useState(false)
    const [voteError, setVoteError]           = useState<string | null>(null)
    const [voted, setVoted]                   = useState(false)
    const [justVoted, setJustVoted]           = useState(false)
    const [voteReceipt, setVoteReceipt]       = useState<{ voteId: string; voteHash: string; voteSalt: string } | null>(null)

    // Selection state for all voting types
    const [singleId, setSingleId]   = useState<string | null>(null)
    const [multiIds, setMultiIds]   = useState<Set<string>>(new Set())
    const [ratings, setRatings]     = useState<Record<string, number>>({})
    const [openText, setOpenText]   = useState('')

    useEffect(() => {
        if (!id) return
        Promise.all([getVotingById(id), getCandidates(id), getVotedVotingIds()])
            .then(([v, c, votedSet]) => {
                setVoting(v)
                setCandidates(c.candidates ?? [])
                setVoted(votedSet.has(id))
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

    const handleVote = async () => {
        if (!voting || !canVote || submitting) return
        setSubmitting(true)
        setVoteError(null)
        try {
            let receipt: Awaited<ReturnType<typeof castVote>> | undefined
            switch (voting.type) {
                case 'SingleChoice':
                    receipt = await castVote(voting.id, { selectedCandidateId: singleId! })
                    break
                case 'MultipleChoice':
                    receipt = await castVote(voting.id, { selectedCandidateIds: [...multiIds] })
                    break
                case 'Rating':
                    receipt = await castVote(voting.id, { ratingAnswers: ratings })
                    break
                case 'OpenAnswer':
                    receipt = await castVote(voting.id, { textAnswer: openText })
                    break
            }
            setVoted(true)
            setJustVoted(true)
            if (receipt) setVoteReceipt(receipt)
        } catch (err) {
            const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
            setVoteError(msg ?? 'Не удалось проголосовать. Попробуйте ещё раз.')
        } finally {
            setSubmitting(false)
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
                    {voting.allowVoteChange ? (
                        <span className="flex items-center gap-1.5 rounded-md bg-secondary px-2.5 py-1 font-medium text-secondary-foreground">
                            <Unlock className="h-3 w-3" />
                            Смена голоса доступна
                        </span>
                    ) : (
                        <span className="flex items-center gap-1.5 rounded-md bg-secondary px-2.5 py-1 font-medium text-secondary-foreground">
                            <Lock className="h-3 w-3" />
                            Смена голоса недоступна
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

            {/* Already voted banner — hidden right after voting to avoid duplicate with success message */}
            {voted && !justVoted && (
                <div className="flex items-center gap-2.5 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700 dark:border-emerald-800 dark:bg-emerald-950/30 dark:text-emerald-400">
                    <Vote className="h-4 w-4 shrink-0" />
                    {voting.allowVoteChange
                        ? 'Вы уже проголосовали. Можете изменить свой голос.'
                        : 'Вы уже проголосовали.'}
                </div>
            )}

            {/* Finished state */}
            {voting.status === 'Finished' ? (
                <div className="space-y-4">
                    <div className="rounded-xl border border-border bg-secondary/50 px-5 py-4 text-sm text-muted-foreground">
                        <p className="font-semibold text-foreground">Голосование завершено</p>
                        <p className="mt-1">Результаты подсчитаны и доступны для просмотра.</p>
                    </div>
                    <Link
                        to={`/votings/${voting.id}/results`}
                        className="flex w-fit items-center gap-2 rounded-xl bg-primary px-6 py-2.5 text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/20 transition-all hover:opacity-90"
                    >
                        <BarChart2 className="h-4 w-4" />
                        Смотреть результаты
                    </Link>
                </div>
            ) : (
                <>
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

                    {/* Vote receipt */}
                    {justVoted && voteReceipt && (
                        <div className="rounded-xl border border-emerald-200 bg-emerald-50 p-4 space-y-3 dark:border-emerald-800 dark:bg-emerald-950/30">
                            <p className="text-sm font-semibold text-emerald-700 dark:text-emerald-400">
                                Ваш голос успешно учтён!
                            </p>
                            <p className="text-xs text-emerald-600/80 dark:text-emerald-500">
                                Сохраните хэш и соль — с их помощью вы сможете убедиться, что ваш голос корректно учтён в результатах.
                            </p>
                            <ReceiptRow
                                label="Хэш голоса"
                                value={voteReceipt.voteHash}
                            />
                            <ReceiptRow
                                label="Соль"
                                value={voteReceipt.voteSalt}
                            />
                        </div>
                    )}
                    {justVoted && !voteReceipt && (
                        <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700 dark:border-emerald-800 dark:bg-emerald-950/30 dark:text-emerald-400">
                            Ваш голос успешно учтён!
                        </div>
                    )}
                    {voteError && (
                        <div className="rounded-xl border border-destructive/20 bg-destructive/10 px-4 py-3 text-sm font-medium text-destructive">
                            {voteError}
                        </div>
                    )}

                    {/* Vote button */}
                    <div className="flex justify-end pt-2">
                        <button
                            onClick={handleVote}
                            disabled={!canVote || submitting || (voted && !voting.allowVoteChange)}
                            className="flex items-center gap-2 rounded-xl bg-primary px-6 py-2.5 text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/20 transition-all hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-40"
                        >
                            <Vote className="h-4 w-4" />
                            {submitting
                                ? 'Отправка...'
                                : voted && !voting.allowVoteChange
                                    ? 'Проголосовано'
                                    : voted
                                        ? 'Изменить голос'
                                        : 'Проголосовать'}
                        </button>
                    </div>
                </>
            )}

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

function ReceiptRow({ label, value }: { label: string; value: string }) {
    const [copied, setCopied] = useState(false)
    return (
        <div className="space-y-1">
            <p className="text-xs text-emerald-600/70 dark:text-emerald-500">{label}</p>
            <div className="flex items-center gap-2 rounded-lg bg-white/60 px-3 py-2 dark:bg-black/20">
                <code className="flex-1 text-xs font-mono text-foreground break-all select-all">{value}</code>
                <button
                    onClick={() => {
                        navigator.clipboard.writeText(value)
                        setCopied(true)
                        setTimeout(() => setCopied(false), 2000)
                    }}
                    className="shrink-0 text-emerald-600 transition-colors hover:text-emerald-800 dark:text-emerald-400 dark:hover:text-emerald-200"
                    title={`Копировать ${label.toLowerCase()}`}
                >
                    {copied ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
                </button>
            </div>
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
