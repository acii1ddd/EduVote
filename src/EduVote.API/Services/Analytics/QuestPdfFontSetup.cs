using QuestPDF.Drawing;

namespace EduVote.API.Services.Analytics;

internal static class QuestPdfFontSetup
{
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized)
            return;

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var fontPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "DejaVuSans.ttf");
        if (File.Exists(fontPath))
            FontManager.RegisterFont(File.OpenRead(fontPath));

        _initialized = true;
    }

    public const string FontFamily = "DejaVu Sans";
}
