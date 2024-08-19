using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {
    class IndicatorData : IAnalyticsData {

        public IndicatorData(DateTime time, decimal value) {
            vTime = time;
            vValue = value;
        }

        private DateTime vTime;
        public DateTime Time { get { return vTime; } }

        private decimal vValue;
        public decimal Value { get { return vValue; } }

        public override bool Equals(object obj) {
            var data = obj as IndicatorData;
            if (data == null) return false;
            return data.Time == Time && data.Value == Value;
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 23 + Time.GetHashCode();
            hash = hash * 23 + Value.GetHashCode();
            return hash;
        }

        public override string ToString() {
            return string.Format("{0}: {1}", Time, Value);
        }
    }
}
