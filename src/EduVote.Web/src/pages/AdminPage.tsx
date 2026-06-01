import { useEffect, useState } from 'react'
import { deleteUserRequest, getUsers, updateUser, createUser, type UserResponse } from '@/api/userApi'
import { getRoles } from '@/api/roleApi'
import { type EducationUnit, getEducationUnits } from '@/api/educationUnitApi'
import { assignUserToEducationUnit } from '@/api/assignUserToEducationUnit'
import { useAuth } from '@/context/AuthContext'
import ProfileCard from '@/components/ProfileCard'
import { Users, Plus, X } from 'lucide-react'

const inputClass = `
    w-full rounded-xl border border-border bg-background
    px-3.5 py-2.5 text-sm text-foreground placeholder:text-muted-foreground
    focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20
    transition-colors
`

const EMPTY_CREATE = { name: '', email: '', password: '', role: '' }

export default function AdminPage() {
    const { claims } = useAuth()
    const [users, setUsers] = useState<UserResponse[]>([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)
    const [roles, setRoles] = useState<string[]>([])
    const [educationUnits, setEducationUnits] = useState<EducationUnit[]>([])

    // Edit modal state
    const [isEditOpen, setIsEditOpen] = useState(false)
    const [editingUser, setEditingUser] = useState<UserResponse | null>(null)
    const [selectedEducationUnitId, setSelectedEducationUnitId] = useState('')

    // Create modal state
    const [isCreateOpen, setIsCreateOpen] = useState(false)
    const [createForm, setCreateForm] = useState(EMPTY_CREATE)
    const [createError, setCreateError] = useState<string | null>(null)
    const [isCreating, setIsCreating] = useState(false)

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

    // ── Create ─────────────────────────────────────────────
    const openCreate = () => {
        setCreateForm({ ...EMPTY_CREATE, role: roles[0] ?? '' })
        setCreateError(null)
        setIsCreateOpen(true)
    }

    const submitCreate = async (e: React.FormEvent) => {
        e.preventDefault()
        setCreateError(null)

        if (!createForm.name.trim() || !createForm.email.trim() || !createForm.password.trim()) {
            setCreateError('Заполните все обязательные поля')
            return
        }

        setIsCreating(true)
        try {
            const newUser = await createUser(createForm)
            setUsers(prev => [newUser, ...prev])
            setIsCreateOpen(false)
        } catch (err: any) {
            setCreateError(err?.response?.data?.message || 'Не удалось создать пользователя')
        } finally {
            setIsCreating(false)
        }
    }

    // ── Edit ───────────────────────────────────────────────
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

    // ── Delete ─────────────────────────────────────────────
    const deleteUser = async (userId: string) => {
        if (!window.confirm('Вы уверены, что хотите удалить пользователя?')) return
        try {
            await deleteUserRequest(userId)
            setUsers(prev => prev.filter(x => x.id !== userId))
        } catch (err) {
            console.error(err)
        }
    }

    return (
        <div className="space-y-8">

            {/* Page header */}
            <div className="space-y-3">
                <div className="flex items-center justify-between gap-4">
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

                    <button
                        onClick={openCreate}
                        className="flex shrink-0 items-center gap-2 rounded-xl bg-primary px-4 py-2.5 text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/20 transition-all hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2"
                    >
                        <Plus className="h-4 w-4" />
                        Добавить
                    </button>
                </div>

                {claims && !loading && (
                    <ProfileCard fields={[
                        { label: 'Имя администратора', value: users.find(u => u.id === claims.nameid)?.name ?? '—' },
                        { label: 'Email',               value: users.find(u => u.id === claims.nameid)?.email ?? '—' },
                        { label: 'Ваша роль',           value: claims.role, highlight: true },
                    ]} />
                )}
            </div>

            {/* Users table */}
            <section className="rounded-2xl border border-border bg-card shadow-sm overflow-hidden">

                <div className="border-b border-border px-6 py-4 flex items-center justify-between">
                    <h2 className="text-base font-semibold text-card-foreground">Пользователи</h2>
                    {!loading && (
                        <span className="rounded-full bg-secondary px-2.5 py-0.5 text-xs font-semibold text-secondary-foreground">
                            {users.length}
                        </span>
                    )}
                </div>

                {loading ? (
                    <div className="px-6 py-10 text-sm text-muted-foreground">Загрузка...</div>
                ) : error ? (
                    <div className="px-6 py-10 text-sm text-destructive">{error}</div>
                ) : users.length === 0 ? (
                    <div className="px-6 py-10 text-sm text-muted-foreground">Нет пользователей</div>
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
                                        <td className="px-5 py-3.5 text-sm font-medium text-foreground">{user.name}</td>
                                        <td className="px-5 py-3.5 text-sm text-muted-foreground">{user.email}</td>
                                        <td className="px-5 py-3.5">
                                            <span className="rounded-full bg-primary/10 px-2.5 py-1 text-xs font-semibold text-primary">
                                                {user.role}
                                            </span>
                                        </td>
                                        <td className="px-5 py-3.5 text-sm text-muted-foreground">{user.educationUnitName || '—'}</td>
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

            {/* ── Create modal ── */}
            {isCreateOpen && (
                <Modal title="Новый пользователь" onClose={() => setIsCreateOpen(false)}>
                    <form onSubmit={submitCreate} className="space-y-4">

                        <Field label="Имя *">
                            <input
                                className={inputClass}
                                placeholder="Иван Иванов"
                                value={createForm.name}
                                onChange={e => setCreateForm(f => ({ ...f, name: e.target.value }))}
                            />
                        </Field>

                        <Field label="Email *">
                            <input
                                type="email"
                                className={inputClass}
                                placeholder="user@example.com"
                                value={createForm.email}
                                onChange={e => setCreateForm(f => ({ ...f, email: e.target.value }))}
                            />
                        </Field>

                        <Field label="Пароль *">
                            <input
                                type="password"
                                className={inputClass}
                                placeholder="Минимум 8 символов"
                                value={createForm.password}
                                onChange={e => setCreateForm(f => ({ ...f, password: e.target.value }))}
                            />
                        </Field>

                        <Field label="Роль">
                            <select
                                className={inputClass}
                                value={createForm.role}
                                onChange={e => setCreateForm(f => ({ ...f, role: e.target.value }))}
                            >
                                {roles.map(r => <option key={r} value={r}>{r}</option>)}
                            </select>
                        </Field>

                        {createError && (
                            <div className="rounded-xl border border-destructive/20 bg-destructive/10 px-3.5 py-2.5 text-sm font-medium text-destructive">
                                {createError}
                            </div>
                        )}

                        <div className="flex justify-end gap-2 pt-1">
                            <button
                                type="button"
                                onClick={() => setIsCreateOpen(false)}
                                className="rounded-xl border border-border px-4 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
                            >
                                Отмена
                            </button>
                            <button
                                type="submit"
                                disabled={isCreating}
                                className="flex items-center gap-2 rounded-xl bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow-sm shadow-primary/20 transition-all hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-50"
                            >
                                {isCreating ? 'Создаём...' : 'Создать'}
                            </button>
                        </div>

                    </form>
                </Modal>
            )}

            {/* ── Edit modal ── */}
            {isEditOpen && editingUser && (
                <Modal title="Редактировать пользователя" onClose={() => setIsEditOpen(false)}>
                    <div className="space-y-4">

                        <Field label="Имя">
                            <input
                                className={inputClass}
                                value={editingUser.name}
                                onChange={e => setEditingUser({ ...editingUser, name: e.target.value })}
                                placeholder="Имя"
                            />
                        </Field>

                        <Field label="Email">
                            <input
                                className={inputClass}
                                value={editingUser.email}
                                onChange={e => setEditingUser({ ...editingUser, email: e.target.value })}
                                placeholder="Email"
                            />
                        </Field>

                        <Field label="Роль">
                            <select
                                className={inputClass}
                                value={editingUser.role}
                                onChange={e => setEditingUser({ ...editingUser, role: e.target.value })}
                            >
                                {roles.map(r => <option key={r} value={r}>{r}</option>)}
                            </select>
                        </Field>

                        <Field label="Учебная единица">
                            <select
                                className={inputClass}
                                value={selectedEducationUnitId}
                                onChange={e => setSelectedEducationUnitId(e.target.value)}
                            >
                                <option value="">Выберите учебную единицу</option>
                                {educationUnits.map(u => (
                                    <option key={u.id} value={u.id}>{u.name} ({u.type})</option>
                                ))}
                            </select>
                        </Field>

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
                </Modal>
            )}

        </div>
    )
}

// ── Shared UI helpers ──────────────────────────────────────

function Modal({ title, onClose, children }: {
    title: string
    onClose: () => void
    children: React.ReactNode
}) {
    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/20 backdrop-blur-sm">
            <div className="w-full max-w-md rounded-2xl border border-border bg-card p-6 shadow-xl">
                <div className="mb-5 flex items-center justify-between">
                    <h2 className="text-lg font-semibold text-card-foreground">{title}</h2>
                    <button
                        onClick={onClose}
                        className="flex h-7 w-7 items-center justify-center rounded-lg text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
                    >
                        <X className="h-4 w-4" />
                    </button>
                </div>
                {children}
            </div>
        </div>
    )
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
    return (
        <div className="space-y-1.5">
            <label className="block text-sm font-medium text-foreground">{label}</label>
            {children}
        </div>
    )
}
