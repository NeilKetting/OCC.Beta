using Avalonia.Controls;
using Avalonia.Input;
using OCC.Client.ViewModels.Shared;

namespace OCC.Client.Views.Shared
{
    public partial class SidebarView : UserControl
    {
        public SidebarView()
        {
            InitializeComponent();
        }

        private void Sidebar_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            this.Focus();
        }

        protected override void OnDataContextChanged(System.EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is SidebarViewModel vm)
            {
                vm.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(SidebarViewModel.IsQuickActionsOpen))
                    {
                        if (!vm.IsQuickActionsOpen)
                        {
                            ActionsButton?.Flyout?.Hide();
                        }
                    }
                    else if (args.PropertyName == nameof(SidebarViewModel.IsSettingsOpen))
                    {
                        if (!vm.IsSettingsOpen)
                        {
                            SettingsButton?.Flyout?.Hide();
                        }
                    }
                };
            }
        }

        private void Sidebar_DoubleTapped(object? sender, TappedEventArgs e)
        {
             if (DataContext is SidebarViewModel vm)
            {
                vm.ToggleCollapse();
            }
        }
    }
}
