import {
    Cell,
    Legend,
    Pie,
    PieChart,
    ResponsiveContainer,
    Tooltip,
} from 'recharts'
import { STATUS_CONFIG } from '@/components/voting/votingConstants'
import type { VotingStatus } from '@/api/votingApi'
import { CHART_COLORS, getChartTheme, tooltipContentStyle } from './chartTheme'
import AnalyticsChartCard from './AnalyticsChartCard'

interface StatusDistributionChartProps {
    data: { status: string; count: number }[]
    isDark: boolean
}

export default function StatusDistributionChart({ data, isDark }: StatusDistributionChartProps) {
    const theme = getChartTheme(isDark)
    const chartData = data.map(row => ({
        name: STATUS_CONFIG[row.status as VotingStatus]?.label ?? row.status,
        value: row.count,
        status: row.status,
    }))
    const isEmpty = chartData.length === 0 || chartData.every(d => d.value === 0)

    return (
        <AnalyticsChartCard title="Распределение по статусам" isEmpty={isEmpty}>
            <ResponsiveContainer width="100%" height={260}>
                <PieChart>
                    <Pie
                        data={chartData}
                        dataKey="value"
                        nameKey="name"
                        cx="50%"
                        cy="50%"
                        innerRadius={50}
                        outerRadius={85}
                        paddingAngle={2}
                    >
                        {chartData.map((_, index) => (
                            <Cell key={index} fill={CHART_COLORS[index % CHART_COLORS.length]} />
                        ))}
                    </Pie>
                    <Tooltip
                        contentStyle={tooltipContentStyle(theme)}
                        formatter={value => [value ?? 0, 'Количество']}
                    />
                    <Legend
                        wrapperStyle={{ fontSize: '0.75rem', color: theme.tick }}
                    />
                </PieChart>
            </ResponsiveContainer>
        </AnalyticsChartCard>
    )
}
