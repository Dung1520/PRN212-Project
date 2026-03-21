using BusinessObjects;
using DataAccess;
using Repositories;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for Window2_Class_.xaml
    /// </summary>
    public partial class Window2_Class_ : Window
    {
        private readonly IClassService _service;
        public Window2_Class_()
        {
            InitializeComponent();

            var context = new LctmsDbContext();
            var repo = new ClassRepository(context);
            _service = new ClassService(repo,context);

            LoadData();
        }

        private void LoadData()
        {
            dgClasses.ItemsSource = _service.GetAllClasses();
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddClassWindow();
            win.ShowDialog();

            LoadData();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgClasses.SelectedItem is Class c)
            {
                var win = new UpdateClassWindow(c);
                win.ShowDialog();
                LoadData();
            }
        }
    }
}
