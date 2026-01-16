using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Time
{
    public partial class WageRunView : UserControl
    {
        public WageRunView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
