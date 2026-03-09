using System.Windows.Controls;

namespace OCC.WpfClient.Features.Main.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            DataContextChanged += MainView_DataContextChanged;
        }

        private void MainView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is System.ComponentModel.INotifyPropertyChanged oldVm)
            {
                oldVm.PropertyChanged -= Vm_PropertyChanged;
            }
            if (e.NewValue is System.ComponentModel.INotifyPropertyChanged newVm)
            {
                newVm.PropertyChanged += Vm_PropertyChanged;
            }
        }

        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSidebarMinimized" && sender is ViewModels.MainViewModel vm)
            {
                var sb = (System.Windows.Media.Animation.Storyboard)Resources[vm.IsSidebarMinimized ? "CollapseSidebar" : "ExpandSidebar"];
                sb.Begin();
            }
        }

        private void OnSidebarClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (DataContext is ViewModels.MainViewModel vm)
                {
                    vm.ToggleSidebarCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }
    }
}
