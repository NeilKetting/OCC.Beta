using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Features.TimeAttendanceHub.Views
{
    public partial class EditAttendanceDialog : Window
    {
        public TimeSpan? ClockInTime { get; private set; }
        public TimeSpan? ClockOutTime { get; private set; }

        public EditAttendanceDialog()
        {
            InitializeComponent();
        }

        public EditAttendanceDialog(TimeSpan? currentIn, TimeSpan? currentOut, bool showIn, bool showOut) : this()
        {
            ClockInPanel.IsVisible = showIn;
            ClockOutPanel.IsVisible = showOut;

            if (currentIn.HasValue)
            {
                InHour.Value = currentIn.Value.Hours;
                InMin.Value = currentIn.Value.Minutes;
            }

            if (currentOut.HasValue)
            {
                OutHour.Value = currentOut.Value.Hours;
                OutMin.Value = currentOut.Value.Minutes;
            }
        }



        private void OnSaveClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ClockInTime = new TimeSpan((int)(InHour.Value ?? 0), (int)(InMin.Value ?? 0), 0);
            ClockOutTime = new TimeSpan((int)(OutHour.Value ?? 0), (int)(OutMin.Value ?? 0), 0);
            Close(true);
        }

        private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
