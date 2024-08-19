using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {
    interface ISimulator {
        IEnumerable<ITradeDecision> GetTradeDecisions(DateTime from, DateTime to, TimeSpan interval);
    }
}
