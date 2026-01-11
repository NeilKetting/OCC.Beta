using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Projects
{
    public partial class CreateProjectDetailsView : UserControl
    {
        public CreateProjectDetailsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
