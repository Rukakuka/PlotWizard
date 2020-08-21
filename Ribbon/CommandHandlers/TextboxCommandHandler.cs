using System;
using System.Globalization;

namespace PlotWizard.Ribbon.CommandHandlers
{
    internal class TextboxCommandHandler : System.Windows.Input.ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            if (parameter is Autodesk.Windows.RibbonTextBox tb)
            {
                return;
            }
        }
    }
}
