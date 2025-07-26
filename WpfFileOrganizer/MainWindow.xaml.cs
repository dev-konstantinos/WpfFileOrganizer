using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WpfFileOrganizer
{
    /// <summary>
    /// The main window of the WPF File Organizer application, providing a user interface to select folders and sort files.
    /// </summary>
    /// <remarks>
    /// This class handles user interactions, validates folder paths, and initiates the file sorting process using the
    /// <see cref="FileSorter"/> class. It also integrates with <see cref="AppSettings"/> to persist folder selections.
    /// </remarks>
    public partial class MainWindow : Window
    {
        private readonly FileSorter _fileSorter;
        private readonly AppSettings _appSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <remarks>
        /// Sets up the UI, loads saved settings from <see cref="AppSettings.Load()"/>, and initializes the
        /// <see cref="FileSorter"/> instance.
        /// </remarks>
        public MainWindow()
        {
            InitializeComponent();

            _appSettings = AppSettings.Load();

            SourceFolderTextBox.Text = _appSettings.SourceFolder;
            ImagesFolderTextBox.Text = _appSettings.ImagesFolder;
            VideosFolderTextBox.Text = _appSettings.VideosFolder;
            TextsFolderTextBox.Text = _appSettings.TextsFolder;
            TablesFolderTextBox.Text = _appSettings.TablesFolder;
            PdfsFolderTextBox.Text = _appSettings.PdfsFolder;
            OthersFolderTextBox.Text = _appSettings.OthersFolder;

            _fileSorter = new FileSorter();
        }

        private void SelectSourceButton_Click(object sender, RoutedEventArgs e)
        {
            SourceFolderTextBox.Text = ShowFolderDialog();
        }

        private void SelectImagesButton_Click(object sender, RoutedEventArgs e)
        {
            ImagesFolderTextBox.Text = ShowFolderDialog();
        }

        private void SelectVideosButton_Click(object sender, RoutedEventArgs e)
        {
            VideosFolderTextBox.Text = ShowFolderDialog();
        }

        private void SelectTextsButton_Click(object sender, RoutedEventArgs e)
        {
            TextsFolderTextBox.Text = ShowFolderDialog();
        }

        private void SelectTablesButton_Click(object sender, RoutedEventArgs e)
        {
            TablesFolderTextBox.Text = ShowFolderDialog();
        }

        private void SelectPdfsButton_Click(object sender, RoutedEventArgs e)
        {
            PdfsFolderTextBox.Text = ShowFolderDialog();
        }

        private void SelectOthersButton_Click(object sender, RoutedEventArgs e)
        {
            OthersFolderTextBox.Text = ShowFolderDialog();
        }

        /// <summary>
        /// Displays a folder selection dialog and returns the selected folder path.
        /// </summary>
        /// <returns>The full path of the selected folder, or an empty string if no folder is selected.</returns>
        /// <remarks>
        /// Uses <see cref="OpenFolderDialog"/> to allow the user to browse and select a folder. Note that
        /// <see cref="OpenFolderDialog"/> should be replaced with <see cref="System.Windows.Forms.FolderBrowserDialog"/>
        /// for proper WPF compatibility.
        /// </remarks>
        private string ShowFolderDialog()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select a folder"
            };
            return dialog.ShowDialog() == true ? dialog.FolderName : string.Empty;
        }

        /// <summary>
        /// Handles the click event of the Start button to initiate the file sorting process.
        /// </summary>
        /// <param name="sender">The object that raised the event (typically the Start button).</param>
        /// <param name="e">The event arguments providing additional context.</param>
        /// <remarks>
        /// Saves the current folder paths to <see cref="AppSettings"/>, validates that all paths are provided and exist,
        /// and calls <see cref="SortFilesWithProgress"/> to sort the files. Displays error messages via
        /// <see cref="StatusLabel"/> if validation fails or an exception occurs.
        /// </remarks>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SourceFolder = SourceFolderTextBox.Text;
            _appSettings.ImagesFolder = ImagesFolderTextBox.Text;
            _appSettings.VideosFolder = VideosFolderTextBox.Text;
            _appSettings.TextsFolder = TextsFolderTextBox.Text;
            _appSettings.TablesFolder = TablesFolderTextBox.Text;
            _appSettings.PdfsFolder = PdfsFolderTextBox.Text;
            _appSettings.OthersFolder = OthersFolderTextBox.Text;
            _appSettings.Save();

            if (string.IsNullOrEmpty(SourceFolderTextBox.Text) ||
                string.IsNullOrEmpty(ImagesFolderTextBox.Text) ||
                string.IsNullOrEmpty(VideosFolderTextBox.Text) ||
                string.IsNullOrEmpty(TextsFolderTextBox.Text) ||
                string.IsNullOrEmpty(TablesFolderTextBox.Text) ||
                string.IsNullOrEmpty(PdfsFolderTextBox.Text) ||
                string.IsNullOrEmpty(OthersFolderTextBox.Text))
            {
                StatusLabel.Content = "Error: Please select or enter all folder paths.";
                StatusLabel.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            try
            {
                // Validate directories
                string[] directories = {
                    SourceFolderTextBox.Text,
                    ImagesFolderTextBox.Text,
                    VideosFolderTextBox.Text,
                    TextsFolderTextBox.Text,
                    TablesFolderTextBox.Text,
                    PdfsFolderTextBox.Text,
                    OthersFolderTextBox.Text
                };
                foreach (var dir in directories)
                {
                    if (!Directory.Exists(dir))
                    {
                        StatusLabel.Content = $"Error: Directory '{dir}' does not exist.";
                        StatusLabel.Foreground = System.Windows.Media.Brushes.Red;
                        return;
                    }
                }

                // Start sorting with progress
                SortFilesWithProgress(
                    SourceFolderTextBox.Text,
                    ImagesFolderTextBox.Text,
                    VideosFolderTextBox.Text,
                    TextsFolderTextBox.Text,
                    TablesFolderTextBox.Text,
                    PdfsFolderTextBox.Text,
                    OthersFolderTextBox.Text
                );
            }
            catch (Exception ex)
            {
                StatusLabel.Content = $"Error: {ex.Message}";
                StatusLabel.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        /// <summary>
        /// Performs the file sorting process and updates the UI with progress or error status.
        /// </summary>
        /// <param name="source">The source directory path from which files will be sorted.</param>
        /// <param name="images">The destination directory path for image files.</param>
        /// <param name="videos">The destination directory path for video files.</param>
        /// <param name="texts">The destination directory path for text files.</param>
        /// <param name="tables">The destination directory path for table/spreadsheet files.</param>
        /// <param name="pdfs">The destination directory path for PDF files.</param>
        /// <param name="others">The destination directory path for files that do not match any predefined category.</param>
        /// <remarks>
        /// Invokes the <see cref="FileSorter.Sort"/> method to categorize and move files. Counts the total number of files
        /// (though progress tracking is not fully implemented) and updates <see cref="StatusLabel"/> with the result.
        /// Exceptions during sorting are caught and displayed to the user.
        /// </remarks>
        private void SortFilesWithProgress(string source, string images, string videos, string texts, string tables, string pdfs, string others)
        {
            try
            {
                _fileSorter.Sort(source, images, videos, texts, tables, pdfs, others);
                string[] files = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);
                int totalFiles = files.Length;

                StatusLabel.Content = "Success: Files sorted successfully.";
                StatusLabel.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (Exception ex)
            {
                StatusLabel.Content = $"Error: {ex.Message}";
                StatusLabel.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        /// <summary>
        /// Handles the click event of the Clear button to reset all folder paths.
        /// </summary>
        /// <param name="sender">The object that raised the event (typically the Clear button).</param>
        /// <param name="e">The event arguments providing additional context.</param>
        /// <remarks>
        /// Clears all text boxes and resets the <see cref="StatusLabel"/> to its initial state.
        /// </remarks>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SourceFolderTextBox.Text = string.Empty;
            ImagesFolderTextBox.Text = string.Empty;
            VideosFolderTextBox.Text = string.Empty;
            TextsFolderTextBox.Text = string.Empty;
            TablesFolderTextBox.Text = string.Empty;
            PdfsFolderTextBox.Text = string.Empty;
            OthersFolderTextBox.Text = string.Empty;
            StatusLabel.Content = "Ready";
            StatusLabel.Foreground = System.Windows.Media.Brushes.Black;
        }

        /// <summary>
        /// Handles the click event of the Exit button to close the application.
        /// </summary>
        /// <param name="sender">The object that raised the event (typically the Exit button).</param>
        /// <param name="e">The event arguments providing additional context.</param>
        /// <remarks>
        /// Closes the main window, terminating the application.
        /// </remarks>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}