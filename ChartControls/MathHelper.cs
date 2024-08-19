using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ChartControls {
    internal class MathHelper {

        internal static T Max<T>(IEnumerable<T> list) where T : IComparable {
            T max = default;
            bool first = true;
            foreach (T item in list) {
                if (first || max.CompareTo(item) < 0) max = item;
                first = false;
            }
            return max;
        }

        internal static T Max<T>(IEnumerable<IEnumerable<T>> list) where T : IComparable {
            T max = default;
            bool first = true;
            T maxvalue;
            foreach (IEnumerable<T> item in list) {
                if (!item.Any()) continue;
                maxvalue = Max(item);
                if (first || max.CompareTo(maxvalue) < 0) max = maxvalue;
                first = false;
            }
            return max;
        }

        internal static T Max<T>(IEnumerable<IEnumerable<IEnumerable<T>>> list) where T : IComparable {
            T max = default;
            bool first = true;
            T maxvalue;
            foreach (IEnumerable<IEnumerable<T>> item in list) {
                if (!item.Any()) continue;
                maxvalue = Max(item);
                if (first || max.CompareTo(maxvalue) < 0) max = maxvalue;
                first = false;
            }
            return max;
        }

        internal static T Min<T>(IEnumerable<T> list) where T : IComparable {
            T min = default;
            bool first = true;
            foreach (T item in list) {
                if (first || min.CompareTo(item) > 0) min = item;
                first = false;
            }
            return min;
        }

        internal static T Min<T>(IEnumerable<IEnumerable<T>> list) where T : IComparable {
            T min = default;
            bool first = true;
            T minvalue;
            foreach (IEnumerable<T> item in list) {
                if (!item.Any()) continue;
                minvalue = Min(item);
                if (first || min.CompareTo(minvalue) > 0) min = minvalue;
                first = false;
            }
            return min;
        }

        internal static T Min<T>(IEnumerable<IEnumerable<IEnumerable<T>>> list) where T : IComparable {
            T min = default;
            bool first = true;
            T minvalue;
            foreach (IEnumerable<IEnumerable<T>> item in list) {
                if (!item.Any()) continue;
                minvalue = Min(item);
                if (first || min.CompareTo(minvalue) > 0) min = minvalue;
                first = false;
            }
            return min;
        }

        internal static T Average<T>(IEnumerable<T> list) {
            dynamic sum = list.FirstOrDefault();
            foreach (dynamic item in list.Skip(1)) {                
                sum += item;
            }
            return sum / list.Count();
        }

        internal static DateTime MaxDate(DateTime date1, DateTime date2) { 
            return date1 > date2 ? date1 : date2;
        }

        internal static DateTime MinDate(DateTime date1, DateTime date2) {
            return date1 < date2 ? date1 : date2;
        }
    }
}
