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
using TTTApp.ViewModel;

namespace TTTApp.View
{
    /// <summary>
    /// Interaction logic for DatasetView.xaml
    /// </summary>
    public partial class DatasetView
    {
        public DatasetView()
        {
            InitializeComponent();
        }

        public DatasetView(DatasetViewModel viewmodel) : this() {
            DataContext = viewmodel;           
        }

        private void OnOK(object sender, RoutedEventArgs e) {
            DatasetViewModel viewmodel = (DatasetViewModel)DataContext;
            if (viewmodel.DoValidate()) {
                DialogResult = true;
                Close();
            } else {
                MessageBox.Show(viewmodel.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancel(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}
