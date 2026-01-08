using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Home.MySummary
{
    public partial class MySummaryPageView : UserControl
    {
        public MySummaryPageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
