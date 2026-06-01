using System.Globalization;
using System.Text.Json;
using EduVote.Application.Analytics.Services;
using EduVote.DAL.Postgresql.Models.Analytics;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EduVote.Infrastructure.Analytics;

public class VotingReportPdfGenerator : IAnalyticsVotingReportPdfGenerator
{
    private const int OpenAnswerPreviewLimit = 30;

    public byte[] Generate(VotingReportData data)
    {
        QuestPdfFontSetup.EnsureInitialized();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily(QuestPdfFontSetup.FontFamily).FontSize(10));

                page.Header().Element(c => ComposeHeader(c, data));
                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(14);
                    col.Item().Element(c => ComposeParticipation(c, data));
                    col.Item().Element(c => ComposeResults(c, data));
                    if (!string.IsNullOrEmpty(data.ResultHash))
                        col.Item().Element(c => ComposeVerification(c, data));
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Сформировано автоматически EduVote · ");
                    text.Span(DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm", CultureInfo.GetCultureInfo("ru-RU")));
                    text.Span(" UTC");
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, VotingReportData data)
    {
        container.Column(col =>
        {
            col.Item().Text("EduVote — отчёт по голосованию").Bold().FontSize(16);
            col.Item().PaddingTop(6).Text(data.Title).Bold().FontSize(14);
            if (!string.IsNullOrWhiteSpace(data.Description))
                col.Item().Text(data.Description).FontColor(Colors.Grey.Darken2);
            col.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text($"Тип: {TypeLabel(data.Type)}");
                row.RelativeItem().Text($"Статус: {StatusLabel(data.Status)}");
            });
            col.Item().Text(
                $"Период: {FmtDate(data.StartTime)} — {FmtDate(data.EndTime)} · " + ("Анонимное"));
        });
    }

    private static void ComposeParticipation(IContainer container, VotingReportData data)
    {
        container.Column(col =>
        {
            col.Item().Text("Участие").Bold().FontSize(12);
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);
                    c.RelativeColumn();
                });
                AddRow(table, "Подано голосов", data.TotalVotes.ToString(CultureInfo.InvariantCulture));
                AddRow(table, "Допущено к голосованию", data.EligibleCount.ToString(CultureInfo.InvariantCulture));
                AddRow(table, "Явка", $"{data.TurnoutPercent.ToString("F1", CultureInfo.InvariantCulture)} %");
            });

            col.Item().PaddingTop(8).Text("По подразделениям").SemiBold();

            if (data.UnitBreakdown.Count > 0)
            {
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });
                    table.Header(h =>
                    {
                        h.Cell().Element(CellHeader).Text("Подразделение");
                        h.Cell().Element(CellHeader).Text("Допущено");
                        h.Cell().Element(CellHeader).Text("Голосов");
                    });
                    foreach (var row in data.UnitBreakdown)
                    {
                        table.Cell().Element(CellBody).Text(row.EducationUnitName);
                        table.Cell().Element(CellBody).Text(row.EligibleCount.ToString(CultureInfo.InvariantCulture));
                        table.Cell().Element(CellBody).Text(row.VotesCount.ToString(CultureInfo.InvariantCulture));
                    }
                });
            }
            else
            {
                col.Item().PaddingTop(4).Text(
                    "Ограничения по подразделениям не заданы. Голосование доступно всем пользователям системы.")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            }
        });
    }

    private static void ComposeResults(IContainer container, VotingReportData data)
    {
        container.Column(col =>
        {
            col.Item().Text("Результаты").Bold().FontSize(12);
            if (string.IsNullOrWhiteSpace(data.ResultDataJson))
            {
                col.Item().Text("Нет данных результатов.");
                return;
            }

            try
            {
                using var doc = JsonDocument.Parse(data.ResultDataJson);
                if (data.Type == "OpenAnswer")
                    ComposeOpenAnswer(col, doc);
                else
                    ComposeCandidateResults(col, doc, data.Type);
            }
            catch
            {
                col.Item().Text("Не удалось разобрать данные результатов.");
            }
        });
    }

    private static void ComposeCandidateResults(ColumnDescriptor col, JsonDocument doc, string type)
    {
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3);
                c.RelativeColumn();
                c.RelativeColumn();
            });

            var headers = type switch
            {
                "Rating" => ("Кандидат", "Средняя оценка", "Оценок"),
                "MultipleChoice" => ("Кандидат", "Выборов", "%"),
                _ => ("Кандидат", "Голосов", "%"),
            };

            table.Header(h =>
            {
                h.Cell().Element(CellHeader).Text(headers.Item1);
                h.Cell().Element(CellHeader).Text(headers.Item2);
                h.Cell().Element(CellHeader).Text(headers.Item3);
            });

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var el = prop.Value;
                var name = el.TryGetProperty("candidateName", out var n) ? n.GetString() ?? prop.Name : prop.Name;

                string col2;
                string col3;
                if (type == "Rating")
                {
                    col2 = el.TryGetProperty("averageRating", out var ar)
                        ? ar.GetDouble().ToString("F2", CultureInfo.InvariantCulture)
                        : "0";
                    col3 = el.TryGetProperty("totalRatings", out var tr)
                        ? tr.GetInt32().ToString(CultureInfo.InvariantCulture)
                        : "0";
                }
                else if (type == "MultipleChoice")
                {
                    col2 = el.TryGetProperty("selectionCount", out var sc)
                        ? sc.GetInt32().ToString(CultureInfo.InvariantCulture)
                        : "0";
                    col3 = el.TryGetProperty("percentage", out var p)
                        ? $"{p.GetDouble().ToString("F1", CultureInfo.InvariantCulture)} %"
                        : "0 %";
                }
                else
                {
                    col2 = el.TryGetProperty("voteCount", out var vc)
                        ? vc.GetInt32().ToString(CultureInfo.InvariantCulture)
                        : "0";
                    col3 = el.TryGetProperty("percentage", out var p)
                        ? $"{p.GetDouble().ToString("F1", CultureInfo.InvariantCulture)} %"
                        : "0 %";
                }

                table.Cell().Element(CellBody).Text(name);
                table.Cell().Element(CellBody).Text(col2);
                table.Cell().Element(CellBody).Text(col3);
            }
        });
    }

    private static void ComposeOpenAnswer(ColumnDescriptor col, JsonDocument doc)
    {
        if (!doc.RootElement.TryGetProperty("openAnswer", out var open))
        {
            col.Item().Text("Нет текстовых ответов.");
            return;
        }

        var total = open.TryGetProperty("totalAnswers", out var t) ? t.GetInt32() : 0;
        col.Item().Text($"Всего ответов: {total}");

        if (!open.TryGetProperty("answers", out var answers) || answers.ValueKind != JsonValueKind.Array)
            return;

        var list = answers.EnumerateArray()
            .Select(a => a.GetString() ?? "")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        var shown = list.Take(OpenAnswerPreviewLimit).ToList();
        col.Item().PaddingTop(4).Table(table =>
        {
            table.ColumnsDefinition(c => c.RelativeColumn());
            table.Header(h => h.Cell().Element(CellHeader).Text("Ответ"));
            foreach (var answer in shown)
                table.Cell().Element(CellBody).Text(answer);
        });

        if (list.Count > OpenAnswerPreviewLimit)
        {
            col.Item().Text($"… и ещё {list.Count - OpenAnswerPreviewLimit} ответ(ов).")
                .FontColor(Colors.Grey.Darken1);
        }
    }

    private static void ComposeVerification(IContainer container, VotingReportData data)
    {
        container.Column(col =>
        {
            col.Item().Text("Верификация голосования").Bold().FontSize(12);
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn(3);
                });
                AddRow(table, "Алгоритм хеширования", "SHA-256");
                AddRow(table, "Хеш результатов голосования", data.ResultHash ?? "—");
                if (!string.IsNullOrEmpty(data.TxHash))
                    AddRow(table, "Хеш блокчейн транзакции", data.TxHash);
                if (!string.IsNullOrEmpty(data.EtherscanUrl))
                    AddHyperlinkRow(table, "Etherscan", data.EtherscanUrl);
            });
            col.Item().PaddingTop(4).Text(
                "Полный список vote_hashes доступен через API верификации; в PDF не включается.")
                .FontSize(8).FontColor(Colors.Grey.Darken1);
        });
    }

    private static void AddRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Element(CellBody).Text(label).SemiBold();
        table.Cell().Element(CellBody).Text(value);
    }

    private static void AddHyperlinkRow(TableDescriptor table, string label, string url)
    {
        table.Cell().Element(CellBody).Text(label).SemiBold();
        table.Cell().Element(CellBody).Text(text =>
        {
            text.Hyperlink("Посмотреть на Etherscan", url)
                .Underline()
                .FontColor(Colors.Blue.Darken2);
        });
    }

    private static IContainer CellHeader(IContainer c) =>
        c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);

    private static IContainer CellBody(IContainer c) =>
        c.PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);

    private static string FmtDate(DateTime dt) =>
        dt.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("ru-RU"));

    private static string TypeLabel(string type) => type switch
    {
        "SingleChoice" => "Один вариант",
        "MultipleChoice" => "Несколько вариантов",
        "Rating" => "Рейтинг",
        "OpenAnswer" => "Свободный ответ",
        _ => type,
    };

    private static string StatusLabel(string status) => status switch
    {
        "Draft" => "Черновик",
        "Active" => "Активно",
        "Paused" => "Приостановлено",
        "Finished" => "Завершено",
        "PendingApproval" => "На согласовании",
        _ => status,
    };
}
