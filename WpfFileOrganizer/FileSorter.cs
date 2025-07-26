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

        public FileSorter()
        {
            _fileTypeMappings = BuildFileTypeMappings();
        }

        // Builds a dictionary mapping file extensions to their respective folder names.
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

        // Sorts files from the source directory into specified directories based on their file types.
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
        /// Generates a unique file name if a file with the same name already exists in the destination directory.
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