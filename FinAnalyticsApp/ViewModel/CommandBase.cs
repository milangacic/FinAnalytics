using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TTTApp.ViewModel {
    public class CommandBase : ICommand {        

        private ExecuteDelegate vExecute;
        private CanExecuteDelegate vCanExecute;

        public delegate Task ExecuteDelegate();
        public delegate bool CanExecuteDelegate();

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public CommandBase(ExecuteDelegate execute, CanExecuteDelegate canexecute) { 
            vExecute = execute;
            vCanExecute = canexecute;
        }

        public bool CanExecute(object parameter) {
            if (vCanExecute == null) return true;
            return vCanExecute();
        }

        public async void Execute(object parameter) {
            if (vExecute == null) return;
            await vExecute();
        }
    }
}
