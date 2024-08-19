using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {
    public abstract class CachedAnalytics : CachedAnalyticsBase {

        #region Fields

        protected IDataProvider vDataProvider;

        #endregion

        #region Public methods

        public CachedAnalytics(IDataProvider dataprovider) : base() {
            if (dataprovider == null) throw new ArgumentException("Dataprovide must not be null!");
            vDataProvider = dataprovider;
            vDataProvider.IntervalChanged += OnIntervalChanged;
        }

        #endregion

        #region Private methods        

        protected override bool UpdateCache(DateTime from, DateTime to) {
            if (vCache.Any()) {
                if (vCacheFrom <= from && vCacheTo < to) {
                    from = vCacheTo;
                } else {
                    ClearCache();
                }
            }
            var marketdata = vDataProvider.GetMarketData(from, to);
            if (!marketdata.Any()) return false;
            from = marketdata.First().Time <= from ? from : marketdata.First().Time;
            bool append = vCache.Any();
            vCache.AddRange(CalculateAnalytics(marketdata.Where(data => data.Time >= from)).Where(item => (append ? item.Time > vCacheTo : item.Time >= from) && item.Time <= to));
            vCacheFrom = vCache.Any() ? vCache.Min(item => item.Time) : DateTime.MaxValue;
            vCacheTo = vCache.Any() ? vCache.Max(item => item.Time) : DateTime.MinValue;
            return true;
        }

        private void OnIntervalChanged(object sender, EventArgs e) {
            ClearCache();
        }

        #endregion

        #region Protected methods

        protected abstract IEnumerable<IAnalyticsData> CalculateAnalytics(IEnumerable<IMarketData> marketdata);

        #endregion
    }
}
