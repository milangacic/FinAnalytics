using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {
    class UpMovesEMA : EMABase {

        #region Properties

        public override TimeSpan TimeSpan { get { return Length * vDataProvider.Interval; } }

        #endregion

        #region Public methods

        public UpMovesEMA(IDataProvider dataprovider, uint length) : base(dataprovider, length) { }

        #endregion

        #region Protected methods

        protected override decimal GetValue(IMarketData item) {
            var move = item.Close - item.Open;
            return move > 0 ? move : 0;
        }

        protected override decimal GetMultiplier(int observations) {
            return 1M / observations;
        }

        #endregion
    }
}
