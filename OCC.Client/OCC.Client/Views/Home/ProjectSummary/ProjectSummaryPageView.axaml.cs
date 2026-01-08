using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Home.ProjectSummary
{
    public partial class ProjectSummaryPageView : UserControl
    {
        public ProjectSummaryPageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
