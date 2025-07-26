using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WpfFileOrganizer
{
    public partial class MainWindow : Window
    {
        private readonly FileSorter _fileSorter;

        private AppSettings _appSettings;

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

        private string ShowFolderDialog()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select a folder"
            };
            return dialog.ShowDialog() == true ? dialog.FolderName : string.Empty;
        }

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

        private void SortFilesWithProgress(string source, string images, string videos, string texts, string tables, string pdfs, string others)
        {

            try
            {
                _fileSorter.Sort(source, images, videos, texts, tables, pdfs, others);
                string[] files = Directory.GetFiles(source, "*.*", SearchOption.TopDirectoryOnly);
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

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}