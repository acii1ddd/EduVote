import { useEffect, useState } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { ArrowLeft, Calendar, ExternalLink, Hash, Lock, Shield, Unlock, Users, Vote } from 'lucide-react'
import {
    getVotingById, getVotingResults,
    type VotingResponse, type VotingResultsData,
    type SingleChoiceResult, type MultipleChoiceResult, type RatingResult,
} from '@/api/votingApi'
import { TYPE_LABELS } from '@/components/voting/votingConstants'

export default function VotingResults() {
    const { id } = useParams<{ id: string }>()
    const navigate = useNavigate()

    const [voting, setVoting]   = useState<VotingResponse | null>(null)
    const [results, setResults] = useState<VotingResultsData | null>(null)
    const [loading, setLoading] = useState(true)
    const [error, setError]     = useState<string | null>(null)

    useEffect(() => {
        if (!id) return
        Promise.all([getVotingById(id), getVotingResults(id)])
            .then(([v, r]) => {
                setVoting(v)
                setResults(r)
            })
            .catch(err => {
                const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
                setError(msg ?? 'Не удалось загрузить результаты')
            })
            .finally(() => setLoading(false))
    }, [id])

    if (loading) return (
        <div className="flex items-center gap-2 text-sm text-muted-foreground py-16 justify-center">
            <Vote className="h-4 w-4 animate-pulse text-primary" />
            Загрузка результатов...
        </div>
    )

    if (error || !voting || !results) return (
        <div className="space-y-4">
            <button onClick={() => navigate(-1)} className="flex items-center gap-1.5 text-sm text-muted-foreground transition-colors hover:text-foreground">
                <ArrowLeft className="h-4 w-4" />
                Назад
            </button>
            <div className="rounded-xl border border-destructive/20 bg-destructive/10 px-4 py-3 text-sm font-medium text-destructive">
                {error ?? 'Результаты не найдены'}
            </div>
        </div>
    )

    const fmt = (iso: string) => iso ? new Date(iso).toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—'
    const fmtFull = (iso: string) => iso ? new Date(iso).toLocaleString('ru-RU') : '—'

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
            <div className="space-y-3">
                <div className="flex flex-wrap items-start gap-3">
                    <h1 className="flex-1 text-2xl font-bold tracking-tight text-foreground min-w-0">
                        {voting.title}
                    </h1>
                    <span className="flex shrink-0 items-center gap-1.5 rounded-full bg-secondary px-3 py-1.5 text-xs font-semibold text-secondary-foreground">
                        Результаты
                    </span>
                </div>

                {voting.description && (
                    <p className="text-sm leading-relaxed text-muted-foreground">
                        {voting.description}
                    </p>
                )}
            </div>

            {/* Voting info */}
            <div className="rounded-2xl border border-border bg-card p-5 space-y-4">
                <h2 className="text-sm font-semibold text-foreground">Характеристики голосования</h2>
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 text-sm">
                    <InfoRow label="Тип" value={TYPE_LABELS[voting.type]} />
                    <InfoRow label="Начало" value={fmt(voting.startTime)} />
                    <InfoRow label="Окончание" value={fmt(voting.endTime)} />
                    <InfoRow
                        label="Анонимность"
                        value={voting.isAnonymous ? 'Анонимное' : 'Публичное'}
                        icon={<Shield className="h-3.5 w-3.5 shrink-0 text-muted-foreground" />}
                    />
                    <InfoRow
                        label="Изменение голоса"
                        value={voting.allowVoteChange ? 'Доступно' : 'Недоступно'}
                        icon={voting.allowVoteChange
                            ? <Unlock className="h-3.5 w-3.5 shrink-0 text-muted-foreground" />
                            : <Lock className="h-3.5 w-3.5 shrink-0 text-muted-foreground" />
                        }
                    />
                </div>
            </div>

            {/* Stats */}
            <div className="grid gap-4 sm:grid-cols-3">
                <StatCard
                    icon={<Users className="h-5 w-5 text-primary" />}
                    label="Всего проголосовало"
                    value={String(results.totalVotes)}
                />
                <StatCard
                    icon={<Calendar className="h-5 w-5 text-primary" />}
                    label="Результат подсчитан"
                    value={fmtFull(results.calculatedAt)}
                />
                <StatCard
                    icon={<Hash className="h-5 w-5 text-primary" />}
                    label="Хэш результата"
                    value={results.resultHash.slice(0, 12) + '…'}
                    title={results.resultHash}
                />
            </div>

            {/* Blockchain */}
            {results.txHash && (
                <div className="rounded-2xl border border-border bg-card p-5 space-y-2">
                    <h2 className="text-sm font-semibold text-foreground">Блокчейн-верификация</h2>
                    <p className="text-xs text-muted-foreground">Результаты голосования записаны в сеть Ethereum Sepolia.</p>
                    <div className="flex flex-wrap items-center gap-2">
                        <code className="rounded bg-muted px-2 py-1 text-xs text-foreground font-mono break-all">
                            {results.txHash}
                        </code>
                        {results.etherscanUrl && (
                            <a
                                href={results.etherscanUrl}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="flex items-center gap-1.5 rounded-lg border border-border px-3 py-1.5 text-xs font-medium text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
                            >
                                <ExternalLink className="h-3.5 w-3.5" />
                                Посмотреть на Etherscan
                            </a>
                        )}
                    </div>
                </div>
            )}

            {/* Results visualization */}
            <div className="space-y-4">
                <h2 className="text-base font-semibold text-foreground">Результаты</h2>
                <ResultsView voting={voting} results={results} />
            </div>

        </div>
    )
}

// ── Info row ───────────────────────────────────────────────

function InfoRow({ label, value, icon, title }: { label: string; value: string; icon?: React.ReactNode; title?: string }) {
    return (
        <div className="space-y-0.5">
            <p className="text-xs text-muted-foreground">{label}</p>
            <div className="flex items-center gap-1.5">
                {icon}
                <p className="text-sm font-medium text-foreground" title={title}>{value}</p>
            </div>
        </div>
    )
}

// ── Stat card ──────────────────────────────────────────────

function StatCard({ icon, label, value, title }: { icon: React.ReactNode; label: string; value: string; title?: string }) {
    return (
        <div className="flex items-start gap-3 rounded-2xl border border-border bg-card p-4">
            <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-primary/10">
                {icon}
            </div>
            <div className="min-w-0">
                <p className="text-xs text-muted-foreground">{label}</p>
                <p className="mt-0.5 text-sm font-semibold text-foreground truncate" title={title}>{value}</p>
            </div>
        </div>
    )
}

// ── Results view dispatcher ────────────────────────────────

function ResultsView({ voting, results }: { voting: VotingResponse; results: VotingResultsData }) {
    if (results.totalVotes === 0) {
        return (
            <div className="rounded-2xl border border-dashed border-border py-12 text-center text-sm text-muted-foreground">
                Никто не проголосовал
            </div>
        )
    }

    switch (voting.type) {
        case 'SingleChoice':
            return <SingleChoiceResults results={results} />
        case 'MultipleChoice':
            return <MultipleChoiceResults results={results} />
        case 'Rating':
            return <RatingResults results={results} />
        case 'OpenAnswer':
            return <OpenAnswerResults results={results} />
        default:
            return null
    }
}

// ── Single choice results ──────────────────────────────────

function SingleChoiceResults({ results }: { results: VotingResultsData }) {
    const entries = Object.values(results.results) as SingleChoiceResult[]
    const sorted = [...entries].sort((a, b) => b.voteCount - a.voteCount)
    const max = sorted[0]?.voteCount ?? 1

    return (
        <div className="space-y-3">
            {sorted.map(entry => (
                <div key={entry.candidateId} className="space-y-1.5">
                    <div className="flex items-center justify-between text-sm">
                        <span className="font-medium text-foreground">{entry.candidateName}</span>
                        <span className="text-muted-foreground">{entry.voteCount} голос. · {entry.percentage}%</span>
                    </div>
                    <div className="h-2.5 w-full overflow-hidden rounded-full bg-secondary">
                        <div
                            className="h-full rounded-full bg-primary transition-all"
                            style={{ width: max > 0 ? `${(entry.voteCount / max) * 100}%` : '0%' }}
                        />
                    </div>
                </div>
            ))}
        </div>
    )
}

// ── Multiple choice results ────────────────────────────────

function MultipleChoiceResults({ results }: { results: VotingResultsData }) {
    const entries = Object.values(results.results) as MultipleChoiceResult[]
    const sorted = [...entries].sort((a, b) => b.selectionCount - a.selectionCount)
    const max = sorted[0]?.selectionCount ?? 1

    return (
        <div className="space-y-3">
            {sorted.map(entry => (
                <div key={entry.candidateId} className="space-y-1.5">
                    <div className="flex items-center justify-between text-sm">
                        <span className="font-medium text-foreground">{entry.candidateName}</span>
                        <span className="text-muted-foreground">{entry.selectionCount} выбор. · {entry.percentage}%</span>
                    </div>
                    <div className="h-2.5 w-full overflow-hidden rounded-full bg-secondary">
                        <div
                            className="h-full rounded-full bg-primary transition-all"
                            style={{ width: max > 0 ? `${(entry.selectionCount / max) * 100}%` : '0%' }}
                        />
                    </div>
                </div>
            ))}
        </div>
    )
}

// ── Rating results ─────────────────────────────────────────

function RatingResults({ results }: { results: VotingResultsData }) {
    const entries = Object.values(results.results) as RatingResult[]
    const sorted = [...entries].sort((a, b) => b.averageRating - a.averageRating)

    return (
        <div className="space-y-3">
            {sorted.map(entry => (
                <div key={entry.candidateId} className="flex items-center justify-between rounded-xl border border-border bg-card p-4">
                    <div>
                        <p className="text-sm font-medium text-foreground">{entry.candidateName}</p>
                        <p className="text-xs text-muted-foreground">{entry.totalRatings} оценок</p>
                    </div>
                    <div className="flex items-center gap-2">
                        <Stars value={entry.averageRating} />
                        <span className="text-sm font-semibold text-foreground w-8 text-right">
                            {entry.averageRating.toFixed(1)}
                        </span>
                    </div>
                </div>
            ))}
        </div>
    )
}

function Stars({ value }: { value: number }) {
    return (
        <div className="flex items-center gap-0.5">
            {[1, 2, 3, 4, 5].map(n => (
                <span
                    key={n}
                    className={`text-lg leading-none ${value >= n ? 'text-amber-400' : value >= n - 0.5 ? 'text-amber-300' : 'text-muted-foreground/25'}`}
                >
                    ★
                </span>
            ))}
        </div>
    )
}

// ── Open answer results ────────────────────────────────────

function OpenAnswerResults({ results }: { results: VotingResultsData }) {
    const raw = results.results as Record<string, unknown>
    const answers = Array.isArray(raw['answers']) ? (raw['answers'] as string[]) : []
    const total = typeof raw['totalAnswers'] === 'number' ? raw['totalAnswers'] : answers.length

    return (
        <div className="space-y-3">
            <p className="text-sm text-muted-foreground">Всего ответов: {total}</p>
            {answers.length === 0 ? (
                <div className="rounded-2xl border border-dashed border-border py-8 text-center text-sm text-muted-foreground">
                    Ответов нет
                </div>
            ) : (
                <div className="space-y-2">
                    {answers.map((ans, i) => (
                        <div key={i} className="rounded-xl border border-border bg-card px-4 py-3 text-sm text-foreground">
                            {ans}
                        </div>
                    ))}
                </div>
            )}
        </div>
    )
}
