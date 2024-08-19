using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {
    public abstract class CachedAnalyticsBase : IAnalytics, IDisposable {

        #region Fields

        protected List<IAnalyticsData> vCache;
        protected DateTime vCacheFrom;
        protected DateTime vCacheTo;

        #endregion

        #region Events

        public event EventHandler DataChanged;

        #endregion

        #region Public methods

        public CachedAnalyticsBase() {
            vCache = new List<IAnalyticsData>();
            vCacheFrom = DateTime.MaxValue;
            vCacheTo = DateTime.MinValue;
        }

        /// <summary>
        /// Returns analytics for a certain time range
        /// </summary>
        /// <param name="from">From Date</param>
        /// <param name="to">To Date</param>
        public IEnumerable<IAnalyticsData> GetAnalyticsData(DateTime from, DateTime to) {
            if (from > to) throw new ArgumentException(@"To-Date must be after From-Date");
            if (!IsCached(from, to)) UpdateCache(from, to);
            return vCache.Where(item => item.Time >= from && item.Time <= to);
        }

        public async Task<IEnumerable<IAnalyticsData>> GetAnalyticsDataAsync(DateTime from, DateTime to) {
            return await Task.Run(() => GetAnalyticsData(from,to));
        }

        public void Dispose() {
            vCache.Clear();
        }

        #endregion

        #region Private methods        

        private bool IsCached(DateTime from, DateTime to) {
            if (vCache.Count == 0) return false;
            return from >= vCacheFrom && to <= vCacheTo;
        }

        protected virtual bool UpdateCache(DateTime from, DateTime to) { 
            return true;
        }

        #endregion

        #region Protected methods

        protected IEnumerable<IAnalyticsData> GetLastCached(int n = 0) {
            if (n == 0) n = 1;
            return vCache.TakeLast(n);
        }

        protected void ClearCache() {
            vCache.Clear();
            vCacheFrom = DateTime.MaxValue;
            vCacheTo = DateTime.MinValue;
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
