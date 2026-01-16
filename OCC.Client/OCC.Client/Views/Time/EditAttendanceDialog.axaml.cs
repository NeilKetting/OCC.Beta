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
            var inPicker = this.FindControl<TimePicker>("ClockInPicker");
            var outPicker = this.FindControl<TimePicker>("ClockOutPicker");
            var inPanel = this.FindControl<StackPanel>("ClockInPanel");
            var outPanel = this.FindControl<StackPanel>("ClockOutPanel");

            if (inPicker != null && currentIn.HasValue) inPicker.SelectedTime = currentIn;
            if (outPicker != null && currentOut.HasValue) outPicker.SelectedTime = currentOut;
            
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
            var inPicker = this.FindControl<TimePicker>("ClockInPicker");
            var outPicker = this.FindControl<TimePicker>("ClockOutPicker");

            ClockInTime = inPicker?.SelectedTime;
            ClockOutTime = outPicker?.SelectedTime;

            Close(true);
        }
    }
}
