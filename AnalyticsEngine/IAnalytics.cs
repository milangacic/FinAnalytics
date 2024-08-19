using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {

    public interface IAnalytics {
        /// <summary>
        /// Calculates Analytics
        /// </summary>
        /// <param name="from">From Date</param>
        /// <param name="to">To Date</param>
        public IEnumerable<IAnalyticsData> GetAnalyticsData(DateTime from, DateTime to);
        public Task<IEnumerable<IAnalyticsData>> GetAnalyticsDataAsync(DateTime from, DateTime to);

        public event EventHandler DataChanged;
    }
}
