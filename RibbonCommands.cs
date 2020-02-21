
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using WApplication = System.Windows.Forms;
using System.Collections.Specialized;
using System.Threading;
using System.Diagnostics;
using acad = Autodesk.AutoCAD.ApplicationServices.Application;
using Ap = Autodesk.AutoCAD.ApplicationServices;
using Db = Autodesk.AutoCAD.DatabaseServices;
using Ed = Autodesk.AutoCAD.EditorInput;
using Rt = Autodesk.AutoCAD.Runtime;
using Gm = Autodesk.AutoCAD.Geometry;
using Wn = Autodesk.AutoCAD.Windows;
using Hs = Autodesk.AutoCAD.DatabaseServices.HostApplicationServices;
using Us = Autodesk.AutoCAD.DatabaseServices.SymbolUtilityServices;
using Br = Autodesk.AutoCAD.BoundaryRepresentation;
using Pt = Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.PlottingServices;


namespace PrintWizard
{
    public static class Images
    {
        public static BitmapImage GetBitmap(Bitmap image)
        {
            MemoryStream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = stream;
            bmp.EndInit();
            return bmp;
        }
    }
    public class TextboxCommandHandler : ICommand
    {
    #pragma warning disable 67
        public event EventHandler CanExecuteChanged;
    #pragma warning restore 67

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            Autodesk.Windows.RibbonTextBox tb = parameter as Autodesk.Windows.RibbonTextBox;
            if (tb != null)
            {
                switch (tb.Name)
                {
                    case "tbBlockName":
                        RibbonCommands.blockName = tb.TextValue;
                        break;
                    case "tbAttrLabelName":
                        RibbonCommands.attrLabelName = tb.TextValue;
                        break;
                    case "tbAttrSheetName":
                        RibbonCommands.attrSheetName = tb.TextValue;
                        break;
                }
            }
        }
    }
    public class RibbonCommands : IExtensionApplication
    {
        public static string blockName;
        public static string attrLabelName;
        public static string attrSheetName;

        // Функции Initialize() и Terminate() необходимы, чтобы реализовать интерфейс IExtensionApplication
        public void Initialize() { }
        public void Terminate() { }
       
        public void AddMyRibbonPanel()
        {
            Autodesk.Windows.RibbonPanelSource source = new Autodesk.Windows.RibbonPanelSource();
            Autodesk.Windows.RibbonControl ribbon = ComponentManager.Ribbon;
            Autodesk.Windows.RibbonPanel plotWizardPanel = new RibbonPanel
            {
                Source = source,
                Id = "plotwizard"
            };
            source.Title = "Печать блоков";

            RibbonLabel labelBlockName = new RibbonLabel
            {
                Text = "Имя блока для печати  ",
                Height = 22,
            };
            RibbonLabel labelAttrLabelName = new RibbonLabel
            {
                Text = "Имя атрибут - чертеж  ",
                Height = 22,
            };
            
            RibbonLabel labelAttrSheetName = new RibbonLabel
            {
                Text = "Имя атрибута - лист  ",
                Height = 22,
            };

            Autodesk.Windows.RibbonTextBox tbBlockName = new RibbonTextBox
            {
                Id = "tbBlockName",
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new TextboxCommandHandler(),
                Width = 150,
                Height = 22,
                Size = RibbonItemSize.Large
            };
            Autodesk.Windows.RibbonTextBox tbAttrLabelName = new RibbonTextBox
            {
                Id = "tbAttrLabelName",
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new TextboxCommandHandler(),
                Width = 150,
                Height = 22,
                Size = RibbonItemSize.Large
            };
            Autodesk.Windows.RibbonTextBox tbAttrSheetName = new RibbonTextBox
            {
                Id = "tbAttrSheetName",
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new TextboxCommandHandler(),
                Width = 150,
                Height = 22,
                Size = RibbonItemSize.Large
            };

            RibbonRowPanel row1 = new RibbonRowPanel();
            row1.Items.Add(labelBlockName);
            row1.Items.Add(new RibbonRowBreak());
            row1.Items.Add(labelAttrLabelName);
            row1.Items.Add(new RibbonRowBreak());
            row1.Items.Add(labelAttrSheetName);
            source.Items.Add(row1);
            //source.Items.Add(new RibbonPanelBreak());
            RibbonRowPanel row2 = new RibbonRowPanel();
            row2.Items.Add(tbBlockName);
            row2.Items.Add(new RibbonRowBreak());
            row2.Items.Add(tbAttrLabelName);
            row2.Items.Add(new RibbonRowBreak());
            row2.Items.Add(tbAttrSheetName);
            source.Items.Add(row2);

            // кнопки 
            // -удалить листы
            // -добавить листы
            // -печатнуть в файл

            // текстбоксы 
            //-имя блока для создания листов
            // -имя атрибута для именования

            // комбобоксы 
            // - тип принтера
            // - формат листа



            foreach (var tab in ribbon.Tabs)
            {
                if (tab.Title.Equals("Вывод"))
                {
                    tab.Panels.Add(plotWizardPanel);
                    tab.IsActive = true;
                    break;
                }
            }

        }

    }
}
