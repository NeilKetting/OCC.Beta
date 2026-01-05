using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Shared;

public partial class WorkHoursPopupView : UserControl
{
    public WorkHoursPopupView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
