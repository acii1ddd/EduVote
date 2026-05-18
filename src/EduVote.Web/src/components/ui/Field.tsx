interface FieldProps {
    label: string
    children: React.ReactNode
}

export function Field({ label, children }: FieldProps) {
    return (
        <div className="space-y-1.5">
            <label className="block text-sm font-medium text-foreground">{label}</label>
            {children}
        </div>
    )
}

interface CheckboxFieldProps {
    label: string
    checked: boolean
    onChange: (v: boolean) => void
}

export function CheckboxField({ label, checked, onChange }: CheckboxFieldProps) {
    return (
        <label className="flex items-center gap-2.5 cursor-pointer select-none">
            <input
                type="checkbox"
                checked={checked}
                onChange={e => onChange(e.target.checked)}
                className="h-4 w-4 rounded border-border text-primary accent-primary cursor-pointer"
            />
            <span className="text-sm text-foreground">{label}</span>
        </label>
    )
}
