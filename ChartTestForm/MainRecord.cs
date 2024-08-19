using AnalyticsEngine;
using ChartControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Controls;

namespace ChartTestForm {
    public class MainRecord : INotifyPropertyChanged {

        public ObservableCollection<Tuple<string, string>> Files { get; } = new ObservableCollection<Tuple<string, string>>();
        public List<Tuple<string, SeriesType>> SeriesTypes { get; } = new List<Tuple<string, SeriesType>>() { Tuple.Create("OHLC Bars", SeriesType.OHLCCandles), Tuple.Create("Volume Bars", SeriesType.VolumeBars), Tuple.Create("Bars", SeriesType.Bars), Tuple.Create("Line", SeriesType.Line) };
        public List<Tuple<string, DataType>> DataTypes { get; } = new List<Tuple<string, DataType>> { Tuple.Create("OHLC", DataType.OHLC), Tuple.Create("Volume", DataType.Volume), Tuple.Create("SMA", DataType.SMA), Tuple.Create("EMA", DataType.EMA), Tuple.Create("RSI", DataType.RSI) };
        
        private ObservableCollection<ChartSeries> vChartSeries;
        public ObservableCollection<ChartSeries> ChartSeries { get { return vChartSeries; } set { vChartSeries = value; OnPropertyChanged(nameof(ChartSeries)); } }
        public ObservableCollection<Tuple<string, CDDDataProvider>> Dataproviders { get; } = new ObservableCollection<Tuple<string, CDDDataProvider>>();
        public ObservableCollection<Tuple<string, IEnumerable<Tuple<DateTime, decimal[]>>>> Datasets { get; } = new ObservableCollection<Tuple<string, IEnumerable<Tuple<DateTime, decimal[]>>>>();

        private string vSelectedFile;
        public string SelectedFile { get { return vSelectedFile; } set { vSelectedFile = value; OnPropertyChanged(nameof(SelectedFile)); } }
        public SeriesType? SelectedSeriestype { get; set; }
        public enum DataType { OHLC, Volume, SMA, EMA, RSI }
        public DataType? SelectedDataType { get; set; }
        public ChartSeries SelectedChartSeries { get; set; }

        private int vSelectedDataprovider = -1;
        public int SelectedDataprovider { get { return vSelectedDataprovider; } set { vSelectedDataprovider = value; OnPropertyChanged(nameof(SelectedDataprovider)); } }

        private int vDataprovider = -1;
        public int Dataprovider { get { return vDataprovider; } set { vDataprovider = value; OnPropertyChanged(nameof(Dataprovider)); } }

        private int vSelectedDataset = -1;
        public int SelectedDataset { get { return vSelectedDataset; } set { vSelectedDataset = value; OnPropertyChanged(nameof(SelectedDataset)); } }

        private int vDataset = -1;
        public int Dataset { get { return vDataset; } set { vDataset = value; OnPropertyChanged(nameof(Dataset)); } }

        private string vSelectedChartName;
        public string SelectedChartName { get { return vSelectedChartName; } set { vSelectedChartName = value; OnPropertyChanged(nameof(SelectedChartName)); } }

        private string vSelectedDatasetName;
        public string SelectedDatasetName { get { return vSelectedDatasetName; } set { vSelectedDatasetName = value; OnPropertyChanged(nameof(SelectedDatasetName)); } }

        private string vSelectedDataproviderName;
        public string SelectedDataproviderName { get { return vSelectedDataproviderName; } set { vSelectedDataproviderName = value; OnPropertyChanged(nameof(SelectedDataproviderName)); } }

        private DateTime vSelectedFromDate;
        public DateTime SelectedFromDate { get { return vSelectedFromDate; } set { vSelectedFromDate = value; OnPropertyChanged(nameof(SelectedFromDate)); } }

        private DateTime vSelectedToDate;
        public DateTime SelectedToDate { get { return vSelectedToDate; } set { vSelectedToDate = value; OnPropertyChanged(nameof(SelectedToDate)); } }

        private uint? vSelectedInterval;
        public uint? SelectedInterval { get { return vSelectedInterval; } set { vSelectedInterval = value; OnPropertyChanged(nameof(SelectedInterval)); } }

        private uint? vSelectedRange;
        public uint? SelectedRange { get { return vSelectedRange; } set { vSelectedRange = value; OnPropertyChanged(nameof(SelectedRange)); } }

        private DateTime? vFrom;
        public DateTime? From { get { return vFrom; } set { vFrom = value; OnPropertyChanged(nameof(From)); } }

        private DateTime? vTo;
        public DateTime? To { get { return vTo; } set { vTo = value; OnPropertyChanged(nameof(To)); } }

        private bool vSelectable;
        public bool Selectable { get { return vSelectable; } set { vSelectable = value; OnPropertyChanged(nameof(Selectable)); } }

        private bool vSeriesSelectable;
        public bool SeriesSelectable { get { return vSeriesSelectable; } set { vSeriesSelectable = value; OnPropertyChanged(nameof(SeriesSelectable)); } }

        private bool vGridLines;
        public bool GridLines { get { return vGridLines; } set { vGridLines = value; OnPropertyChanged(nameof(GridLines)); } }

        private bool vLabels;
        public bool Labels { get { return vLabels; } set { vLabels = value; OnPropertyChanged(nameof(Labels)); } }

        private bool vAllowEditAxis;
        public bool AllowEditAxis { get { return vAllowEditAxis; } set { vAllowEditAxis = value; OnPropertyChanged(nameof(AllowEditAxis)); } }

        private bool vDarkMode;
        public bool DarkMode { get { return vDarkMode; } set { vDarkMode = value; OnPropertyChanged(nameof(DarkMode)); } }

        private uint? vMaxRecords;
        public uint? MaxRecords { get { return vMaxRecords; } set { vMaxRecords = value; OnPropertyChanged(nameof(MaxRecords)); } }

        public event PropertyChangedEventHandler PropertyChanged;        

        public MainRecord()
        {
            MaxRecords = 250;
            GridLines = true;
            SeriesSelectable = true;
            Selectable = true;
            Labels = true;
            AllowEditAxis = true;
            InitializeData();
        }

        private void InitializeData() {
            Files.Add(Tuple.Create("gemini_ETHUSD_2021_1min.csv", @"Data/gemini_ETHUSD_2021_1min.csv"));

            CDDDataProvider dataprovider = new CDDDataProvider(Files[0].Item2, TimeSpan.FromHours(1), DateTimeKind.Local);
            Dataproviders.Add(new Tuple<string, CDDDataProvider>("Gemini ETH/USD (1h)", dataprovider));
            CDDDataProvider dataprovider2 = new CDDDataProvider(Files[0].Item2, TimeSpan.FromMinutes(15), DateTimeKind.Local);
            Dataproviders.Add(new Tuple<string, CDDDataProvider>("Gemini ETH/USD (15min)", dataprovider2));

            var ema8 = new EMA(dataprovider, 8);
            var ema25 = new EMA(dataprovider, 25);
            var ema25diff = new Derivative(ema25, 5, TimeSpan.FromMinutes(1), 3);
            var rsi10 = new RSI(dataprovider, 10);
            var rsi10diff = new Derivative(rsi10, 5, TimeSpan.FromMinutes(1), 5);

            var data = dataprovider.GetMarketData(new DateTime(2021, 5, 1), new DateTime(2021, 6, 10));
            var data2 = dataprovider2.GetMarketData(new DateTime(2021, 1, 1), new DateTime(2021, 2, 1));
            var ema8data = ema8.GetAnalyticsData(new DateTime(2021, 5, 1), new DateTime(2021, 6, 10));
            var ema25data = ema25.GetAnalyticsData(new DateTime(2021, 5, 1), new DateTime(2021, 6, 10));
            var ema25diffdata = ema25diff.GetAnalyticsData(new DateTime(2021, 5, 1), new DateTime(2021, 6, 10));
            var rsi10data = rsi10.GetAnalyticsData(new DateTime(2021, 5, 1), new DateTime(2021, 6, 10));
            var rsi10diffdata = rsi10diff.GetAnalyticsData(new DateTime(2021, 5, 1), new DateTime(2021, 6, 10));

            var dataset = data.Select(item => new Tuple<DateTime, decimal[]>(item.Time, new decimal[4] { item.Open, item.High, item.Low, item.Close })).ToList();
            var dataset2 = data2.Select(item => new Tuple<DateTime, decimal[]>(item.Time, new decimal[4] { item.Open, item.High, item.Low, item.Close })).ToList();
            var ema8dataset = ema8data.Select(item => new Tuple<DateTime, decimal[]>(item.Time, new decimal[1] { item.Value })).ToList();
            var ema25dataset = ema25data.Select(item => new Tuple<DateTime, decimal[]>(item.Time, new decimal[1] { item.Value })).ToList();
            var ema25diffdataset = ema25diffdata.Select(item => new Tuple<DateTime, decimal[]>(item.Time, new decimal[1] { item.Value * 100 })).ToList();
            var rsi10dataset = rsi10data.Select(item => new Tuple<DateTime, decimal[]>(item.Time, new decimal[1] { item.Value })).ToList();
            var rsi10diffdataset = rsi10diffdata.Select(item => new Tuple<DateTime, decimal[]>(item.Time, new decimal[1] { item.Value * 100 })).ToList();
            var volumedataset = data.Select(item => new Tuple<DateTime, decimal[]>(item.Time, new decimal[3] { item.Volume, item.Open, item.Close })).ToList();

            Datasets.Add(new Tuple<string, IEnumerable<Tuple<DateTime, decimal[]>>>("ETH/USD (1h)", dataset));
            Datasets.Add(new Tuple<string, IEnumerable<Tuple<DateTime, decimal[]>>>("ETH/USD (1h) EMA(8)", ema8dataset));
            Datasets.Add(new Tuple<string, IEnumerable<Tuple<DateTime, decimal[]>>>("ETH/USD (1h) EMA(25)", ema25dataset));
            Datasets.Add(new Tuple<string, IEnumerable<Tuple<DateTime, decimal[]>>>("ETH/USD (1h) EMADiff(25)", ema25diffdataset));
            Datasets.Add(new Tuple<string, IEnumerable<Tuple<DateTime, decimal[]>>>("ETH/USD (1h) RSI(10)", rsi10dataset));
            Datasets.Add(new Tuple<string, IEnumerable<Tuple<DateTime, decimal[]>>>("ETH/USD (1h) RSIDiff(10)", rsi10diffdataset));
            Datasets.Add(new Tuple<string, IEnumerable<Tuple<DateTime, decimal[]>>>("ETH/USD (1h) Volume", volumedataset));
            Datasets.Add(new Tuple<string, IEnumerable<Tuple<DateTime, decimal[]>>>("ETH/USD (15min)", dataset2));

            ChartSeries = new ObservableCollection<ChartSeries> {
                new ChartSeries("ETH/USD", SeriesType.OHLCCandles, dataset) { ChartDefinition = "main" },
                new ChartSeries("EMA(8)", SeriesType.Line, ema8dataset) { ChartDefinition = "main" },
                new ChartSeries("EMA(25)", SeriesType.Line, ema25dataset) { ChartDefinition = "main" },
                new ChartSeries("EMA Diff (25)", SeriesType.Line, AxisType.Right, ema25diffdataset) { /*ChartDefinition = "main"*/ },
                new ChartSeries("Volume", SeriesType.Bars, AxisType.Right, volumedataset) { ChartDefinition = "main" },
                new ChartSeries("RSI (10)", SeriesType.Line, rsi10dataset) { ChartDefinition = "sub" },
                new ChartSeries("RSI Diff (10)", SeriesType.Line, AxisType.Right, rsi10diffdataset) { },
                new ChartSeries("ETH/USD (15min)", SeriesType.OHLCCandles, dataset2)
            };
        }

        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
