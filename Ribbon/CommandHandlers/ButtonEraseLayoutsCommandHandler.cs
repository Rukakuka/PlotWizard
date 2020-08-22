using System;
using Autodesk.Windows;


namespace PlotWizard.Ribbon.CommandHandlers
{
    class ButtonEraseLayoutsCommandHandler : System.Windows.Input.ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object param)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            if (!(parameter is RibbonCommandItem))
                throw new TypeAccessException();
            Wizard.EraseAllLayouts();
        }
    }
}