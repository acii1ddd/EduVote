import { useEffect, useState } from 'react'
import { Vote } from 'lucide-react'
import { getVotingsForUser, type VotingResponse } from '@/api/votingApi'
import { getUsers, type UserResponse } from '@/api/userApi'
import { getEducationUnits, type EducationUnit } from '@/api/educationUnitApi'
import { useAuth } from '@/context/AuthContext'
import StudentVotingCard from '@/components/student/StudentVotingCard'
import ProfileCard from '@/components/ProfileCard'

export default function StudentDashboard() {
    const { claims } = useAuth()

    const [votings,   setVotings]   = useState<VotingResponse[]>([])
    const [allUnits,  setAllUnits]  = useState<EducationUnit[]>([])
    const [userMap,   setUserMap]   = useState<Record<string, UserResponse>>({})
    const [unitName,  setUnitName]  = useState<string | null>(null)
    const [loading,   setLoading]   = useState(true)
    const [error,     setError]     = useState<string | null>(null)

    useEffect(() => {
        if (!claims?.nameid) return

        const userId = claims.nameid

        Promise.all([
            getVotingsForUser(userId),
            getUsers(),
            getEducationUnits(),
        ])
            .then(([votingsData, usersData, unitsData]) => {
                setVotings(votingsData.votings ?? [])
                setAllUnits(unitsData.educationUnits ?? [])

                const map: Record<string, UserResponse> = {}
                for (const u of usersData.users) map[u.id] = u
                setUserMap(map)

                const me = usersData.users.find(u => u.id === userId)
                if (me?.educationUnitName) setUnitName(me.educationUnitName)
            })
            .catch(err => setError(err?.response?.data?.message ?? 'Не удалось загрузить данные'))
            .finally(() => setLoading(false))
    }, [claims?.nameid])

    const active = votings.filter(v => v.status === 'Active')
    const other  = votings.filter(v => v.status !== 'Active')

    return (
        <div className="space-y-8">

            {/* Header */}
            <div className="space-y-3">
                <div>
                    <h1 className="text-2xl font-bold tracking-tight text-foreground">
                        Мои голосования
                    </h1>
                    <p className="mt-1 text-sm text-muted-foreground">
                        Голосования, доступные для вашей учебной группы
                    </p>
                </div>

                {claims && (
                    <ProfileCard fields={[
                        { label: 'Имя студента',    value: userMap[claims.nameid]?.name ?? '—' },
                        { label: 'Email',            value: userMap[claims.nameid]?.email ?? '—' },
                        { label: 'Ваша роль',        value: claims.role, highlight: true },
                        ...(unitName ? [{ label: 'Учебная группа', value: unitName }] : []),
                    ]} />
                )}
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
            {!loading && !error && votings.length === 0 && (
                <div className="flex flex-col items-center gap-3 rounded-2xl border border-dashed border-border bg-card py-20 text-center">
                    <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-muted">
                        <Vote className="h-7 w-7 text-muted-foreground" />
                    </div>
                    <p className="text-sm font-semibold text-foreground">Нет доступных голосований</p>
                    <p className="text-xs text-muted-foreground max-w-xs">
                        Голосования появятся, когда администратор их создаст для вашей группы
                    </p>
                </div>
            )}

            {/* Active votings */}
            {active.length > 0 && (
                <section className="space-y-3">
                    <h2 className="flex items-center gap-2 text-xs font-semibold uppercase tracking-widest text-muted-foreground">
                        <span className="h-2 w-2 rounded-full bg-emerald-500 animate-pulse" />
                        Активные
                    </h2>
                    <div className="grid gap-4 sm:grid-cols-2">
                        {active.map(v => (
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

            {/* Other votings */}
            {other.length > 0 && (
                <section className="space-y-3">
                    <h2 className="text-xs font-semibold uppercase tracking-widest text-muted-foreground">
                        Остальные
                    </h2>
                    <div className="grid gap-4 sm:grid-cols-2">
                        {other.map(v => (
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

        </div>
    )
}
