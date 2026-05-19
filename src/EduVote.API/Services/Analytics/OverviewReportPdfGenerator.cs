using System.Globalization;
using EduVote.DAL.Postgresql.Models.Analytics;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EduVote.API.Services.Analytics;

public class OverviewReportPdfGenerator : IOverviewReportPdfGenerator
{
    public byte[] Generate(AnalyticsOverviewData overview, AnalyticsFilters filters)
    {
        QuestPdfFontSetup.EnsureInitialized();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily(QuestPdfFontSetup.FontFamily).FontSize(10));

                page.Content().Column(col =>
                {
                    col.Spacing(12);
                    col.Item().Text("EduVote — сводная аналитика").Bold().FontSize(16);
                    col.Item().Text(BuildPeriodLabel(filters)).FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingTop(8).Text("Ключевые показатели").Bold().FontSize(12);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn();
                        });
                        AddMetric(table, "Всего голосований", overview.TotalVotings);
                        AddMetric(table, "Активных", overview.ActiveVotings);
                        AddMetric(table, "На согласовании", overview.PendingApprovalVotings);
                        AddMetric(table, "Завершённых", overview.FinishedVotings);
                        AddMetric(table, "Черновиков", overview.DraftVotings);
                        AddMetric(table, "Приостановленных", overview.PausedVotings);
                        AddMetric(table, "Подано голосов", overview.TotalVotesCast);
                    });

                    if (overview.StatusCounts.Count > 0)
                    {
                        col.Item().Text("По статусам").SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.ConstantColumn(60);
                            });
                            foreach (var row in overview.StatusCounts)
                            {
                                table.Cell().Text(row.Status);
                                table.Cell().AlignRight().Text(row.Count.ToString(CultureInfo.InvariantCulture));
                            }
                        });
                    }

                    if (overview.TypeCounts.Count > 0)
                    {
                        col.Item().Text("По типам").SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.ConstantColumn(60);
                            });
                            foreach (var row in overview.TypeCounts)
                            {
                                table.Cell().Text(row.Type);
                                table.Cell().AlignRight().Text(row.Count.ToString(CultureInfo.InvariantCulture));
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(
                    $"Сформировано {DateTime.UtcNow:dd.MM.yyyy HH:mm} UTC");
            });
        });

        return document.GeneratePdf();
    }

    private static void AddMetric(TableDescriptor table, string label, int value)
    {
        table.Cell().Text(label);
        table.Cell().AlignRight().Text(value.ToString(CultureInfo.InvariantCulture));
    }

    private static string BuildPeriodLabel(AnalyticsFilters filters)
    {
        var from = filters.DateFrom?.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("ru-RU")) ?? "—";
        var to = filters.DateTo?.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("ru-RU")) ?? "—";
        var unit = filters.EducationUnitId.HasValue
            ? $" · подразделение {filters.EducationUnitId}"
            : "";
        return $"Период: {from} — {to}{unit}";
    }
}
