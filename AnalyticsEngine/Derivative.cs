using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {

    // First derivative of an analytics
    // =========================================
    // dAnalytics/dt
    public class Derivative : CachedAnalyticsSimple {

        #region Fields        

        private IAnalytics vAnalytics;
        private decimal vValueScale;
        private TimeSpan vTimeScale;
        private int vLinReg;

        #endregion

        #region Public methods        

        /// <summary>
        /// First derivative of an analytics
        /// dAnalytics / dt = (Analytics(t1) - Analytics(t0)) / (t1 - t0)
        /// </summary>
        /// <param name="analytics">Analytics</param>
        public Derivative(IAnalytics analytics) : this(analytics, 1, TimeSpan.FromMilliseconds(1), 0) { }

        /// <summary>
        /// First derivative of an analytics
        /// dAnalytics / dt = Slope from linear regression
        /// </summary>
        /// <param name="analytics">Analytics</param>
        /// <param name="linreg">Number of points for linear regression</param>
        public Derivative(IAnalytics analytics, uint linreg) : this(analytics, 1, TimeSpan.FromMilliseconds(1), linreg) { }

        /// <summary>
        /// First derivative of an analytics (scaled)
        /// dAnalytics / dt = ((Analytics(t1) - Analytics(t0)) * dt) / ((t1 - t0) * dy)
        /// </summary>
        /// <param name="analytics">Analytics</param>
        /// <param name="dy">Value scaling</param>
        /// <param name="dt">Time scaling</param>
        public Derivative(IAnalytics analytics, decimal dy, TimeSpan dt) : this(analytics, dy, dt, 0) { }

        /// <summary>
        /// First derivative of an analytics (scaled)
        /// dAnalytics / dt = Scaled slope from linear regression
        /// </summary>
        /// <param name="analytics">Analytics</param>
        /// <param name="dy">Value scaling</param>
        /// <param name="dt">Time scaling</param>
        /// <param name="linreg">Number of points for linear regression </param>
        public Derivative(IAnalytics analytics, decimal dy, TimeSpan dt, uint linreg) {
            vAnalytics = analytics;
            vValueScale = dy;
            vTimeScale = dt;
            vLinReg = (int)linreg;
            vAnalytics.DataChanged += OnDataChanged;
        }

        #endregion

        #region Private Methods

        private void OnDataChanged(object sender, EventArgs e) {
            ClearCache();
        }

        #endregion

        #region Overrides        

        protected override IEnumerable<IAnalyticsData> CalculateAnalytics(DateTime from, DateTime to) {
            Queue<IAnalyticsData> prevdata = new(GetLastCached(vLinReg-1));
            foreach (var data in vAnalytics.GetAnalyticsData(from, to)) {
                if (prevdata == null) {
                    prevdata = new Queue<IAnalyticsData>();
                }
                if (prevdata.Count == 0) {
                    prevdata.Enqueue(data);
                    yield return new IndicatorData(data.Time, 0);
                    continue;
                }
                decimal diff;                
                if (vLinReg <= 1) {
                    IAnalyticsData data0 = prevdata.Dequeue();
                    diff = MathHelper.Differentiate(Tuple.Create(0M, data0.Value / vValueScale), Tuple.Create((decimal)((data.Time - data0.Time) / vTimeScale), data.Value / vValueScale));
                    prevdata.Enqueue(data);
                } else {
                    prevdata.Enqueue(data);
                    IAnalyticsData data0 = prevdata.First();
                    var datapoints = prevdata.Select(item => Tuple.Create((decimal)((item.Time - data0.Time) / vTimeScale), item.Value / vValueScale));
                    diff = MathHelper.LinearReg(datapoints).Item1;
                    if (prevdata.Count >= vLinReg) prevdata.Dequeue();                    
                }                
                yield return new IndicatorData(data.Time, diff);
            }
        }

        public override string ToString() {
            return vAnalytics.ToString() + " Diff";
        }

        #endregion
    }
}
