using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace WpfFileOrganizer
{
    /// <summary>
    /// Provides functionality to sort files from a source directory into categorized destination directories based on
    /// their file types.
    /// </summary>
    /// <remarks>The <see cref="FileSorter"/> class categorizes files into predefined groups such as Images,
    /// Videos, Texts, Tables, and PDFs, based on their file extensions. Files that do not match any predefined
    /// category are moved to a specified "others" directory. The class ensures that all destination directories are 
    /// created if they do not already exist.</remarks>
    public class FileSorter
    {
        private readonly Dictionary<string, string> _fileTypeMappings;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSorter"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor populates the internal <see cref="_fileTypeMappings"/> dictionary with predefined file
        /// extension-to-folder mappings by calling the <see cref="BuildFileTypeMappings"/> method.
        /// </remarks>
        public FileSorter()
        {
            _fileTypeMappings = BuildFileTypeMappings();
        }

        /// <summary>
        /// Builds a dictionary that maps file extensions to their respective folder names.
        /// </summary>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> where the key is a file extension (e.g., ".jpg") and the
        /// value is the corresponding folder name (e.g., "Images").</returns>
        /// <remarks>
        /// The method defines a set of categories (e.g., Images, Videos) and their associated file extensions. It then
        /// populates the dictionary with these mappings using case-insensitive comparison.
        /// </remarks>
        private static Dictionary<string, string> BuildFileTypeMappings()
        {
            var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var categories = new[]
            {
                ("Images", new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" }),
                ("Videos", new[] { ".mp4", ".mov", ".avi", ".mkv" }),
                ("Texts", new[] { ".txt", ".doc", ".docx", ".rtf" }),
                ("Tables", new[] { ".csv", ".xls", ".xlsx" }),
                ("PDFs", new[] { ".pdf" })
            };

            foreach (var (folder, extensions) in categories)
            {
                foreach (var ext in extensions)
                {
                    mappings[ext] = folder;
                }
            }
            return mappings;
        }

        /// <summary>
        /// Sorts files from the source directory into specified destination directories based on their file types.
        /// </summary>
        /// <param name="source">The source directory path from which files will be sorted.</param>
        /// <param name="images">The destination directory path for image files.</param>
        /// <param name="videos">The destination directory path for video files.</param>
        /// <param name="texts">The destination directory path for text files.</param>
        /// <param name="tables">The destination directory path for table/spreadsheet files.</param>
        /// <param name="pdfs">The destination directory path for PDF files.</param>
        /// <param name="others">The destination directory path for files that do not match any predefined category.</param>
        /// <exception cref="DirectoryNotFoundException">Thrown when the source directory does not exist.</exception>
        /// <remarks>
        /// This method recursively scans the source directory and its subdirectories for files. It categorizes each file
        /// based on its extension using the <see cref="_fileTypeMappings"/> dictionary. If a file with the same name
        /// already exists at the destination, a dialog prompts the user to rename or skip the file. Files already located
        /// in destination directories are skipped to prevent infinite recursion.
        /// </remarks>
        public void Sort(string source, string images, string videos, string texts, string tables, string pdfs, string others)
        {
            if (!Directory.Exists(source))
                throw new DirectoryNotFoundException($"Source directory '{source}' does not exist.");

            string[] destinations = { images, videos, texts, tables, pdfs, others };
            foreach (var dir in destinations)
            {
                Directory.CreateDirectory(dir);
            }

            string[] files = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                Console.WriteLine("Warning: No files found in source directory and subdirectories.");
                return;
            }

            foreach (var file in files)
            {
                string extension = Path.GetExtension(file).ToLowerInvariant();
                string fileName = Path.GetFileName(file);
                string destinationFolder = others;

                if (_fileTypeMappings.TryGetValue(extension, out var folder))
                {
                    destinationFolder = folder switch
                    {
                        "Images" => images,
                        "Videos" => videos,
                        "Texts" => texts,
                        "Tables" => tables,
                        "PDFs" => pdfs,
                        _ => others
                    };
                }
                string destinationPath = Path.Combine(destinationFolder, fileName);

                // Check if the file is already in one of the destination folders
                string sourceFullPath = Path.GetFullPath(file);
                bool isInDestination = false;
                foreach (var dest in destinations)
                {
                    string destFullPath = Path.GetFullPath(dest);
                    if (sourceFullPath.StartsWith(destFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        isInDestination = true;
                        break;
                    }
                }
                if (isInDestination)
                {
                    continue;
                }
                // Check if the destination file already exists
                if (File.Exists(destinationPath))
                {
                    var result = MessageBox.Show(
                        $"The file '{fileName}' already exists at '{destinationPath}'.\n\n" +
                        "What would you like to do?\n" +
                        "- Yes: Rename\n" +
                        "- No: Skip",
                        "File Conflict",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        destinationPath = GetSafeDestinationPath(destinationFolder, fileName);
                    }
                    else
                    {
                        Console.WriteLine($"Skipped {fileName} due to conflict.");
                        continue;
                    }
                }
                // Attempt to move the file to the destination folder
                try
                {
                    File.Move(file, destinationPath);
                    Console.WriteLine($"Moved {fileName} to {Path.GetFileName(destinationFolder)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error moving {fileName}: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// Generates a unique file name if a file with the same name already exists in the destination directory.
        /// </summary>
        /// <param name="directory">The destination directory path where the file will be moved.</param>
        /// <param name="fileName">The original name of the file (including extension).</param>
        /// <returns>A unique file path that does not conflict with existing files in the directory.</returns>
        /// <remarks>
        /// If a file with the same name exists, the method appends a numeric suffix (e.g., "_1", "_2") to the file name
        /// until a unique name is found. The original extension is preserved.
        /// </remarks>
        private static string GetSafeDestinationPath(string directory, string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);
            string destination = Path.Combine(directory, fileName);
            int counter = 1;

            while (File.Exists(destination))
            {
                string newName = $"{name}_{counter}{ext}";
                destination = Path.Combine(directory, newName);
                counter++;
            }

            return destination;
        }
    }
}