using System.Windows;

namespace TodoOverlayApp.Views
{
    public partial class EditWindow : Window
    {
        public string NewContent { get; set; } = string.Empty;

        public EditWindow(string initialContent)
        {
            InitializeComponent();
            txtEditContent.Text = initialContent;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            NewContent = txtEditContent.Text;
            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}