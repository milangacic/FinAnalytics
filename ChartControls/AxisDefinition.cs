using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ChartControls {

    public enum AxisType {
        Left,
        Right
    }

    public class AxisDefinition : INotifyPropertyChanged {

        #region Properties        

        private ObservableCollection<decimal> ticks;
        public ObservableCollection<decimal> Ticks { get { return ticks; } set { ticks = value; AttachEventHandlers(ticks, nameof(Ticks)); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ticks))); } }

        private decimal? from;
        public decimal? From { get { return from; } set { from = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(From))); } }

        private decimal? to;
        public decimal? To { get { return to; } set { to = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(To))); } }

        private ObservableCollection<decimal> lines;
        public ObservableCollection<decimal> Lines { get { return lines; } set { lines = value; AttachEventHandlers(lines, nameof(Lines)); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Lines))); } }

        private bool gridlines;
        public bool GridLines { get { return gridlines; } set { gridlines = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GridLines))); } }

        private bool labels;
        public bool Labels { get { return labels; } set { labels = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Labels))); } }

        private float scale;
        public float Scale { get { return scale; } set { scale = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Scale))); } }

        public bool Default { get { return Ticks == null || !Ticks.Any(); } }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public methods

        public AxisDefinition() : this(1) { }

        public AxisDefinition(ObservableCollection<decimal> ticks) : this(null, null, ticks) { }

        public AxisDefinition(float scale) : this(null, null, null, scale) { }

        public AxisDefinition(AxisDefinition axis) : this(axis?.From, axis?.To, axis?.Ticks, axis != null ? axis.Scale : 1, axis?.Lines) { }

        public AxisDefinition(decimal? from, decimal? to, ObservableCollection<decimal> ticks = null, float scale = 1, ObservableCollection<decimal> lines = null) {
            this.from = from;
            this.to = to;
            this.ticks = ticks ?? new ObservableCollection<decimal>();
            this.scale = scale;
            this.lines = lines ?? new ObservableCollection<decimal>();
            this.labels = true;
            AttachEventHandlers(ticks, nameof(Ticks));
            AttachEventHandlers(lines, nameof(Lines));
        }

        #endregion

        #region Private methods

        private void AttachEventHandlers(ObservableCollection<decimal> list, string property) {
            if (list == null) return;
            list.CollectionChanged += (object s, NotifyCollectionChangedEventArgs e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}