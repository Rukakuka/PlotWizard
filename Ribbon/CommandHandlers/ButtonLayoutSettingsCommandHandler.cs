using System;
using System.Collections.Generic;
using Autodesk.Windows;
using Autodesk.AutoCAD.DatabaseServices;

namespace PlotWizard.Ribbon.CommandHandlers
{
    internal class ButtonLayoutSettingsCommandHandler : System.Windows.Input.ICommand
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
           new Ribbon.LayoutSettingsWindow().Show();
        }
    }
}
