using System.Windows.Controls;

namespace OCC.WpfClient.Features.Main.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
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
