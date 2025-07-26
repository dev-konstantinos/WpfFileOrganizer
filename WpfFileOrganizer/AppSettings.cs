using System;
using System.IO;
using System.Text.Json;

namespace WpfFileOrganizer
{
    /// <summary>
    /// Represents selected folder settings for organizing files into specific folders.
    /// </summary>
    /// <remarks>This class provides properties to configure folder paths for different file types, such as
    /// images, videos, texts, tables, PDFs, and others. It also includes methods to load and save these settings to a
    /// JSON file stored in the application's data directory.</remarks>
    public class AppSettings
    {
        public string SourceFolder { get; set; }
        public string ImagesFolder { get; set; }
        public string VideosFolder { get; set; }
        public string TextsFolder { get; set; }
        public string TablesFolder { get; set; }
        public string PdfsFolder { get; set; }
        public string OthersFolder { get; set; }

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WpfFileOrganizer", "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json);
                }
            }
            catch { }

            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }
    }
}
