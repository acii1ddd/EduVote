export function canViewOpenAnswerTexts(
    role: string | undefined,
    userId: string | undefined,
    createdById: string,
): boolean {
    if (role === 'Administrator')
        return true

    if (role === 'Teacher' && userId && userId === createdById)
        return true

    return false
}
