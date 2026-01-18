using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Projects
{
    public partial class EditProjectView : UserControl
    {
        public EditProjectView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
