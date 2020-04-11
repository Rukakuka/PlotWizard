using System;
using Autodesk.Windows;
using acad = Autodesk.AutoCAD.ApplicationServices.Application;

namespace PlotWizard
{
    internal class ButtonCommandHandler : System.Windows.Input.ICommand
    {

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object param)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            if (parameter is RibbonCommandItem ribbonItem)
            {
                var doc = acad.DocumentManager.MdiActiveDocument;
                //Make sure the command text either ends with ";", or a " "
                string cmdText = ((string)ribbonItem.CommandParameter).Trim();
                if (!cmdText.EndsWith(";"))
                    cmdText += " ";
                doc.SendStringToExecute(cmdText, true, false, true);
            }
        }
    }
}
