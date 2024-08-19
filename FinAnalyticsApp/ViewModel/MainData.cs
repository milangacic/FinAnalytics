using System;
using AnalyticsEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;
using TTTApp.View;
using static TTTApp.Model.Enumerations;
using ChartControls;
using System.Windows;
using System.Collections;
using TTTApp.Model;

namespace TTTApp.ViewModel {
    public class MainData : ViewModelBase, IDisposable
    {
        private DateTime vFrom;
        public DateTime From { get { return vFrom; } set { vFrom = value; OnPropertyChanged(); } }

        private DateTime vTo;
        public DateTime To { get { return vTo; } set { vTo = value; OnPropertyChanged(); } }

        private ObservableCollection<Tuple<string, DatasetViewModel>> vDatasets = new();
        public ObservableCollection<Tuple<string, DatasetViewModel>> Datasets { get { return vDatasets; } }

        private Tuple<string, DatasetViewModel> vSelectedDataset;
        public Tuple<string, DatasetViewModel> SelectedDataset { get { return vSelectedDataset; } set { vSelectedDataset = value; OnPropertyChanged(); } }

        private Tuple<string, DatasetViewModel> vAppliedDataset;
        public Tuple<string, DatasetViewModel> AppliedDataset { get { return vAppliedDataset; } set { vAppliedDataset = value; OnPropertyChanged(); } }

        private IDataProvider vDataprovider;
        private List<IAnalytics> vIndicators = new();

        private ObservableCollection<ChartSeries> vChartSeries = new();
        public ObservableCollection<ChartSeries> ChartSeries { get { return vChartSeries; } set { vChartSeries = value; OnPropertyChanged(); } }

        public CommandBase AddDatasetCommand { get; private set; }
        public CommandBase ApplyDatasetCommand { get; private set; }
        public CommandBase ApplyTimerangeCommand { get; private set; }

        public event EventHandler DatasetChanged;

        public MainData() {
            AddDatasetCommand = new(DoAddDataset, CanExecuteAddDataset);
            ApplyDatasetCommand = new(DoApplyDataset, CanExecuteApplyDataset);
            ApplyTimerangeCommand = new(null, CanExecuteApplyTimerange);
            var defaultdataset = new DatasetViewModel() { Name = "ETH/USD", Interval = TimeSpan.FromHours(1), Source = DatasetViewModel.SourceList[0].Item2 };
            defaultdataset.Indicators.Add(Tuple.Create("EMA 25", new IndicatorViewModel() { Name = "EMA 25", @Type = Indicator.EMA, Length = 25 }));
            defaultdataset.Indicators.Add(Tuple.Create("EMA 10", new IndicatorViewModel() { Name = "EMA 10", @Type = Indicator.EMA, Length = 10 }));
            defaultdataset.Indicators.Add(Tuple.Create("RSI 10", new IndicatorViewModel() { Name = "RSI 10", @Type = Indicator.RSI, Length = 10 }));
            Datasets.Add(Tuple.Create(defaultdataset.Name, defaultdataset));
        }

        private bool CanExecuteApplyTimerange()
        {
            return From != default && To != default;
        }

        private bool CanExecuteApplyDataset()
        {
            return SelectedDataset != null && SelectedDataset != AppliedDataset;
        }

        private async Task DoApplyDataset()
        {
            DatasetViewModel dataset = SelectedDataset.Item2;
            ChartSeries.Clear();
            vIndicators.Clear();
            if (vDataprovider != null) vDataprovider.Dispose();
            AppliedDataset = null;
            try {
                vDataprovider = new CDDDataProvider(dataset.Source, dataset.Interval);
                var data = vDataprovider.GetLatestMarketData(1000).Select(item => Tuple.Create(item.Time, new decimal[4] { item.Open, item.High, item.Low, item.Close })).ToList();
                ChartSeries.Add(new(dataset.Name, SeriesType.OHLCCandles, data) { ChartDefinition = "main" });
                foreach (var indicator in dataset.Indicators) {
                    IAnalytics analytics = IndicatorFactory.Create(indicator.Item2.Type.Value, vDataprovider, indicator.Item2.Length);
                    vIndicators.Add(analytics);
                    data = analytics.GetAnalyticsData(data.First().Item1, data.Last().Item1).Select(item => Tuple.Create(item.Time, new decimal[1] { item.Value })).ToList();
                    ChartSeries.Add(new(indicator.Item2.Name, SeriesType.Line, data));
                }                
            } catch (Exception exception) {
                MessageBox.Show(exception.Message + "\n" + exception.StackTrace, "Error applying dataset", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            AppliedDataset = SelectedDataset;
            DatasetChanged.Invoke(this, EventArgs.Empty);
        }
        
        private bool CanExecuteAddDataset()
        {
            return true;
        }

        private async Task DoAddDataset()
        {
            DatasetView datasetview = new(new DatasetViewModel());
            datasetview.ShowDialog();
            bool? result = datasetview.DialogResult;
            if (result != null && result.Value) {
                DatasetViewModel dataset = (DatasetViewModel)datasetview.DataContext;
                Datasets.Add(Tuple.Create(dataset.Name, dataset));
            }
        }

        public void Dispose()
        {
            vIndicators.Clear();
            if (vDataprovider != null) vDataprovider.Dispose();            
        }
    }
}
