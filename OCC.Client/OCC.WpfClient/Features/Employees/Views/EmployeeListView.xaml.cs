using System.Windows.Controls;

namespace OCC.WpfClient.Features.Employees.Views
{
    public partial class EmployeeListView : UserControl
    {
        public EmployeeListView()
        {
            InitializeComponent();
        }
        public void DataGrid_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            if (DataContext is ViewModels.EmployeeListViewModel vm)
            {
                vm.SaveLayoutCommand.Execute(null);
            }
        }
    }
}
