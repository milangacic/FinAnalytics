using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ChartControls {
    internal class CacheManager {

        #region Fields

        public struct TextWidthStruct {
            public double DayWidth;
            public double DayTimeWidth;
            public double HourWidth;
            public double MinuteWidth;
            public double SecondWidth;
        };

        public int? Count;
        public bool? HasData;
        public DateTime? MinTime;
        public DateTime? MaxTime;
        public Dictionary<ChartDefinition, Dictionary<AxisType, decimal>> MaxValue;
        public Dictionary<ChartDefinition, Dictionary<AxisType, decimal>> MinValue;
        public Dictionary<ChartDefinition, IEnumerable<ChartSeries>> ChartSeriesGroup;
        public Dictionary<ChartDefinition, float> ChartOffsets;
        public Dictionary<ChartSeries, TimeSpan> Intervals;
        public TimeSpan? MaxInterval;
        public TimeSpan? MinInterval;
        public double? TotalChartHeight;
        public bool? HasBars;
        public double? ChartWidth;
        public double? ChartHeight;
        public TextWidthStruct? TextWidths;
        public double? TextHeight;
        public Dictionary<ChartDefinition, ContextMenu> ContextMenus;
        public Popup SelectBox;
        public Dictionary<ChartSeries, SeriesStyle> DefaultChartStyles;
        public int ColorIndex;

        #endregion

        #region Public methods        

        public void OnChartDataChanged() {
            MinValue?.Clear();
            MaxValue?.Clear();
            MinValue = null;
            MaxValue = null;
            MinTime = null;
            MaxTime = null;
            MaxInterval = null;
            MinInterval = null;
            Intervals = null;
            Count = null;
            ChartSeriesGroup?.Clear();
            ChartSeriesGroup = null;
            ChartOffsets?.Clear();
            ChartOffsets = null;
            TotalChartHeight = null;
            HasBars = null;
            HasData = null;
            ContextMenus?.Clear();
            ContextMenus = null;
        }

        public void OnDpiChanged() {
            TextWidths = null;
            TextHeight = null;
            ChartWidth = null;
            ChartHeight = null;
        }

        public void OnLayoutChanged() {
            TextWidths = null;
            TextHeight = null;
            ChartWidth = null;
            ChartHeight = null;
            ContextMenus?.Clear();
            ContextMenus = null;
            if (SelectBox != null) SelectBox.IsOpen = false;
            SelectBox = null;
        }

        public void OnResize() {
            ChartWidth = null;
            ChartHeight = null;
        }

        public void OnScrollingZooming() {
            MinValue?.Clear();
            MaxValue?.Clear();
            MinValue = null;
            MaxValue = null;
            Count = null;
        }

        public void ResetCache(bool clearstyles = false) {
            MinValue?.Clear();
            MaxValue?.Clear();
            MinValue = null;
            MaxValue = null;
            MinTime = null;
            MaxTime = null;
            MaxInterval = null;
            MinInterval = null;
            Intervals = null;
            Count = null;
            ChartSeriesGroup?.Clear();
            ChartSeriesGroup = null;
            ChartOffsets?.Clear();
            ChartOffsets = null;
            TotalChartHeight = null;
            HasBars = null;
            HasData = null;
            ChartWidth = null;
            ChartHeight = null;
            TextWidths = null;
            TextHeight = null;
            ContextMenus?.Clear();
            ContextMenus = null;
            if (SelectBox != null) SelectBox.IsOpen = false;
            SelectBox = null;
            if (clearstyles) {
                DefaultChartStyles?.Clear();
                DefaultChartStyles = null;                
            }
            ColorIndex = 0;
        }

        #endregion
    }
}
