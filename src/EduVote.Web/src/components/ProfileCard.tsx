interface Field {
    label: string
    value: string
    highlight?: boolean
}

interface Props {
    fields: Field[]
}

export default function ProfileCard({ fields }: Props) {
    return (
        <div className="rounded-xl border border-border bg-card px-5 py-4">
            <div className="flex flex-wrap gap-x-8 gap-y-3">
                {fields.map(f => (
                    <div key={f.label}>
                        <p className="text-xs text-muted-foreground">{f.label}</p>
                        {f.highlight ? (
                            <span className="mt-1 inline-flex items-center rounded-full bg-primary/10 px-2.5 py-0.5 text-xs font-semibold text-primary">
                                {f.value}
                            </span>
                        ) : (
                            <p className="mt-0.5 text-sm font-medium text-foreground">{f.value}</p>
                        )}
                    </div>
                ))}
            </div>
        </div>
    )
}
