using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TTTApp.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo {        

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string property = null) {
            PropertyChanged?.Invoke(this, new(property));
        }

        protected Dictionary<string, string> Errors = new Dictionary<string, string>();

        public string this[string columnName] {
            get { return Errors[columnName]; }
            set { Errors[columnName] = value; }
        }

        public string Error { get; set; }

        public string ErrorMessage { 
            get {
                StringBuilder msg = new StringBuilder();
                if (Error != null) msg.AppendLine(Error);
                foreach (var item in Errors) msg.AppendLine($"{item.Key}: {item.Value}");
                return msg.ToString();
            }
        }
    }
}
