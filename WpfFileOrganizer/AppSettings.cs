using System;
using System.IO;
using System.Text.Json;

namespace WpfFileOrganizer
{
    /// <summary>
    /// Represents selected folder settings for organizing files into specific folders.
    /// </summary>
    /// <remarks>
    /// This class provides properties to configure folder paths for different file types, such as images, videos, texts,
    /// tables, PDFs, and others. It includes methods to load and save these settings to a JSON file stored in the
    /// application's data directory. The settings are persisted using the <see cref="System.Text.Json"/> serializer.
    /// </remarks>
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the source folder path where files to be organized are located.
        /// </summary>
        public string SourceFolder { get; set; }

        /// <summary>
        /// Gets or sets the destination folder path for image files.
        /// </summary>
        public string ImagesFolder { get; set; }

        /// <summary>
        /// Gets or sets the destination folder path for video files.
        /// </summary>
        public string VideosFolder { get; set; }

        /// <summary>
        /// Gets or sets the destination folder path for text files.
        /// </summary>
        public string TextsFolder { get; set; }

        /// <summary>
        /// Gets or sets the destination folder path for table or spreadsheet files.
        /// </summary>
        public string TablesFolder { get; set; }

        /// <summary>
        /// Gets or sets the destination folder path for PDF files.
        /// </summary>
        public string PdfsFolder { get; set; }

        /// <summary>
        /// Gets or sets the destination folder path for files that do not match any predefined category.
        /// </summary>
        public string OthersFolder { get; set; }

        /// <summary>
        /// The full file path where the settings are stored as a JSON file.
        /// </summary>
        /// <remarks>
        /// The path is constructed using the application's data directory (<see cref="Environment.SpecialFolder.ApplicationData"/>)
        /// and a subdirectory "WpfFileOrganizer" with the file name "settings.json".
        /// </remarks>
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WpfFileOrganizer", "settings.json");

        /// <summary>
        /// Loads the application settings from the JSON file.
        /// </summary>
        /// <returns>An instance of <see cref="AppSettings"/> containing the loaded settings, or a new instance if the
        /// file does not exist or an error occurs.</returns>
        /// <remarks>
        /// If the settings file exists at <see cref="SettingsPath"/>, it is deserialized into an <see cref="AppSettings"/>
        /// object using <see cref="JsonSerializer.Deserialize{TValue}(string)"/>. If the file is missing or an exception
        /// occurs (e.g., invalid JSON), a new <see cref="AppSettings"/> instance is returned with default values.
        /// </remarks>
        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch { }

            return new AppSettings();
        }

        /// <summary>
        /// Saves the current application settings to the JSON file.
        /// </summary>
        /// <remarks>
        /// The method serializes the current instance into a JSON string with indentation for readability and writes it
        /// to <see cref="SettingsPath"/>. If the directory does not exist, it is created. Errors during saving are silently
        /// ignored to prevent application crashes.
        /// </remarks>
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