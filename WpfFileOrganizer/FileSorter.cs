using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            string[] directories = { images, videos, texts, tables, pdfs, others };
            foreach (var dir in directories)
            {
                Directory.CreateDirectory(dir);
            }

            string[] files = Directory.GetFiles(source, "*.*", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                Console.WriteLine("Warning: No files found in source directory.");
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

                try
                {
                    File.Move(file, Path.Combine(destinationFolder, fileName));
                    Console.WriteLine($"Moved {fileName} to {Path.GetFileName(destinationFolder)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error moving {fileName}: {ex.Message}");
                }
            }
        }
    }
}