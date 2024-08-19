using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {    
    public abstract class RSIBase : CachedAnalytics {

        #region Fields
        
        private EMABase vUpMoves;
        private EMABase vDownMoves;

        #endregion

        #region Properties

        uint vLength;
        public uint Length {
            get { return vLength; }
            set {
                vLength = value;
                vUpMoves.Length = value;
                vDownMoves.Length = value;
                ClearCache();
            }
        }

        public TimeSpan TimeSpan { get { return vLength * vDataProvider.Interval; } }

        #endregion

        #region Public methods

        public RSIBase(IDataProvider dataprovider, uint length) : base(dataprovider) {
            if (length < 1) throw new ArgumentException("Length must be at least 1!");
            vLength = length;
            vUpMoves = CreateUpAverage(dataprovider, vLength);
            vDownMoves = CreateDownAverage(dataprovider, vLength);
        }

        public new void Dispose() {            
            vUpMoves.Dispose();
            vDownMoves.Dispose();
            base.Dispose();
        }

        #endregion

        #region Overrides

        protected override IEnumerable<IAnalyticsData> CalculateAnalytics(IEnumerable<IMarketData> marketdata) {
            if (!marketdata.Any()) yield break;
            var upmoves = vUpMoves.GetAnalyticsData(marketdata.First().Time, marketdata.Last().Time);
            var downmoves = vDownMoves.GetAnalyticsData(marketdata.First().Time, marketdata.Last().Time);
            if (!upmoves.Any() || !downmoves.Any()) yield break;
            if (upmoves.First().Time != downmoves.First().Time || upmoves.Count() != downmoves.Count() || upmoves.Last().Time != downmoves.Last().Time) {
                throw new ApplicationException(@"Up-Moves and Down-Moves not aligned!");
            }
            var firstdate = upmoves.First().Time;
            var data = marketdata.GetEnumerator();
            var upmove = upmoves.GetEnumerator();
            var downmove = downmoves.GetEnumerator();
            while (data.MoveNext()) {
                if (data.Current.Time >= firstdate) {
                    upmove.MoveNext();
                    downmove.MoveNext();
                    if (upmove.Current == null || downmove.Current == null) yield break;
                    yield return new IndicatorData(data.Current.Time, 100M - ((downmove.Current.Value != 0) ? 100M / (1 + upmove.Current.Value / downmove.Current.Value) : 0));
                }
            }
        }

        public override string ToString() {
            return "RSI (" + Length + ")";
        }

        #endregion

        #region Protected methods

        protected abstract EMABase CreateUpAverage(IDataProvider dataprovider, uint length);
        protected abstract EMABase CreateDownAverage(IDataProvider dataprovider, uint length);

        #endregion
    }
}
