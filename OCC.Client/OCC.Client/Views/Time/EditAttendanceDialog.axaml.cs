using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace OCC.Client.Views.Time
{
    public partial class EditAttendanceDialog : Window
    {
        public TimeSpan? ClockInTime { get; private set; }
        public TimeSpan? ClockOutTime { get; private set; }

        public EditAttendanceDialog()
        {
            InitializeComponent();
        }

        public EditAttendanceDialog(TimeSpan? currentIn, TimeSpan? currentOut, bool showIn = true, bool showOut = true) : this()
        {
            var inHour = this.FindControl<NumericUpDown>("InHour");
            var inMin = this.FindControl<NumericUpDown>("InMin");
            var outHour = this.FindControl<NumericUpDown>("OutHour");
            var outMin = this.FindControl<NumericUpDown>("OutMin");

            var inPanel = this.FindControl<StackPanel>("ClockInPanel");
            var outPanel = this.FindControl<StackPanel>("ClockOutPanel");

            if (inHour != null && inMin != null && currentIn.HasValue) 
            {
                inHour.Value = currentIn.Value.Hours;
                inMin.Value = currentIn.Value.Minutes;
            }
            
            if (outHour != null && outMin != null && currentOut.HasValue) 
            {
                outHour.Value = currentOut.Value.Hours;
                outMin.Value = currentOut.Value.Minutes;
            }
            
            if (inPanel != null) inPanel.IsVisible = showIn;
            if (outPanel != null) outPanel.IsVisible = showOut;
            
            // Adjust title based on what we are showing
            if (showIn && !showOut) Title = "Clock In Time";
            else if (!showIn && showOut) Title = "Clock Out Time";
            else Title = "Correct Attendance";
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            var inHour = this.FindControl<NumericUpDown>("InHour");
            var inMin = this.FindControl<NumericUpDown>("InMin");
            var outHour = this.FindControl<NumericUpDown>("OutHour");
            var outMin = this.FindControl<NumericUpDown>("OutMin");

            if (inHour?.Value != null && inMin?.Value != null)
                ClockInTime = new TimeSpan((int)inHour.Value.Value, (int)inMin.Value.Value, 0);
            
            if (outHour?.Value != null && outMin?.Value != null)
                ClockOutTime = new TimeSpan((int)outHour.Value.Value, (int)outMin.Value.Value, 0);

            Close(true);
        }
    }
}
