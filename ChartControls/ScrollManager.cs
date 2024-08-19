using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Windows.Threading;
using Point = System.Windows.Point;

namespace ChartControls {

    internal class ScrollManager : IDisposable {

        #region Fields        

        private readonly FinancialChartControl vChart;
        private readonly DimensionManager vDimensions;
        private readonly DataManager vData;
        private readonly DispatcherTimer vTimer;
        private Point? vScrollPoint;
        private DateTime? vScrollDate;
        private int vScrollDelta;

        #endregion

        #region Events

        public event EventHandler<TimeSpan> Scrolling;
        public event EventHandler Scrolled;

        #endregion

        #region Public methods

        public ScrollManager(FinancialChartControl chart, DimensionManager dimensions, DataManager data) {
            vChart = chart;
            vDimensions = dimensions;
            vData = data;
            vTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
            vTimer.Tick += AutoScrollElapsed;
        }

        public void ScrollTo(Point point) {
            if (!IsPointScrolling()) {
                StartScrolling(point);
                return;
            }
            DateTime fromdate = vDimensions.PointToData(vScrollPoint.Value.X);
            DateTime todate = vDimensions.PointToData(point.X);
            TimeSpan timedelta = vScrollDate.Value - vChart.From + (fromdate - todate);
            ScrollBy(timedelta);
        }

        public bool ScrollBy(TimeSpan delta) {
            if (delta.TotalMilliseconds == 0) return false;
            TimeSpan maxinterval = vData.GetMaxInterval(vChart.From, vChart.To);
            if (vChart.From + delta < vChart.MinTime - maxinterval / 2) delta = vChart.MinTime - maxinterval / 2 - vChart.From;
            if (vChart.To + delta > vChart.MaxTime + maxinterval / 2) delta = vChart.MaxTime + maxinterval / 2 - vChart.To;            
            Scrolling?.Invoke(this, delta);
            //Debug.WriteLine($"Scroll by {delta}");
            return true;
        }

        public void AutoScrollBy(int delta) {
            if (!IsPointScrolling()) return;
            vScrollDelta = delta;
            //Debug.WriteLine($"Auto scroll started with Delta = {delta}");
            vTimer.Start();
        }

        public void StopScrolling() {
            vTimer.Stop();
            vScrollDelta = 0;
            vScrollPoint = null;
            vScrollDate = null;
            //Debug.WriteLine($"Scrolling stopped");
            Scrolled?.Invoke(this, null);
        }

        public bool IsPointScrolling() {
            return vScrollPoint != null;
        }

        public bool IsAutoScrolling() {
            return vScrollDelta != 0 && vTimer.IsEnabled;
        }

        public bool IsScrolling() {
            return IsPointScrolling() || IsAutoScrolling();
        }

        public void Dispose() {
            vTimer.Stop();
            vTimer.Tick -= AutoScrollElapsed;
        }

        #endregion

        #region Private methods

        private void StartScrolling(Point point) {
            vScrollPoint = point;
            vScrollDate = vChart.From;
        }

        private bool AutoScroll(int delta) {
            if (!IsPointScrolling()) return false;
            TimeSpan timedelta = -vDimensions.GetPixelToTimeRatio() * delta;
            if(!ScrollBy(timedelta)) {
                return false;
            }
            vChart.InvalidateVisual();
            return true;
        }

        private void AutoScrollElapsed(object sender, EventArgs e) {
            if (!IsPointScrolling()) {
                vTimer.Stop();
                vScrollDelta = 0;
                return;
            }
            if (vScrollDelta == 0) {
                StopScrolling();
                return;
            }
            if (!AutoScroll(vScrollDelta)) {
                StopScrolling();
                return;
            }
            vScrollDelta -= Math.Sign(vScrollDelta);
        }

        #endregion

    }
}