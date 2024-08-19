using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

namespace ChartControls {
    internal class ZoomManager {

        #region Fields

        private readonly FinancialChartControl vChart;
        private readonly DataManager vData;
        private DateTime vZoomFrom;
        private DateTime vZoomTo;

        #endregion

        #region Events

        public event EventHandler<Tuple<DateTime, DateTime>> Zooming;

        #endregion

        #region Public methods

        public ZoomManager(FinancialChartControl chart, DataManager data) {
            vChart = chart;
            vData = data;
        }

        public bool Zoom(int delta) {
            int sign = Math.Sign(delta);
            int abs = Math.Abs(delta);
            delta = sign * Math.Min(abs, (int)FinancialChartControl.MaxZoomSteps);
            if (abs > System.Windows.SystemParameters.WheelScrollLines) {
                var newfrom = vChart.From + (vChart.To - vChart.From) * delta / vChart.Count;
                var newto = vChart.To - (vChart.To - vChart.From) * delta / vChart.Count;
                int numpoints = vChart.GetSelectedChartSeries().SelectMany(ser => ser.Data.Where(item => item.Item1 >= newfrom && item.Item1 <= newto).Select(item => item.Item1)).Distinct().Count();
                if (numpoints < FinancialChartControl.MinPoints) return false;
                Zooming?.Invoke(this, Tuple.Create(newfrom, newto));
                return true;
            } else if (abs != 0) {
                var newfrom = delta > 0 ? vData.GetNextClosestDate(vChart.From) : vData.GetPreviousDate(vChart.From);
                var newto = delta > 0 ? vData.GetPreviousClosestDate(vChart.To) : vData.GetNextDate(vChart.To);
                int numpoints = vChart.GetSelectedChartSeries().SelectMany(ser => ser.Data.Where(item => item.Item1 >= newfrom && item.Item1 <= newto).Select(item => item.Item1)).Distinct().Count();
                if (numpoints < FinancialChartControl.MinPoints) return false;
                Zooming?.Invoke(this, Tuple.Create(newfrom, newto));
                return true;
            }
            return false;
        }

        public bool FinishRangeZoom(DateTime to = default) {
            if (!IsRangeZooming()) return false;
            if (to == default) {
                if (vZoomTo == default) {
                    StopRangeZoom();
                    return false;
                }                
            } else {  
                vZoomTo = to;
            }
            if (vZoomFrom > vZoomTo) {
                (vZoomFrom, vZoomTo) = (vZoomTo, vZoomFrom);
            }            
            var range = Tuple.Create(vZoomFrom, vZoomTo);
            StopRangeZoom();
            int numpoints = vChart.GetSelectedChartSeries().SelectMany(ser => ser.Data.Where(item => item.Item1 >= range.Item1 && item.Item1 <= range.Item2).Select(item => item.Item1)).Distinct().Count();
            if (numpoints < FinancialChartControl.MinPoints) return false;
            Zooming?.Invoke(this, range);            
            return true;
        }

        public void StopRangeZoom() {
            vZoomFrom = default;
            vZoomTo = default;
        }

        public bool IsRangeZooming() {
            return vZoomFrom != default;
        }

        public void RangeZoom(DateTime date) {
            if (!IsRangeZooming()) {
                vZoomFrom = date;
            } else {
                vZoomTo = date;
            }
        }

        public (DateTime from, DateTime to) GetZoomRange() {
            return vZoomFrom <= vZoomTo ? (vZoomFrom, vZoomTo) : (vZoomTo, vZoomFrom);
        }

        #endregion

    }
}