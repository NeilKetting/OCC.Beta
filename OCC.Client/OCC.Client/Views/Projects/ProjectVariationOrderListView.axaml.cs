using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Projects
{
    public partial class ProjectVariationOrderListView : UserControl
    {
        public ProjectVariationOrderListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
