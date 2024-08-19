using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {
    interface ITradeDecision {
        public DateTime Time { get; }
        public TradeDecisionType Decision { get; }
        public decimal Amount { get; }
    }

    enum TradeDecisionType {
        Wait,
        Sell,
        Buy
    }
}
