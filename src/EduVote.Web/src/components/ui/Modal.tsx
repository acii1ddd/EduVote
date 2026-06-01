import { X } from 'lucide-react'

interface ModalProps {
    title: string
    onClose: () => void
    children: React.ReactNode
    maxWidth?: string
}

export default function Modal({ title, onClose, children, maxWidth = 'max-w-lg' }: ModalProps) {
    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/20 backdrop-blur-sm">
            <div className={`w-full ${maxWidth} rounded-2xl border border-border bg-card p-6 shadow-xl max-h-[90vh] overflow-y-auto`}>
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
