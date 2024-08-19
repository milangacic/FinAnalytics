using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {
    interface IStrategy {
        public ITradeDecision GetTradeDecision(DateTime time);
    }
}
