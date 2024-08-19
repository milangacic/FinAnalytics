using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {

    // Daily EMA: Exponential Moving Average
    // =====================================
    // [EMA (today)] =  [Multiplier] x [Closing price (today)] + (1 - [Multiplier]) x [EMA (previous day)]
    // [Multiplier] = 2 ÷ ([Number of observations] + 1)
    // [First EMA] = [SMA] = Sum([Closing prices]) / [Number of observations]
    public class EMADaily : EMABase {

        #region Properties

        public override TimeSpan TimeSpan { get { return TimeSpan.FromDays(Length); } }

        #endregion

        #region Public methods

        /// <summary>
        /// Daily EMA: Exponential Moving Average
        /// [EMA (today)] =  [Multiplier] x [Closing price (today)] + (1 - [Multiplier]) x [EMA (previous day)]
        /// </summary>
        /// <param name="dataprovider">Dataprovide</param>
        /// <param name="length">EMA length in days (length > 0)</param>
        public EMADaily(IDataProvider dataprovider, uint length) : base(dataprovider, length) { }

        #endregion

        #region Overrides

        protected override IEnumerable<IMarketData> GetObservations(DateTime date) {
            var observations = vDataProvider.GetMarketData(date - TimeSpan, date).SkipLast(1);
            if (observations.Any()) {
                var firstdate = observations.First().Time + TimeSpan;
                if (firstdate > date) {
                    return vDataProvider.GetMarketData(firstdate - TimeSpan, firstdate).SkipLast(1);
                }
            }
            return observations;
        }

        public override string ToString() {
            return "EMA (" + Length + " days)";
        }

        #endregion
    }
}
