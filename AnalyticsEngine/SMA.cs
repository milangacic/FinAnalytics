using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {

    // SMA: Standard Moving Average
    // =========================================
    // [SMA] =  Sum([Closing prices]) / [Number of observations]    
    public class SMA : CachedAnalytics {

        #region Properties

        uint vLength;
        public uint Length { 
            get { return vLength; } 
            set {
                vLength = value;
                ClearCache();
            }
        }

        public TimeSpan TimeSpan { get { return vLength * vDataProvider.Interval; } }

        #endregion

        #region Static methods

        public static decimal CalculateSMA(IEnumerable<IMarketData> data, Func<IMarketData, decimal> getvalue) {
            return data.Sum(item => getvalue(item)) / data.Count();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// SMA: Standard Moving Average
        /// [SMA (now)] = Sum([Closing prices]) / [Number of observations] 
        /// </summary>
        /// <param name="dataprovider">Dataprovide</param>
        /// <param name="length">SMA length in observations (length > 0)</param>
        public SMA(IDataProvider dataprovider, uint length) : base(dataprovider) {
            if (length < 1) throw new ArgumentException("Length must be at least 1!");            
            vLength = length;
        }

        #endregion

        #region Overrides

        protected override IEnumerable<IAnalyticsData> CalculateAnalytics(IEnumerable<IMarketData> marketdata) {
            var observations = GetObservations(marketdata.First().Time);
            DateTime firstdate = observations.Last().Time;
            if (firstdate == default) yield break;
            Queue<IMarketData> observationqueue = new Queue<IMarketData>(observations);
            decimal? sum = null;
            IAnalyticsData lastcached = GetLastCached().LastOrDefault();
            DateTime lastdate = lastcached != null ? lastcached.Time : DateTime.MinValue;
            foreach (var item in marketdata) {
                if (item.Time <= lastdate || item.Time < firstdate) continue;
                if (sum.HasValue) {
                    sum = sum - observationqueue.Dequeue().Close + item.Close;
                    observationqueue.Enqueue(item);
                } else {
                    sum = observationqueue.Sum(item => item.Close);
                }
                yield return new IndicatorData(item.Time, sum.Value / vLength);
            }
        }

        public override string ToString() {
            return "SMA (" + Length + ")";
        }

        #endregion

        #region Private methods

        private IEnumerable<IMarketData> GetObservations(DateTime date) {
            var observations = vDataProvider.GetMarketData(date, (int)-vLength);          
            if (observations.Any() && observations.Count() < vLength) {
                return vDataProvider.GetMarketData(observations.First().Time, (int)vLength);
            }
            return observations;
        }

        #endregion
    }
}
