using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {
    public abstract class EMABase : CachedAnalytics {

        #region Properties

        uint vLength;
        public uint Length { 
            get { return vLength; } 
            set {
                vLength = value;
                ClearCache();
            }
        }

        public abstract TimeSpan TimeSpan { get; }

        #endregion

        #region Public methods

        public EMABase(IDataProvider dataprovider, uint length) : base(dataprovider) {
            if (length < 1) throw new ArgumentException("Length must be at least 1!");
            vLength = length;
        }

        #endregion

        #region Overrides

        protected override IEnumerable<IAnalyticsData> CalculateAnalytics(IEnumerable<IMarketData> marketdata/*, IAnalyticsData lastcached*/) {
            var observations = GetObservations(marketdata.First().Time);
            DateTime firstdate = marketdata.Select(item => item.Time).FirstOrDefault(time => time > observations.Last().Time);
            if (firstdate == default) yield break;
            IAnalyticsData lastcached = GetLastCached().LastOrDefault();
            DateTime lastdate = lastcached != null ? lastcached.Time : DateTime.MinValue;
            decimal? lastema = lastcached != null ? lastcached.Value : null;
            decimal multiplier = GetMultiplier(observations.Count());

            foreach (var item in marketdata) {
                if (item.Time <= lastdate || item.Time < firstdate) {
                    continue;
                } else if (lastema.HasValue) {
                    lastema = multiplier * GetValue(item) + (1 - multiplier) * lastema;
                    yield return new IndicatorData(item.Time, lastema.Value);
                } else {
                    lastema = multiplier * GetValue(item) + (1 - multiplier) * SMA.CalculateSMA(observations, GetValue);
                    yield return new IndicatorData(item.Time, lastema.Value);
                }
            }
        }

        #endregion

        #region Protected methods

        protected virtual IEnumerable<IMarketData> GetObservations(DateTime date) {
            var observations = vDataProvider.GetMarketData(date, (int)-(vLength + 1));
            if (observations.Any() && observations.Count() < vLength + 1) {
                return vDataProvider.GetMarketData(observations.First().Time, (int)vLength);
            }
            return observations.SkipLast(1);
        }

        protected virtual decimal GetValue(IMarketData item) {
            return item.Close;
        }

        protected virtual decimal GetMultiplier(int observations) {
            return 2M / (observations + 1);
        }        

        #endregion
    }
}
