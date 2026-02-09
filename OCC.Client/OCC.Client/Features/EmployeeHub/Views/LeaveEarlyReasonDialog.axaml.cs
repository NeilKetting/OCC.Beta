using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Features.EmployeeHub.Views
{
    public partial class LeaveEarlyReasonDialog : Window
    {
        public string? Reason => (ReasonComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        public string? Note => NoteTextBox.Text;

        public LeaveEarlyReasonDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnConfirmClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(true);
        }

        private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
