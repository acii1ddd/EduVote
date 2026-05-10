import { useEffect, useState } from "react"
import {deleteUserRequest, getUsers, updateUser, type UserResponse} from "@/api/userApi.ts";
import {getRoles} from "@/api/roleApi.ts";
import {type EducationUnit, getEducationUnits} from "@/api/educationUnitApi.ts";
import {assignUserToEducationUnit} from "@/api/assignUserToEducationUnit.ts";

export default function AdminPage() {

    const [users, setUsers] = useState<UserResponse[]>([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)
    const [roles, setRoles] = useState<string[]>([])
    const [educationUnits, setEducationUnits] = useState<EducationUnit[]>([])

    const [selectedEducationUnitId, setSelectedEducationUnitId] = useState<string>("")
    
    useEffect(() => {
        loadUsers()
        loadRoles()
    }, [])

    const loadRoles = async () => {
        const data = await getRoles()
        setRoles(data.roles.map(r => r.name))
    }

    useEffect(() => {
        loadUsers()
        loadEducationUnits()
        loadRoles()
    }, [])

    const loadEducationUnits = async () => {
        const data = await getEducationUnits()
        setEducationUnits(data.educationUnits)
    }
    
    // form state
    // const [email, setEmail] = useState('')
    // const [password, setPassword] = useState('')
    // const [role, setRole] = useState('User')

    const [isEditOpen, setIsEditOpen] = useState(false)
    const [editingUser, setEditingUser] = useState<UserResponse | null>(null)

    const openEdit = (user: UserResponse) => {
        setEditingUser(user)
        setIsEditOpen(true)
    }

    const saveUser = async () => {

        if (!editingUser) return

        try {

            // 1. update basic user info
            await updateUser({
                id: editingUser.id,
                name: editingUser.name,
                email: editingUser.email,
                role: editingUser.role
            })

            // 2. assign education unit
            if (selectedEducationUnitId) {

                await assignUserToEducationUnit(
                    editingUser.id,
                    selectedEducationUnitId
                )
            }

            // 3. reload users from backend
            await loadUsers()

            // 4. close modal
            setIsEditOpen(false)
            setEditingUser(null)

        } catch (err) {

            console.error(err)
        }
    }
    
    useEffect(() => {
        loadUsers()
    }, [])

    const loadUsers = async () => {

        try {

            setLoading(true)

            const data = await getUsers()

            setUsers(data.users)

        } catch (err: any) {

            setError(
                err?.response?.data?.message || String(err)
            )

        } finally {

            setLoading(false)
        }
    }

    // const addUser = async (
    //     e: React.FormEvent
    // ) => {
    //
    //     e.preventDefault()
    //
    //     if (!email.trim() || !password.trim()) {
    //         return
    //     }
    //
    //     try {
    //
    //         // TODO:
    //         // backend request for create user
    //
    //         const newUser: UserResponse = {
    //             id: crypto.randomUUID(),
    //             email,
    //             role,
    //             createdAt: new Date().toISOString()
    //         }
    //
    //         setUsers(prev => [...prev, newUser])
    //
    //         setEmail('')
    //         setPassword('')
    //         setRole('User')
    //
    //     } catch (err) {
    //         console.error(err)
    //     }
    // }

    const deleteUser = async (
        userId: string
    ) => {

        const confirmed = window.confirm(
            "Вы уверены, что хотите удалить пользователя?"
        )

        if (!confirmed) {
            return
        }

        try {

            await deleteUserRequest(userId)

            setUsers(prev =>
                prev.filter(x => x.id !== userId)
            )

        } catch (err) {

            console.error(err)
        }
    }

    return (
        <div className="space-y-8">

            {/* Page title */}
            <div>
                <h1 className="text-3xl font-bold tracking-tight">
                    Admin Panel
                </h1>

                <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
                    Управление пользователями системы
                </p>
            </div>

            {/* Add user */}
            {/*<section className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm dark:border-gray-800 dark:bg-gray-900">*/}
            
            {/*    <h2 className="mb-5 text-xl font-semibold">*/}
            {/*        Добавить пользователя*/}
            {/*    </h2>*/}
            
            {/*    <form*/}
            {/*        onSubmit={addUser}*/}
            {/*        className="grid gap-4 md:grid-cols-4"*/}
            {/*    >*/}
            
            {/*        <input*/}
            {/*            type="email"*/}
            {/*            placeholder="Email"*/}
            {/*            value={email}*/}
            {/*            onChange={(e) =>*/}
            {/*                setEmail(e.target.value)*/}
            {/*            }*/}
            {/*            className="*/}
            {/*                h-11 rounded-lg border border-gray-300*/}
            {/*                bg-transparent px-3 text-sm*/}
            {/*                focus:outline-none focus:ring-2*/}
            {/*                focus:ring-indigo-500*/}
            {/*                dark:border-gray-700*/}
            {/*            "*/}
            {/*        />*/}
            
            {/*        <input*/}
            {/*            type="password"*/}
            {/*            placeholder="Password"*/}
            {/*            value={password}*/}
            {/*            onChange={(e) =>*/}
            {/*                setPassword(e.target.value)*/}
            {/*            }*/}
            {/*            className="*/}
            {/*                h-11 rounded-lg border border-gray-300*/}
            {/*                bg-transparent px-3 text-sm*/}
            {/*                focus:outline-none focus:ring-2*/}
            {/*                focus:ring-indigo-500*/}
            {/*                dark:border-gray-700*/}
            {/*            "*/}
            {/*        />*/}
            
            {/*        <select*/}
            {/*            value={role}*/}
            {/*            onChange={(e) =>*/}
            {/*                setRole(e.target.value)*/}
            {/*            }*/}
            {/*            className="*/}
            {/*                h-11 rounded-lg border border-gray-300*/}
            {/*                bg-transparent px-3 text-sm*/}
            {/*                focus:outline-none focus:ring-2*/}
            {/*                focus:ring-indigo-500*/}
            {/*                dark:border-gray-700*/}
            {/*            "*/}
            {/*        >*/}
            {/*            <option value="User">*/}
            {/*                User*/}
            {/*            </option>*/}
            
            {/*            <option value="Admin">*/}
            {/*                Admin*/}
            {/*            </option>*/}
            {/*        </select>*/}
            
            {/*        <button*/}
            {/*            type="submit"*/}
            {/*            className="*/}
            {/*                inline-flex items-center justify-center*/}
            {/*                rounded-lg bg-indigo-600 px-4 py-2*/}
            {/*                text-sm font-medium text-white*/}
            {/*                transition hover:bg-indigo-500*/}
            {/*            "*/}
            {/*        >*/}
            {/*            Добавить*/}
            {/*        </button>*/}
            
            {/*    </form>*/}
            
            {/*</section>*/}

            {/* Users table */}
            <section className="rounded-2xl border border-gray-200 bg-white shadow-sm dark:border-gray-800 dark:bg-gray-900">

                <div className="border-b border-gray-200 px-6 py-4 dark:border-gray-800">

                    <h2 className="text-xl font-semibold">
                        Пользователи
                    </h2>

                </div>

                {loading ? (

                    <div className="p-6 text-sm text-gray-500">
                        Загрузка пользователей...
                    </div>

                ) : error ? (

                    <div className="p-6 text-sm text-red-500">
                        {error}
                    </div>

                ) : (

                    <div className="overflow-x-auto">

                        <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-800">

                            <thead className="bg-gray-50 dark:bg-gray-800/50">

                            <tr>

                                <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">
                                    Name
                                </th>

                                <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">
                                    Email
                                </th>

                                <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">
                                    Role
                                </th>

                                <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">
                                    educationUnitName
                                </th>
                                
                                <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">
                                    Created
                                </th>

                            </tr>

                            </thead>

                            <tbody className="divide-y divide-gray-200 dark:divide-gray-800">

                            {users.map(user => (

                                <tr
                                    key={user.id}
                                    className="hover:bg-gray-50 dark:hover:bg-gray-800/40"
                                >
                                    <td className="px-6 text-left py-4 text-sm">
                                        {user.name}
                                    </td>

                                    <td className="px-6 text-left py-4 text-sm">
                                        {user.email}
                                    </td>

                                    <td className="px-6 text-left py-4 text-sm">
                                        <span className="
                                            rounded-full bg-indigo-100
                                            px-2.5 py-1 text-xs font-medium
                                            text-indigo-700
                                            dark:bg-indigo-900/40
                                            dark:text-indigo-300
                                        ">
                                            {user.role}
                                        </span>
                                    </td>

                                    <td className="px-6 text-left py-4 text-sm">
                                        {user.educationUnitName}
                                    </td>
                                    
                                    <td className="px-6 text-left py-4 text-sm text-gray-500">
                                        {new Date(user.createdAt)
                                            .toLocaleDateString()}
                                    </td>

                                    <td className="px-6 py-4 text-right">

                                        <button
                                            onClick={() =>
                                                deleteUser(user.id)
                                            }
                                            className="
                                                rounded-lg bg-red-500
                                                px-3 py-2 text-sm
                                                font-medium text-white
                                                transition hover:bg-red-400
                                            "
                                        >
                                            Delete
                                        </button>

                                    </td>
                                    
                                    <td className="px-6 py-4 text-right">
                                        <button
                                            onClick={() => openEdit(user)}
                                            className="mr-2 rounded-lg bg-yellow-500 px-3 py-2 text-sm font-medium text-white hover:bg-yellow-400"
                                        >
                                            Edit
                                        </button>
                                    </td>
                                    
                                </tr>

                            ))}

                            </tbody>

                        </table>

                    </div>

                )}

            </section>

            {isEditOpen && editingUser && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">

                    <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-lg dark:bg-gray-900 space-y-4">

                        <h2 className="text-xl font-semibold">
                            Edit user
                        </h2>

                        {/* NAME */}
                        <input
                            className="w-full rounded-lg border px-3 py-2 dark:bg-gray-800"
                            value={editingUser.name}
                            onChange={(e) =>
                                setEditingUser({
                                    ...editingUser,
                                    name: e.target.value
                                })
                            }
                            placeholder="Name"
                        />

                        {/* EMAIL */}
                        <input
                            className="w-full rounded-lg border px-3 py-2 dark:bg-gray-800"
                            value={editingUser.email}
                            onChange={(e) =>
                                setEditingUser({
                                    ...editingUser,
                                    email: e.target.value
                                })
                            }
                            placeholder="Email"
                        />

                        {/* ROLE */}
                        <select
                            className="w-full rounded-lg border px-3 py-2 dark:bg-gray-800"
                            value={editingUser.role}
                            onChange={(e) =>
                                setEditingUser({
                                    ...editingUser,
                                    role: e.target.value
                                })
                            }
                        >
                            {roles.map(role => (
                                <option key={role} value={role}>
                                    {role}
                                </option>
                            ))}
                        </select>

                        <select
                            className="w-full rounded-lg border px-3 py-2 dark:bg-gray-800"
                            value={selectedEducationUnitId}
                            onChange={(e) => setSelectedEducationUnitId(e.target.value)}
                        >
                            <option value="">
                                Select education unit
                            </option>

                            {educationUnits.map(unit => (
                                <option key={unit.id} value={unit.id}>
                                    {unit.name} ({unit.type})
                                </option>
                            ))}
                        </select>

                        {/* ACTIONS */}
                        <div className="flex justify-end gap-2">

                            <button
                                onClick={() => setIsEditOpen(false)}
                                className="rounded-lg bg-gray-500 px-4 py-2 text-white hover:bg-gray-400"
                            >
                                Cancel
                            </button>

                            <button
                                onClick={saveUser}
                                className="rounded-lg bg-indigo-600 px-4 py-2 text-white hover:bg-indigo-500"
                            >
                                Save
                            </button>

                        </div>

                    </div>

                </div>
            )}
        </div>
    )
}