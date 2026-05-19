import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { AlertCircle, ArrowLeft, Calendar, Check, CheckCircle2, ChevronDown, ChevronUp, Copy, ExternalLink, Hash, Lock, Shield, ShieldCheck, Terminal, Unlock, Users, Vote } from 'lucide-react'
import {
    getVotingById, getVotingResults, getMyVote, getVerificationData,
    type VotingResponse, type VotingResultsData,
    type SingleChoiceResult, type MultipleChoiceResult, type RatingResult,
    type MyVoteResult, type VotingVerificationData,
} from '@/api/votingApi'
import { TYPE_LABELS } from '@/components/voting/votingConstants'

export default function VotingResults() {
    const { id } = useParams<{ id: string }>()
    const navigate = useNavigate()

    const [voting, setVoting]         = useState<VotingResponse | null>(null)
    const [results, setResults]       = useState<VotingResultsData | null>(null)
    const [loading, setLoading]       = useState(true)
    const [error, setError]           = useState<string | null>(null)
    const [hashCopied, setHashCopied] = useState(false)
    const [myVote, setMyVote]         = useState<MyVoteResult | null>(null)

    const copyHash = (text: string) => {
        navigator.clipboard.writeText(text)
        setHashCopied(true)
        setTimeout(() => setHashCopied(false), 2000)
    }

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

    // Load user's own vote silently — 404 means they haven't voted
    useEffect(() => {
        if (!id) return
        getMyVote(id).then(setMyVote).catch(() => {})
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
                {/* Hash card with copy button */}
                <div className="flex items-start gap-3 rounded-2xl border border-border bg-card p-4">
                    <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-primary/10">
                        <Hash className="h-5 w-5 text-primary" />
                    </div>
                    <div className="min-w-0 flex-1">
                        <p className="text-xs text-muted-foreground">Хэш результатов голосования</p>
                        <p
                            className="mt-0.5 text-sm font-semibold text-foreground font-mono truncate"
                            title={results.resultHash}
                        >
                            {results.resultHash.slice(0, 12)}…
                        </p>
                        <button
                            onClick={() => copyHash(results.resultHash)}
                            className="mt-1.5 flex items-center gap-1 text-xs text-muted-foreground transition-colors hover:text-foreground"
                        >
                            {hashCopied
                                ? <><Check className="h-3 w-3 text-emerald-500" /><span className="text-emerald-500">Скопировано</span></>
                                : <><Copy className="h-3 w-3" />Копировать хэш</>
                            }
                        </button>
                    </div>
                </div>
            </div>

            {/* Vote verification */}
            {myVote && <VoteVerificationBlock myVote={myVote} />}

            {/* Result verification */}
            <ResultVerificationBlock votingId={id!} myVoteHash={myVote?.voteHash} />

            {/* Blockchain */}
            {results.txHash && (
                <div className="rounded-2xl border border-border bg-card p-5 space-y-2">
                    <h2 className="text-sm font-semibold text-foreground">Блокчейн-верификация</h2>
                    <p className="text-xs text-muted-foreground">Результаты голосования записаны в сеть Ethereum Sepolia и не могут быть изменены задним числом.</p>
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

// ── Vote verification block ────────────────────────────────

type VerifyStatus = 'idle' | 'ok' | 'fail'

function VoteVerificationBlock({ myVote }: { myVote: MyVoteResult }) {
    const [isOpen, setIsOpen]             = useState(true)
    const [verifyStatus, setVerifyStatus] = useState<VerifyStatus>('idle')
    const [verifying, setVerifying]       = useState(false)
    const [showManual, setShowManual]     = useState(false)

    const verifyInBrowser = async () => {
        setVerifying(true)
        try {
            const encoded = new TextEncoder().encode(myVote.hashInput)
            const buffer  = await crypto.subtle.digest('SHA-256', encoded)
            const computed = Array.from(new Uint8Array(buffer))
                .map(b => b.toString(16).padStart(2, '0')).join('')
            setVerifyStatus(computed === myVote.voteHash.toLowerCase() ? 'ok' : 'fail')
        } finally {
            setVerifying(false)
        }
    }

    return (
        <div className="rounded-2xl border border-border bg-card p-5 space-y-5">
            <button
                onClick={() => setIsOpen(v => !v)}
                className="flex w-full items-center gap-2 text-left"
            >
                <ShieldCheck className="h-4 w-4 text-primary shrink-0" />
                <h2 className="flex-1 text-sm font-semibold text-foreground">Верификация голоса</h2>
                {isOpen ? <ChevronUp className="h-4 w-4 text-muted-foreground" /> : <ChevronDown className="h-4 w-4 text-muted-foreground" />}
            </button>

            {isOpen && (
                <>
                    {/* What the user voted for */}
                    <VoteDataView data={myVote.voteData} />

                    {/* Hash and input string */}
                    <div className="space-y-3">
                        <CopyRow label="Хэш голоса" value={myVote.voteHash} mono />
                        <CopyRow label="Строка для хэширования" value={myVote.hashInput} mono />
                    </div>

                    {/* Browser verification */}
                    <div className="space-y-2">
                        <button
                            onClick={verifyInBrowser}
                            disabled={verifying}
                            className="flex items-center gap-2 rounded-xl bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/20 transition-all hover:opacity-90 disabled:opacity-50"
                        >
                            <ShieldCheck className="h-4 w-4" />
                            {verifying ? 'Проверяем...' : 'Проверить в браузере'}
                        </button>
                        <p className="text-xs text-muted-foreground">
                            SHA-256 вычисляется локально в браузере — данные никуда не отправляются.
                        </p>
                        {verifyStatus === 'ok' && (
                            <div className="flex items-center gap-2 rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm font-medium text-emerald-700 dark:border-emerald-800 dark:bg-emerald-950/30 dark:text-emerald-400">
                                <CheckCircle2 className="h-4 w-4 shrink-0" />
                                SHA256(строка) = ваш хэш — всё верно
                            </div>
                        )}
                        {verifyStatus === 'fail' && (
                            <div className="flex items-center gap-2 rounded-lg border border-destructive/20 bg-destructive/10 px-3 py-2 text-sm font-medium text-destructive">
                                <AlertCircle className="h-4 w-4 shrink-0" />
                                Хэши не совпадают
                            </div>
                        )}
                    </div>

                    {/* Manual verification */}
                    <div className="space-y-3 border-t border-border pt-4">
                        <button
                            onClick={() => setShowManual(v => !v)}
                            className="flex items-center gap-1.5 text-sm font-medium text-muted-foreground transition-colors hover:text-foreground"
                        >
                            <Terminal className="h-3.5 w-3.5" />
                            Ручная проверка
                            {showManual ? <ChevronUp className="h-3.5 w-3.5" /> : <ChevronDown className="h-3.5 w-3.5" />}
                        </button>

                        {showManual && (
                            <div className="space-y-4 text-sm">
                                <p className="text-muted-foreground">
                                    Используйте любой SHA-256 инструмент. Хэш строки ниже должен совпасть с вашим хэшем голоса.
                                </p>

                                <div className="space-y-1.5">
                                    <p className="text-xs font-medium text-muted-foreground">Linux / macOS / WSL</p>
                                    <CopyRow
                                        label=""
                                        value={`echo -n '${myVote.hashInput}' | sha256sum`}
                                        mono
                                    />
                                </div>

                                <div className="flex items-center gap-2">
                                    <span className="text-muted-foreground text-xs">Онлайн-инструмент:</span>
                                    <a
                                        href="https://emn178.github.io/online-tools/sha256.html"
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="flex items-center gap-1 text-xs font-medium text-primary hover:underline"
                                    >
                                        SHA-256 Online Tool
                                        <ExternalLink className="h-3 w-3" />
                                    </a>
                                </div>
                            </div>
                        )}
                    </div>
                </>
            )}
        </div>
    )
}

// ── Vote data readable view ────────────────────────────────

function VoteDataView({ data }: { data: MyVoteResult['voteData'] }) {
    return (
        <div className="rounded-xl border border-border bg-secondary/30 px-4 py-3 space-y-2">
            <p className="text-xs font-medium text-muted-foreground">Ваш выбор</p>
            {data.type === 'SingleChoice' && data.candidate && (
                <p className="text-sm font-medium text-foreground">{data.candidate.name}</p>
            )}
            {data.type === 'MultipleChoice' && data.candidates && (
                <ul className="space-y-1">
                    {data.candidates.map(c => (
                        <li key={c.id} className="text-sm text-foreground">• {c.name}</li>
                    ))}
                </ul>
            )}
            {data.type === 'Rating' && data.ratings && (
                <ul className="space-y-1.5">
                    {data.ratings.map(r => (
                        <li key={r.id} className="flex items-center gap-2 text-sm text-foreground">
                            <span className="w-28 truncate">{r.name}</span>
                            <span className="text-amber-400">{'★'.repeat(r.rating)}{'☆'.repeat(5 - r.rating)}</span>
                            <span className="text-xs text-muted-foreground">{r.rating}/5</span>
                        </li>
                    ))}
                </ul>
            )}
            {data.type === 'OpenAnswer' && (
                <p className="text-sm text-foreground italic">«{data.textAnswer}»</p>
            )}
        </div>
    )
}

// ── Copy row ───────────────────────────────────────────────

function CopyRow({ label, value, mono }: { label: string; value: string; mono?: boolean }) {
    const [copied, setCopied] = useState(false)
    return (
        <div className="space-y-1">
            {label && <p className="text-xs text-muted-foreground">{label}</p>}
            <div className="flex items-center gap-2 rounded-lg border border-border bg-muted/40 px-3 py-2">
                <code className={`flex-1 text-xs break-all select-all text-foreground ${mono ? 'font-mono' : ''}`}>
                    {value}
                </code>
                <button
                    onClick={() => {
                        navigator.clipboard.writeText(value)
                        setCopied(true)
                        setTimeout(() => setCopied(false), 2000)
                    }}
                    className="shrink-0 text-muted-foreground transition-colors hover:text-foreground"
                    title="Копировать"
                >
                    {copied ? <Check className="h-3.5 w-3.5 text-emerald-500" /> : <Copy className="h-3.5 w-3.5" />}
                </button>
            </div>
        </div>
    )
}

// ── Result verification block ──────────────────────────────

function ResultVerificationBlock({ votingId, myVoteHash }: { votingId: string; myVoteHash?: string }) {
    const [isOpen, setIsOpen]         = useState(true)
    const [data, setData]             = useState<VotingVerificationData | null>(null)
    const [loading, setLoading]       = useState(false)
    const [verifyStatus, setVerify]   = useState<'idle' | 'ok' | 'fail'>('idle')
    const [showHashes, setShowHashes] = useState(false)
    const [showManual, setShowManual] = useState(false)

    const load = async () => {
        setLoading(true)
        try {
            setData(await getVerificationData(votingId))
        } finally {
            setLoading(false)
        }
    }

    const verifyInBrowser = async () => {
        if (!data) return
        const sorted = [...data.voteHashes].sort()
        const combined = sorted.join('')
        const encoded = new TextEncoder().encode(combined)
        const buffer = await crypto.subtle.digest('SHA-256', encoded)
        const computed = Array.from(new Uint8Array(buffer))
            .map(b => b.toString(16).padStart(2, '0')).join('')
        setVerify(computed === data.resultHash ? 'ok' : 'fail')
    }

    const downloadJson = () => {
        if (!data) return
        const payload = {
            votingId: data.votingId,
            resultHash: data.resultHash,
            hashAlgorithm: data.hashAlgorithm,
            combineMethod: data.combineMethod,
            totalVotes: data.totalVotes,
            voteHashes: [...data.voteHashes].sort(),
        }
        const blob = new Blob([JSON.stringify(payload, null, 2)], { type: 'application/json' })
        const url = URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = `verification-${data.votingId}.json`
        a.click()
        URL.revokeObjectURL(url)
    }

    return (
        <div className="rounded-2xl border border-border bg-card p-5 space-y-4">
            <button
                onClick={() => setIsOpen(v => !v)}
                className="flex w-full items-center gap-2 text-left"
            >
                <ShieldCheck className="h-4 w-4 text-primary shrink-0" />
                <h2 className="flex-1 text-sm font-semibold text-foreground">Верификация результатов</h2>
                {isOpen ? <ChevronUp className="h-4 w-4 text-muted-foreground" /> : <ChevronDown className="h-4 w-4 text-muted-foreground" />}
            </button>

            {isOpen && !data && (
                <div className="space-y-2">
                    <p className="text-xs text-muted-foreground">
                        Загрузите список всех хэшей голосов, чтобы самостоятельно проверить ResultHash.
                    </p>
                    <button
                        onClick={load}
                        disabled={loading}
                        className="flex items-center gap-2 rounded-xl bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/20 transition-all hover:opacity-90 disabled:opacity-50"
                    >
                        <ShieldCheck className="h-4 w-4" />
                        {loading ? 'Загружаем...' : 'Загрузить данные верификации'}
                    </button>
                </div>
            )}

            {isOpen && data && (
                <div className="space-y-4">
                    {/* Summary */}
                    <div className="space-y-0.5">
                        <p className="text-xs text-muted-foreground">Алгоритм</p>
                        <p className="font-mono text-xs font-medium text-foreground">{data.hashAlgorithm}</p>
                    </div>

                    <CopyRow label="ResultHash" value={data.resultHash} mono />

                    {/* Action buttons */}
                    <div className="flex flex-wrap gap-2">
                        <button
                            onClick={verifyInBrowser}
                            className="flex items-center gap-2 rounded-xl bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/20 transition-all hover:opacity-90"
                        >
                            <ShieldCheck className="h-4 w-4" />
                            Проверить в браузере
                        </button>
                        <button
                            onClick={downloadJson}
                            className="flex items-center gap-2 rounded-xl border border-border px-4 py-2 text-sm font-medium text-foreground transition-colors hover:bg-secondary"
                        >
                            <Hash className="h-4 w-4" />
                            Скачать JSON
                        </button>
                    </div>

                    {verifyStatus === 'ok' && (
                        <div className="flex items-center gap-2 rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm font-medium text-emerald-700 dark:border-emerald-800 dark:bg-emerald-950/30 dark:text-emerald-400">
                            <CheckCircle2 className="h-4 w-4 shrink-0" />
                            SHA256(sorted hashes) = ResultHash — результат не изменён
                        </div>
                    )}
                    {verifyStatus === 'fail' && (
                        <div className="flex items-center gap-2 rounded-lg border border-destructive/20 bg-destructive/10 px-3 py-2 text-sm font-medium text-destructive">
                            <AlertCircle className="h-4 w-4 shrink-0" />
                            Хэши не совпадают
                        </div>
                    )}

                    {/* Manual verification */}
                    <div className="space-y-3 border-t border-border pt-4">
                        <button
                            onClick={() => setShowManual(v => !v)}
                            className="flex items-center gap-1.5 text-sm font-medium text-muted-foreground transition-colors hover:text-foreground"
                        >
                            <Terminal className="h-3.5 w-3.5" />
                            Ручная проверка
                            {showManual ? <ChevronUp className="h-3.5 w-3.5" /> : <ChevronDown className="h-3.5 w-3.5" />}
                        </button>
                        {showManual && (
                            <div className="space-y-3 text-sm">
                                <p className="text-xs text-muted-foreground">
                                    Скачайте JSON и проверьте локально — он содержит хеши голосов всех участников, включая ваш, затем выполните команду — результат должен совпасть с ResultHash, подтверждая что ваш голос учтён.
                                </p>
                                <div className="space-y-1.5">
                                    <p className="text-xs font-medium text-muted-foreground">Linux / macOS / WSL</p>
                                    <CopyRow label="" value={`cat verification-${data.votingId}.json | jq -r '.voteHashes[]' | sort | tr -d '\\n' | sha256sum`} mono />
                                </div>
                            </div>
                        )}
                    </div>

                    {/* Hash list */}
                    <div className="space-y-2 border-t border-border pt-4">
                        <button
                            onClick={() => setShowHashes(v => !v)}
                            className="flex items-center gap-1.5 text-sm font-medium text-muted-foreground transition-colors hover:text-foreground"
                        >
                            {showHashes ? <ChevronUp className="h-3.5 w-3.5" /> : <ChevronDown className="h-3.5 w-3.5" />}
                            {showHashes ? 'Скрыть' : 'Показать'} все хэши голосов ({data.totalVotes})
                        </button>
                        {showHashes && (
                            <div className="max-h-64 overflow-y-auto rounded-xl border border-border bg-muted/30 p-3 space-y-1.5">
                                {[...data.voteHashes].sort().map((hash, i) => (
                                    <div
                                        key={i}
                                        className={`flex items-center gap-2 rounded-lg px-2 py-1 text-xs font-mono ${
                                            hash === myVoteHash
                                                ? 'bg-primary/10 text-primary font-semibold'
                                                : 'text-foreground'
                                        }`}
                                    >
                                        <span className="shrink-0 text-muted-foreground w-5 text-right">{i + 1}</span>
                                        <span className="break-all">{hash}</span>
                                        {hash === myVoteHash && (
                                            <span className="shrink-0 ml-auto rounded bg-primary/20 px-1.5 py-0.5 text-xs font-semibold text-primary">вы</span>
                                        )}
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </div>
            )}
        </div>
    )
}

// ── Open answer results ────────────────────────────────────

function OpenAnswerResults({ results }: { results: VotingResultsData }) {
    const raw = (results.results['openAnswer'] ?? {}) as Record<string, unknown>
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
