import {
    Bar,
    BarChart,
    CartesianGrid,
    ResponsiveContainer,
    Tooltip,
    XAxis,
    YAxis,
} from 'recharts'
import { TYPE_AXIS_LABELS, TYPE_LABELS } from '@/components/voting/votingConstants'
import type { VotingType } from '@/api/votingApi'
import { getChartTheme, tooltipContentStyle } from './chartTheme'
import AnalyticsChartCard from './AnalyticsChartCard'

interface TypeDistributionChartProps {
    data: { type: string; count: number }[]
    isDark: boolean
}

type ChartRow = {
    shortName: string
    fullName: string
    count: number
}

export default function TypeDistributionChart({ data, isDark }: TypeDistributionChartProps) {
    const theme = getChartTheme(isDark)
    const chartData: ChartRow[] = data.map(row => {
        const type = row.type as VotingType
        const fullName = TYPE_LABELS[type] ?? row.type
        return {
            shortName: TYPE_AXIS_LABELS[type] ?? fullName,
            fullName,
            count: row.count,
        }
    })
    const isEmpty = chartData.length === 0 || chartData.every(d => d.count === 0)

    return (
        <AnalyticsChartCard title="Распределение по типам" isEmpty={isEmpty}>
            <ResponsiveContainer width="100%" height={260}>
                <BarChart data={chartData} margin={{ top: 8, right: 8, left: -16, bottom: 28 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke={theme.grid} vertical={false} />
                    <XAxis
                        dataKey="shortName"
                        interval={0}
                        tick={{ fill: theme.tick, fontSize: 11 }}
                        tickLine={false}
                        axisLine={{ stroke: theme.grid }}
                    />
                    <YAxis
                        allowDecimals={false}
                        tick={{ fill: theme.tick, fontSize: 11 }}
                        tickLine={false}
                        axisLine={false}
                    />
                    <Tooltip
                        contentStyle={tooltipContentStyle(theme)}
                        labelFormatter={(_, payload) => {
                            const row = payload?.[0]?.payload as ChartRow | undefined
                            return row?.fullName ?? ''
                        }}
                        formatter={value => [value ?? 0, 'Голосований']}
                    />
                    <Bar dataKey="count" fill="#4F46E5" radius={[6, 6, 0, 0]} />
                </BarChart>
            </ResponsiveContainer>
        </AnalyticsChartCard>
    )
}
