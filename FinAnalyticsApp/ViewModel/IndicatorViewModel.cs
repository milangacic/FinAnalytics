using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using TTTApp.Model;
using System.Text;
using System.Threading.Tasks;

namespace TTTApp.ViewModel
{
    public class IndicatorViewModel : ViewModelBase {

        private string vName;
        public string Name { get { return vName; } set { vName = value; OnPropertyChanged(); } }

        private Enumerations.Indicator? vType;
        public Enumerations.Indicator? Type { get { return vType; } set { vType = value; OnPropertyChanged(); } }

        private uint vLength;
        public uint Length { get { return vLength; } set { vLength = value; OnPropertyChanged(); } }

        private List<Tuple<string, Enumerations.Indicator>> vAvailableTypes = new() { 
            Tuple.Create("SMA", Enumerations.Indicator.SMA),
            Tuple.Create("EMA", Enumerations.Indicator.EMA),
            Tuple.Create("RSI", Enumerations.Indicator.RSI),
            Tuple.Create("SMA Diff", Enumerations.Indicator.SMADiff),
            Tuple.Create("EMA Diff", Enumerations.Indicator.EMADiff),
            Tuple.Create("RSI Diff", Enumerations.Indicator.RSIDiff),
            Tuple.Create("SMA Diff2", Enumerations.Indicator.SMADiff2),
            Tuple.Create("EMA Diff2", Enumerations.Indicator.EMADiff2),
            Tuple.Create("RSI Diff2", Enumerations.Indicator.RSIDiff2)
        };
        public List<Tuple<string, Enumerations.Indicator>> AvailableTypes { get { return vAvailableTypes; } set { vAvailableTypes = value; OnPropertyChanged(); } }

        public bool DoValidate() {
            Errors.Clear();
            if (string.IsNullOrWhiteSpace(Name)) {
                Errors[nameof(Name)] = "Name is mandatory";
            }
            if (Type == null) {
                Errors[nameof(Type)] = "Type is mandatory";
            }
            if (Length == default) {
                Errors[nameof(Length)] = "Length is mandatory";
            }
            return !Errors.Any();
        }
    }
}
