import {
    Area,
    AreaChart,
    CartesianGrid,
    ResponsiveContainer,
    Tooltip,
    XAxis,
    YAxis,
} from 'recharts'
import { formatWeekPeriod } from '@/services/analyticsService'
import { getChartTheme, tooltipContentStyle } from './chartTheme'
import AnalyticsChartCard from './AnalyticsChartCard'

interface TimeSeriesChartProps {
    title: string
    chartId: string
    data: { period: string; count: number }[]
    isDark: boolean
    color?: string
}

export default function TimeSeriesChart({
    title,
    chartId,
    data,
    isDark,
    color = '#4F46E5',
}: TimeSeriesChartProps) {
    const theme = getChartTheme(isDark)
    const chartData = data.map(row => ({
        period: row.period,
        label: formatWeekPeriod(row.period),
        count: row.count,
    }))
    const isEmpty = chartData.length === 0

    return (
        <AnalyticsChartCard title={title} isEmpty={isEmpty}>
            <ResponsiveContainer width="100%" height={260}>
                <AreaChart data={chartData} margin={{ top: 8, right: 8, left: -16, bottom: 0 }}>
                    <defs>
                        <linearGradient id={`gradient-${chartId}`} x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor={color} stopOpacity={0.35} />
                            <stop offset="95%" stopColor={color} stopOpacity={0} />
                        </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke={theme.grid} vertical={false} />
                    <XAxis
                        dataKey="label"
                        tick={{ fill: theme.tick, fontSize: 10 }}
                        tickLine={false}
                        axisLine={{ stroke: theme.grid }}
                        interval="preserveStartEnd"
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
                            const row = payload?.[0]?.payload as { label?: string } | undefined
                            return row?.label ?? ''
                        }}
                        formatter={value => [value ?? 0, 'Количество']}
                    />
                    <Area
                        type="monotone"
                        dataKey="count"
                        stroke={color}
                        strokeWidth={2}
                        fill={`url(#gradient-${chartId})`}
                    />
                </AreaChart>
            </ResponsiveContainer>
        </AnalyticsChartCard>
    )
}
