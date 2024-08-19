using AnalyticsEngine;
using ChartControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Automation;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Text;
using Path = System.IO.Path;
using System.Windows.Media.Media3D;

namespace ChartControlsTest {
    [TestClass]    
    public class FinancialChartControlTest {

        #region Constants

        const string PATH_GEMINI_1MIN = @"Data/gemini_ETHUSD_2021_1min.csv";
        const int WINDOW_WIDTH = 1900;
        const int WINDOW_HEIGHT = 950;
        const int CONTROL_WIDTH = 1884;
        const int CONTROL_HEIGHT = 911;
        const int MAXRECORDS = 250;
        const bool GENERATEIMAGES = false; // Switch between run test and generate results
        static readonly DateTime FROM = new DateTime(2021, 5, 1);
        static readonly DateTime TO = new DateTime(2021, 6, 10);
        static readonly DateTime FROM2 = new DateTime(2021, 1, 1);
        static readonly DateTime TO2 = new DateTime(2021, 2, 1);

        #endregion

        #region Fields

        Window TestWindow;
        FinancialChartControl ChartControl;
        ObservableCollection<ChartDefinition> ChartDefinitions = new();
        ObservableCollection<ChartSeries> ChartSeries = new();
        string DelayedView;

        static CDDDataProvider Dataprovider;
        static CDDDataProvider Dataprovider2;
        static EMA EMA8;
        static EMA EMA25;
        static RSI RSI10;
        static List<Tuple<DateTime, decimal[]>> ChartData;
        static List<Tuple<DateTime, decimal[]>> ChartData2;
        static List<Tuple<DateTime, decimal[]>> EMA8Data;
        static List<Tuple<DateTime, decimal[]>> EMA25Data;
        static List<Tuple<DateTime, decimal[]>> RSI10Data;
        static List<Tuple<DateTime, decimal[]>> VolumeData;
        static ChartSeries MainSeries;
        static ChartSeries MainSeries2;
        static ChartSeries EMA8Series;
        static ChartSeries EMA25Series;
        static ChartSeries VolumeSeries;
        static ChartSeries VolumeSeries2;
        static ChartSeries RSI10Series;

        #endregion

        #region Initialization & Cleanup    

        [ClassInitialize]
        public static void InitializeData(TestContext context) {
            InitializeData();
        }

        [ClassCleanup]
        public static void CleanupData() {
            EMA8?.Dispose();
            EMA25?.Dispose();
            RSI10?.Dispose();
            Dataprovider?.Dispose();
        }

        [TestInitialize]
        public void InitializeControls() {            
            InitializeChartDefinitions();
            InitializeChartSeries();

            ChartControl = new FinancialChartControl() {
                GridLines = true,
                Labels = true,
                ChartDefinitions = ChartDefinitions,
                ChartSeries = ChartSeries,
                Layout = GetLayout(),
                Width = CONTROL_WIDTH,
                Height = CONTROL_HEIGHT
            };

            TestWindow = new() { Left = 0, Top = 0, Width = WINDOW_WIDTH, Height = WINDOW_HEIGHT };
            TestWindow.Content = ChartControl;
            TestWindow.Show();
        }
        
        [TestCleanup]
        public void CleanupControls() {
            TestWindow.Close();
        }

        #endregion

        /*
         * RANGE TEST
         * NO MAX RECORDS, GRID LINES, LABELS
         * ------------------------
         * a) Full view (days): 1/5 - 6/10
         * b) Medium view (days, hours): 15/5 - 17/5
         * c) Close view (days, hours): 16/5 - 17/5
         * d) Very close view (days, hours, minutes): 16/5 10:00h - 16/5 22:00h
         * e) Reset to full view again
         * f) Full view (days, hours): 1/5 - 2/5
         * g) Medium view (days, hours, minutes): 1/5 6:00 - 1/5 18:00
         * h) Close view (days, hours, minutes): 1/5 11:00 - 1/5 13:00
         * i) Very close view (days, hours, minutes): 1/5 12:50 - 1/5 13:10
         * j) Extreme close view (days, hours, minutes, seconds): 1/5 12:55 - 1/5 13:05
         * k) Reset to full view again
        */

        [WpfTestMethod]
        [TestCategory("Static test cases")]
        public void RangeTest() {
            ChartControl.Labels = true;
            ChartControl.GridLines = true;
            ChartControl.MaxRecords = null;
            // a) Full view (days): 1/5 - 6/10
            CompareView("FullView");
            // b) Medium view (days, hours): 15/5 - 17/5
            ChartControl.SetRange(new DateTime(2021, 5, 15), new DateTime(2021, 5, 17));
            CompareView("MediumView");
            // c) Close view (days, hours): 16/5 - 17/5
            ChartControl.SetRange(new DateTime(2021, 5, 16), new DateTime(2021, 5, 17));
            CompareView("CloseView");
            // d) Very close view (days, hours, minutes): 16/5 10:00h - 16/5 22:00h
            ChartControl.SetRange(new DateTime(2021, 5, 16, 10, 0, 0), new DateTime(2021, 5, 16, 22, 0, 0));
            CompareView("VeryCloseView");
            // e) Reset to full view again
            ChartControl.Reset();
            CompareView("FullView");
            // f) Full view (days, hours): 1/5 - 2/5
            InitializeData(1);
            InitializeChartSeries();
            ChartControl.Reset();
            CompareView("DetailedFullView");
            // g) Medium view (days, hours, minutes): 1/5 6:00 - 1/5 18:00
            ChartControl.SetRange(new DateTime(2021, 5, 1, 6, 0, 0), new DateTime(2021, 5, 1, 18, 0, 0));
            CompareView("DetailedMediumView");
            // h) Close view (days, hours, minutes): 1/5 11:00 - 1/5 13:00
            ChartControl.SetRange(new DateTime(2021, 5, 1, 11, 0, 0), new DateTime(2021, 5, 1, 13, 0, 0));
            CompareView("DetailedCloseView");
            // i) Very close view (days, hours, minutes): 1/5 12:50 - 1/5 13:10
            ChartControl.SetRange(new DateTime(2021, 5, 1, 12, 50, 0), new DateTime(2021, 5, 1, 13, 10, 0));
            CompareView("DetailedVeryCloseView");
            // j) Extreme close view (days, hours, minutes, seconds): 1/5 12:55 - 1/5 13:05
            ChartControl.SetRange(new DateTime(2021, 5, 1, 12, 55, 0), new DateTime(2021, 5, 1, 13, 5, 0));
            CompareView("DetailedExtremeCloseView");
            // k) Reset to full view again
            ChartControl.Reset();
            CompareView("DetailedFullView");
            InitializeData();
        }

        /*
         * SCROLL TEST (01/05/21 - 10/06/21; 1H)
         * NO MAX RECORDS, GRID LINES, LABELS, 15/5 - 17/5
         * ------------------------
         * a) Scroll to right: 18/5
         * b) Scroll to center: 18/5
         * c) Scroll to left: 14/5
         * d) Scroll to right (adjust to grid) : 18/5 0:29
        */

        [WpfTestMethod]
        [TestCategory("Static test cases")]
        public void ScrollTest() {
            ChartControl.Labels = true;
            ChartControl.GridLines = true;
            ChartControl.MaxRecords = null;
            // a) Scroll to right: 18/5
            ChartControl.SetRange(new DateTime(2021, 5, 15), new DateTime(2021, 5, 17));
            ChartControl.ScrollTo(new DateTime(2021, 5, 18));
            CompareView("ScrollRight");
            // b) Scroll to center: 18/5
            ChartControl.ScrollTo(new DateTime(2021, 5, 18));
            CompareView("ScrollCenter");
            // c) Scroll to left: 14/5
            ChartControl.ScrollTo(new DateTime(2021, 5, 14));
            CompareView("ScrollLeft");
            // d) Scroll to right (adjust to grid) : 18/5 0:29
            ChartControl.ScrollTo(new DateTime(2021, 5, 18, 0, 29, 0));
            CompareView("ScrollRight");
        }

        /*
         * REFRESH TEST (01/05/21 - 10/06/21; 1H)
         * ------------------------------------------------
         * a) Change Max Records
         * b) Change Grid Lines
         * c) Change Labels
         * d) Change Chart Series selection
         * e) Change Chart Definition height
         * f) Change Layout
         * e) Change Chart Series style
         * f) Change Axis grid lines
         * g) Change Axis lines
         * h) Refresh and Capture method
         */

        [WpfTestMethod]
        [TestCategory("Static test cases")]
        public void RefreshTest() {
            ChartControl.Labels = true;
            ChartControl.GridLines = true;
            // a) Change Max Records
            ChartControl.MaxRecords = MAXRECORDS;
            CompareView("FullMaxRecView");
            // b) Change Grid Lines
            ChartControl.GridLines = false;
            CompareView("FullMaxRecNoGridView");
            // c) Change Labels
            ChartControl.Labels = false;
            ChartDefinitions[0].LeftAxis.Labels = false;
            ChartDefinitions[0].RightAxis.Labels = false;
            ChartDefinitions[1].LeftAxis.Labels = false;
            CompareView("FullMaxRecNoGridLabelsView");            
            ChartControl.MaxRecords = null;
            ChartControl.Labels = true;
            ChartControl.GridLines = true;
            ChartDefinitions[0].LeftAxis.Labels = true;
            ChartDefinitions[0].RightAxis.Labels = true;
            ChartDefinitions[1].LeftAxis.Labels = true;
            CompareView("FullView");
            // d) Change Chart Series selection
            VolumeSeries.ChartDefinition = null;
            VolumeSeries2.ChartDefinition = "main";
            CompareView("FullSwitchVolumeView");
            VolumeSeries2.ChartDefinition = null;
            // e) Change Chart Definition height
            ChartDefinitions[0].Height = 90;
            ChartDefinitions[1].Height = 10;
            CompareView("FullNoVolume9010View");
            // f) Change Layout
            ChartControl.Layout.BackgroundBrush = Brushes.LightSkyBlue;
            ChartControl.Layout.TextSize = 10;
            CompareView("FullNoVolume9010BlueLayoutView");            
            ChartSeries.Single(item => item.Title == "Volume").ChartDefinition = "main";
            ChartDefinitions[0].Height = 75;
            ChartDefinitions[1].Height = 25;
            ChartControl.Layout = GetLayout();
            CompareView("FullView");
            // e) Change Chart Series style
            ChartSeries.Single(item => item.Title == "Volume").SeriesStyle = new SeriesStyle(Colors.DarkGray, 0.4);
            CompareView("FullGreyBarView");
            // f) Change Axis grid lines
            ChartDefinitions[0].LeftAxis.GridLines = false;
            ChartDefinitions[0].RightAxis.GridLines = true;
            CompareView("FullGreyBarOppositeGridView");
            // g) Change Axis lines
            ChartDefinitions[1].LeftAxis.Lines[0] = 60;
            ChartDefinitions[1].LeftAxis.Lines[1] = 40;
            CompareView("FullGreyBarOppositeGrid6040LinesView");
            ChartDefinitions[0].LeftAxis.GridLines = true;
            ChartDefinitions[0].RightAxis.GridLines = false;
            ChartDefinitions[1].LeftAxis.Lines[0] = 70;
            ChartDefinitions[1].LeftAxis.Lines[1] = 30;
            ChartSeries.Single(item => item.Title == "Volume").SeriesStyle = new SeriesStyle();
            CompareView("FullView");
            // h) Refresh and Capture method
            BitmapSource imagebeforerefresh = ChartControl.CaptureScreenshot();
            ChartControl.Reload();
            BitmapSource imageafterrefresh = ChartControl.CaptureScreenshot();
            imagebeforerefresh.PixelsEquals(imageafterrefresh);
        }

        /*
         * OUTSIDE RANGE TEST (01/05/21 - 10/06/21; 1H)
         * NO MAX RECORDS, GRID LINES, LABELS
         * ------------------------------------------------
         * a) From > To
         * b) From < Min / To > Max
         * c) From = To
         * d) MaxRecords <= 0
         * e) Lines outside range
         * f) Ticks outside range
         * g) Invalid style
         * h) Invalid layout
         * i) Chart heights sum != 100
         * j) Chart heights <= 0
         * k) No axis
         * l) Invalid axis
         * m) Partially no data
         * n) No data
         * o) No series
         * p) No chart definitions
         */

        [WpfTestMethod]
        [TestCategory("Static test cases")]
        public void OutsideRangeTest() {
            // a) From > To
            ChartControl.To = new DateTime(2021, 6, 7);
            ChartControl.From = new DateTime(2021, 6, 9);
            CompareView("FromAfterToView");
            // b) From < Min / To > Max
            ChartControl.Reset();
            ChartControl.SetRange(new DateTime(2021, 6, 9), new DateTime(2021, 6, 7));
            CompareView("ToBeforeFromView");         
            ChartControl.Reset();
            ChartControl.From = new DateTime(2021, 4, 20);
            ChartControl.To = new DateTime(2021, 7, 10);
            CompareView("FullView");
            ChartControl.Reset();
            ChartControl.SetRange(new DateTime(2021, 4, 20), new DateTime(2021, 7, 10));
            CompareView("FullView");
            // c) From = To
            ChartControl.Reset();
            ChartControl.From = new DateTime(2021, 5, 20);
            ChartControl.To = new DateTime(2021, 5, 20);
            CompareView("FromEqualsToView");
            ChartControl.Reset();
            ChartControl.SetRange(new DateTime(2021, 5, 20), new DateTime(2021, 5, 20));
            CompareView("FromEqualsToView");
            // d) MaxRecords <= 0
            ChartControl.Reset();
            ChartControl.MaxRecords = 0;
            CompareView("MaxBelowLimitView");
            // e) Lines outside range
            ChartControl.MaxRecords = null;
            ChartDefinitions[1].LeftAxis.Lines[0] = -200;
            ChartDefinitions[1].LeftAxis.Lines[1] = 200;
            CompareView("LinesOutsideRangeView");
            // f) Ticks outside range
            ChartDefinitions[1].LeftAxis.Lines[0] = 70;
            ChartDefinitions[1].LeftAxis.Lines[1] = 30;
            ChartDefinitions[1].LeftAxis.Ticks.Add(1000);
            ChartDefinitions[1].LeftAxis.Ticks.Add(-1000);
            CompareView("FullView");
            // g) Invalid style
            ChartDefinitions[1].LeftAxis.Ticks.Remove(1000);
            ChartDefinitions[1].LeftAxis.Ticks.Remove(-1000);
            RSI10Series.SeriesStyle = default;
            CompareView("FullView");
            // h) Invalid layout
            RSI10Series.SeriesStyle = new SeriesStyle();
            ChartLayout oldlayout = ChartControl.Layout;
            ChartControl.Layout = null;
            CompareView("FullView");
            ChartControl.Layout = new() { LeftMargin = WINDOW_WIDTH + 100, RightMargin = WINDOW_WIDTH + 100, TopMargin = WINDOW_HEIGHT + 100, BottomMargin = WINDOW_HEIGHT + 100, LargeBarSize = 1000, SmallBarSize = 900, TickLength = WINDOW_HEIGHT };
            Assert.IsNull(ChartControl.CaptureScreenshot());
            ChartControl.Layout = new() { LargeBarSize = 1000, SmallBarSize = 900, TickLength = WINDOW_HEIGHT };
            CompareView("InvalidLayoutView");
            Assert.ThrowsException<ArgumentException>(() => { ChartControl.Layout.TopMargin = -40; });
            // i) Chart heights sum != 100
            ChartControl.Layout = oldlayout;
            ChartDefinitions[0].Height = 90;
            ChartDefinitions[1].Height = 50;
            CompareView("HeightsTooLargeView"); 
            ChartDefinitions[0].Height = 30;
            ChartDefinitions[1].Height = 40;
            CompareView("HeightsTooSmallView");
            // j) Chart heights <= 0
            Assert.ThrowsException<ArgumentException>(() => { ChartDefinitions[0].Height = -40; });
            ChartDefinitions[0].Height = 0;
            ChartDefinitions[1].Height = 0;
            CompareView("HeightsZeroView");
            // k) No axis
            ChartDefinitions[0].Height = 75;
            ChartDefinitions[1].Height = 25;
            ChartDefinitions[1].LeftAxis = null;
            CompareView("NoAxisView");
            // l) Invalid axis
            ChartDefinitions[1].LeftAxis = null;
            CompareView("NoAxisView");
            // m) Partially no data
            InitializeChartSeries();
            InitializeChartDefinitions();
            RSI10Data.Clear();
            VolumeData.Clear();
            ChartControl.Reload();
            CompareView("PartiallyMissingDataView"); 
            ReloadData();            
            EMA25Series.Data = null;
            EMA8Series.Data = null;
            CompareView("PartiallyNotExistingDataView");
            // n) No data
            ReloadData();
            EMA25Data.Clear();
            EMA8Data.Clear();
            RSI10Data.Clear();
            VolumeData.Clear();
            ChartData.Clear();
            ChartControl.Reset();
            CompareView("EmptyView2");
            ReloadData();
            EMA25Series.Data = null;
            EMA8Series.Data = null;
            RSI10Series.Data = null;
            VolumeSeries.Data = null;
            VolumeSeries2.Data = null;
            MainSeries.Data = null;
            CompareView("EmptyView2");
            // o) No series
            ReloadData();
            InitializeChartSeries();
            ChartSeries.Remove(RSI10Series);
            ChartSeries.Remove(EMA8Series);
            CompareView("PartiallyMissingSeriesView");
            ChartSeries.Clear();
            CompareView("EmptyView");
            InitializeChartSeries();
            InitializeChartDefinitions();
            ChartControl.ChartSeries = null;
            CompareView("EmptyView");
            // p) No chart definitions
            ChartControl.ChartSeries = ChartSeries;          
            ChartDefinitions[0] = null;            
            CompareView("PartiallyMissingDefinitionsView");
            InitializeChartDefinitions();
            ChartDefinitions.RemoveAt(0);
            CompareView("PartiallyMissingDefinitionsView");
            InitializeChartDefinitions();
            ChartDefinitions.Clear();
            CompareView("BlankView");
            InitializeChartDefinitions();
            ChartControl.ChartDefinitions = null;
            CompareView("BlankView");
        }

        /*
        * DYNAMIC SCROLL TEST
        * MAX RECORDS
        * ------------------------------------------------
        * a) Scroll by 1/4 width to the left
        * b) Scroll by 1/4 width to the right
        * c) Scroll outside range to the right
        * d) Scroll outside range to the left
        * e) Scroll outside window to the right and come back by 1/4 width to the left
        * f) Scroll & throw by 1/4 width to the left
        * g) Scroll & throw by 1/4 width to the right
        * h) Scroll & throw outside range to the left
        * i) Scroll & throw outside range to the right
        */

        [WpfTestMethod]
        [TestCategory("Dynamic test cases")]
        public void DynamicScrollTest() {
            ChartControl.MaxRecords = MAXRECORDS;
            ChartControl.Reset();
            ChartControls.EventManager eventmanager = ChartControl.GetEventManager();
            CompareView("InitialScrollView");
            // a) Scroll by 1/4 width to the left
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(0, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(500, new Point(WINDOW_WIDTH / 2 - 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(1000, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(1500, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            CompareView("ScrollLeftView");
            // b) Scroll by 1/4 width to the right
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(1500, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(2000, new Point(WINDOW_WIDTH / 2 + 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(2500, new Point(3 * WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(3000, new Point(3 * WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            CompareView("InitialScrollView"); // ScrollRightView
            // c) Scroll outside range to the right
            ChartControl.Reset();
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(3000, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(3500, new Point(WINDOW_WIDTH / 2 + 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(4000, new Point(3 * WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(4500, new Point(3 * WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            CompareView("InitialScrollView"); // ScrollRightMaxView
            // d) Scroll outside range to the left
            ChartControl.ScrollTo(ChartControl.MaxTime);
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(4500, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(5000, new Point(WINDOW_WIDTH / 2 - 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(5500, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(6000, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            CompareView("ScrollLeftMaxView");
            // e) Scroll outside window to the right and come back by 1/4 width to the left
            ChartControl.Reset();
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(6000, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(6500, new Point(WINDOW_WIDTH / 2 + 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(7000, new Point(WINDOW_WIDTH, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseLeaveEvent(7001, new Point(WINDOW_WIDTH, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseEnterEvent(7500, new Point(WINDOW_WIDTH, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(7510, new Point(WINDOW_WIDTH - 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(7750, new Point(3 * WINDOW_WIDTH / 4 - ChartControl.Layout.RightMargin, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(8000, new Point(3 * WINDOW_WIDTH / 4 - ChartControl.Layout.RightMargin, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            CompareView("ScrollLeftView"); // ScrollOutAndBackView
            // f) Scroll & throw by 1/4 width to the left
            ChartControl.Reset();
            ChartControl.HasScrolled += DynamicScrollTest_HasScrolled;
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(8000, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(8500, new Point(WINDOW_WIDTH / 2 - 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(8600, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(8650, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            CompareView("ScrollThrowLeftView", delayed: true);
            Dispatcher.Run();
            // g) Scroll & throw by 1 / 4 width to the right
            ChartControl.ScrollTo(ChartControl.MaxTime);
            ChartControl.HasScrolled += DynamicScrollTest_HasScrolled;
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(9000, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(9500, new Point(WINDOW_WIDTH / 2 + 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(9600, new Point(3 * WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(9650, new Point(3 * WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            CompareView("ScrollThrowRightView", delayed: true);
            Dispatcher.Run();
            // h) Scroll & throw outside range to the left
            ChartControl.Reset();
            ChartControl.ScrollTo(ChartControl.MaxTime - (ChartControl.MaxTime - ChartControl.MinTime) / 5);
            ChartControl.HasScrolled += DynamicScrollTest_HasScrolled;
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(10000, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(10500, new Point(WINDOW_WIDTH / 2 - 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(10600, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(10650, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            CompareView("ScrollLeftMaxView", delayed: true); // ScrollThrowOutOfRangeLeftView
            Dispatcher.Run();
            // i) Scroll & throw outside range to the right
            ChartControl.Reset();
            ChartControl.ScrollBy((ChartControl.MaxTime - ChartControl.MinTime) / 5);
            ChartControl.HasScrolled += DynamicScrollTest_HasScrolled;
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(11000, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(11500, new Point(WINDOW_WIDTH / 2 + 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(11600, new Point(3 * WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(11650, new Point(3 * WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            CompareView("InitialScrollView", delayed: true); // ScrollThrowOutOfRangeRightView
            Dispatcher.Run();
        }

        /*
        * DYNAMIC ZOOM TEST
        * MAX RECORDS
        * ------------------------------------------------      
        * a) Zoom in 5 steps slow 
        * b) Zoom out 5 steps slow
        * c) Zoom in 10 steps fast
        * d) Zoom out 10 steps fast
        * e) Zoom in max
        * f) Reset
        * g) Zoom out max        
        */

        [WpfTestMethod]
        [TestCategory("Dynamic test cases")]
        public void DynamicZoomTest() {
            ChartControl.MaxRecords = MAXRECORDS;
            ChartControl.Reset();
            ChartControls.EventManager eventmanager = ChartControl.GetEventManager();            
            CompareView("InitialZoomView");
            // a) Zoom in 5 steps slow 
            for (int ts = 0; ts <= 2000; ts+= 500) {
                eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseWheelEvent(ts, 120));
            }            
            CompareView("ZoomInSlowView");
            // b) Zoom out 5 steps slow
            for (int ts = 2500; ts <= 4500; ts -= 500) {
                eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseWheelEvent(ts, -120));
            }
            CompareView("InitialZoomView"); // ZoomOutSlowView
            // c) Zoom in 10 steps fast
            for (int ts = 5000; ts <= 5400; ts += 100) {
                eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseWheelEvent(ts, 240));
            }
            CompareView("ZoomInFastView");
            // d) Zoom out 10 steps fast
            for (int ts = 5500; ts <= 5900; ts += 100) {
                eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseWheelEvent(ts, -240));
            }
            CompareView("InitialZoomView"); // ZoomOutFastView
            // e) Zoom in max
            for (int ts = 6000; ts <= 7000; ts += 100) {
                eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseWheelEvent(ts, 1200));
            }
            CompareView("ZoomInMaxView");
            // f) Reset
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(0, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed, 2));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(5, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            CompareView("InitialZoomView");
            // g) Zoom out max
            //ChartControl.Reset();
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseWheelEvent(6000, -1000));
            CompareView("InitialZoomView"); // ZoomOutMaxView
        }

        /*
        * SELECT TEST
        * MAX RECORDS
        * ------------------------------------------------      
        * a) First Select
        * b) Second Select
        * c) Unselect
        * d) Scroll after select
        * e) Move point 5 steps to the right
        * f) Move point 5 steps to the left
        * g) Move point outside the range  
        * i) Open context menu
        */

        [WpfTestMethod]
        [TestCategory("Dynamic test cases")]
        public void DynamicSelectTest() {
            ChartControl.MaxRecords = MAXRECORDS;
            ChartControl.Reset();
            ChartControls.EventManager eventmanager = ChartControl.GetEventManager();
            CompareView("InitialSelectView");    
            // a) First Select
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(0, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(5, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));            
            CompareView("FirstSelectView");
            Popup selectbox = ChartControl.GetSelectBox();
            Assert.IsNotNull(selectbox);
            Assert.IsTrue(selectbox.IsOpen);
            Border popupborder = selectbox.Child as Border;
            Assert.IsNotNull(popupborder);
            Border border = popupborder.Child as Border;
            Assert.IsNotNull(border);
            Grid popupgrid = border.Child as Grid;
            Assert.IsNotNull(popupgrid);
            Assert.IsTrue(popupgrid.Children.Count == 12 && popupgrid.Children[4] is TextBlock);
            TextBlock popuptext = (TextBlock)popupgrid.Children[3];
            Assert.IsTrue(popuptext.Text == "O 3486.3300 H 3492.3800 L 3436.0900 C 3464.2300");
            // b) Second Select
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(100, new Point(3*WINDOW_WIDTH / 4, WINDOW_HEIGHT / 4), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(105, new Point(3*WINDOW_WIDTH / 4, WINDOW_HEIGHT / 4), MouseButton.Left, MouseButtonState.Released));
            CompareView("SecondSelectView");
            Assert.IsTrue(selectbox.IsOpen);
            // c) Unselect
            selectbox.Closed += DynamicSelectTest_BoxClosed;
            selectbox.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 200, MouseButton.Left) { RoutedEvent = Popup.MouseDownEvent });
            CompareView("InitialSelectView", true); // UnselectView
            Dispatcher.Run();
            Assert.IsFalse(selectbox.IsOpen);
            // d) Scroll after select
            //  i. Select
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(300, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(305, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            Assert.IsTrue(selectbox.IsOpen);
            CompareView("FirstSelectView");
            //  ii. Scroll
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(500, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(1500, new Point(WINDOW_WIDTH / 2 - 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));            
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(1800, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(2000, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));            
            CompareView("ScrollAfterSelectionView");
            Assert.IsFalse(selectbox.IsOpen);            
            // e) Move selection 5 steps to the right            
            //  i. Select
            ChartControl.Reset();
            ChartControl.SetRange(new DateTime(2021, 5, 5), new DateTime(2021, 5, 7));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(2200, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(2205, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            selectbox = ChartControl.GetSelectBox();
            Assert.IsNotNull(selectbox);            
            Assert.IsTrue(selectbox.IsOpen);
            //  ii. Move
            for (int i = 0; i < 5; i++) {
                eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateKeyDownEvent(2500 + i * 200, Key.Right, KeyStates.Down));
                eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateKeyUpEvent(2600 + i * 200, Key.Right, KeyStates.None));
            }
            CompareView("MoveRightAfterSelectionView");
            // f) Move selection 5 steps to the left 
            for (int i = 0; i < 5; i++) {
                eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateKeyDownEvent(3500 + i * 200, Key.Left, KeyStates.Down));
                eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateKeyUpEvent(3600 + i * 200, Key.Left, KeyStates.None));
            }
            CompareView("MoveLeftAfterSelectionView");
            // g) Move selection outside the range
            //  i. Select at the edge
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(4500, new Point(ChartControl.ActualWidth - ChartControl.Layout.RightMargin - 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(4505, new Point(ChartControl.ActualWidth - ChartControl.Layout.RightMargin - 2, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            //  ii. Move
            for (int i = 0; i < 5; i++) {
                eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateKeyDownEvent(4600 + i * 200, Key.Right, KeyStates.Down));
                eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateKeyUpEvent(4700 + i * 200, Key.Right, KeyStates.None));
            }
            CompareView("MoveOutsideRangeAfterSelectionView");
            // h) Context Menu
            ContextMenu menu = ChartControl.GetContextMenu(new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2));
            Assert.IsNotNull(menu);
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(200, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Right, MouseButtonState.Pressed, 1, MouseButtonState.Released, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(300, new Point(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), MouseButton.Right, MouseButtonState.Released, 1, MouseButtonState.Released, MouseButtonState.Released));
            Assert.IsTrue(menu.IsOpen);
            Assert.IsTrue(menu.Items.Count == 11);
            Assert.IsTrue(((MenuItem) menu.Items[0]).Header.ToString() == MainSeries.Title && ((MenuItem)menu.Items[0]).IsChecked);
            Assert.IsTrue(((MenuItem)menu.Items[6]).Header.ToString() == VolumeSeries2.Title && !((MenuItem)menu.Items[6]).IsChecked);
            Assert.IsTrue(((MenuItem)menu.Items[10]).Header.ToString() == "Right axis" && ((MenuItem)menu.Items[10]).Items.Count == 2 && ((MenuItem)((MenuItem)menu.Items[10]).Items[0]).IsChecked);
        }


        /*
        * ADVANCED SCROLL/ZOOM TEST
        * MAX RECORDS
        * TWO SERIES WITH GAP
        * ------------------------------------------------      
        * a) Scroll between left dataset and gap
        * b) Scroll into left gap
        * c) Scroll between right dataset and gap
        * d) Scroll into right gap
        * e) Number of points stays constant after scrolling
        * f) Zoom by selecting range
        */

        [WpfTestMethod]
        [TestCategory("Dynamic test cases")]
        public void AdvancedScrollZoomTest() {
            ChartControl.MaxRecords = MAXRECORDS;
            MainSeries2.ChartDefinition = "main";
            ChartControls.EventManager eventmanager = ChartControl.GetEventManager();            
            ChartControl.Reset();   
            
            // Select left dataset
            ChartControl.SetRange(new DateTime(2021, 01, 30), new DateTime(2021, 02, 01));
            CompareView("LeftDataScrollView");
            // a) Scroll between left dataset and gap
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(1500, new Point(3 * WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(2000, new Point(3 * WINDOW_WIDTH / 4 + 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(2500, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(3000, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            CompareView("BetweenLeftDataGapScrollView");
            // b) Scroll into left gap
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(4500, new Point(4 * WINDOW_WIDTH / 5, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(5000, new Point(4 * WINDOW_WIDTH / 5 + 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(5500, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(6000, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            CompareView("LeftDataGapScrollView");
            // Select right dataset
            ChartControl.SetRange(new DateTime(2021, 05, 01), new DateTime(2021, 05, 02));
            CompareView("RightDataScrollView");
            // c) Scroll between right dataset and gap
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(7500, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(8000, new Point(WINDOW_WIDTH / 4 + 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(8500, new Point(3 * WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(9000, new Point(3 * WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            CompareView("BetweenRightDataGapScrollView");
            // d) Scroll into right gap
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(10500, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(11000, new Point(WINDOW_WIDTH / 4 + 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(11500, new Point(4 * WINDOW_WIDTH / 5, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(12000, new Point(4 * WINDOW_WIDTH / 5, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            CompareView("RightDataGapScrollView");
            // Select 20 data points
            ChartControl.SetRange(new DateTime(2021, 01, 20, 13, 15, 00), new DateTime(2021, 01, 20, 18, 00, 00));
            var charts = ChartControl.GetVisibleChartSeries();
            Assert.AreEqual(1, charts.Count());
            var data = ChartControl.GetData(charts.First());
            Assert.AreEqual(20, data.Count());
            // e) Number of points stays constant after scrolling
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(1500, new Point(3 * WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(2000, new Point(3 * WINDOW_WIDTH / 4 + 10, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(2500, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(3000, new Point(WINDOW_WIDTH / 4, WINDOW_HEIGHT / 2), MouseButton.Left, MouseButtonState.Released));
            charts = ChartControl.GetVisibleChartSeries();
            Assert.AreEqual(1, charts.Count());
            data = ChartControl.GetData(charts.First());
            Assert.AreEqual(20, data.Count());
            // f) Zoom by selecting range
            ChartControl.Reset();
            CompareView("InitialLeftView");
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(7500, new Point(WINDOW_WIDTH / 3, WINDOW_HEIGHT - 10), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(8000, new Point(WINDOW_WIDTH / 3 + 10, WINDOW_HEIGHT - 10), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(8500, new Point(2 * WINDOW_WIDTH / 3, WINDOW_HEIGHT - 10), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(9000, new Point(2 * WINDOW_WIDTH / 3, WINDOW_HEIGHT - 10), MouseButton.Left, MouseButtonState.Released));
            CompareView("SelectedRangeZoomView");
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(7500, new Point(2 * WINDOW_WIDTH / 5, WINDOW_HEIGHT - 10), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(8000, new Point(2 * WINDOW_WIDTH / 5 + 10, WINDOW_HEIGHT - 10), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(8500, new Point(3 * WINDOW_WIDTH / 5, WINDOW_HEIGHT - 10), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(9000, new Point(3 * WINDOW_WIDTH / 5, WINDOW_HEIGHT - 10), MouseButton.Left, MouseButtonState.Released));
            CompareView("SelectedRange2ZoomView");
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseDownEvent(7500, new Point(2 * WINDOW_WIDTH / 5, WINDOW_HEIGHT - 10), MouseButton.Left, MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(8000, new Point(2 * WINDOW_WIDTH / 5 + 10, WINDOW_HEIGHT - 10), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseMoveEvent(8500, new Point(3 * WINDOW_WIDTH / 5, WINDOW_HEIGHT - 10), MouseButtonState.Pressed));
            eventmanager.RaiseEvent(ChartControls.EventManager.Event.CreateMouseUpEvent(9000, new Point(3 * WINDOW_WIDTH / 5, WINDOW_HEIGHT - 10), MouseButton.Left, MouseButtonState.Released));
            CompareView("SelectedRange2ZoomView");
            MainSeries2.ChartDefinition = null;
        }

        #region Private Methods    

        private static void InitializeData(uint interval = 60) {
            CleanupData();
            Dataprovider = new CDDDataProvider(PATH_GEMINI_1MIN, TimeSpan.FromMinutes(interval), DateTimeKind.Local);
            Dataprovider2 = new CDDDataProvider(PATH_GEMINI_1MIN, TimeSpan.FromMinutes(15), DateTimeKind.Local);
            EMA8 = new EMA(Dataprovider, 8);
            EMA25 = new EMA(Dataprovider, 25);
            RSI10 = new RSI(Dataprovider, 10);
            ChartData = new();
            ChartData2 = new();
            EMA8Data = new();
            EMA25Data = new();
            RSI10Data = new();
            VolumeData = new();
            ReloadData(interval);
        }

        private static void ReloadData(uint interval = 60) {
            ChartData?.Clear();
            ChartData2?.Clear();
            EMA8Data?.Clear();
            EMA25Data?.Clear();
            RSI10Data?.Clear();
            VolumeData?.Clear();
            ChartData.AddRange(Dataprovider.GetMarketData(FROM, interval < 60 ? FROM.AddDays(1) : TO).Select(item => new Tuple<DateTime, decimal[]>(item.Time, new decimal[4] { item.Open, item.High, item.Low, item.Close })).ToList());
            ChartData2.AddRange(Dataprovider2.GetMarketData(FROM2, TO2).Select(item => new Tuple<DateTime, decimal[]>(item.Time, new decimal[4] { item.Open, item.High, item.Low, item.Close })).ToList());
            EMA8Data.AddRange(EMA8.GetAnalyticsData(FROM, interval < 60 ? FROM.AddDays(1) : TO).Select(item => new Tuple<DateTime, decimal[]>(item.Time, new decimal[1] { item.Value })).ToList());
            EMA25Data.AddRange(EMA25.GetAnalyticsData(FROM, interval < 60 ? FROM.AddDays(1) : TO).Select(item => new Tuple<DateTime, decimal[]>(item.Time, new decimal[1] { item.Value })).ToList());
            RSI10Data.AddRange(RSI10.GetAnalyticsData(FROM, interval < 60 ? FROM.AddDays(1) : TO).Select(item => new Tuple<DateTime, decimal[]>(item.Time, new decimal[1] { item.Value })).ToList());
            VolumeData.AddRange(Dataprovider.GetMarketData(FROM, interval < 60 ? FROM.AddDays(1) : TO).Select(item => new Tuple<DateTime, decimal[]>(item.Time, new decimal[3] { item.Volume, item.Open, item.Close })).ToList());
        }

        private void InitializeChartSeries() {
            ChartSeries.Clear();
            MainSeries = new ChartSeries("ETH/USD", SeriesType.OHLCCandles, ChartData) { ChartDefinition = "main" };
            MainSeries2 = new ChartSeries("ETH/USD (15 min)", SeriesType.OHLCCandles, ChartData2);
            EMA8Series = new ChartSeries("EMA(8)", SeriesType.Line, EMA8Data) { ChartDefinition = "main" };
            EMA25Series = new ChartSeries("EMA(25)", SeriesType.Line, EMA25Data) { ChartDefinition = "main" };
            VolumeSeries = new ChartSeries("Volume", SeriesType.Bars, AxisType.Right, VolumeData) { ChartDefinition = "main" };
            VolumeSeries2 = new ChartSeries("Volume (OHLC)", SeriesType.VolumeBars, AxisType.Right, VolumeData);
            RSI10Series = new ChartSeries("RSI(10)", SeriesType.Line, RSI10Data) { ChartDefinition = "sub" };
            ChartSeries.Add(MainSeries);
            ChartSeries.Add(MainSeries2);
            ChartSeries.Add(EMA8Series);            
            ChartSeries.Add(EMA25Series);
            ChartSeries.Add(VolumeSeries);
            ChartSeries.Add(VolumeSeries2);
            ChartSeries.Add(RSI10Series);
        }

        private void InitializeChartDefinitions() {
            ChartDefinitions.Clear();
            ChartDefinitions.Add(new ChartDefinition("main", 75, new AxisDefinition() { GridLines = true }, new AxisDefinition(0.25f)));
            ChartDefinitions.Add(new ChartDefinition("sub", 25, new AxisDefinition(from: 15, to: 85, ticks: new ObservableCollection<decimal>() { 30, 50, 70 }, lines: new ObservableCollection<decimal>() { 30, 70 })));
        }

        private static ChartLayout GetLayout() {
            string fontpath = "Data" + Path.DirectorySeparatorChar + "Font" + Path.DirectorySeparatorChar + "#Segoe UI";
            Typeface font = new(new FontFamily(fontpath), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            return new() {
                LeftMargin = 10,
                TopMargin = 10,
                RightMargin = 10,
                BottomMargin = 40,
                TextFont = font,                
                TextSize = 14,
                TextBrush = Brushes.Black,
                BackgroundBrush = Brushes.White,
                HighlightPen = new Pen(Brushes.Black, 2),
                LinePen = new Pen(Brushes.Black, 0.5) { DashStyle = DashStyles.Dash },
                MainPen = new Pen(Brushes.Black, 0.5),
                GridPen = new Pen(Brushes.DarkGray, 0.5),
                OHLCGainBrush = Brushes.Green,
                OHLCLossBrush = Brushes.Red,
                LargeBarSize = 3 / 4D,
                SmallBarSize = 1 / 10D,
                PointSelectionSize = 5,
                TickLength = 5,
                PopupBackroundBrush = new SolidColorBrush(Colors.WhiteSmoke) { Opacity = 1 },
                PopupMargin = 5,
                PopupOpacity = 1,
                PopupBorderBrush = Brushes.Black,
                PopupBorderThickness = new(1),
                PopupBorderRadius = 5,
                DefaultColors = new Color[] {
                    Colors.Aqua,
                    Colors.Yellow,
                    Colors.LightGreen,
                    Colors.LightPink,
                    Colors.Aquamarine,
                    Colors.ForestGreen,
                    Colors.BlueViolet,
                    Colors.HotPink,
                    Colors.IndianRed,
                    Colors.Salmon
                }
            };
        }

        private void DynamicScrollTest_HasScrolled(object sender, EventArgs e) {
            CompareView();
            ChartControl.HasScrolled -= DynamicScrollTest_HasScrolled;
            Dispatcher.ExitAllFrames();
        }

        private void DynamicSelectTest_BoxClosed(object sender, EventArgs e) {
            CompareView();
            ChartControl.GetSelectBox().Closed -= DynamicSelectTest_BoxClosed;
            Dispatcher.ExitAllFrames();
        }

        private static BitmapImage LoadRefImage(string filename) {
            string path = Path.Combine(Environment.CurrentDirectory, "Scenarios", "FinancialChartControl", filename);
            if (!File.Exists(path)) throw new InternalTestFailureException($"Test file {filename} not found!");
            return new BitmapImage(new Uri(path));
        }

        private void CompareView(string view = null, bool delayed = false) {
            if (delayed) {
                DelayedView = view;
                return;
            } else if (view == null) {
                view = DelayedView;
                DelayedView = null;
            }
            if (view == null) throw new ArgumentException("Delayed test view missing!");
            string filename = view + ".png";            
            BitmapSource image = ChartControl.CaptureScreenshot();
            Assert.IsNotNull(image);
            image.SaveToFile(filename);
            if (GENERATEIMAGES) return;
            BitmapImage refimage = LoadRefImage(filename);
            bool isequal = image.PixelsEquals(refimage);
            if (!isequal) {
                BitmapSource difference = image.Subtract(refimage);
                if (difference != null) {
                    string difffilename = view + ".diff.png";
                    difference.SaveToFile(difffilename);
                }
            }
            Assert.IsTrue(isequal);
        }

        #endregion

    }
}
