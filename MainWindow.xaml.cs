using DataGridFilterComponent.UserController;
using System.Data;
using System.Windows;

namespace DataGridFilterComponent
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            mainGrid.Children.Add(new DataGridFilterUserController(CreateExampleData()));
        }
        private DataTable CreateExampleData()
        {
            DataTable table = new DataTable("patients");
            table.Columns.Add("Id");
            table.Columns.Add("Name");
            table.Columns.Add("Surname");

            for (int i = 0; i < 300; i++)
            {
                table.Rows.Add(i + 1, "Rezan", "Yıldız");
                table.Rows.Add(i + 2, "David", "Johnson");
                table.Rows.Add(i + 3, "Marie", "Barr");
                table.Rows.Add(i + 4, "Ronald", "Bar");
                table.Rows.Add(i + 5, "Margaret", "Forbis");
                table.Rows.Add(i + 6, "Susan", "Connor");
            }


            return table;
        }
    }
}
