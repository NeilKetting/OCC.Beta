using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Linq;
using Avalonia.Input;

namespace OCC.Client.Views.Dev
{
    public partial class TestView : UserControl
    {
        public TestView()
        {
            InitializeComponent();
        }

        private void AutoCompleteBox_GotFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is AutoCompleteBox box)
            {
                // Delay slightly to ensure focus is settled before opening
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    box.IsDropDownOpen = true;
                });
            }
        }

        private void TextBox_GotFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }
    }
}
