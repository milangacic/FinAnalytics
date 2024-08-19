using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {

    // RSI: Relative Strength Index (RSI)
    // =========================================
    // [RSI] =  100 - 100/(1 + [RS])
    // [RS] = [Average Ups] / [Average Dows]
    // [Average Ups/Downs] = EMA([Up Moves / Down Moves], [Number of observations])
    public class RSI : RSIBase {

        #region Public methods

        /// <summary>
        /// RSI: Relative Strength Index (RSI)
        /// [RSI] =  100 - 100/(1 + [RS])
        /// [RS] = [Ups Moves EMA] / [Down Moves EMA]
        /// </summary>
        /// /// <param name="dataprovider">Dataprovide</param>
        /// <param name="length">Length in observations (length > 0)</param>
        public RSI(IDataProvider dataprovider, uint length) : base(dataprovider, length) { }

        #endregion

        #region Protected methods

        protected override EMABase CreateDownAverage(IDataProvider dataprovider, uint length) {
            return new DownMovesEMA(dataprovider, length);
        }

        protected override EMABase CreateUpAverage(IDataProvider dataprovider, uint length) {
            return new UpMovesEMA(dataprovider, length);
        }

        #endregion
    }
}
