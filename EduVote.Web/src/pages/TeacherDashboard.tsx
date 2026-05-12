import { useEffect, useState } from 'react'
import { BookOpen, Vote } from 'lucide-react'
import { getVotingsCreatedByUser, getVotingsForUser, type VotingResponse } from '@/api/votingApi'
import { getUsers } from '@/api/userApi'
import { getEducationUnits, type EducationUnit } from '@/api/educationUnitApi'
import { useAuth } from '@/context/AuthContext'
import StudentVotingCard from '@/components/student/StudentVotingCard'

export default function TeacherDashboard() {
    const { claims } = useAuth()

    const [myVotings, setMyVotings] = useState<VotingResponse[]>([])
    const [availableVotings, setAvailableVotings] = useState<VotingResponse[]>([])
    const [allUnits, setAllUnits] = useState<EducationUnit[]>([])
    const [facultyName, setFacultyName] = useState<string | null>(null)
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)

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
                setMyVotings(myData.votings ?? [])
                setAvailableVotings(availableData.votings ?? [])
                setAllUnits(unitsData.educationUnits ?? [])

                const me = usersData.users.find(u => u.id === userId)
                if (me?.educationUnitName) setFacultyName(me.educationUnitName)
            })
            .catch(err => setError(err?.response?.data?.message ?? 'Не удалось загрузить данные'))
            .finally(() => setLoading(false))
    }, [claims?.nameid])

    const isEmpty = myVotings.length === 0 && availableVotings.length === 0

    return (
        <div className="space-y-8">

            {/* Header */}
            <div>
                <h1 className="text-2xl font-bold tracking-tight text-foreground">
                    Доступнык голосования
                </h1>
                <p className="mt-1 text-sm text-muted-foreground">
                    {facultyName ? (
                        <>
                            <BookOpen className="inline h-3.5 w-3.5 mr-1 align-text-bottom" />
                            Факультет{' '}
                            <span className="font-semibold text-foreground">{facultyName}</span>
                        </>
                    ) : (
                        'Голосования вашего факультета'
                    )}
                </p>
            </div>

            {/* Loading */}
            {loading && (
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Vote className="h-4 w-4 animate-pulse text-primary" />
                    Загрузка голосований...
                </div>
            )}

            {/* Error */}
            {error && (
                <div className="rounded-xl border border-destructive/20 bg-destructive/10 px-4 py-3 text-sm font-medium text-destructive">
                    {error}
                </div>
            )}

            {/* Empty */}
            {!loading && !error && isEmpty && (
                <div className="flex flex-col items-center gap-3 rounded-2xl border border-dashed border-border bg-card py-20 text-center">
                    <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-muted">
                        <Vote className="h-7 w-7 text-muted-foreground" />
                    </div>
                    <p className="text-sm font-semibold text-foreground">Нет голосований</p>
                    <p className="text-xs text-muted-foreground max-w-xs">
                        Создайте первое голосование в разделе «Голосования»
                    </p>
                </div>
            )}

            {/* My votings */}
            {!loading && !error && myVotings.length > 0 && (
                <section className="space-y-3">
                    <h2 className="text-xs font-semibold uppercase tracking-widest text-muted-foreground">
                        Мои голосования
                    </h2>
                    <div className="grid gap-4 sm:grid-cols-2">
                        {myVotings.map(v => (
                            <StudentVotingCard key={v.id} voting={v} allUnits={allUnits} />
                        ))}
                    </div>
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
                            <StudentVotingCard key={v.id} voting={v} allUnits={allUnits} />
                        ))}
                    </div>
                </section>
            )}

        </div>
    )
}
