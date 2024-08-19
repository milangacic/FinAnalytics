using AnalyticsEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using TTTApp.View;
using System.Windows;
using System.Windows.Threading;

namespace TTTApp.ViewModel
{
    public class DatasetViewModel : ViewModelBase
    {
        private string vName;
        public string Name { get { return vName; } set { vName = value; OnPropertyChanged(); } }

        private Uri vSource;
        public Uri Source { get { return vSource; } set { vSource = value; OnPropertyChanged(); } }

        public static List<Tuple<string, Uri>> SourceList = new() {
            Tuple.Create("ETH/USD 2021", new Uri($@"File://{Environment.CurrentDirectory}/Data/gemini_ETHUSD_2021_1min.csv"))
        };

        public List<Tuple<string, Uri>> AvailableSources { get { return SourceList; } }

        private TimeSpan vInterval;
        public TimeSpan Interval { get { return vInterval; } set { vInterval = value; OnPropertyChanged(); } }

        private List<Tuple<string, TimeSpan>> vAvailableIntervals = new() {
            Tuple.Create("1 min", TimeSpan.FromMinutes(1)),
            Tuple.Create("5 min", TimeSpan.FromMinutes(5)),
            Tuple.Create("15 min", TimeSpan.FromMinutes(15)),
            Tuple.Create("30 min", TimeSpan.FromMinutes(30)),
            Tuple.Create("1 h", TimeSpan.FromHours(1)),
            Tuple.Create("2 h", TimeSpan.FromHours(2)),
            Tuple.Create("4 h", TimeSpan.FromHours(4)),
            Tuple.Create("8 h", TimeSpan.FromHours(8)),
            Tuple.Create("16 h", TimeSpan.FromHours(16)),
            Tuple.Create("1D", TimeSpan.FromDays(1))
        };
        public List<Tuple<string, TimeSpan>> AvailableIntervals { get { return vAvailableIntervals; } set { vAvailableIntervals = value; OnPropertyChanged(); } }

        private ObservableCollection<Tuple<string, IndicatorViewModel>> vIndicators = new();
        public ObservableCollection<Tuple<string, IndicatorViewModel>> Indicators { get { return vIndicators; } set { vIndicators = value; OnPropertyChanged(); } }

        private Tuple<string, IndicatorViewModel> vSelectedIndicator;
        public Tuple<string, IndicatorViewModel> SelectedIndicator { get { return vSelectedIndicator; } set { vSelectedIndicator = value; OnPropertyChanged(); OnPropertyChanged(nameof(RemoveIndicatorCommand)); CommandManager.InvalidateRequerySuggested(); } }

        private CommandBase vAddIndicatorCommand;
        public CommandBase AddIndicatorCommand { get { return vAddIndicatorCommand; } }

        private CommandBase vRemoveIndicatorCommand;
        public CommandBase RemoveIndicatorCommand { get { return vRemoveIndicatorCommand; } }

        public DatasetViewModel() {
            vAddIndicatorCommand = new(DoAddIndicator, CanExecuteAddIndicator);
            vRemoveIndicatorCommand = new(DoRemoveIndicator, CanExecuteRemoveIndicator);
        }

        public bool DoValidate() {
            Errors.Clear();
            if (string.IsNullOrWhiteSpace(Name)) {
                Errors[nameof(Name)] = "Name is mandatory";
            }
            if (Source == null) {
                Errors[nameof(Source)] = "Source is mandatory";
            }
            if (Interval == default) {
                Errors[nameof(Interval)] = "Interval is mandatory";
            }
            return !Errors.Any();
        }

        private async Task DoAddIndicator() {                        
            IndicatorViewModel indicatordata = new IndicatorViewModel();
            IndicatorView indicatorview = new IndicatorView(indicatordata);
            bool? result = indicatorview.ShowDialog();
            if (result != null && result.Value) {
                Indicators.Add(Tuple.Create(indicatordata.Name, indicatordata));
            }
        }

        private bool CanExecuteAddIndicator() {
            return true;
        }

        private async Task DoRemoveIndicator() {
            Indicators.Remove(SelectedIndicator);
        }

        private bool CanExecuteRemoveIndicator() {
            return SelectedIndicator != null;
        }
    }
}
