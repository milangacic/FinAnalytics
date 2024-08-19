using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace ChartControls {
    internal class SelectionManager {

        #region Fields

        private readonly FinancialChartControl vChart;
        private readonly DimensionManager vDimensions;
        private readonly DataManager vData;
        private readonly CacheManager vCache;

        #endregion

        #region Properties        

        public Point SelectedPoint { get; private set; }
        public DateTime SelectedTime { get; private set; }
        public ChartSeries[] SelectedSeries { get; private set; }

        #endregion

        #region Events

        public event EventHandler SelectionChanged;

        #endregion

        #region Public methods

        public SelectionManager(FinancialChartControl chart, DimensionManager dimensions, DataManager data, CacheManager cache) {
            vChart = chart;
            vDimensions = dimensions;
            vData = data;
            vCache = cache;
        }

        public void SelectPoint(Point point) {
            SelectedTime = vData.GetClosestDate(vDimensions.PointToData(point.X));
            SelectedSeries = GetSelectedChartSeries();
            if (!SelectedSeries.Any()) {
                ResetSelection();
                return;
            }
            SelectedPoint = new Point(vDimensions.DataToPoint(SelectedTime).X, point.Y);
            SelectionChanged?.Invoke(this, null);
        }

        public void ResetSelection() {
            SelectedSeries = null;
            SelectedTime = default;
            SelectedPoint = default;
            SelectionChanged?.Invoke(this, null);
        }

        public void MoveSelection(int direction) {
            if (!IsSelected() || direction == 0) return;
            DateTime newdate = direction > 0 ? vData.GetNextDate(SelectedTime) : vData.GetPreviousDate(SelectedTime);
            if (newdate == default) return;
            if (newdate < vChart.From && !vChart.ScrollTo(newdate)) return;
            if (newdate > vChart.To && !vChart.ScrollTo(newdate)) return;
            SelectedTime = newdate;
            SelectedPoint = new Point(vDimensions.DataToPoint(SelectedTime).X, SelectedPoint.Y);
            SelectedSeries = GetSelectedChartSeries();
            SelectionChanged?.Invoke(this, null);
        }

        public bool IsSelected() {
            return SelectedSeries != null;
        }

        public bool IsSelected(ChartSeries chartdef) {
            return SelectedSeries != null && SelectedSeries.Contains(chartdef);
        }

        #endregion

        #region Private methods

        private ChartSeries[] GetSelectedChartSeries() {
            return vChart.ChartSeries.Where(ser => ser.IsUsed && vChart.GetData(ser).Any(val => val.Item1 == SelectedTime)).ToArray();
        }       

        #endregion
    }
}