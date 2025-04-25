using System.Windows;

namespace TodoOverlayApp
{
    public partial class EditWindow : Window
    {
        public string NewContent { get; set; }

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