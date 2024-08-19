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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AnalyticsEngine;
using System.Windows.Controls.Ribbon;
using System.Windows.Markup;


namespace TTTApp {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        ViewModel.MainData data;

        public MainWindow() {
            InitializeComponent();
            data = new ViewModel.MainData();
            data.DatasetChanged += OnDatasetChanged;
            DataContext = data;
        }        

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            data.Dispose();
        }

        private void OnApplyTimerange(object sender, RoutedEventArgs e) {
            mChartControl.SetRange(data.From, data.To);
        }

        private void OnDatasetChanged(object sender, EventArgs e) {
            mChartControl.Reset();
        }
    }
}
