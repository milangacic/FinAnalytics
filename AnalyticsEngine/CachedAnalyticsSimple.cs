using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {
    public abstract class CachedAnalyticsSimple : CachedAnalyticsBase {

        #region Public methods

        public CachedAnalyticsSimple() : base() { }

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
            bool append = vCache.Any();
            vCache.AddRange(CalculateAnalytics(from, to).Where(item => (append ? item.Time > vCacheTo : item.Time >= from) && item.Time <= to));
            vCacheFrom = vCache.Any() ? vCache.Min(item => item.Time) : DateTime.MaxValue;
            vCacheTo = vCache.Any() ? vCache.Max(item => item.Time) : DateTime.MinValue;
            return true;
        }

        #endregion

        #region Protected methods

        protected abstract IEnumerable<IAnalyticsData> CalculateAnalytics(DateTime from, DateTime to);

        #endregion
    }
}
