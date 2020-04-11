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
                switch (tb.Id)
                {
                    case "tbBlockName":
                        RibbonCommands.BlockName = tb.TextValue;
                        break;
                    case "tbAttrLabel":
                        RibbonCommands.AttrLabelName = tb.TextValue;
                        break;
                    case "tbAttrSheet":
                        RibbonCommands.AttrSheetName = tb.TextValue;
                        break;
                    case "tbViewportScaling":
                        try
                        {
                            double sc = double.Parse(tb.TextValue, CultureInfo.InvariantCulture);
                            sc = Extensions.Clamp(sc, 0, 1);
                            tb.TextValue = sc.ToString();
                            RibbonCommands.ViewportScaling = sc;
                            Wizard.MyViewportScaling = RibbonCommands.ViewportScaling;
                        }
                        catch (System.Exception) //Fromat, Argument, Overflow exceptions of Int32.Parse
                        {
                            tb.TextValue = RibbonCommands.ViewportScaling.ToString();
                        }
                        break;
                    case "tbContentScaling":
                        try
                        {
                            double sc = double.Parse(tb.TextValue, CultureInfo.InvariantCulture);
                            sc = Extensions.Clamp(sc, 0, (double)Int32.MaxValue);
                            tb.TextValue = sc.ToString();
                            RibbonCommands.ContentScaling = sc;
                            Wizard.MyContentScaling = RibbonCommands.ContentScaling;
                        }
                        catch (System.Exception)
                        {
                            tb.TextValue = RibbonCommands.ContentScaling.ToString();
                        }
                        break;
                }
            }
        }
    }
}
