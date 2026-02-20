using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;

namespace OCC.Client.Features.MobileHub.Views.Shells
{
    public partial class MobileShellDrawerView : UserControl
    {
        public MobileShellDrawerView()
        {
            InitializeComponent();
        }

        private void OnMenuItemClick(object sender, RoutedEventArgs e)
        {
            var hamburger = this.FindControl<ToggleButton>("HamburgerButton");
            if (hamburger != null)
            {
                hamburger.IsChecked = false;
            }
        }
    }
}
