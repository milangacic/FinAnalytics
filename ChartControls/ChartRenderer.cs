using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace ChartControls {

    internal class ChartRenderer {

        #region Fields

        private readonly FinancialChartControl vChart;
        private readonly SelectionManager vSelection;
        private readonly DimensionManager vDimensions;
        private readonly ZoomManager vZoom;
        private readonly DataManager vData;
        private readonly CacheManager vCache;
        private DpiScale vDpiSettings;

        #endregion

        #region Public methods        

        public ChartRenderer(FinancialChartControl chart, SelectionManager selection, DimensionManager dimensions, ZoomManager zoom, DataManager data, CacheManager cache) {
            vChart = chart;
            vDimensions = dimensions;
            vSelection = selection;
            vDpiSettings = VisualTreeHelper.GetDpi(vChart);
            vData = data;
            vZoom = zoom;
            vCache = cache;
            RenderOptions.SetCachingHint(vChart, CachingHint.Cache);
            RenderOptions.SetCacheInvalidationThresholdMinimum(vChart, 0.5);
            RenderOptions.SetCacheInvalidationThresholdMaximum(vChart, 2);
        }

        public void RenderChart(DrawingContext drawingContext) {
            if (vChart.ChartWidth <= 0 || vChart.ChartHeight <= 0) return;
            Clear(drawingContext);
            foreach (ChartDefinition chartdef in vChart.ChartDefinitions) {
                if (chartdef == null) continue;
                RenderChartDefinition(drawingContext, chartdef);
            }
            RenderAxis(drawingContext);
            DrawSelectionLine(drawingContext);
        }

        public void RenderChart(DrawingContext drawingContext, DpiScale dpi) {
            var olddpi = vDpiSettings;
            vDpiSettings = dpi;
            vCache.OnDpiChanged();
            RenderChart(drawingContext);
            vDpiSettings = olddpi;
            vCache.OnDpiChanged();
        }

        public void SetDpi(DpiScale dpi) {
            vDpiSettings = dpi;
            vCache.OnDpiChanged();
        }

        public double GetFontSize(int size) {
            return size; // / vDpiSettings.PixelsPerDip;
        }

        #endregion

        #region Private methods

        private void Clear(DrawingContext drawingContext) {
            drawingContext.DrawRectangle(vChart.Layout.BackgroundBrush, null, new Rect(vChart.RenderSize));
        }

        private void DrawSelectionLine(DrawingContext drawingContext) {
            if (vSelection.IsSelected()) {
                drawingContext.DrawLine(vChart.Layout.LinePen, new Point(vSelection.SelectedPoint.X, vChart.Layout.TopMargin), new Point(vSelection.SelectedPoint.X, vChart.Layout.TopMargin + vChart.ChartHeight));
            }
        }

        #region Chart definition

        private void RenderChartDefinition(DrawingContext drawingContext, ChartDefinition chartdefinition) {
            RectangleGeometry clip = new RectangleGeometry(vDimensions.GetChartRect(chartdefinition));
            drawingContext.PushClip(clip);
            foreach (ChartSeries series in vChart.GetSelectedChartSeries(chartdefinition)) {
                RenderChartSeries(drawingContext, chartdefinition, series);
            }
            DrawFixedLines(drawingContext, chartdefinition, AxisType.Left);
            DrawFixedLines(drawingContext, chartdefinition, AxisType.Right);
            drawingContext.Pop();
        }

        #endregion

        #region Chart series

        private void RenderChartSeries(DrawingContext drawingContext, ChartDefinition chartdefinition, ChartSeries chartseries) {
            if (vChart.Count == 0) return;
            if (chartseries.Data == null || !chartseries.Data.Any()) return;
            if (chartdefinition.GetAxis(chartseries.Axis) == null) return;
            DateTime from = vChart.From - 3 * vData.GetMaxInterval();
            DateTime to = vChart.To + 3 * vData.GetMaxInterval();
            IEnumerable<Tuple<DateTime, decimal[]>> data = chartseries.Data.Where(item => item.Item1 >= from && item.Item1 <= to);
            Tuple<DateTime, decimal[]> prevdatapoint = data.FirstOrDefault();
            bool selected;
            foreach (Tuple<DateTime, decimal[]> datapoint in data) {
                selected = vSelection.IsSelected(chartseries) && datapoint.Item1 == vSelection.SelectedTime;
                switch (chartseries.Type) {
                    case SeriesType.OHLCCandles:
                        DrawOHLCCandle(drawingContext, chartdefinition, chartseries, datapoint, selected);
                        break;
                    case SeriesType.Line:
                        DrawLine(drawingContext, chartdefinition, chartseries, prevdatapoint, datapoint, selected);
                        break;
                    case SeriesType.Bars:
                        DrawBar(drawingContext, chartdefinition, chartseries, datapoint, selected);
                        break;
                    case SeriesType.VolumeBars:
                        DrawVolumeBar(drawingContext, chartdefinition, chartseries, datapoint, selected);
                        break;
                    default:
                        throw new ArgumentException("Unsupported chart type");
                }
                prevdatapoint = datapoint;
            }
        }

        private void DrawBar(DrawingContext drawingContext, ChartDefinition chartdef, ChartSeries series, Tuple<DateTime, decimal[]> datapoint, bool selected = false) {
            Point point0 = vDimensions.DataToPoint(chartdef, series.Axis, datapoint.Item1, 0);
            Point point1 = vDimensions.DataToPoint(chartdef, series.Axis, datapoint.Item1, datapoint.Item2[0]);
            int barsize = Math.Max(2, (int)(vData.GetInterval(series) / vDimensions.GetPixelToTimeRatio() * vChart.Layout.LargeBarSize));
            drawingContext.DrawRectangle(GetBarBrush(series), selected ? vChart.Layout.HighlightPen : null, new Rect(new Point(point0.X - barsize / 2, point0.Y), new Point(point1.X + barsize / 2, point1.Y)));
        }

        private void DrawVolumeBar(DrawingContext drawingContext, ChartDefinition chartdef, ChartSeries series, Tuple<DateTime, decimal[]> datapoint, bool selected = false) {
            Point point0 = vDimensions.DataToPoint(chartdef, series.Axis, datapoint.Item1, 0);
            Point point1 = vDimensions.DataToPoint(chartdef, series.Axis, datapoint.Item1, datapoint.Item2[0]);
            int barsize = Math.Max(2, (int)(vData.GetInterval(series) / vDimensions.GetPixelToTimeRatio() * vChart.Layout.LargeBarSize));
            drawingContext.DrawRectangle(GetOCBrush(datapoint.Item2[1], datapoint.Item2[2]), selected ? vChart.Layout.HighlightPen : null, new Rect(new Point(point0.X - barsize / 2, point0.Y), new Point(point1.X + barsize / 2, point1.Y)));
        }

        private void DrawOHLCCandle(DrawingContext drawingContext, ChartDefinition chartdef, ChartSeries series, Tuple<DateTime, decimal[]> datapoint, bool selected = false) {
            int ocbarsize = Math.Max(4, (int)(vData.GetInterval(series) / vDimensions.GetPixelToTimeRatio() * vChart.Layout.LargeBarSize));
            int hlbarsize = Math.Max(2, (int)(vData.GetInterval(series) / vDimensions.GetPixelToTimeRatio() * vChart.Layout.SmallBarSize));        
            Point point0 = vDimensions.DataToPoint(chartdef, series.Axis, datapoint.Item1, datapoint.Item2[0]);
            Point point1 = vDimensions.DataToPoint(chartdef, series.Axis, datapoint.Item1, datapoint.Item2[1]);
            Point point2 = vDimensions.DataToPoint(chartdef, series.Axis, datapoint.Item1, datapoint.Item2[2]);
            Point point3 = vDimensions.DataToPoint(chartdef, series.Axis, datapoint.Item1, datapoint.Item2[3]);
            drawingContext.DrawRectangle(GetOCBrush(datapoint.Item2[0], datapoint.Item2[3]), null, new Rect(new Point(point1.X - hlbarsize / 2, point1.Y), new Point(point2.X + hlbarsize / 2, point2.Y)));
            drawingContext.DrawRectangle(GetOCBrush(datapoint.Item2[0], datapoint.Item2[3]), selected ? vChart.Layout.HighlightPen : null, new Rect(new Point(point0.X - ocbarsize / 2, point0.Y), new Point(point3.X + ocbarsize / 2, point3.Y)));
        }

        private void DrawLine(DrawingContext drawingContext, ChartDefinition chartdef, ChartSeries series, Tuple<DateTime, decimal[]> datapoint0, Tuple<DateTime, decimal[]> datapoint1, bool selected = false) {
            Point point0 = vDimensions.DataToPoint(chartdef, series.Axis, datapoint0.Item1, datapoint0.Item2[0]);
            Point point1 = vDimensions.DataToPoint(chartdef, series.Axis, datapoint1.Item1, datapoint1.Item2[0]);
            drawingContext.DrawLine(GetLinePen(series), point0, point1);
            if (selected) {
                drawingContext.DrawEllipse(GetBarBrush(series), null, point1, vChart.Layout.PointSelectionSize, vChart.Layout.PointSelectionSize);
            }
        }

        private Brush GetOCBrush(decimal open, decimal close) {
            return (close - open) >= 0 ? vChart.Layout.OHLCGainBrush : vChart.Layout.OHLCLossBrush;
        }

        private Pen GetLinePen(ChartSeries series) {
            if (series.SeriesStyle.LinePen != null) return series.SeriesStyle.LinePen;
            if (vCache.DefaultChartStyles == null) vCache.DefaultChartStyles = new();
            if (vCache.DefaultChartStyles.TryGetValue(series, out var def)) return def.LinePen;
            SeriesStyle style = new(GetNextColor());
            vCache.DefaultChartStyles.Add(series, style);
            return style.LinePen;
        }

        private Brush GetBarBrush(ChartSeries series) {
            if (series.SeriesStyle.BarBrush != null) return series.SeriesStyle.BarBrush;
            if (vCache.DefaultChartStyles == null) vCache.DefaultChartStyles = new();
            if (vCache.DefaultChartStyles.TryGetValue(series, out var def)) return def.BarBrush;
            SeriesStyle style = new(GetNextColor());
            vCache.DefaultChartStyles.Add(series, style);
            return style.BarBrush;
        }

        private Color GetNextColor() {
            return vChart.Layout.DefaultColors[vCache.ColorIndex++ % vChart.Layout.DefaultColors.Length];
        }

        #endregion

        #region Axis        

        private void RenderAxis(DrawingContext drawingContext) {
            drawingContext.DrawRectangle(null, vChart.Layout.MainPen, new Rect(vChart.Layout.LeftMargin, vChart.Layout.TopMargin, vChart.ChartWidth, vChart.ChartHeight));
            DrawXAxis(drawingContext);
            foreach (ChartDefinition chartdef in vChart.ChartDefinitions) {
                if (chartdef == null) continue;
                Rect rect = vDimensions.GetChartRect(chartdef);
                drawingContext.DrawLine(vChart.Layout.MainPen, rect.BottomLeft, rect.BottomRight);
                DrawYAxis(drawingContext, chartdef, AxisType.Left);
                DrawYAxis(drawingContext, chartdef, AxisType.Right);
            }
            if (vZoom.IsRangeZooming()) {
                (DateTime from, DateTime to) = vZoom.GetZoomRange();
                (double fromx, double tox) = (vDimensions.DataToPoint(from).X, vDimensions.DataToPoint(to).X);
                drawingContext.DrawRectangle(new SolidColorBrush(vChart.Layout.ForegroundColor) { Opacity = 0.5 }, null, new Rect(fromx, vChart.Layout.TopMargin, tox - fromx, vChart.ChartHeight));
            }
        }

        private void DrawXAxis(DrawingContext drawingContext) {
            if (vChart.From == default || vChart.To == default || !vChart.GetSelectedChartSeries().Any()) {
                return;
            }
            (DateTime[] ticks, string tickformat, DateTime[] bands, string bandformat) = GetXAxisTicksBands();
            if (ticks == null || !ticks.Any()) return;
            Vector tickvec = new Vector(0, vChart.Layout.TickLength);
            // Ticks
            foreach (DateTime date in ticks) {
                Point tickpos = vDimensions.DataToPoint(date);
                FormattedText text = GetLableText(date, tickformat);
                if (vChart.GridLines) {
                    Vector gridlinevec = new Vector(0, vChart.ChartHeight);
                    drawingContext.DrawLine(vChart.Layout.GridPen, tickpos, tickpos - gridlinevec);
                }
                drawingContext.DrawLine(vChart.Layout.MainPen, tickpos - tickvec / 2, tickpos + tickvec / 2);
                if (vChart.Labels && tickpos.X - text.Width / 2 >= 0 && tickpos.X + text.Width / 2 <= vChart.ActualWidth) {
                    Vector textvec = new Vector(-text.Width / 2, vChart.Layout.TickLength);
                    drawingContext.DrawText(text, tickpos + textvec);
                }
            }
            if (bands == null || !vChart.Labels) return;
            // Bands
            for (int i = 0; i < bands.Length; i++) {
                DateTime date = bands[i];
                Point tickpos = vDimensions.DataToPoint(date);
                FormattedText text = GetLableText(date, bandformat);
                if (tickpos.X >= vChart.Layout.LeftMargin && tickpos.X <= vChart.Layout.LeftMargin + vChart.ChartWidth) {
                    Vector heightvec = new Vector(0, text.Height);
                    drawingContext.DrawLine(vChart.Layout.MainPen, tickpos + heightvec + tickvec, tickpos + 2 * heightvec + tickvec);
                }
                if (!vChart.Labels) continue;
                double bandfrom = Math.Max(tickpos.X, 0);
                double bandto = i < (bands.Length - 1) ? Math.Min(vDimensions.DataToPoint(bands[i + 1]).X, vChart.ActualWidth) : vChart.ActualWidth;
                Point textpos = new Point(bandfrom + (bandto - bandfrom) / 2, tickpos.Y);
                if (textpos.X - 1.2 * text.Width / 2 >= 0 && textpos.X + 1.2 * text.Width / 2 <= vChart.ActualWidth) {
                    Vector textvec = new Vector(-text.Width / 2, text.Height + vChart.Layout.TickLength);
                    drawingContext.DrawText(text, textpos + textvec);
                }
            }
        }

        private void DrawYAxis(DrawingContext drawingContext, ChartDefinition chartdef, AxisType type) {
            AxisDefinition axis = chartdef.GetAxis(type);
            if (axis is null) return;
            if (vChart.Count == 0) return;
            if (!vChart.ChartSeries.Any(item => item.ChartDefinition == chartdef.ID && item.Axis == type)) return;
            if (!(vDimensions.GetChartHeight(chartdef) > 0)) return;
            (decimal[] ticks, string format) = GetYAxisTicks(chartdef, type);
            if (ticks == null) return;
            Vector tickvec = new Vector(vChart.Layout.TickLength / 2, 0);
            foreach (decimal y in ticks) {
                if (y == 0 && !ticks.Any(tick => tick < 0)) continue;
                Point ypos = vDimensions.DataToPoint(chartdef, type, y);
                FormattedText text = GetLableText(y, format);
                if (axis.GridLines) {
                    Vector linevec = new Vector(vChart.ChartWidth, 0);
                    drawingContext.DrawLine(vChart.Layout.GridPen, ypos, ypos + (type == AxisType.Left ? linevec : -linevec));
                }
                drawingContext.DrawLine(vChart.Layout.MainPen, ypos - tickvec, ypos + tickvec);
                if (axis.Labels) {
                    Vector labelvec = new Vector(type == AxisType.Left ? vChart.Layout.TickLength : -(text.Width + vChart.Layout.TickLength), -text.Height / 2);
                    drawingContext.DrawText(text, ypos + labelvec);
                }
            }
        }

        private void DrawFixedLines(DrawingContext drawingContext, ChartDefinition chartdef, AxisType axis) {
            AxisDefinition axisdef = chartdef.GetAxis(axis);
            if (axisdef == null || axisdef.Lines.Count == 0) return;
            if (!vChart.ChartSeries.Any(item => item.ChartDefinition == chartdef.ID && item.Axis == axis)) return;
            foreach (decimal line in axisdef.Lines) {
                Point point0 = vDimensions.DataToPoint(chartdef, axis, vChart.From, line);
                Point point1 = vDimensions.DataToPoint(chartdef, axis, vChart.To, line);
                point0.X = vChart.Layout.LeftMargin; point1.X = vChart.Layout.LeftMargin + vChart.ChartWidth;
                drawingContext.DrawLine(vChart.Layout.LinePen, point0, point1);
            }
        }

        private (DateTime[] Ticks, string TickFormat, DateTime[] Bands, string BandFormat) GetXAxisTicksBands() {
            double sizefactor = 1.2;
            DateTime maxdate = new DateTime(9999, 12, 31, 23, 59, 59);
            TimeSpan timerange = vChart.To - vChart.From;
            if (!vCache.TextWidths.HasValue) {
                CacheManager.TextWidthStruct textwidths;
                textwidths.DayWidth = sizefactor * GetLableText(maxdate, "d").Width;
                textwidths.DayTimeWidth = sizefactor * GetLableText(maxdate, "g").Width;
                textwidths.HourWidth = sizefactor * GetLableText(maxdate, "HH:mm").Width;
                textwidths.MinuteWidth = sizefactor * GetLableText(maxdate, "mm").Width;
                textwidths.SecondWidth = sizefactor * GetLableText(maxdate, "ss").Width;
                vCache.TextWidths = textwidths;
            }
            // Calculate max/min ticks for the different tick sizes
            int maxdayticks = (int)Math.Floor(vChart.ChartWidth / vCache.TextWidths.Value.DayWidth);
            int maxdaytimeticks = (int)Math.Floor(vChart.ChartWidth / vCache.TextWidths.Value.DayTimeWidth);
            int maxhourticks = (int)Math.Floor(vChart.ChartWidth / vCache.TextWidths.Value.HourWidth);
            int maxminuteticks = (int)Math.Floor(vChart.ChartWidth / vCache.TextWidths.Value.MinuteWidth);
            int maxsecondticks = (int)Math.Floor(vChart.ChartWidth / vCache.TextWidths.Value.SecondWidth);
            int mindayticks = (int)Math.Floor(timerange / TimeSpan.FromDays(1));
            int minhourticks = (int)Math.Floor(timerange / TimeSpan.FromHours(1));
            int minminuteticks = (int)Math.Floor(timerange / TimeSpan.FromMinutes(1));
            int minsecondticks = (int)Math.Floor(timerange / TimeSpan.FromSeconds(1));
            int min6hoursticks = (int)Math.Floor(timerange / TimeSpan.FromHours(6));
            int min15minutesticks = (int)Math.Floor(timerange / TimeSpan.FromMinutes(15));            
            int min15secondsticks = (int)Math.Floor(timerange / TimeSpan.FromSeconds(15));

            // Select best fitting layout
            TimeSpan ticksize;
            TimeSpan bandsize = default;
            string tickformat = null;
            string bandformat = null;
            if (minsecondticks <= maxsecondticks && minminuteticks <= maxdaytimeticks) {
                ticksize = TimeSpan.FromSeconds(1);
                tickformat = "ss";
                bandsize = TimeSpan.FromMinutes(1);
                bandformat = "g";
            } else if (min15secondsticks <= maxsecondticks && minminuteticks <= maxdaytimeticks) {
                ticksize = TimeSpan.FromSeconds(15);
                tickformat = "ss";
                bandsize = TimeSpan.FromMinutes(1);
                bandformat = "g";
            } else if (minminuteticks <= maxminuteticks && minhourticks <= maxdaytimeticks) {
                ticksize = TimeSpan.FromMinutes(1);
                tickformat = "mm";
                bandsize = TimeSpan.FromHours(1);
                bandformat = "g";
            } else if (min15minutesticks <= maxminuteticks && minhourticks <= maxdaytimeticks) {
                ticksize = TimeSpan.FromMinutes(15);
                tickformat = "mm";
                bandsize = TimeSpan.FromHours(1);
                bandformat = "g";
            } else if (minhourticks <= maxhourticks && mindayticks <= maxdayticks) {
                ticksize = TimeSpan.FromHours(1);
                tickformat = "HH:mm";
                bandsize = TimeSpan.FromDays(1);
                bandformat = "d";
            } else if (min6hoursticks <= maxhourticks && mindayticks <= maxdayticks) {
                ticksize = TimeSpan.FromHours(6);
                tickformat = "HH:mm";
                bandsize = TimeSpan.FromDays(1);
                bandformat = "d";
            } else if (mindayticks <= maxdayticks) {
                ticksize = TimeSpan.FromDays(1);
                tickformat = "d";
            } else if (timerange.Days < maxdayticks) {
                return (Ticks: null, TickFormat: null, Bands: null, BandFormat: null);
            } else {
                ticksize = TimeSpan.FromDays((int)Math.Ceiling(timerange.Days / (double)maxdayticks));
                tickformat = "d";
            }

            // Center ticks and map from date to grid
            DateTime from = vChart.From + (timerange / 2) - 0.5 * ticksize * Math.Floor(timerange / ticksize);
            if (ticksize < TimeSpan.FromDays(1)) {
                from = vChart.From + Math.Floor((from - vChart.From) / ticksize) * ticksize;
            }
            from = MapDateToGrid(from, ticksize);
            if (from < vChart.From) from += ticksize;
            // Calculate tick dates
            int ticks = (int)Math.Floor((vChart.To - from) / ticksize);
            DateTime[] tickdates = Enumerable.Range(0, ticks + 1).Select(item => from.AddSeconds(item * ticksize.TotalSeconds)).ToArray();
            if (bandsize != default) {
                DateTime[] banddates = tickdates.Select(tick => MapDateToGrid(tick, bandsize, -1)).Distinct().ToArray();
                return (Ticks: tickdates, TickFormat: tickformat, Bands: banddates, BandFormat: bandformat);
            }
            return (Ticks: tickdates, TickFormat: tickformat, Bands: null, BandFormat: null);
        }

        private static DateTime MapDateToGrid(DateTime date, TimeSpan interval, short rounddirection = 0) {
            DateTime date0 = new DateTime(date.Year, date.Month, date.Day, interval.Days != 0 || interval.Hours != 0 ? 0 : date.Hour, interval.Hours != 0 || interval.Minutes != 0 ? 0 : date.Minute, 0);
            DateTime mappeddate = date0 + Math.Floor((date - date0) / interval) * interval;
            if (rounddirection == 0) {
                return (date - mappeddate) < (mappeddate + interval - date) ? mappeddate : mappeddate + interval;
            } else if (rounddirection > 0) {
                return date != mappeddate ? mappeddate + interval : mappeddate;
            } else {
                return mappeddate;
            }
        }

        private (decimal[] Ticks, string Format) GetYAxisTicks(ChartDefinition chartdef, AxisType axis) {
            AxisDefinition axisdef = chartdef.GetAxis(axis);
            if (axisdef != null && !axisdef.Default && axisdef.Ticks.Count != 0) {
                decimal[] customticks = axisdef.Ticks.OrderBy(item => item).ToArray();
                decimal mindelta = Math.Abs(customticks[0]);
                for (int i = 1; i < customticks.Length; i++) {
                    mindelta = Math.Min(mindelta, Math.Abs(customticks[i] - customticks[i - 1]));
                }
                return (Ticks: axisdef.Ticks.OrderBy(item => item).ToArray(), Format: GetYLabelFormat(mindelta));
            }
            if (!vCache.TextHeight.HasValue) {
                vCache.TextHeight = GetLableText(100.0000M, "N4").Height;
            }
            decimal valuerange = vData.GetMaxValue(chartdef, axis) - vData.GetMinValue(chartdef, axis);
            if (valuerange == 0) return (Ticks: null, Format: null);
            double size = Math.Floor(Math.Log10((double)valuerange));
            decimal delta = (decimal)Math.Pow(10, size - 1) * 5;
            for (double sz = size; sz >= size - 2; sz--) {
                foreach (double mult in new double[] { 5, 2.5, 1 }) {
                    decimal newdelta = (decimal)(Math.Pow(10, sz) * mult);
                    int ticks = (int)Math.Floor(valuerange / newdelta);
                    if (ticks * vCache.TextHeight < (vDimensions.GetChartHeight(chartdef) - vCache.TextHeight) * chartdef.GetAxis(axis).Scale / 2) {
                        delta = newdelta;
                    }
                }
            }
            string format = GetYLabelFormat(delta);
            decimal yfrom = vData.GetMinValue(chartdef, axis) + delta * (Math.Ceiling(vData.GetMinValue(chartdef, axis) / delta) - vData.GetMinValue(chartdef, axis) / delta);
            return (Ticks: Enumerable.Range(0, (int)Math.Ceiling((valuerange - (yfrom - vData.GetMinValue(chartdef, axis))) / delta)).Select(i => yfrom + i * delta).ToArray(), Format: format);
        }

        private string GetYLabelFormat(decimal delta) {
            double decimalpart = (double)(delta - decimal.Truncate(delta));
            int decimalpoints = -(int)Math.Floor(Math.Log10(decimalpart));
            return "N" + Math.Max(0, Math.Min(4, decimalpoints));
        }

        #endregion

        #region Texts

        private FormattedText GetLableText(object value, string format = null) {
            return GetFormattedText(string.Format(format != null ? "{0:" + format + "}" : "{0}", value), vChart.Layout.TextSize, vChart.Layout.TextBrush);
        }

        private FormattedText GetFormattedText(string text, int size, Brush color) {
            return new FormattedText(text, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, vChart.Layout.TextFont, GetFontSize(size), color, vDpiSettings.PixelsPerDip);
        }

        #endregion

        #endregion

    }
}