using System.Windows;

namespace wpf_full
{
    public partial class ContentWindow : Window
    {
        public ContentWindow(string content)
        {
            InitializeComponent();
            ContentTextBlock.Text = content;
        }
    }
}
