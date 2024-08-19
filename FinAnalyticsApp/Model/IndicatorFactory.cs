using AnalyticsEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTApp.Model
{
    public static class IndicatorFactory {

        public static IAnalytics Create(Enumerations.Indicator indicator, IDataProvider dataprovider, uint length) {
            switch (indicator) {
                case Enumerations.Indicator.SMA:
                    return new SMA(dataprovider, length);
                case Enumerations.Indicator.EMA: 
                    return new EMA(dataprovider, length);
                case Enumerations.Indicator.RSI: 
                    return new RSI(dataprovider, length);
                case Enumerations.Indicator.SMADiff:
                    return new Derivative(new SMA(dataprovider, length));
                case Enumerations.Indicator.EMADiff:
                    return new Derivative(new EMA(dataprovider, length));
                case Enumerations.Indicator.RSIDiff:
                    return new Derivative(new RSI(dataprovider, length));
                case Enumerations.Indicator.SMADiff2:
                    return new Derivative(new Derivative(new SMA(dataprovider, length)));
                case Enumerations.Indicator.EMADiff2:
                    return new Derivative(new Derivative(new EMA(dataprovider, length)));
                case Enumerations.Indicator.RSIDiff2:
                    return new Derivative(new Derivative(new RSI(dataprovider, length)));
                default:
                    throw new ArgumentException($"Indicator {indicator} is not supported.");
            }
        }
    }
}
