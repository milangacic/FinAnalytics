using AnalyticsEngine;
using ChartControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChartTestForm {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : RibbonWindow {
        public MainRecord Record { get; set; }

        public MainWindow() {
            Record = new MainRecord();
            DataContext = Record;            
            InitializeComponent();
        }

        private void OnDeleteSeries(object sender, RoutedEventArgs e) {
            if (Record.SelectedChartSeries == null) return;
            Record.ChartSeries.Remove(Record.SelectedChartSeries);
        }

        private void OnDeleteDataset(object sender, RoutedEventArgs e) {
            if (Record.SelectedDataset < 0) return;
            Record.Datasets.RemoveAt(Record.SelectedDataset);       
        }

        private void OnDeleteDataprovider(object sender, RoutedEventArgs e) {
            if (Record.Dataprovider < 0) return;
            Record.Dataproviders.RemoveAt(Record.Dataprovider);
        }

        private void OnAddSeries(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(Record.SelectedChartName)) {
                MessageBox.Show("Name required!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Record.SelectedDataset < 0) {
                MessageBox.Show("Dataset required!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Record.SelectedSeriestype == null) {
                MessageBox.Show("Series type required!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBox.Show($"Series {Record.SelectedChartName} created!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            Record.ChartSeries.Add(new ChartSeries(Record.SelectedChartName, Record.SelectedSeriestype.Value, Record.Datasets[Record.SelectedDataset].Item2));
            Record.SelectedChartName = null;
            Record.SelectedDataset = -1;
            Record.SelectedSeriestype = null;            
        }

        private void OnAddDataset(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(Record.SelectedDatasetName)) {
                MessageBox.Show("Name required!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Record.SelectedDataprovider < 0) {
                MessageBox.Show("Dataprovider required!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Record.SelectedDataType == null) {
                MessageBox.Show("Datatype required!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            } 
            if (Record.SelectedFromDate == default) {
                MessageBox.Show("From-Date required!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Record.SelectedToDate == default) {
                MessageBox.Show("To-Date required!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Record.SelectedFromDate >= Record.SelectedToDate) {
                MessageBox.Show("From-Date must be before To-Date!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if ( Record.SelectedDataType != MainRecord.DataType.OHLC && Record.SelectedDataType != MainRecord.DataType.Volume
                && (Record.SelectedRange == null || Record.SelectedRange == 0)) {
                MessageBox.Show("Positive range required!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            switch (Record.SelectedDataType) {
                case MainRecord.DataType.OHLC:
                    IEnumerable<Tuple<DateTime, decimal[]>> marketdata = Record.Dataproviders[Record.SelectedDataprovider].Item2.GetMarketData(Record.SelectedFromDate, Record.SelectedToDate).Select(item => Tuple.Create(item.Time, new decimal[] { item.Open, item.High, item.Low, item.Close })).ToList();
                    Record.Datasets.Add(Tuple.Create(Record.SelectedDatasetName, marketdata));                  
                    break;
                case MainRecord.DataType.Volume:
                    IEnumerable<Tuple<DateTime, decimal[]>> volumedata = Record.Dataproviders[Record.SelectedDataprovider].Item2.GetMarketData(Record.SelectedFromDate, Record.SelectedToDate).Select(item => Tuple.Create(item.Time, new decimal[] { item.Volume, item.Open, item.Close })).ToList();
                    Record.Datasets.Add(Tuple.Create(Record.SelectedDatasetName, volumedata));
                    break;
                case MainRecord.DataType.EMA:
                    IAnalytics ema = new EMA(Record.Dataproviders[Record.SelectedDataprovider].Item2, Record.SelectedRange.Value);
                    IEnumerable<Tuple<DateTime, decimal[]>> emadata = ema.GetAnalyticsData(Record.SelectedFromDate, Record.SelectedToDate).Select(item => Tuple.Create(item.Time, new decimal[] { item.Value })).ToList();
                    Record.Datasets.Add(Tuple.Create(Record.SelectedDatasetName, emadata));
                    break;
                case MainRecord.DataType.SMA:
                    IAnalytics sma = new SMA(Record.Dataproviders[Record.SelectedDataprovider].Item2, Record.SelectedRange.Value);
                    IEnumerable<Tuple<DateTime, decimal[]>> smadata = sma.GetAnalyticsData(Record.SelectedFromDate, Record.SelectedToDate).Select(item => Tuple.Create(item.Time, new decimal[] { item.Value })).ToList();
                    Record.Datasets.Add(Tuple.Create(Record.SelectedDatasetName, smadata));
                    break;
                case MainRecord.DataType.RSI:
                    IAnalytics rsi = new RSI(Record.Dataproviders[Record.SelectedDataprovider].Item2, Record.SelectedRange.Value);
                    IEnumerable<Tuple<DateTime, decimal[]>> rsidata = rsi.GetAnalyticsData(Record.SelectedFromDate, Record.SelectedToDate).Select(item => Tuple.Create(item.Time, new decimal[] { item.Value })).ToList();
                    Record.Datasets.Add(Tuple.Create(Record.SelectedDatasetName, rsidata));
                    break;
                default:
                    MessageBox.Show("Not supported data type!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }
            MessageBox.Show($"Dataset {Record.SelectedDatasetName} created!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            Record.SelectedDataproviderName = null;
            Record.SelectedDataprovider = -1;
            Record.SelectedDataType = null;
            Record.SelectedFromDate = default;
            Record.SelectedToDate = default;
            Record.SelectedRange = null;            
        }

        private void OnAddDataprovider(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(Record.SelectedDataproviderName)) {
                MessageBox.Show("Name required!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(Record.SelectedFile)) {
                MessageBox.Show("File required!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Record.SelectedInterval == null) {
                MessageBox.Show("Intervall required!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            } else if (Record.SelectedInterval == 0) {
                MessageBox.Show("Intervall must be positive!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Record.Dataproviders.Add(Tuple.Create(Record.SelectedDataproviderName, new CDDDataProvider(Record.SelectedFile, TimeSpan.FromSeconds((double)Record.SelectedInterval), DateTimeKind.Local)));
            MessageBox.Show($"Dataprovider {Record.SelectedDataproviderName} created!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            Record.SelectedDataproviderName = null;
            Record.SelectedFile = null;
            Record.SelectedInterval = null;            
        }
    }
}
