using System.Text.Json;
using Serilog;

namespace Cau3_KeHoachDaNgoai;

public sealed class UserSettings
{
    public string Language { get; set; } = "vi";
}

public static class UserSettingsStore
{
    private static readonly string Folder = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        "Cau3_KeHoachDaNgoai");

    private static readonly string FilePath =
        Path.Combine(Folder, "usersettings.json");

    public static UserSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return new UserSettings();

            var json = File.ReadAllText(FilePath);

            var settings =
                JsonSerializer.Deserialize<UserSettings>(json)
                ?? new UserSettings();

            if (settings.Language != "vi" &&
                settings.Language != "en")
            {
                settings.Language = "vi";
            }

            return settings;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Không đọc được UserSettings");
            return new UserSettings();
        }
    }

    public static void Save(string language)
    {
        try
        {
            Directory.CreateDirectory(Folder);

            var settings = new UserSettings
            {
                Language = language == "en" ? "en" : "vi"
            };

            var json = JsonSerializer.Serialize(
                settings,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Không lưu được UserSettings");
        }
    }
}
