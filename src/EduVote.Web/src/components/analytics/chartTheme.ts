export const CHART_COLORS = [
    '#4F46E5',
    '#6366F1',
    '#818CF8',
    '#A5B4FC',
    '#10B981',
    '#F59E0B',
    '#EC4899',
    '#8B5CF6',
]

export interface ChartTheme {
    tick: string
    grid: string
    tooltipBg: string
    tooltipBorder: string
    tooltipFg: string
}

export function getChartTheme(isDark: boolean): ChartTheme {
    if (isDark) {
        return {
            tick: '#9CA3CF',
            grid: '#2A2D4A',
            tooltipBg: '#13152A',
            tooltipBorder: '#2A2D4A',
            tooltipFg: '#E8EAFF',
        }
    }
    return {
        tick: '#6366A8',
        grid: '#DDE1F5',
        tooltipBg: '#FFFFFF',
        tooltipBorder: '#DDE1F5',
        tooltipFg: '#0E1020',
    }
}

export const tooltipContentStyle = (theme: ChartTheme) => ({
    backgroundColor: theme.tooltipBg,
    borderColor: theme.tooltipBorder,
    color: theme.tooltipFg,
    borderRadius: '0.75rem',
    fontSize: '0.8125rem',
})
