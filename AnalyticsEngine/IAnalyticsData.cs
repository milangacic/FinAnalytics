using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {
    public interface IAnalyticsData {
        public DateTime Time { get; }
        public decimal Value { get; }
    }
}
