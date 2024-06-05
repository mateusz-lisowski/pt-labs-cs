using System;
using System.Windows;

namespace wpf_full
{
    public partial class CreateElementDialog : Window
    {
        public string ElementName { get; private set; }
        public bool IsFile { get; private set; }
        public bool ReadOnly { get; private set; }
        public bool Archive { get; private set; }
        public bool Hidden { get; private set; }
        public bool SystemAttribute { get; private set; }

        public CreateElementDialog()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Wprowadź nazwę dla nowego elementu.");
                return;
            }

            if (FileTypeComboBox.SelectedIndex == 0)
            {
                IsFile = true;
            }
            else
            {
                IsFile = false;
            }

            if (IsFile && !System.Text.RegularExpressions.Regex.IsMatch(name, @"^[\w-]{1,8}\.[txt|php|html]+$"))
            {
                MessageBox.Show("Nieprawidłowa nazwa pliku. Nazwa powinna składać się z litery, cyfry, podkreślenia, tyldy lub minusa oraz mieć rozszerzenie .txt, .php lub .html.");
                return;
            }

            ElementName = name;

            ReadOnly = ReadOnlyCheckBox.IsChecked == true;
            Archive = ArchiveCheckBox.IsChecked == true;
            Hidden = HiddenCheckBox.IsChecked == true;
            SystemAttribute = SystemCheckBox.IsChecked == true;

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
