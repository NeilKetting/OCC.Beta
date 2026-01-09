using Avalonia.Controls;
using Avalonia.Interactivity;

namespace OCC.Client.Views.Time
{
    public partial class LeaveEarlyReasonDialog : Window
    {
        public string? Reason { get; private set; }
        public string? Note { get; private set; }

        public LeaveEarlyReasonDialog()
        {
            InitializeComponent();
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }

        private void OnConfirmClick(object? sender, RoutedEventArgs e)
        {
            var combo = this.FindControl<ComboBox>("ReasonComboBox");
            var textBox = this.FindControl<TextBox>("NoteTextBox");

            if (combo?.SelectedItem is ComboBoxItem item)
            {
                Reason = item.Content?.ToString();
            }
            
            Note = textBox?.Text;

            // If "Other" is selected, append note to reason or just use note?
            // Requirement was: "Dropdown with option other which makes a textbox visible"
            // For simplicity, we return both.
            
            Close(true);
        }
    }
}
