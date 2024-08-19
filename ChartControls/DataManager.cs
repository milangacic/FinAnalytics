using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Markup;

namespace ChartControls {
    internal class DataManager {

        #region Fields

        private readonly FinancialChartControl vChart;
        private readonly CacheManager vCache;

        #endregion

        #region Public methods        

        public DataManager(FinancialChartControl chart, CacheManager cache) {
            vChart = chart;
            vCache = cache;
        }

        public DateTime GetMinFrom(DateTime to, bool margin = true) {
            if (!vChart.MaxRecords.HasValue || to == default) return vChart.MinTime;
            if (!vChart.HasData) return default;
            DateTime min = vChart.GetSelectedChartSeries().SelectMany(item => item.Data).Select(item => item.Item1).Where(item => item <= to).Distinct().OrderBy(item => item).TakeLast((int)vChart.MaxRecords).FirstOrDefault();
            return margin ? min - GetMaxInterval(min, to) / 2 : min;
        }

        public DateTime GetMaxFrom(DateTime to) {
            if (!vChart.HasData) return default;
            if (to == default) to = DateTime.MaxValue;
            return vChart.GetSelectedChartSeries().SelectMany(item => item.Data).Select(item => item.Item1).Where(item => item <= to).Distinct().OrderByDescending(item => item).Take((int)FinancialChartControl.MinPoints).LastOrDefault();
        }

        public DateTime GetMaxTo(DateTime from, bool addmargin = true) {
            if (!vChart.MaxRecords.HasValue || from == default) return vChart.MaxTime;
            if (!vChart.HasData) return default;
            DateTime max = vChart.GetSelectedChartSeries().SelectMany(item => item.Data).Select(item => item.Item1).Where(item => item >= from).Distinct().OrderBy(item => item).Take((int)vChart.MaxRecords).LastOrDefault();
            return addmargin ? max + GetMaxInterval(from, max) / 2 : max;
        }

        public DateTime GetMinTo(DateTime from) {
            if (!vChart.HasData) return default;
            if (from == default) from = DateTime.MinValue;
            return vChart.GetSelectedChartSeries().SelectMany(item => item.Data).Select(item => item.Item1).Where(item => item >= from).Distinct().OrderBy(item => item).Take((int)FinancialChartControl.MinPoints).LastOrDefault();
        }

        public decimal GetMaxValue(ChartDefinition chartdef, AxisType axis) {
            if (vCache.MaxValue == null) {
                static decimal _GetValue(ChartSeries ser, decimal[] item) => ser.Type switch { SeriesType.OHLCCandles => MathHelper.Max(item[0..4]), _ => item[0] };
                vCache.MaxValue = new();
                foreach (var def in vChart.ChartDefinitions) {
                    if (def == null) continue;
                    Dictionary<AxisType, decimal> maxvalue = new();
                    vCache.MaxValue.Add(def, maxvalue);
                    if (def.LeftAxis != null) {
                        maxvalue[AxisType.Left] = def.LeftAxis.To ?? MathHelper.Max(vChart.GetSelectedChartSeries(def).Where(ser => ser.Axis == AxisType.Left).Select(ser => vChart.GetData(ser).Select(data => _GetValue(ser, data.Item2))));
                    }
                    if (def.RightAxis != null) {
                        maxvalue[AxisType.Right] = def.RightAxis.To ?? MathHelper.Max(vChart.GetSelectedChartSeries(def).Where(ser => ser.Axis == AxisType.Right).Select(ser => vChart.GetData(ser).Select(data => _GetValue(ser, data.Item2))));
                    }
                }
            }
            if (chartdef.GetAxis(axis) == null) throw new ArgumentException(string.Format("No {0} axis defined!", axis));
            if (!vCache.MaxValue[chartdef].TryGetValue(axis, out decimal value)) return default;
            return value;
        }

        public decimal GetMinValue(ChartDefinition chartdef, AxisType axis) {
            if (vCache.MinValue == null) {
                vCache.MinValue = new();
                foreach (var def in vChart.ChartDefinitions) {
                    if (def == null) continue;
                    static decimal _GetValue(ChartSeries ser, decimal[] item) => ser.Type switch { SeriesType.OHLCCandles => MathHelper.Min(item[0..4]), SeriesType.Bars or SeriesType.VolumeBars => Math.Min(0, item[0]), _ => item[0] };
                    Dictionary<AxisType, decimal> minvalue = new();
                    vCache.MinValue.Add(def, minvalue);
                    if (def.LeftAxis != null) {
                        minvalue[AxisType.Left] = def.LeftAxis.From ?? MathHelper.Min(vChart.GetSelectedChartSeries(def).Where(ser => ser.Axis == AxisType.Left).Select(ser => vChart.GetData(ser).Select(data => _GetValue(ser, data.Item2))));
                    }
                    if (def.RightAxis != null) {
                        minvalue[AxisType.Right] = def.RightAxis.From ?? MathHelper.Min(vChart.GetSelectedChartSeries(def).Where(ser => ser.Axis == AxisType.Right).Select(ser => vChart.GetData(ser).Select(data => _GetValue(ser, data.Item2))));
                    }
                }
            }
            if (chartdef.GetAxis(axis) == null) throw new ArgumentException(string.Format("No {0} axis defined!", axis));
            if (!vCache.MinValue[chartdef].TryGetValue(axis, out decimal value)) return default;
            return value;
        }

        public DateTime GetPreviousDate(DateTime date) {
            if (!vChart.HasData) return default;            
            DateTime prevdate = default;
            foreach (ChartSeries ser in vChart.GetSelectedChartSeries()) {
                DateTime newdate = ser.Data.Select(item => item.Item1).OrderBy(item => item).LastOrDefault(item => item < date);
                if (newdate == default) continue;
                prevdate = prevdate == default ? newdate : MathHelper.MaxDate(prevdate, newdate);
            }
            return prevdate;
        }

        public DateTime GetPreviousClosestDate(DateTime date) {
            if (!vChart.HasData) return default;
            IOrderedEnumerable<DateTime> dates = vChart.GetSelectedChartSeries().SelectMany(item => item.Data).Select(item => item.Item1).Distinct().OrderByDescending(item => item);
            DateTime prevdate = dates.FirstOrDefault(vdate => vdate <= date);
            if (prevdate != default) return dates.FirstOrDefault(vdate => vdate < prevdate);
            return prevdate;
        }

        public DateTime GetNextDate(DateTime date) {
            if (!vChart.HasData) return default;
            DateTime nextdate = default;
            foreach (ChartSeries ser in vChart.GetSelectedChartSeries()) {
                DateTime newdate = ser.Data.Select(item => item.Item1).OrderBy(item => item).FirstOrDefault(item => item > date);
                if (newdate == default) continue;
                nextdate = nextdate == default ? newdate : MathHelper.MinDate(nextdate, newdate);
            }
            return nextdate;
        }

        public DateTime GetNextClosestDate(DateTime date) {
            if (!vChart.HasData) return default;
            IOrderedEnumerable<DateTime> dates = vChart.GetSelectedChartSeries().SelectMany(item => item.Data).Select(item => item.Item1).Distinct().OrderBy(item => item);
            DateTime nextdate = dates.FirstOrDefault(vdate => vdate >= date);
            if (nextdate != default) return dates.FirstOrDefault(vdate => vdate > nextdate);
            return nextdate;
        }

        public DateTime GetClosestDate(DateTime date, bool roundup = true) {
            DateTime closestdate = default;
            double distance = double.MaxValue;
            foreach (var ser in vChart.GetSelectedChartSeries()) {
                foreach (var data in ser.Data) {
                    double newdistance = Math.Abs((data.Item1 - date).TotalMilliseconds);
                    if (newdistance == 0) return data.Item1;
                    if (newdistance < distance) {
                        distance = newdistance;
                        closestdate = data.Item1;
                    } else if (newdistance == distance) {
                        closestdate = closestdate == default ? data.Item1 : (roundup ? MathHelper.MaxDate(data.Item1, closestdate) : MathHelper.MinDate(data.Item1, closestdate));
                    }
                }
            }
            return closestdate;
        }

        public DateTime GetClosestDate(DateTime date, int direction, bool roundup = true) {
            if (!vChart.HasData) return default;
            if (direction == 0) {
                return GetClosestDate(date, roundup);
            }
            IEnumerable<DateTime> dates = vChart.GetSelectedChartSeries().SelectMany(item => item.Data).Select(item => item.Item1).Distinct();
            DateTime closestdate = direction > 0 ? dates.OrderBy(item => item).FirstOrDefault(vdate => vdate >= date) : dates.OrderByDescending(item => item).FirstOrDefault(vdate => vdate <= date);           
            return closestdate;
        }

        public TimeSpan GetMaxInterval(DateTime from, DateTime to) {
            if (!vChart.HasData) return default;
            var chartseries = vChart.GetVisibleChartSeries(from, to).ToList();
            return MathHelper.Max(GetIntervals().Where(item => chartseries.Contains(item.Key)).Select(item => item.Value));
        }

        public TimeSpan GetMaxInterval(bool visible = false) {
            if (!vChart.HasData) return default;
            if (visible) {
                var chartseries = vChart.GetVisibleChartSeries().ToList();
                return MathHelper.Max(GetIntervals().Where(item => chartseries.Contains(item.Key)).Select(item => item.Value));
            } else {
                return MathHelper.Max(GetIntervals().Values);
            }
        }

        public TimeSpan GetInterval(ChartSeries series) {
            if (!vChart.HasData) return default;
            var intervals = GetIntervals();
            intervals.TryGetValue(series, out TimeSpan time);
            return time;
        }

        public IEnumerable<TimeSpan> GetIntervals(List<ChartSeries> series) {
            if (!vChart.HasData) return Enumerable.Empty<TimeSpan>();
            return GetIntervals().Where(item => series.Contains(item.Key)).Select(item => item.Value);
        }

        #endregion

        #region Private

        private Dictionary<ChartSeries, TimeSpan> GetIntervals() {
            if (vCache.Intervals == null) {
                vCache.Intervals = new();
                foreach (var ser in vChart.GetSelectedChartSeries()) {
                    if (ser.Data == null) continue;
                    var ordereddata = ser.Data.OrderBy(item => item.Item1);
                    var intervals = ordereddata.Skip(-1).Zip(ordereddata.Skip(1)).Select(item => item.Second.Item1 - item.First.Item1);
                    if (!intervals.Any()) continue;
                    vCache.Intervals[ser] = MathHelper.Average(intervals);
                }
            }
            return vCache.Intervals;
        }

        #endregion
    }
}
