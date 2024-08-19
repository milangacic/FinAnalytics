using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {
    public class PriceData : IMarketData {

        public PriceData(string symbol, DateTime time, decimal open, decimal close, decimal high, decimal low, decimal volume) {
            vSymbol = symbol;
            vTime = time;
            vOpen = open;
            vClose = close;
            vHigh = high;
            vLow = low;
            vVolume = volume;
        }

        #region Properties        

        string vSymbol;
        public string Symbol { get { return vSymbol; } }

        DateTime vTime;
        public DateTime Time { get { return vTime; } }

        decimal vOpen;
        public decimal Open { get { return vOpen; } }

        decimal vClose;
        public decimal Close { get { return vClose; } }

        decimal vHigh;
        public decimal High { get { return vHigh; } }

        decimal vLow;
        public decimal Low { get { return vLow; } }

        decimal vVolume;
        public decimal Volume { get { return vVolume; } }

        #endregion

        #region Override

        public override string ToString() {
            return string.Format("{0}, {1}, {2}, {3}, {4}", Time, Open, High, Low, Close);
        }

        #endregion
    }
}
