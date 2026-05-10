import { useEffect, useState } from 'react'
import { deleteUserRequest, getUsers, updateUser, type UserResponse } from '@/api/userApi'
import { getRoles } from '@/api/roleApi'
import { type EducationUnit, getEducationUnits } from '@/api/educationUnitApi'
import { assignUserToEducationUnit } from '@/api/assignUserToEducationUnit'
import { Users } from 'lucide-react'

export default function AdminPage() {
    const [users, setUsers] = useState<UserResponse[]>([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)
    const [roles, setRoles] = useState<string[]>([])
    const [educationUnits, setEducationUnits] = useState<EducationUnit[]>([])
    const [selectedEducationUnitId, setSelectedEducationUnitId] = useState<string>('')

    const [isEditOpen, setIsEditOpen] = useState(false)
    const [editingUser, setEditingUser] = useState<UserResponse | null>(null)

    useEffect(() => {
        loadUsers()
        loadEducationUnits()
        loadRoles()
    }, [])

    const loadRoles = async () => {
        const data = await getRoles()
        setRoles(data.roles.map(r => r.name))
    }

    const loadEducationUnits = async () => {
        const data = await getEducationUnits()
        setEducationUnits(data.educationUnits)
    }

    const loadUsers = async () => {
        try {
            setLoading(true)
            const data = await getUsers()
            setUsers(data.users)
        } catch (err: any) {
            setError(err?.response?.data?.message || String(err))
        } finally {
            setLoading(false)
        }
    }

    const openEdit = (user: UserResponse) => {
        setEditingUser(user)
        setSelectedEducationUnitId('')
        setIsEditOpen(true)
    }

    const saveUser = async () => {
        if (!editingUser) return
        try {
            await updateUser({
                id: editingUser.id,
                name: editingUser.name,
                email: editingUser.email,
                role: editingUser.role,
            })
            if (selectedEducationUnitId) {
                await assignUserToEducationUnit(editingUser.id, selectedEducationUnitId)
            }
            await loadUsers()
            setIsEditOpen(false)
            setEditingUser(null)
        } catch (err) {
            console.error(err)
        }
    }

    const deleteUser = async (userId: string) => {
        if (!window.confirm('Вы уверены, что хотите удалить пользователя?')) return
        try {
            await deleteUserRequest(userId)
            setUsers(prev => prev.filter(x => x.id !== userId))
        } catch (err) {
            console.error(err)
        }
    }

    const inputClass = `
        w-full rounded-xl border border-border bg-background
        px-3.5 py-2.5 text-sm text-foreground placeholder:text-muted-foreground
        focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20
        transition-colors
    `

    return (
        <div className="space-y-8">

            <div className="flex items-center gap-3">
                <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary/10 text-primary">
                    <Users className="h-5 w-5" />
                </div>
                <div>
                    <h1 className="text-2xl font-bold tracking-tight text-foreground">
                        Панель администратора
                    </h1>
                    <p className="text-sm text-muted-foreground">
                        Управление пользователями системы
                    </p>
                </div>
            </div>

            {/* Users table */}
            <section className="rounded-2xl border border-border bg-card shadow-sm overflow-hidden">

                <div className="border-b border-border px-6 py-4">
                    <h2 className="text-base font-semibold text-card-foreground">
                        Пользователи
                    </h2>
                </div>

                {loading ? (
                    <div className="px-6 py-10 text-sm text-muted-foreground">
                        Загрузка пользователей...
                    </div>
                ) : error ? (
                    <div className="px-6 py-10 text-sm text-destructive">
                        {error}
                    </div>
                ) : (
                    <div className="overflow-x-auto">
                        <table className="min-w-full">

                            <thead className="bg-muted/40">
                                <tr>
                                    {['Имя', 'Email', 'Роль', 'Учебная группа', 'Создан', ''].map(h => (
                                        <th key={h} className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                                            {h}
                                        </th>
                                    ))}
                                </tr>
                            </thead>

                            <tbody className="divide-y divide-border">
                                {users.map(user => (
                                    <tr key={user.id} className="hover:bg-muted/20 transition-colors">

                                        <td className="px-5 py-3.5 text-sm font-medium text-foreground">
                                            {user.name}
                                        </td>

                                        <td className="px-5 py-3.5 text-sm text-muted-foreground">
                                            {user.email}
                                        </td>

                                        <td className="px-5 py-3.5">
                                            <span className="rounded-full bg-primary/10 px-2.5 py-1 text-xs font-semibold text-primary">
                                                {user.role}
                                            </span>
                                        </td>

                                        <td className="px-5 py-3.5 text-sm text-muted-foreground">
                                            {user.educationUnitName || '—'}
                                        </td>

                                        <td className="px-5 py-3.5 text-sm text-muted-foreground">
                                            {new Date(user.createdAt).toLocaleDateString('ru-RU')}
                                        </td>

                                        <td className="px-5 py-3.5 text-right">
                                            <div className="flex items-center justify-end gap-2">
                                                <button
                                                    onClick={() => openEdit(user)}
                                                    className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-foreground transition-colors hover:bg-secondary hover:border-primary/30"
                                                >
                                                    Изменить
                                                </button>
                                                <button
                                                    onClick={() => deleteUser(user.id)}
                                                    className="rounded-lg border border-destructive/20 bg-destructive/10 px-3 py-1.5 text-xs font-medium text-destructive transition-colors hover:bg-destructive/20"
                                                >
                                                    Удалить
                                                </button>
                                            </div>
                                        </td>

                                    </tr>
                                ))}
                            </tbody>

                        </table>
                    </div>
                )}

            </section>

            {/* Edit modal */}
            {isEditOpen && editingUser && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/20 backdrop-blur-sm">
                    <div className="w-full max-w-md rounded-2xl border border-border bg-card p-6 shadow-xl space-y-4">

                        <h2 className="text-lg font-semibold text-card-foreground">
                            Редактировать пользователя
                        </h2>

                        <div className="space-y-3">
                            <input
                                className={inputClass}
                                value={editingUser.name}
                                onChange={(e) => setEditingUser({ ...editingUser, name: e.target.value })}
                                placeholder="Имя"
                            />
                            <input
                                className={inputClass}
                                value={editingUser.email}
                                onChange={(e) => setEditingUser({ ...editingUser, email: e.target.value })}
                                placeholder="Email"
                            />
                            <select
                                className={inputClass}
                                value={editingUser.role}
                                onChange={(e) => setEditingUser({ ...editingUser, role: e.target.value })}
                            >
                                {roles.map(role => (
                                    <option key={role} value={role}>{role}</option>
                                ))}
                            </select>
                            <select
                                className={inputClass}
                                value={selectedEducationUnitId}
                                onChange={(e) => setSelectedEducationUnitId(e.target.value)}
                            >
                                <option value="">Выбрать учебную группу</option>
                                {educationUnits.map(unit => (
                                    <option key={unit.id} value={unit.id}>
                                        {unit.name} ({unit.type})
                                    </option>
                                ))}
                            </select>
                        </div>

                        <div className="flex justify-end gap-2 pt-1">
                            <button
                                onClick={() => setIsEditOpen(false)}
                                className="rounded-xl border border-border px-4 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
                            >
                                Отмена
                            </button>
                            <button
                                onClick={saveUser}
                                className="rounded-xl bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/20 transition-all hover:opacity-90"
                            >
                                Сохранить
                            </button>
                        </div>

                    </div>
                </div>
            )}

        </div>
    )
}
