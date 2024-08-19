using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ChartControls {
    public class ChartLayout : INotifyPropertyChanged {

        #region Properties

        #region Sizing

        private double vTopMargin;
        public double TopMargin { 
            get { return vTopMargin; } 
            set {
                if (value < 0) throw new ArgumentException($"{nameof(TopMargin)} must be positive!");
                vTopMargin = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TopMargin))); 
            } 
        }
        
        private double vBottomMargin;
        public double BottomMargin { 
            get { return vBottomMargin; } 
            set {
                if (value < 0) throw new ArgumentException($"{nameof(BottomMargin)} must be positive!");
                vBottomMargin = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BottomMargin))); 
            } 
        }

        private double vLeftMargin;
        public double LeftMargin { 
            get { return vLeftMargin; } 
            set {
                if (value < 0) throw new ArgumentException($"{nameof(LeftMargin)} must be positive!");
                vLeftMargin = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LeftMargin))); 
            } 
        }

        private double vRightMargin;
        public double RightMargin { 
            get { return vRightMargin; } 
            set {
                if (value < 0) throw new ArgumentException($"{nameof(RightMargin)} must be positive!");
                vRightMargin = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RightMargin))); 
            } 
        }

        private double vTickLength;
        public double TickLength { 
            get { return vTickLength; } 
            set {
               if (value < 0) throw new ArgumentException($"{nameof(TickLength)} must be positive!");
               vTickLength = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TickLength))); 
            } 
        }

        private double vPointSelectionSize;
        public double PointSelectionSize { 
            get { return vPointSelectionSize; } 
            set { 
                if (value < 0) throw new ArgumentException($"{nameof(PointSelectionSize)} must be positive!");
                vPointSelectionSize = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PointSelectionSize))); 
            } 
        }

        private double vLargeBarSize;
        public double LargeBarSize { 
            get { return vLargeBarSize; } 
            set {
                if (value < 0) throw new ArgumentException($"{nameof(LargeBarSize)} must be positive!");               
                vLargeBarSize = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LargeBarSize))); 
            } 
        }

        private double vSmallBarSize;
        public double SmallBarSize { 
            get { return vSmallBarSize; } 
            set {
                if (value < 0) throw new ArgumentException($"{nameof(SmallBarSize)} must be positive!");               
                vSmallBarSize = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SmallBarSize))); 
            } 
        }

        #endregion

        #region Text

        private Typeface vTextFont;
        public Typeface TextFont { get { return vTextFont; } set { vTextFont = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextFont))); } }

        private int vTextSize;
        public int TextSize { 
            get { return vTextSize; } 
            set {
                if (value < 0) throw new ArgumentException($"{nameof(TextSize)} must be positive!");               
                vTextSize = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextSize))); 
            } 
        }

        private Brush vTextBrush;
        public Brush TextBrush { get { return vTextBrush; } set { vTextBrush = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextBrush))); } }

        #endregion

        #region Popup

        private double vPopupMargin;
        public double PopupMargin { 
            get { return vPopupMargin; } 
            set {
               if (value < 0) throw new ArgumentException($"{nameof(PopupMargin)} must be positive!");
               vPopupMargin = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PopupMargin))); 
            } 
        }

        private Brush vPopupBackroundBrush;
        public Brush PopupBackroundBrush { get { return vPopupBackroundBrush; } set { vPopupBackroundBrush = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PopupBackroundBrush))); } }

        private double vPopupOpacity;
        public double PopupOpacity { 
            get { return vPopupOpacity; } 
            set { 
                if (value < 0 || value > 1) throw new ArgumentException($"{nameof(PopupOpacity)} must be between 0 and 1!");
                vPopupOpacity = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PopupOpacity))); 
            } 
        }

        private Brush vPopupBorderBrush;
        public Brush PopupBorderBrush { get { return vPopupBorderBrush; } set { vPopupBorderBrush = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PopupBorderBrush))); } }

        private Thickness vPopupBorderThickness;
        public Thickness PopupBorderThickness { get { return vPopupBorderThickness; } set { vPopupBorderThickness = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PopupBorderThickness))); } }

        private double vPopupBorderRadius;
        public double PopupBorderRadius { 
            get { return vPopupBorderRadius; }
            set {
                if (value < 0) throw new ArgumentException($"{nameof(PopupBorderRadius)} must be positive!");
                vPopupBorderRadius = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PopupBorderRadius))); 
            } 
        }

        #endregion

        #region Colours

        private Color vBackgroundColor;
        public Color BackgroundColor { get { return vBackgroundColor; } set { vBackgroundColor = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackgroundColor))); } }

        private Color vForegroundColor;
        public Color ForegroundColor { get { return vForegroundColor; } set { vForegroundColor = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForegroundColor))); } }

        private Brush vBackgroundBrush;
        public Brush BackgroundBrush { get { return vBackgroundBrush; } set { vBackgroundBrush = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackgroundBrush))); } }

        private Pen vMainPen;
        public Pen MainPen { get { return vMainPen; } set { vMainPen = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainPen))); } }

        private Pen vLinePen;
        public Pen LinePen { get { return vLinePen; } set { vLinePen = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LinePen))); } }

        private Pen vHighlightPen;
        public Pen HighlightPen { get { return vHighlightPen; } set { vHighlightPen = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighlightPen))); } }

        private Pen vGridPen;
        public Pen GridPen { get { return vGridPen; } set { vGridPen = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GridPen))); } }

        private Brush vOHLCGainBrush;
        public Brush OHLCGainBrush { get { return vOHLCGainBrush; } set { vOHLCGainBrush = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OHLCGainBrush))); } }

        private Brush vOHLCLossBrush;
        public Brush OHLCLossBrush { get { return vOHLCLossBrush; } set { vOHLCLossBrush = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OHLCLossBrush))); } }

        private Color[] vDefaultColors;
        public Color[] DefaultColors { get { return vDefaultColors; } set { vDefaultColors = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DefaultColors))); } }

        #endregion

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public methods

        public ChartLayout() : this(false) { }

        public ChartLayout(bool dark = false) {
            if (dark) InitializeDarkDefault();
            else InitializeBrightDefault();
        }

        #endregion

        #region Private methods

        private void InitializeDarkDefault() {
            vLeftMargin = 10;
            vTopMargin = 10;
            vRightMargin = 10;
            vBottomMargin = 40;
            vForegroundColor = Colors.White;
            vBackgroundColor = Colors.Black;
            vTextFont = new Typeface(SystemFonts.CaptionFontFamily.Source);
            vTextSize = (int)SystemFonts.CaptionFontSize;
            vTextBrush = Brushes.White;
            vBackgroundBrush = Brushes.Black;
            vHighlightPen = new Pen(Brushes.White, 2);
            vLinePen = new Pen(Brushes.White, 0.5) { DashStyle = DashStyles.Dash };
            vMainPen = new Pen(Brushes.White, 0.5);
            vGridPen = new Pen(Brushes.DarkGray, 0.5);
            vOHLCGainBrush = Brushes.Green;
            vOHLCLossBrush = Brushes.Red;
            vLargeBarSize = 3 / 4D;
            vSmallBarSize = 1 / 10D;
            vPointSelectionSize = 5;
            vTickLength = 5;
            vPopupBackroundBrush = new SolidColorBrush(Colors.Black) { Opacity = 0.5 };
            vPopupMargin = 5;
            vPopupOpacity = 1;
            vPopupBorderBrush = Brushes.White;
            vPopupBorderThickness = new(1);
            vPopupBorderRadius = 5;
            vDefaultColors = new Color[] {
                Colors.Aqua,
                Colors.Yellow,
                Colors.LightGreen,
                Colors.LightPink,
                Colors.Aquamarine,
                Colors.ForestGreen,
                Colors.BlueViolet,
                Colors.HotPink,
                Colors.IndianRed,
                Colors.Salmon
            };
        }

        private void InitializeBrightDefault() {
            vLeftMargin = 10;
            vTopMargin = 10;
            vRightMargin = 10;
            vBottomMargin = 40;
            vForegroundColor = Colors.Black;
            vBackgroundColor = Colors.White;
            vTextFont = new Typeface(SystemFonts.CaptionFontFamily.Source);
            vTextSize = (int)SystemFonts.CaptionFontSize;
            vTextBrush = Brushes.Black;
            vBackgroundBrush = Brushes.White;
            vHighlightPen = new Pen(Brushes.Black, 2);
            vLinePen = new Pen(Brushes.Black, 0.5) { DashStyle = DashStyles.Dash };
            vMainPen = new Pen(Brushes.Black, 0.5);
            vGridPen = new Pen(Brushes.DarkGray, 0.5);
            vOHLCGainBrush = Brushes.Green;
            vOHLCLossBrush = Brushes.Red;
            vLargeBarSize = 3 / 4D;
            vSmallBarSize = 1 / 10D;
            vPointSelectionSize = 5;
            vTickLength = 5;
            vPopupBackroundBrush = new SolidColorBrush(SystemColors.MenuColor) { Opacity = 0.5 };
            vPopupMargin = 5;
            vPopupOpacity = 1;
            vPopupBorderBrush = Brushes.Black;
            vPopupBorderThickness = new(1);
            vPopupBorderRadius = 5;// 2;
            vDefaultColors = new Color[] {
                Colors.Aqua,
                Colors.Orange,
                Colors.LawnGreen,
                Colors.DeepPink,
                Colors.Aquamarine,
                Colors.ForestGreen,
                Colors.BlueViolet,
                Colors.HotPink,
                Colors.IndianRed,
                Colors.DarkSalmon
            };
        }

        #endregion

    }
}