using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {
    public static class MathHelper {

        public static decimal Differentiate(Tuple<decimal, decimal> p0, Tuple<decimal, decimal> p1) {
            return (p1.Item2 - p0.Item2) / (p1.Item1  - p0.Item1);
        }

        public static Tuple<decimal, decimal> LinearReg(IEnumerable<Tuple<decimal, decimal>> data) {
            int n = data.Count();
            decimal m = (n * data.Sum(item => item.Item1 * item.Item2) - data.Sum(item => item.Item1) * data.Sum(item => item.Item2)) / (n * data.Sum(item => item.Item1 * item.Item1) - data.Sum(item => item.Item1) * data.Sum(item => item.Item1));
            decimal r = (data.Sum(item => item.Item2) - m * data.Sum(item => item.Item1)) / n;
            return Tuple.Create(m, r);
        }

    }
}
