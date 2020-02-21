
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Input;


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
            Autodesk.Windows.RibbonPanel plotWizardTab = new RibbonPanel
            {
                Source = source,
                Id = "plotwizard"
            };
            source.Title = "Печать блоков";

            RibbonLabel labelBlockName = new RibbonLabel
            {
                Text = "Имя блока для печати",
                Width = 150
            };
            RibbonLabel labelAttrLabelName = new RibbonLabel
            {
                Text = "Имя атрибут - чертеж",
                Width = 150
            };
            RibbonLabel labelAttrSheetName = new RibbonLabel
            {
                Text = "Имя атрибута - лист",
                Width = 150
            };

            Autodesk.Windows.RibbonTextBox tbBlockName = new RibbonTextBox
            {
                Id = "tbBlockName",
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new TextboxCommandHandler(),
                Width = 150,
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
                Size = RibbonItemSize.Large
            };

            RibbonRowPanel panelRow1 = new RibbonRowPanel();
            panelRow1.Items.Add(labelBlockName);
            panelRow1.Items.Add(tbBlockName);
            panelRow1.Items.Add(new RibbonRowBreak());
            panelRow1.Items.Add(labelAttrLabelName);
            panelRow1.Items.Add(tbAttrLabelName);
            panelRow1.Items.Add(new RibbonRowBreak());
            panelRow1.Items.Add(labelAttrSheetName);
            panelRow1.Items.Add(tbAttrSheetName);

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

            source.Items.Add(panelRow1);
            foreach (var tab in ribbon.Tabs)
            {
                if (tab.Title.Equals("Вывод"))
                {
                    tab.Panels.Add(plotWizardTab);
                    break;
                }
            }

        }

    }
}
