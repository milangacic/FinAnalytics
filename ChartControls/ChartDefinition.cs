using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace ChartControls {

    public class ChartDefinition : INotifyPropertyChanged {

        #region Properties        

        private string id;
        public string ID { get { return id; } set { id = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ID))); } }

        private AxisDefinition leftaxis;
        public AxisDefinition LeftAxis { get { return leftaxis; } set { leftaxis = value; AttachEventHandlers(leftaxis, nameof(LeftAxis)); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LeftAxis))); } }

        private AxisDefinition rightaxis;
        public AxisDefinition RightAxis { get { return rightaxis; } set { rightaxis = value; AttachEventHandlers(rightaxis, nameof(RightAxis)); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RightAxis))); } }

        private float height;
        public float Height { 
            get { return height; } 
            set {
                if (value < 0) throw new ArgumentException($"{nameof(Height)} must be positive!");
                height = value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Height))); 
            } 
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public methods

        public ChartDefinition() : this(null, 100) { }

        public ChartDefinition(string id, float height, AxisDefinition leftaxis = null, AxisDefinition rightaxis = null) {
            this.id = id;
            this.height = height;
            this.leftaxis = leftaxis ?? new AxisDefinition();
            this.rightaxis = rightaxis;
            AttachEventHandlers(leftaxis, nameof(LeftAxis));
            AttachEventHandlers(rightaxis, nameof(RightAxis));
        }

        public AxisDefinition GetAxis(ChartSeries series) {
            return GetAxis(series.Axis);
        }

        public AxisDefinition GetAxis(AxisType type) {
            return type == AxisType.Left ? LeftAxis : RightAxis;
        }

        #endregion

        #region Private methods

        private void AttachEventHandlers(AxisDefinition axis, string property) {
            if (axis == null) return;
            axis.PropertyChanged += (object s, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}