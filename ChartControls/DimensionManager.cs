using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows;

namespace ChartControls {
    internal class DimensionManager {

        #region Fields

        private readonly FinancialChartControl vChart;
        private readonly DataManager vData;
        private readonly CacheManager vCache;

        #endregion

        #region Public methods        

        public DimensionManager(FinancialChartControl chart, DataManager data, CacheManager cache) {
            vChart = chart;
            vData = data;
            vCache = cache;
        }

        public Tuple<DateTime, decimal> PointToData(ChartDefinition chartdef, AxisType axistype, Point point) {
            DateTime date = XToTime(point.X);
            decimal value = YToValue(chartdef, axistype, point.Y);
            return new Tuple<DateTime, decimal>(date, value);
        }

        public DateTime PointToData(double x) {
            return XToTime(x);
        }

        public decimal PointToData(ChartDefinition chartdef, AxisType axistype, double y) {
            return YToValue(chartdef, axistype, y);
        }

        public Point DataToPoint(DateTime time) {
            int x = TimeToX(time);
            return new Point(x, vChart.Layout.TopMargin + vChart.ChartHeight);
        }

        public Point DataToPoint(ChartDefinition chartdef, AxisType axistype, DateTime time, decimal value) {
            int x = TimeToX(time);
            int y = ValueToY(chartdef, axistype, value);
            return new Point(x, y);
        }

        public Point DataToPoint(ChartDefinition chartdef, AxisType axistype, decimal value) {
            int y = ValueToY(chartdef, axistype, value);
            return new Point(axistype == AxisType.Left ? vChart.Layout.LeftMargin : vChart.Layout.LeftMargin + vChart.ChartWidth, y);
        }

        public TimeSpan GetPixelToTimeRatio() {
            if (!vCache.HasBars.HasValue) vCache.HasBars = vChart.ChartSeries.Any(ser => ser.IsUsed && ser.Type != SeriesType.Line);
            return (vChart.To - vChart.From) / vChart.ChartWidth;
        }

        public double GetPixelToValueRatio(ChartDefinition chartdef, AxisType axis) {
            return (double)(vData.GetMaxValue(chartdef, axis) - vData.GetMinValue(chartdef, axis)) / GetChartHeight(chartdef);
        }

        public Rect GetChartRect(ChartDefinition chartdef) {
            return new Rect(vChart.Layout.LeftMargin, GetChartY0(chartdef) - GetChartHeight(chartdef), vChart.ChartWidth, GetChartHeight(chartdef));
        }

        public ChartDefinition PointToChart(Point point) {
            foreach (var chart in vChart.ChartDefinitions) {
                if (GetChartRect(chart).Contains(point)) return chart;
            }
            return null;
        }

        public double GetChartHeight(ChartDefinition chartdef) {
            if (vCache.TotalChartHeight == null) vCache.TotalChartHeight = vChart.ChartDefinitions.Sum(def => def != null ? def.Height : 0);
            return vCache.TotalChartHeight > 0 ? vChart.ChartHeight * chartdef.Height / vCache.TotalChartHeight.Value : 0;
        }

        public bool IsXAxis(Point point) {
            return point.Y > vChart.ChartHeight + vChart.Layout.TopMargin && IsXAxis(point.X);
        }

        public bool IsXAxis(double x) {
            return x > vChart.Layout.LeftMargin && x < vChart.ChartWidth + vChart.Layout.LeftMargin;
        }

        #endregion

        #region Private methods

        private int ValueToY(ChartDefinition chartdef, AxisType axistype, decimal value) {
            AxisDefinition axisdef = chartdef.GetAxis(axistype);
            if (axisdef == null) throw new ArgumentException(string.Format($"No {axistype} axis defined!"));
            return GetChartY0(chartdef) + (int)((double)(vData.GetMinValue(chartdef, axistype) - value) * axisdef.Scale / GetPixelToValueRatio(chartdef, axistype));
        }

        private decimal YToValue(ChartDefinition chartdef, AxisType axistype, double y) {
            AxisDefinition axisdef = chartdef.GetAxis(axistype);
            if (axisdef == null) throw new ArgumentException(string.Format($"No {axistype} axis defined!"));
            return vData.GetMinValue(chartdef, axistype) + (decimal)((GetChartY0(chartdef) - y) / (axisdef.Scale * GetPixelToValueRatio(chartdef, axistype)));
        }

        private int TimeToX(DateTime date) {
            return GetChartX0() + (int)((date - vChart.From) / GetPixelToTimeRatio());
        }

        private DateTime XToTime(double x) {
            return vChart.From + (x - GetChartX0()) * GetPixelToTimeRatio();
        }

        private int GetChartX0() {
            if (!vCache.HasBars.HasValue) vCache.HasBars = vChart.ChartSeries.Any(ser => ser.IsUsed && ser.Type != SeriesType.Line);
            return (int)vChart.Layout.LeftMargin;
        }

        private int GetChartY0(ChartDefinition chartdef) {
            float height = 0;
            if (vCache.TotalChartHeight == null) vCache.TotalChartHeight = vChart.ChartDefinitions.Sum(def => def != null ? def.Height : 0);
            if (vCache.ChartOffsets == null || !vCache.ChartOffsets.TryGetValue(chartdef, out height)) {
                vCache.ChartOffsets = new();
                foreach (ChartDefinition def in vChart.ChartDefinitions) {
                    if (def == null) continue;
                    height += def.Height;
                    vCache.ChartOffsets[def] = height;
                }
                height = vCache.ChartOffsets[chartdef];
            }
            return (int)(vChart.Layout.TopMargin + height * vChart.ChartHeight / vCache.TotalChartHeight);
        }

        #endregion
    }
}