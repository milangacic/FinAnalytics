using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnalyticsEngine {

    public abstract class CachedDataProvider : IDataProvider {

        #region Fields

        TimeSpan vInterval;
        List<PriceData> vCache = new List<PriceData>();
        IMarketData vFirst, vLast;
        DateTime vCachedFrom = DateTime.MaxValue;
        DateTime vCachedTo = DateTime.MinValue;
        bool vMoreDataPre = true;
        bool vMoreDataPost = true;

        #endregion

        #region Events

        public event EventHandler DataChanged;
        public event EventHandler IntervalChanged;

        #endregion

        #region Properties

        public TimeSpan Interval {
            get { return vInterval; }
            set {
                vInterval = value;
                ClearCache();
                IntervalChanged?.Invoke(this, null);
            }
        }

        public virtual DateTimeKind TimeKind { get { return DateTimeKind.Unspecified; } }

        #endregion

        #region Public methods

        public CachedDataProvider(TimeSpan interval) {
            vInterval = interval;
        }

        public IEnumerable<IMarketData> GetMarketData(DateTime from, DateTime to) {
            if (from > to) throw new ArgumentException(@"To-Date must be after From-Date");
            if (!IsCached(from, to)) UpdateCache(from, to);
            return vCache.Where(item => item.Time >= from && item.Time <= to).OrderBy(item => item.Time);
        }

        public async Task<IEnumerable<IMarketData>> GetMarketDataAsync(DateTime from, DateTime to) {
            return await Task.Run(() => GetMarketData(from, to));
        }

        public IEnumerable<IMarketData> GetMarketData(DateTime from, int observations) {
            if (!IsCached(from, observations)) UpdateCache(from, observations);
            return observations < 0 ? vCache.Where(item => item.Time <= from).OrderBy(item => item.Time).TakeLast(-observations) : vCache.Where(item => item.Time >= from).OrderBy(item => item.Time).Take(observations); ;
        }

        public async Task<IEnumerable<IMarketData>> GetMarketDataAsync(DateTime from, int observations) {
            return await Task.Run(() => GetMarketData(from, observations));
        }

        public IEnumerable<IMarketData> GetLatestMarketData(int observations) {
            if (!IsCached(observations)) UpdateCache(observations);
            return observations < 0 ? vCache.Take(-observations).OrderBy(item => item.Time) : vCache.TakeLast(observations).OrderBy(item => item.Time); ;
        }

        public async Task<IEnumerable<IMarketData>> GetLatestMarketDataAsync(int observations) {
            return await Task.Run(() => GetLatestMarketData(observations));
        }

        public void Dispose() {
            vCache.Clear();
        }

        #endregion

        #region Protected methods

        protected bool AddToCache(PriceData data) {
            if (data.Time < vCachedFrom || data.Time > vCachedTo) {
                vCache.Add(data);
                return true;
            }
            return false;
        }

        protected void ClearCache() {
            vCache.Clear();
            vCachedFrom = DateTime.MaxValue;
            vCachedTo = DateTime.MinValue;
            vFirst = null;
            vLast = null;
            DataChanged?.Invoke(this, null);
        }

        protected abstract bool ReadData(DateTime from, DateTime to, ref bool moredatapre, ref bool moredatapost);

        protected abstract IMarketData ReadFirst();

        protected abstract IMarketData ReadLast();

        #endregion

        #region Private methods

        private bool IsCached(DateTime from, DateTime to) {
            if (vCache.Count == 0) return false;                        
            return vCachedFrom <= from && vCachedTo >= to;            
        }

        private bool IsCached(DateTime date, int observations) {
            if (vCache.Count < Math.Abs(observations)) return false;
            return vCache.Where(item => observations < 0 ? item.Time <= date : item.Time >= date).Count() >= Math.Abs(observations);
        }

        private bool IsCached(int observations) {
            if (vCache.Count < Math.Abs(observations)) return false;
            if (observations > 0) {
                if (vLast == null) vLast = ReadLast();
                return vCache.Last().Time == vLast.Time;
            } else {
                if (vFirst == null) vFirst = ReadFirst();
                return vCache.First().Time == vFirst.Time;
            }
        }

        private bool UpdateCache(int observations) {
            if (observations > 0) {
                if (vLast == null) vLast = ReadLast();
                return UpdateCache(vLast.Time, -observations);
            } else {
                if (vFirst == null) vFirst = ReadFirst();
                return UpdateCache(vFirst.Time, observations);
            }
        }

        private bool UpdateCache(DateTime date, int observations) {
            int count = observations;
            while (!IsCached(date, observations)) {
                if (count >= 0) {
                    UpdateCache(date, date + vInterval * count);
                } else {
                    UpdateCache(date + vInterval * count, date);
                }
                if (observations >= 0 && !vMoreDataPost && !IsCached(date, observations)) break;
                if (observations < 0 && !vMoreDataPre && !IsCached(date, observations)) break;
                count += Math.Sign(observations);
            }
            return IsCached(date, observations);
        }        

        private bool UpdateCache(DateTime from, DateTime to) {
            if (vCachedFrom > from && vCachedTo >= to) {
                to = vCachedFrom;
            } else if (vCachedFrom <= from && vCachedTo < to) {
                from = vCachedTo;
            }
            bool datachanged = false;
            try {
                datachanged = ReadData(from, to, ref vMoreDataPre, ref vMoreDataPost);
            } catch (Exception) {
                throw;
            } finally {
                if (datachanged) {
                    vCachedFrom = vCache.Min(item => item.Time);
                    vCachedTo = vCache.Max(item => item.Time);
                    DataChanged?.Invoke(this, null);
                }
            }                              
            return datachanged;
        }

        #endregion

    }
}
