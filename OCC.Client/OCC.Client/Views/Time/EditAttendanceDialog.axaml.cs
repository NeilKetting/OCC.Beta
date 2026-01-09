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

        public EditAttendanceDialog(TimeSpan? currentIn, TimeSpan? currentOut) : this()
        {
            var inPicker = this.FindControl<TimePicker>("ClockInPicker");
            var outPicker = this.FindControl<TimePicker>("ClockOutPicker");

            if (inPicker != null && currentIn.HasValue) inPicker.SelectedTime = currentIn;
            if (outPicker != null && currentOut.HasValue) outPicker.SelectedTime = currentOut;
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
