using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Features.TimeAttendanceHub.Views
{
    public partial class LeaveEarlyReasonDialog : Window
    {
        public string? Reason => (this.FindControl<ComboBox>("ReasonComboBox")?.SelectedItem as ComboBoxItem)?.Content?.ToString();
        public string? Note => this.FindControl<TextBox>("NoteTextBox")?.Text;

        public LeaveEarlyReasonDialog()
        {
            InitializeComponent();
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
