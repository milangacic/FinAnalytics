using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using System.Windows.Markup;

[assembly: InternalsVisibleTo("ChartControlsTest")]
namespace ChartControls {

    // TODO: Use dynamic enumerations as data source instead of static lists

    /// <summary>
    /// Interaction logic for FinancialChartControl.xaml
    /// </summary>
    public partial class FinancialChartControl : FrameworkElement {

        #region Constants

        internal const uint MinPoints = 10;
        internal const uint MaxZoomSteps = 500;

        #endregion

        #region Fields        

        private readonly SelectionManager Selection;
        private readonly ScrollManager Scrolling;
        private readonly ZoomManager Zooming;
        private readonly EventManager Events;
        private readonly ChartRenderer Renderer;
        private readonly DimensionManager Dimensions;
        private readonly DataManager Data;
        private readonly CacheManager Cache;
        private readonly TextResources TextResources;

        #endregion

        #region Static

        #region Dependency properties

        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(DateTime), typeof(FinancialChartControl), new FrameworkPropertyMetadata(default(DateTime), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFromChanged));
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(DateTime), typeof(FinancialChartControl), new FrameworkPropertyMetadata(default(DateTime), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnToChanged));
        public static readonly DependencyProperty MaxRecordsProperty = DependencyProperty.Register("MaxRecords", typeof(uint?), typeof(FinancialChartControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMaxRecordsChanged));
        public static readonly DependencyProperty ChartDefitionsProperty = DependencyProperty.Register("ChartDefitions", typeof(ObservableCollection<ChartDefinition>), typeof(FinancialChartControl), new FrameworkPropertyMetadata(new ObservableCollection<ChartDefinition>() { new ChartDefinition() }, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnChartDefinitionsChanged));
        public static readonly DependencyProperty ChartSeriesProperty = DependencyProperty.Register("ChartSeries", typeof(ObservableCollection<ChartSeries>), typeof(FinancialChartControl), new FrameworkPropertyMetadata(new ObservableCollection<ChartSeries>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnChartSeriesChanged));
        public static readonly DependencyProperty SelectableProperty = DependencyProperty.Register("Selectable", typeof(bool), typeof(FinancialChartControl), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty ChartSeriesSelectableProperty = DependencyProperty.Register("ChartSeriesSelectable", typeof(bool), typeof(FinancialChartControl), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnChartSeriesSelectableChanged));
        public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register("Layout", typeof(ChartLayout), typeof(FinancialChartControl), new FrameworkPropertyMetadata(new ChartLayout(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnLayoutChanged));
        public static readonly DependencyProperty GridLinesProperty = DependencyProperty.Register("GridLines", typeof(bool), typeof(FinancialChartControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnGridLinesChanged));
        public static readonly DependencyProperty LabelsProperty = DependencyProperty.Register("Labels", typeof(bool), typeof(FinancialChartControl), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnLabelsChanged));
        public static readonly DependencyProperty AllowEditAxisProperty = DependencyProperty.Register("AllowEditAxis", typeof(bool), typeof(FinancialChartControl), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnAllowEditAxisChanged));
        public static readonly DependencyProperty DarkModeProperty = DependencyProperty.Register("DarkMode", typeof(bool), typeof(FinancialChartControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDarkModeChanged));


        #endregion

        #region Event handler

        private static void OnFromChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is not FinancialChartControl control) return;
            DateTime newfrom = (DateTime)e.NewValue;
            if (newfrom != control.vFrom) {
                (control.vFrom, control.vTo) = control.AdjustDates(newfrom, control.vTo, false);
                control.Cache.OnScrollingZooming();
                control.InvalidateVisual();
            }
        }

        private static void OnToChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is not FinancialChartControl control) return;
            DateTime newto = (DateTime)e.NewValue;
            if (newto != control.vTo) {
                (control.vFrom, control.vTo) = control.AdjustDates(control.vFrom, newto, false);
                control.Cache.OnScrollingZooming();
                control.InvalidateVisual();
            }
        }

        private static void OnMaxRecordsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is not FinancialChartControl control) return;
            uint? val = e.NewValue as uint?;
            if (val is not null and < MinPoints) val = MinPoints;
            if (control.vMaxRecords != val) {
                control.vMaxRecords = val;
                control.Cache.OnChartDataChanged();
                control.SetDates(control.Data.GetClosestDate(control.vFrom), control.Data.GetMaxTo(control.vFrom, false));
            }
        }

        private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is not FinancialChartControl control) return;
            if (e.NewValue is not ChartLayout val) return;
            val.PropertyChanged += control.OnLayoutPropertyChanged;
            if (control.vLayout != null) control.vLayout.PropertyChanged -= control.OnLayoutPropertyChanged;
            control.vLayout = val;
            control.Cache.ResetCache(true);
            control.InvalidateVisual();
        }

        private static void OnGridLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is not FinancialChartControl control) return;
            bool val = (bool)e.NewValue;
            control.vGridLines = val;
            control.InvalidateVisual();
        }

        private static void OnLabelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is not FinancialChartControl control) return;
            bool val = (bool)e.NewValue;
            control.vLabels = val;
            control.InvalidateVisual();
        }

        private static void OnAllowEditAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is not FinancialChartControl control) return;
            bool val = (bool)e.NewValue;
            control.vAllowEditAxis = val;
            control.Cache.OnLayoutChanged();
            control.InvalidateVisual();
        }

        private static void OnDarkModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is not FinancialChartControl control) return;
            bool val = (bool)e.NewValue;
            control.Layout = new(val);
        }

        private static void OnChartSeriesSelectableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is not FinancialChartControl control) return;
            control.Cache.OnLayoutChanged();
            control.InvalidateVisual();
        }

        private static void OnChartDefinitionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is not FinancialChartControl control) return;
            ObservableCollection<ChartDefinition> list = e.NewValue as ObservableCollection<ChartDefinition>;
            if (control.vChartDefinition != null) {
                control.DetachChartDefinitionsEventHandlers(control.vChartDefinition);
            }
            control.AttachChartDefinitionsEventHandlers(list);
            control.vChartDefinition = list;
            control.Cache.OnChartDataChanged();
            control.InvalidateVisual();
        }

        private static void OnChartSeriesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is not FinancialChartControl control) return;
            ObservableCollection<ChartSeries> list = e.NewValue as ObservableCollection<ChartSeries>;
            if (control.vChartSeries != null) {
                control.DetachChartSeriesEventHandlers(control.vChartSeries);
            }
            control.AttachChartSeriesEventHandlers(list);
            control.vChartSeries = list;
            control.Cache.OnChartDataChanged();
            control.InvalidateVisual();
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler HasScrolled;

        #endregion

        #region Properties

        private ObservableCollection<ChartDefinition> vChartDefinition;
        public ObservableCollection<ChartDefinition> ChartDefinitions {
            get {
                if (vChartDefinition == null) {
                    vChartDefinition = new();
                    SetValue(ChartDefitionsProperty, vChartDefinition);
                }
                return vChartDefinition;
            }
            set {
                SetValue(ChartDefitionsProperty, value);
            }
        }

        private ObservableCollection<ChartSeries> vChartSeries;
        public ObservableCollection<ChartSeries> ChartSeries {
            get {
                if (vChartSeries == null) {
                    vChartSeries = new();
                    SetValue(ChartSeriesProperty, vChartSeries);
                }
                return vChartSeries;
            }
            set {
                SetValue(ChartSeriesProperty, value);
            }
        }

        private DateTime vFrom;
        public DateTime From {
            get {
                if (vFrom == default) SetDates(Data.GetMinFrom(vTo), vTo);
                return vFrom;
            }
            set {
                SetRange(value, vTo);
            }
        }

        private DateTime vTo;
        public DateTime To {
            get {
                if (vTo == default) SetDates(vFrom, Data.GetMaxTo(vFrom));
                return vTo;
            }
            set {
                SetDates(vFrom, value);
            }
        }

        private uint? vMaxRecords;
        public uint? MaxRecords {
            get { return vMaxRecords; }
            set { SetValue(MaxRecordsProperty, value); }
        }

        public bool Selectable {
            get { return (bool)GetValue(SelectableProperty); }
            set { SetValue(SelectableProperty, value); }
        }

        public bool DarkMode {
            get { return (bool)GetValue(DarkModeProperty); }
            set { SetValue(DarkModeProperty, value); }
        }

        public bool ChartSeriesSelectable {
            get { return (bool)GetValue(ChartSeriesSelectableProperty); }
            set { SetValue(ChartSeriesSelectableProperty, value); }
        }

        private ChartLayout vLayout;
        public ChartLayout Layout {
            get {
                if (vLayout == null) vLayout = new(DarkMode);
                return vLayout;
            }
            set { SetValue(LayoutProperty, value); }
        }

        private bool vGridLines;
        public bool GridLines {
            get { return vGridLines; }
            set { SetValue(GridLinesProperty, value); }
        }

        private bool vLabels = true;
        public bool Labels {
            get { return vLabels; }
            set { SetValue(LabelsProperty, value); }
        }

        private bool vAllowEditAxis = true;
        public bool AllowEditAxis {
            get { return vAllowEditAxis; }
            set { SetValue(AllowEditAxisProperty, value); }
        }

        public int Count {
            get {
                if (!Cache.Count.HasValue) {
                    if (!ChartDefinitions.Any()) return 0;
                    Cache.Count = ChartSeries.Where(item => item.IsUsed).SelectMany(ser => GetData(ser).Select(item => item.Item1)).Distinct().Count();
                }
                return Cache.Count.Value;
            }
        }

        public bool HasData {
            get {
                if (!Cache.HasData.HasValue) {
                    if (!ChartDefinitions.Any()) return false;
                    if (!GetSelectedChartSeries().Any()) return false;
                    Cache.HasData = GetSelectedChartSeries().Any(ser => ser.Data != null && ser.Data.Any());
                }
                return Cache.HasData.Value;
            }
        }

        public DateTime MinTime {
            get {
                if (!Cache.MinTime.HasValue) {
                    if (!HasData) return default;
                    Cache.MinTime = MathHelper.Min(vChartSeries.Where(item => item.IsUsed && item.Data != null).Select(item => item.Data.Select(data => data.Item1)));
                }
                return Cache.MinTime.Value;
            }
        }

        public DateTime MaxTime {
            get {
                if (!Cache.MaxTime.HasValue) {
                    if (!HasData) return default;
                    Cache.MaxTime = MathHelper.Max(vChartSeries.Where(item => item.IsUsed && item.Data != null).Select(item => item.Data.Select(data => data.Item1)));
                }
                return Cache.MaxTime.Value;
            }
        }

        public double ChartWidth {
            get {
                if (!Cache.ChartWidth.HasValue) {
                    Cache.ChartWidth = RenderSize.Width - Layout.LeftMargin - Layout.RightMargin;
                }
                return Cache.ChartWidth.Value;
            }
        }

        public double ChartHeight {
            get {
                if (!Cache.ChartHeight.HasValue) {
                    Cache.ChartHeight = RenderSize.Height - Layout.TopMargin - Layout.BottomMargin;
                }
                return Cache.ChartHeight.Value;
            }
        }

        public new ContextMenu ContextMenu { get { return GetContextMenu(Mouse.PrimaryDevice.GetPosition(this)); } }

        #endregion

        #region Public methods        

        public FinancialChartControl() {
            Focusable = true;
            TextResources = new TextResources();
            Cache = new CacheManager();
            ChartDefinitions = new ObservableCollection<ChartDefinition>();
            Data = new DataManager(this, Cache);
            Dimensions = new DimensionManager(this, Data, Cache);
            Selection = new SelectionManager(this, Dimensions, Data, Cache);
            Scrolling = new ScrollManager(this, Dimensions, Data);
            Zooming = new ZoomManager(this, Data);
            Events = new EventManager(this);
            Renderer = new ChartRenderer(this, Selection, Dimensions, Zooming, Data, Cache);
            InitializeContextMenu();
            AttachEventHandlers();
            InitializeComponent();
        }

        public bool ScrollTo(DateTime date) {
            if (!HasData || date == default) return false;
            TimeSpan delta = date < From ? date - Data.GetClosestDate(From, 1):
                date > To ? date - Data.GetClosestDate(To, -1, false) :
                date >= From && date <= To ? date - Data.GetClosestDate(From + (To - From) / 2) :
                default;
            return ScrollBy(delta);
        }

        public bool ScrollBy(TimeSpan delta) {
            if (!Scrolling.ScrollBy(delta)) return false;
            MoveDates(vFrom, vTo, Math.Sign(delta.TotalMilliseconds));
            return true;
        }

        public void SetRange(DateTime from, DateTime to) {
            SetDates(from, to);
        }     

        public void Reset() {
            Cache.ResetCache();
            vFrom = default;
            vTo = default;
            if (HasData) {
                SetDates(Data.GetMinFrom(vTo, false), Data.GetMaxTo(vFrom, false));
            } else {
                InvalidateVisual();
            }
        }

        public void Reload() {
            Cache.ResetCache();
            Selection.ResetSelection();
            Zooming.StopRangeZoom();
            InvalidateVisual();
        }

        public BitmapSource CaptureScreenshot() {
            if (ChartWidth <= 0 || ChartHeight <= 0) return null;
            DrawingVisual drawing = new DrawingVisual();
            DrawingContext context = drawing.RenderOpen();            
            Renderer.RenderChart(context, new DpiScale(1, 1));
            context.Close();
            RenderTargetBitmap renderbitmap = new((int)RenderSize.Width, (int)RenderSize.Height, 96, 96, PixelFormats.Pbgra32);
            renderbitmap.Render(drawing);
            return renderbitmap;
        }

        ~FinancialChartControl() {
            DetachEventHandlers();
            Scrolling.Dispose();
        }

        #endregion

        #region Internal methods

        internal IEnumerable<Tuple<DateTime, decimal[]>> GetData(ChartSeries series) {
            if (series.Data == null) return Enumerable.Empty<Tuple<DateTime, decimal[]>>();
            IEnumerable<Tuple<DateTime, decimal[]>> data = series.Data.Where(item => item.Item1 >= (vFrom != default ? vFrom : DateTime.MinValue ) && item.Item1 <= (vTo != default ? vTo : DateTime.MaxValue));
            return MaxRecords.HasValue && (vFrom == default || vTo == default) ? data.Take((int)MaxRecords.Value) : data;
        }

        internal IEnumerable<ChartSeries> GetSelectedChartSeries(ChartDefinition chartdef = null) {
            if (Cache.ChartSeriesGroup == null) {
                Cache.ChartSeriesGroup = ChartDefinitions.Where(def => def != null).ToDictionary(def => def, def => ChartSeries.Where(series => series.ChartDefinition != null && series.ChartDefinition == def.ID));
            }
            return chartdef != null ? Cache.ChartSeriesGroup[chartdef] : Cache.ChartSeriesGroup.SelectMany(item => item.Value);
        }

        internal IEnumerable<ChartSeries> GetVisibleChartSeries(DateTime date = default) {
            if (date == default) return GetVisibleChartSeries(vFrom != default ? vFrom : DateTime.MinValue, vTo != default ? vTo : DateTime.MaxValue);
            bool _condition(ChartSeries ser, DateTime time) {
                if (ser.HasBars) {
                    return time - Layout.LargeBarSize * Data.GetInterval(ser) / 2 <= date && time + Layout.LargeBarSize * Data.GetInterval(ser) / 2 >= date;
                } else {
                    return time == date;
                }
            }
            return ChartSeries.Where(ser => ser.IsUsed && ser.Data.Any(val => _condition(ser, val.Item1)));
        }

        internal IEnumerable<ChartSeries> GetVisibleChartSeries(DateTime from, DateTime to) {
            if (to == default) to = DateTime.MaxValue;
            if (from == default) from = DateTime.MinValue;
            bool _condition(ChartSeries ser, DateTime time) {
                if (ser.HasBars) {
                    return time - Layout.LargeBarSize * Data.GetInterval(ser) / 2 <= to && time + Layout.LargeBarSize * Data.GetInterval(ser) / 2 >= from;
                } else {
                    return time >= from && time <= to;
                }
            }
            return ChartSeries.Where(ser => ser.IsUsed && ser.Data.Any(val => _condition(ser, val.Item1)));
        }

        internal EventManager GetEventManager() {
            return Events;
        }

        internal ContextMenu GetContextMenu(Point point) {
            ChartDefinition chart = Dimensions.PointToChart(point);
            if (chart == null) return null;
            if (Cache.ContextMenus == null) InitializeContextMenu();
            if (Cache.ContextMenus != null && Cache.ContextMenus.TryGetValue(chart, out ContextMenu menu)) return menu;
            return null;
        }

        internal Popup GetSelectBox() {
            if (Cache.SelectBox == null) InitializeSelectBox();
            return Cache.SelectBox;
        }        

        #endregion

        #region Event Handler        

        private void OnChartSeriesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            AttachChartSeriesItemsEventHandlers(e.NewItems?.Cast<ChartSeries>());
            DetachChartSeriesItemsEventHandlers(e.OldItems?.Cast<ChartSeries>());
            Cache.OnChartDataChanged();
            InvalidateVisual();
        }

        private void OnChartDefinitionsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            AttachChartDefinitionsItemsEventHandlers(e.NewItems?.Cast<ChartDefinition>());
            DetachChartDefinitionsItemsEventHandlers(e.OldItems?.Cast<ChartDefinition>());
            Cache.OnChartDataChanged();
            InvalidateVisual();
        }

        private void OnChartSeriesDataChanged(object sender, PropertyChangedEventArgs e) {
            Cache.OnChartDataChanged();
            InvalidateVisual();
        }

        private void OnChartDefinitionChanged(object sender, PropertyChangedEventArgs e) {
            Cache.OnChartDataChanged();
            InvalidateVisual();
        }

        private void OnLayoutPropertyChanged(object sender, PropertyChangedEventArgs e) {
            Cache.OnLayoutChanged();
            InvalidateVisual();
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi) {
            base.OnDpiChanged(oldDpi, newDpi);
            Renderer.SetDpi(newDpi);
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
            if (ChartWidth <= 0 || ChartHeight <= 0) return;
            Renderer.RenderChart(drawingContext);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            bool rangeselection = Dimensions.IsXAxis(e.GetPosition(this));
            Mouse.OverrideCursor = rangeselection ? Cursors.SizeWE : Cursor;
        }

        protected override Size MeasureOverride(Size availableSize) {
            if (availableSize.Width < Layout.LeftMargin + Layout.RightMargin + 100) return availableSize;
            if (availableSize.Height < Layout.TopMargin + Layout.BottomMargin + 100) return availableSize;
            Selection.ResetSelection();
            Cache.OnResize();
            return base.MeasureOverride(availableSize);
        }

        private void OnLeftRightKey(object sender, int e) {
            if (!Selection.IsSelected()) return;
            Selection.MoveSelection(e);
        }

        private void OnDrag(object sender, Point e) {
            if (!GetSelectedChartSeries().Any()) return;
            if (Selection.IsSelected()) {
                Selection.ResetSelection();
                Cache.SelectBox.IsOpen = false;
            }
            if (Zooming.IsRangeZooming()) {
                if (Dimensions.IsXAxis(e.X)) {
                    Zooming.RangeZoom(Dimensions.PointToData(e.X));
                    InvalidateVisual();
                }
            } else if (!Scrolling.IsScrolling() && Dimensions.IsXAxis(e)) {
                Zooming.RangeZoom(Dimensions.PointToData(e.X));
            } else {
                Scrolling.ScrollTo(e);
            }
        }

        private void OnStopDragging(object sender, Point e) {
            if (Zooming.IsRangeZooming()) {
                if (Dimensions.IsXAxis(e.X)) {
                    Zooming.FinishRangeZoom(Dimensions.PointToData(e.X));
                    InvalidateVisual();
                } else {
                    Zooming.StopRangeZoom();
                    InvalidateVisual();
                }
            } else Scrolling.StopScrolling();
        }

        private void OnThrow(object sender, int e) {
            if (Zooming.IsRangeZooming()) {
                Zooming.FinishRangeZoom();
                InvalidateVisual();
            } else if (e == 0) Scrolling.StopScrolling();
            else Scrolling.AutoScrollBy(e);
        }

        private void OnLeftClick(object sender, Point e) {
            if (Scrolling.IsScrolling()) Scrolling.StopScrolling();
            if (Selectable && Dimensions.PointToChart(e) != null && Count > 0) {
                if (Cache.SelectBox == null) InitializeSelectBox();
                Selection.SelectPoint(e);
                if (Selection.IsSelected()) {
                    Cache.SelectBox.IsOpen = true;
                }
            }
        }

        private void OnRightClick(object sender, Point e) {            
            if (Selection.IsSelected()) {
                Selection.ResetSelection();
            }
            ContextMenu menu = GetContextMenu(e);
            if (menu == null) return;
            menu.IsOpen = true;
        }

        private void OnDoubleLeftClick(object sender, Point e) {
            Reset();
        }

        private void OnWheel(object sender, int e) {
            if (Selection.IsSelected()) Selection.ResetSelection();
            if (Scrolling.IsScrolling()) Scrolling.StopScrolling();
            if (e < 0 && MaxRecords.HasValue && Count >= MaxRecords) return;
            Zooming.Zoom(e);
            Cache.OnScrollingZooming();
        }

        private void OnScrolling(object sender, TimeSpan e) {
            Cache.OnScrollingZooming();
            vFrom += e;
            vTo += e;
            InvalidateVisual();
        }

        private void OnScrolled(object sender, EventArgs e) {
            (DateTime newfrom, DateTime newto) = AdjustDates(vFrom, vTo, true);
            UpdateDates(newfrom, newto);
            HasScrolled?.Invoke(this, null);
        }

        private void OnSelectionChanged(object sender, EventArgs e) {
            if (Selection.IsSelected()) {
                UpdateSelectBox();
            }
            InvalidateVisual();
        }

        private void OnZooming(object sender, Tuple<DateTime, DateTime> e) {
            Cache.OnScrollingZooming();
            SetDates(e.Item1, e.Item2);
        }

        private void OnLeave(object sender, EventArgs e) {
            if (Zooming.IsRangeZooming()) {
                Zooming.StopRangeZoom();
                InvalidateVisual();
            }
        }

        #endregion

        #region Private methods                     

        private void SetDates(DateTime from, DateTime to) {
            (DateTime newfrom, DateTime newto) = AdjustDates(from, to, false);
            UpdateDates(newfrom, newto);
        }

        private void MoveDates(DateTime from, DateTime to, int direction) {
            (DateTime newfrom, DateTime newto) = AdjustDates(from, to, true, direction);
            UpdateDates(newfrom, newto);
        }

        private void UpdateDates(DateTime from, DateTime to) {
            if (vFrom != from || vTo != to) {
                vFrom = from;
                vTo = to;
                Cache.OnScrollingZooming();
                SetValue(FromProperty, vFrom);
                SetValue(ToProperty, vTo);
                InvalidateVisual();
            }
        }

        private (DateTime, DateTime) AdjustDates(DateTime from, DateTime to, bool keeprange, int direction = 0) {            
            if (from != default && to != default) {                
                if (from > to) {
                    (from, to) = (to, from);
                }
                // Adjust to max range
                if (!keeprange) {
                    DateTime fromdate = Data.GetClosestDate(from, true);
                    DateTime todate = Data.GetClosestDate(to, false);
                    DateTime maxto = Data.GetMaxTo(fromdate, false);
                    if (maxto < todate) {
                        from = fromdate;
                        to = maxto;
                    }
                }
            }
            if (from == default || to == default || !GetVisibleChartSeries(from, to).Any()) return (from, to);
            // Map to actual data points
            var fromdatapoint = Data.GetClosestDate(from, direction, true);
            var todatapoint = Data.GetClosestDate(to, direction, false);
            // Adjust by half an interval to make bars fully visible
            var fromseries = GetVisibleChartSeries(fromdatapoint).Where(ser => ser.HasBars).ToList();
            var toseries = GetVisibleChartSeries(todatapoint).Where(ser => ser.HasBars).ToList();
            var fromadj = fromseries.Any() ? -Data.GetIntervals(fromseries).Max() / 2 : new TimeSpan(0);
            var toadj = toseries.Any() ? Data.GetIntervals(toseries).Max() / 2 : new TimeSpan(0);
            // Make adjustments (keep or disregard initial range)
            TimeSpan maxint = Data.GetMaxInterval(from, to);
            if (keeprange) {
                TimeSpan range = to - from;                
                if (range < maxint) range = maxint;
                var fromdistance = Math.Abs((fromdatapoint + fromadj - from).TotalMilliseconds);
                var todistance = Math.Abs((todatapoint + toadj - to).TotalMilliseconds);
                if (fromdistance < todistance) {
                    from = fromdatapoint + fromadj;
                    to = from + range;
                } else {
                    to = todatapoint + toadj;
                    from = to - range;
                }
            } else {
                from = fromdatapoint + fromadj;
                to = todatapoint + toadj;
                if (to - from < maxint) to = from + maxint;
            }
            return (from, to);
        }

        private bool InitializeContextMenu() {            
            Cache.ContextMenus = new();
            if (!ChartSeriesSelectable && !AllowEditAxis) return false;
            foreach (ChartDefinition chartdef in ChartDefinitions) {
                ContextMenu menu = new() { PlacementTarget = this, Placement = PlacementMode.MousePoint };
                if (ChartSeriesSelectable) {
                    IEnumerable<ChartSeries> assigned = GetSelectedChartSeries(chartdef);
                    IEnumerable<ChartSeries> unassigned = ChartSeries.Where(item => string.IsNullOrEmpty(item.ChartDefinition) && chartdef.GetAxis(item.Axis) != null);
                    foreach (ChartSeries series in assigned) {
                        MenuItem item = new() { Header = series.Title, IsCheckable = true, IsChecked = true };
                        item.Click += (object s, RoutedEventArgs arg) => AssignSeries(s, chartdef, series);
                        menu.Items.Add(item);
                    }
                    if (assigned.Any() && unassigned.Any()) menu.Items.Add(new Separator());
                    foreach (ChartSeries series in unassigned) {
                        MenuItem item = new() { Header = series.Title, IsCheckable = true, IsChecked = false };
                        item.Click += (object s, RoutedEventArgs arg) => AssignSeries(s, chartdef, series);
                        menu.Items.Add(item);
                    }
                }
                if (AllowEditAxis) {
                    if (ChartSeriesSelectable) menu.Items.Add(new Separator());
                    menu.Items.Add(CreateAxisMenu((string)TextResources["TimeAxis"], this));
                    if (chartdef.LeftAxis != null) {
                        menu.Items.Add(CreateAxisMenu((string)TextResources["LeftAxis"], chartdef.LeftAxis));
                    }
                    if (chartdef.RightAxis != null) {
                        menu.Items.Add(CreateAxisMenu((string)TextResources["RightAxis"], chartdef.RightAxis));
                    }
                }
                Cache.ContextMenus.Add(chartdef, menu);
            }
            return true;
        }

        private MenuItem CreateAxisMenu(string name, object bindingobject) {
            MenuItem axismenu = new() { Header = name };
            MenuItem item = new() { Header = (string)TextResources["Labels"], IsCheckable = true };
            Binding labelbinding = new("Labels");
            labelbinding.Source = bindingobject;
            BindingOperations.SetBinding(item, MenuItem.IsCheckedProperty, labelbinding);
            axismenu.Items.Add(item);
            item = new() { Header = (string)TextResources["GridLines"], IsCheckable = true };
            Binding gridlinesbinding = new Binding("GridLines");
            gridlinesbinding.Source = bindingobject;
            BindingOperations.SetBinding(item, MenuItem.IsCheckedProperty, gridlinesbinding);
            axismenu.Items.Add(item);
            return axismenu;
        }

        private void InitializeContextLayout() {
            Style contextstyle = new(typeof(ContextMenu));
            contextstyle.Setters.Add(new Setter(ContextMenu.BackgroundProperty, Layout.PopupBackroundBrush));
            contextstyle.Setters.Add(new Setter(ContextMenu.OpacityProperty, Layout.PopupOpacity));
            contextstyle.Setters.Add(new Setter(ContextMenu.BorderBrushProperty, Layout.PopupBorderBrush));
            contextstyle.Setters.Add(new Setter(ContextMenu.BorderThicknessProperty, Layout.PopupBorderThickness));
            contextstyle.Setters.Add(new Setter(ContextMenu.OverridesDefaultStyleProperty, true));
            contextstyle.Setters.Add(new Setter(ContextMenu.SnapsToDevicePixelsProperty, true));
            FrameworkElementFactory contextborder = new FrameworkElementFactory(typeof(Border));
            contextborder.SetValue(Border.BackgroundProperty, Layout.PopupBackroundBrush);
            contextborder.SetValue(Border.OpacityProperty, Layout.PopupOpacity);
            contextborder.SetValue(Border.CornerRadiusProperty, new CornerRadius(Layout.PopupBorderRadius));
            contextborder.SetValue(Border.BorderBrushProperty, Layout.PopupBorderBrush);
            contextborder.SetValue(Border.BorderThicknessProperty, Layout.PopupBorderThickness);
            FrameworkElementFactory contextpanel = new FrameworkElementFactory(typeof(StackPanel));
            contextpanel.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);
            contextpanel.SetValue(StackPanel.IsItemsHostProperty, true);
            contextpanel.SetValue(StackPanel.ClipToBoundsProperty, true);
            contextpanel.SetValue(StackPanel.MarginProperty, new Thickness(5, 4, 5, 4));
            contextborder.AppendChild(contextpanel);
            ControlTemplate contexttemplate = new(typeof(ContextMenu));
            contexttemplate.VisualTree = contextborder;
            contextstyle.Setters.Add(new Setter(ContextMenu.TemplateProperty, contexttemplate));
            Style menuitemstyle = new(typeof(MenuItem));
            menuitemstyle.Setters.Add(new Setter(MenuItem.BackgroundProperty, Layout.PopupBackroundBrush));
            menuitemstyle.Setters.Add(new Setter(MenuItem.ForegroundProperty, Layout.TextBrush));
            menuitemstyle.Setters.Add(new Setter(MenuItem.BorderBrushProperty, Layout.PopupBorderBrush));
            menuitemstyle.Setters.Add(new Setter(MenuItem.FontFamilyProperty, Layout.TextFont.FontFamily));
            menuitemstyle.Setters.Add(new Setter(MenuItem.FontStretchProperty, Layout.TextFont.Stretch));
            menuitemstyle.Setters.Add(new Setter(MenuItem.FontStyleProperty, Layout.TextFont.Style));
            menuitemstyle.Setters.Add(new Setter(MenuItem.FontWeightProperty, Layout.TextFont.Weight));
            menuitemstyle.Setters.Add(new Setter(MenuItem.FontSizeProperty, Renderer.GetFontSize(Layout.TextSize)));
            if (Resources.Contains(typeof(ContextMenu))) Resources[typeof(ContextMenu)] = contextstyle;
            else Resources.Add(typeof(ContextMenu), menuitemstyle);
            if (Resources.Contains(typeof(MenuItem))) Resources[typeof(MenuItem)] = menuitemstyle;
            else Resources.Add(typeof(MenuItem), menuitemstyle);
        }

        private void InitializeSelectBox() {
            Cache.SelectBox = new() { Placement = PlacementMode.RelativePoint, PlacementTarget = this, StaysOpen = false, AllowsTransparency = true, PopupAnimation = PopupAnimation.Fade };
            Border border = new() { CornerRadius = new CornerRadius(Layout.PopupBorderRadius), Padding = new Thickness(Layout.PopupMargin), Background = Layout.PopupBackroundBrush, BorderBrush = Layout.PopupBorderBrush, BorderThickness = Layout.PopupBorderThickness };
            Grid grid = new Grid();
            Style textstyle = new Style(typeof(TextBlock));
            textstyle.Setters.Add(new Setter(TextBlock.FontSizeProperty, Renderer.GetFontSize(Layout.TextSize)));
            if (Resources.Contains(typeof(TextBlock))) Resources[typeof(TextBlock)] = textstyle;
            else Resources.Add(typeof(TextBlock), textstyle);
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            TextBlock titleblock = new TextBlock() { Text = (string)TextResources["Time"], Margin = new Thickness(0, 0, 5, 0), Foreground = Layout.TextBrush };
            TextBlock valueblock = new TextBlock() { Margin = new Thickness(5, 0, 0, 0), Foreground = Layout.TextBrush };
            Grid.SetColumn(titleblock, 0);
            Grid.SetRow(titleblock, 0);
            Grid.SetColumn(valueblock, 1);
            Grid.SetRow(valueblock, 0);
            grid.Children.Add(titleblock);
            grid.Children.Add(valueblock);
            border.Child = grid;
            Cache.SelectBox.Child = new Border() { Padding = new Thickness(Layout.PopupMargin), Child = border };
            Cache.SelectBox.Focusable = false;
            Cache.SelectBox.MouseDown += (object sender, MouseButtonEventArgs e) => { Cache.SelectBox.IsOpen = false; };
            Cache.SelectBox.Closed += (object sender, EventArgs e) => { Selection.ResetSelection(); InvalidateVisual(); };
        }

        private void UpdateSelectBox() {
            if (Cache.SelectBox is null) throw new Exception("Popup control missing!");
            if ((Cache.SelectBox.Child as Border).Child is not Border border) throw new ArgumentException("No border found!");
            if (border.Child is not Grid grid) throw new ArgumentException("No grid found!");
            if (grid.Children[1] is not TextBlock dateblock) throw new ArgumentException("No textblock found!");
            dateblock.Text = string.Format("{0}", Selection.SelectedTime);
            int row = 1;
            foreach (ChartSeries series in Selection.SelectedSeries) {
                Tuple<DateTime, decimal[]> data = series.Data.FirstOrDefault(item => item.Item1 == Selection.SelectedTime);
                if (data is null) continue;
                TextBlock titleblock, valueblock;
                if (grid.RowDefinitions.Count <= row) {
                    grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    titleblock = new TextBlock() { Margin = new Thickness(0, 0, 5, 0) };
                    valueblock = new TextBlock() { Margin = new Thickness(5, 0, 0, 0) };
                    Grid.SetColumn(titleblock, 0);
                    Grid.SetRow(titleblock, row);
                    Grid.SetColumn(valueblock, 1);
                    Grid.SetRow(valueblock, row);
                    grid.Children.Add(titleblock);
                    grid.Children.Add(valueblock);
                } else {
                    titleblock = grid.Children[2 * row] as TextBlock;
                    valueblock = grid.Children[2 * row + 1] as TextBlock;
                    if (titleblock == null || valueblock == null) throw new ArgumentException("No textblock found!");
                    titleblock.Visibility = Visibility.Visible;
                    valueblock.Visibility = Visibility.Visible;
                }
                titleblock.Foreground = series.Type == SeriesType.OHLCCandles ? (data.Item2[3] >= data.Item2[0] ? Layout.OHLCGainBrush : Layout.OHLCLossBrush) : GetTextBrush(series);
                valueblock.Foreground = titleblock.Foreground;
                titleblock.Text = series.Title;
                if (series.Type == SeriesType.OHLCCandles) {
                    valueblock.Text = string.Format("O {0:F4} H {1:F4} L {2:F4} C {3:F4}", data.Item2[0], data.Item2[1], data.Item2[2], data.Item2[3]);
                } else {
                    valueblock.Text = string.Format("{0:F4}", data.Item2[0]);
                }
                row++;
            }
            for (int i = 2 * row; i < grid.Children.Count; i += 2) {
                grid.Children[i].Visibility = Visibility.Collapsed;
                grid.Children[i + 1].Visibility = Visibility.Collapsed;
            }
            Cache.SelectBox.HorizontalOffset = Selection.SelectedPoint.X;
            Cache.SelectBox.VerticalOffset = Selection.SelectedPoint.Y;
        }

        private Brush GetTextBrush(ChartSeries series) {
            if (series.SeriesStyle.TextBrush != null) return series.SeriesStyle.TextBrush;
            if (Cache.DefaultChartStyles == null) Cache.DefaultChartStyles = new();
            if (Cache.DefaultChartStyles.TryGetValue(series, out SeriesStyle def)) return def.TextBrush;
            return Layout.TextBrush;
        }

        private static void AssignSeries(object item, ChartDefinition chart, ChartSeries series) {
            if (item is not MenuItem menu) return;
            series.ChartDefinition = menu.IsChecked ? chart.ID : null;
        }

        #region Event Handler initialization

        private void AttachEventHandlers() {
            Events.Wheel += OnWheel;
            Events.LeftClick += OnLeftClick;
            Events.RightClick += OnRightClick;
            Events.Drag += OnDrag;
            Events.StopDragging += OnStopDragging;
            Events.Throw += OnThrow;
            Events.LeftRightKey += OnLeftRightKey;
            Events.DoubleLeftClick += OnDoubleLeftClick;
            Events.Leave += OnLeave;
            Scrolling.Scrolling += OnScrolling;
            Scrolling.Scrolled += OnScrolled;
            Selection.SelectionChanged += OnSelectionChanged;
            Zooming.Zooming += OnZooming;
        }

        private void DetachEventHandlers() {
            Events.Wheel -= OnWheel;
            Events.LeftClick -= OnLeftClick;
            Events.RightClick -= OnRightClick;
            Events.Drag -= OnDrag;
            Events.StopDragging -= OnStopDragging;
            Events.Throw -= OnThrow;
            Events.LeftRightKey -= OnLeftRightKey;
            Events.DoubleLeftClick -= OnDoubleLeftClick;
            Events.Leave -= OnLeave;
            Scrolling.Scrolling -= OnScrolling;
            Scrolling.Scrolled -= OnScrolled;
            Selection.SelectionChanged -= OnSelectionChanged;
            Zooming.Zooming -= OnZooming;
        }

        private void AttachChartDefinitionsEventHandlers(ObservableCollection<ChartDefinition> items) {
            if (items == null) return;
            items.CollectionChanged += OnChartDefinitionsCollectionChanged;
            AttachChartDefinitionsItemsEventHandlers(items);
        }

        private void AttachChartDefinitionsItemsEventHandlers(IEnumerable<ChartDefinition> items) {
            if (items == null) return;
            foreach (ChartDefinition item in items) {
                if (item == null) continue;
                item.PropertyChanged += OnChartDefinitionChanged;
            }
        }

        private void DetachChartDefinitionsEventHandlers(ObservableCollection<ChartDefinition> items) {
            items.CollectionChanged -= OnChartDefinitionsCollectionChanged;
            DetachChartDefinitionsItemsEventHandlers(items);
        }

        private void DetachChartDefinitionsItemsEventHandlers(IEnumerable<ChartDefinition> items) {
            if (items == null) return;
            foreach (ChartDefinition item in items) {
                if (item == null) continue;
                item.PropertyChanged -= OnChartDefinitionChanged;
            }
        }

        private void AttachChartSeriesEventHandlers(ObservableCollection<ChartSeries> items) {
            if (items == null) return;
            items.CollectionChanged += OnChartSeriesCollectionChanged;
            AttachChartSeriesItemsEventHandlers(items);
        }

        private void AttachChartSeriesItemsEventHandlers(IEnumerable<ChartSeries> items) {
            if (items == null) return;
            foreach (ChartSeries item in items) {
                if (item == null) continue;
                item.PropertyChanged += OnChartSeriesDataChanged;
            }
        }

        private void DetachChartSeriesEventHandlers(ObservableCollection<ChartSeries> items) {
            if (items == null) return;
            items.CollectionChanged -= OnChartSeriesCollectionChanged;
            DetachChartSeriesItemsEventHandlers(items);
        }

        private void DetachChartSeriesItemsEventHandlers(IEnumerable<ChartSeries> items) {
            if (items == null) return;
            foreach (ChartSeries item in items) {
                if (item == null) continue;
                item.PropertyChanged -= OnChartSeriesDataChanged;
            }
        }

        #endregion

        #endregion
    }
}