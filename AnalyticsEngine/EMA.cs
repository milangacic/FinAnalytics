using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {

    // EMA: Exponential Moving Average
    // =========================================
    // [EMA (now)] =  [Multiplier] x [Closing price (now)] + (1 - [Multiplier]) x [EMA (previous observation)]
    // [Multiplier] = 2 ÷ ([Number of observations] + 1)
    // [First EMA] = [SMA] = Sum([Closing prices]) / [Number of observations]
    public class EMA : EMABase {

        #region Properties

        public override TimeSpan TimeSpan { get { return Length * vDataProvider.Interval; } }

        #endregion

        #region Public methods

        /// <summary>
        /// EMA: Exponential Moving Average
        /// [EMA (now)] =  [Multiplier] x [Closing price (now)] + (1 - [Multiplier]) x [EMA (previous observation)]
        /// </summary>
        /// <param name="dataprovider">Dataprovide</param>
        /// <param name="length">EMA length in observations (length > 0)</param>

        public EMA(IDataProvider dataprovider, uint length) : base(dataprovider, length) { }

        #endregion

        #region Overrides

        public override string ToString() {
            return "EMA (" + Length + ")";
        }

        #endregion
    }
}
