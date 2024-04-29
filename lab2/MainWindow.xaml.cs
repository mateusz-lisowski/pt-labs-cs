using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.IO;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace wpf_full
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog { Description = "Wybierz folder do otwarcia" };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedFolder = dialog.SelectedPath;
                DisplayFolder(selectedFolder);
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = (TreeViewItem)e.NewValue;
            string path = (string)selectedItem.Tag;
            DisplayFileAttributes(path);
        }

        private void TreeView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (TreeViewItem)TreeView.SelectedItem;

            if (selectedItem == null)
            {
                MessageBox.Show("Wybierz element do usunięcia.");
                return;
            }

            string path = (string)selectedItem.Tag;

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }

                var parent = (TreeViewItem)selectedItem.Parent;
                parent.Items.Remove(selectedItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas usuwania: {ex.Message}");
            }
        }

        private void CreateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (TreeViewItem)TreeView.SelectedItem;

            if (selectedItem == null || !Directory.Exists((string)selectedItem.Tag))
            {
                MessageBox.Show("Wybierz folder, w którym chcesz utworzyć nowy element.");
                return;
            }

            var dialog = new CreateElementDialog();
            if (dialog.ShowDialog() == true)
            {
                string name = dialog.ElementName;
                bool isFile = dialog.IsFile;
                bool readOnly = dialog.ReadOnly;
                bool archive = dialog.Archive;
                bool hidden = dialog.Hidden;
                bool system = dialog.SystemAttribute;
                string path = System.IO.Path.Combine((string)selectedItem.Tag, name);

                try
                {
                    if (dialog.IsFile)
                    {
                        File.Create(path).Close();
                    }
                    else
                    {
                        Directory.CreateDirectory(path);
                    }

                    FileAttributes attributes = File.GetAttributes(path);
                    if (readOnly)
                        attributes |= FileAttributes.ReadOnly;
                    if (archive)
                        attributes |= FileAttributes.Archive;
                    if (hidden)
                        attributes |= FileAttributes.Hidden;
                    if (system)
                        attributes |= FileAttributes.System;

                    File.SetAttributes(path, attributes);

                    var newItem = new TreeViewItem
                    {
                        Header = name,
                        Tag = path
                    };

                    selectedItem.Items.Add(newItem);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Wystąpił błąd podczas tworzenia: {ex.Message}");
                }
            }
        }

        private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (TreeViewItem)TreeView.SelectedItem;

            if (selectedItem == null || !File.Exists((string)selectedItem.Tag))
            {
                MessageBox.Show("Wybierz plik, który chcesz otworzyć.");
                return;
            }

            string filePath = (string)selectedItem.Tag;
            string fileContent = File.ReadAllText(filePath);

            var contentWindow = new ContentWindow(fileContent);
            contentWindow.ShowDialog();
        }

        private void DisplayFolder(string folderPath)
        {
            TreeView.Items.Clear();

            var rootItem = new TreeViewItem
            {
                Header = folderPath,
                Tag = folderPath
            };

            TreeView.Items.Add(rootItem);

            try
            {
                DisplayFolderContents(folderPath, rootItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas wyświetlania zawartości folderu: {ex.Message}");
            }
        }

        private void DisplayFolderContents(string folderPath, TreeViewItem parentItem)
        {
            string[] directories = Directory.GetDirectories(folderPath);
            foreach (string directory in directories)
            {
                var directoryInfo = new DirectoryInfo(directory);
                var directoryItem = new TreeViewItem
                {
                    Header = directoryInfo.Name,
                    Tag = directoryInfo.FullName
                };
                parentItem.Items.Add(directoryItem);
                DisplayFolderContents(directory, directoryItem);
            }

            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files)
            {
                var fileInfo = new FileInfo(file);
                var fileItem = new TreeViewItem
                {
                    Header = fileInfo.Name,
                    Tag = fileInfo.FullName
                };
                parentItem.Items.Add(fileItem);
            }
        }

        private void DisplayFileAttributes(string filePath)
        {
            var attributes = File.GetAttributes(filePath);
            string attributeString = GetAttributeString(attributes);
            StatusBarTextBlock.Text = attributeString;
        }

        private string GetAttributeString(FileAttributes attributes)
        {
            string attributeString = "";

            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                attributeString += "r";
            }
            else
            {
                attributeString += "-";
            }

            if ((attributes & FileAttributes.Archive) == FileAttributes.Archive)
            {
                attributeString += "a";
            }
            else
            {
                attributeString += "-";
            }

            if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                attributeString += "h";
            }
            else
            {
                attributeString += "-";
            }

            if ((attributes & FileAttributes.System) == FileAttributes.System)
            {
                attributeString += "s";
            }
            else
            {
                attributeString += "-";
            }

            return attributeString;
        }

        private TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = System.Windows.Media.VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }
    }

    


}
