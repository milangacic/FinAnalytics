using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ChartControls {
    public enum SeriesType {
        Line,
        Bars,
        VolumeBars,
        OHLCCandles
    }

    public struct SeriesStyle {

        #region Properties

        public Pen LinePen { get; }

        public Brush BarBrush { get; }

        public Brush TextBrush { get; }

        #endregion

        #region Public methods

        public SeriesStyle(Pen linepen, Brush textbrush) {
            LinePen = linepen;
            TextBrush = textbrush;
            BarBrush = null;
        }

        public SeriesStyle(Brush barbrush, Brush textbrush) {
            LinePen = null;
            TextBrush = textbrush;
            BarBrush = barbrush;
        }

        public SeriesStyle(Brush barbrush, Pen linepen, Brush textbrush) {
            LinePen = linepen;
            TextBrush = textbrush;
            BarBrush = barbrush;
        }

        public SeriesStyle(Color color, double opacity = 1, double width = ChartSeries.DefaultPenWidth) {
            BarBrush = new SolidColorBrush(color) { Opacity = opacity };
            TextBrush = new SolidColorBrush(color);
            LinePen = new Pen(BarBrush, width);
        }

        public SeriesStyle(SeriesStyle style) {
            BarBrush = style.BarBrush;
            TextBrush = style.TextBrush;
            LinePen = style.LinePen;
        }

        #endregion
    }

    public class ChartSeries : INotifyPropertyChanged {

        #region Constants

        internal const double DefaultPenWidth = 1.5;

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        private string title;
        public string Title { get => title; set { title = value; OnPropertyChanged(nameof(Title)); } }

        private SeriesType type;
        public SeriesType Type { get => type; set { type = value; OnPropertyChanged(nameof(Type)); } }

        private AxisType axis;
        public AxisType Axis { get => axis; set { axis = value; OnPropertyChanged(nameof(Axis)); } }

        private SeriesStyle seriesstyle;
        public SeriesStyle SeriesStyle { get => seriesstyle; set { seriesstyle = value; OnPropertyChanged(nameof(SeriesStyle)); } }

        private IEnumerable<Tuple<DateTime, decimal[]>> data;
        public IEnumerable<Tuple<DateTime, decimal[]>> Data { get => data; set { data = value; OnPropertyChanged(nameof(Data)); } }

        private string chartdefinition;
        public string ChartDefinition { get => chartdefinition; set { chartdefinition = value; OnPropertyChanged(nameof(ChartDefinition)); OnPropertyChanged(nameof(IsUsed)); } }
        public bool IsUsed { get { return !string.IsNullOrEmpty(ChartDefinition); } }
        public bool HasBars { get { return Type == SeriesType.Bars || Type == SeriesType.VolumeBars || Type == SeriesType.OHLCCandles; } }

        #endregion

        #region Public methods

        public ChartSeries() {
            Type = SeriesType.Line;
            Axis = AxisType.Left;
            SeriesStyle = new SeriesStyle();
        }

        public ChartSeries(string name, SeriesType type, AxisType axis, IEnumerable<Tuple<DateTime, decimal[]>> data, SeriesStyle? style = null) {
            Title = name;
            Type = type;
            Axis = axis;
            Data = data;
            SeriesStyle = style ?? new SeriesStyle();
        }

        public ChartSeries(string name, SeriesType type, IEnumerable<Tuple<DateTime, decimal[]>> data, SeriesStyle? style = null) : this(name, type, AxisType.Left, data, style) { }

        public ChartSeries(string name, IEnumerable<Tuple<DateTime, decimal[]>> data, SeriesStyle? style = null) : this(name, SeriesType.Line, AxisType.Left, data, style) { }

        public override string ToString() {
            return Title + " (" + Type + ")";
        }

        #endregion

        #region Private methods        

        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}