using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnalyticsEngine {
    public interface IDataProvider : IDisposable {

        public TimeSpan Interval { get; }
        public DateTimeKind TimeKind { get; }
        public IEnumerable<IMarketData> GetMarketData(DateTime from, DateTime to);
        public IEnumerable<IMarketData> GetMarketData(DateTime from, int observations);
        public IEnumerable<IMarketData> GetLatestMarketData(int observations);
        public Task<IEnumerable<IMarketData>> GetMarketDataAsync(DateTime from, DateTime to);
        public Task<IEnumerable<IMarketData>> GetMarketDataAsync(DateTime from, int observations);
        public Task<IEnumerable<IMarketData>> GetLatestMarketDataAsync(int observations);

        public event EventHandler DataChanged;
        public event EventHandler IntervalChanged;
    }
}
