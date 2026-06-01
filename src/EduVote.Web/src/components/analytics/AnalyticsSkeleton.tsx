export default function AnalyticsSkeleton() {
    return (
        <div className="space-y-8 animate-pulse">
            <div className="h-10 w-64 rounded-xl bg-muted" />
            <div className="h-32 rounded-2xl bg-muted" />
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                {Array.from({ length: 7 }).map((_, i) => (
                    <div key={i} className="h-24 rounded-2xl bg-muted" />
                ))}
            </div>
            <div className="grid gap-4 lg:grid-cols-2">
                {Array.from({ length: 4 }).map((_, i) => (
                    <div key={i} className="h-72 rounded-2xl bg-muted" />
                ))}
            </div>
            <div className="h-48 rounded-2xl bg-muted" />
            <div className="h-80 rounded-2xl bg-muted" />
        </div>
    )
}
