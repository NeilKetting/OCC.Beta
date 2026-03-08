using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace OCC.WpfClient.Features.AuthHub.Views
{
    public partial class AuthView : UserControl
    {
        public AuthView()
        {
            InitializeComponent();
        }

        private void OnRegisterClick(object sender, RoutedEventArgs e)
        {
            var sb = (Storyboard)this.Resources["FlipToRegister"];
            if (sb != null) sb.Begin();
        }

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            var sb = (Storyboard)this.Resources["FlipToLogin"];
            if (sb != null) sb.Begin();
        }
    }
}
