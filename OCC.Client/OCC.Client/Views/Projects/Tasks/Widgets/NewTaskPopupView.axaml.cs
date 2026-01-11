using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Projects.Tasks.Widgets
{
    public partial class NewTaskPopupView : UserControl
    {
        public NewTaskPopupView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
