# WpfFileOrganizer

A simple WPF application designed to organize files from a source directory into categorized destination folders based on file types (e.g., Images, Videos, Texts, Tables, PDFs, and Others).
Inspired by the need for a simple file organization tool. The project is in an early stage, so any advice and contributions are welcome!

## Features
- Select source and destination folders via a user-friendly interface.
- Automatically categorize files by their extensions (e.g., `.jpg` to Images, `.mp4` to Videos).
- Handle file conflicts with options to rename or skip duplicates.
- Save and load folder settings (folders) for future comfortable use.
- Display status updates and error messages in real-time.

## Usage
- Launch the application.
- Use the "Select" buttons to choose the source folder and destination folders for each category.
- Click "Start" to begin the file organization process.
- Check the status label for success or error messages.
- Use "Clear" to reset folder selections or "Exit" to close the app.
- Settings are automatically saved and loaded for the next session.

## Project Structure
- MainWindow.xaml: Defines the user interface layout.
- MainWindow.xaml.cs: Contains the logic for folder selection and file sorting.
- FileSorter.cs: Implements the file categorization and file moving logic.
- AppSettings.cs: Manages persistent storage of folder paths in a JSON file.

## License

This project is licensed under the MIT License - see the LICENSE.md file for details.
