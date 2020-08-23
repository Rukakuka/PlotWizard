
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

using acad = Autodesk.AutoCAD.ApplicationServices.Application;
using Ap = Autodesk.AutoCAD.ApplicationServices;
using Ed = Autodesk.AutoCAD.EditorInput;
using Rt = Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;

using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;


namespace PlotWizard.Ribbon
{
    internal class RibbonCommands : IExtensionApplication
    {
        public const string TargetTabName = "Вывод";
        private const string TargetPanelName = "Печать блоков";

        private RibbonTextBox tbBlockName;
        private RibbonTextBox tbPrefix;
        private RibbonTextBox tbPostfix;

        private RibbonButton btnChooseBlock;
        private RibbonButton btnLayoutSettings;
        private RibbonButton btnCreateLayouts;
        private RibbonButton btnEraseLayouts;
        private RibbonButton btnMultiPlot;

        // Функции Initialize() и Terminate() необходимы, чтобы реализовать интерфейс IExtensionApplication
        public void Initialize() { }
        public void Terminate() { }
        public void AddMyRibbonPanel()
        {
            RibbonTab targetTab = null;
            foreach (var tab in ComponentManager.Ribbon.Tabs)
            {
                if (tab.Title.Equals(TargetTabName))
                {
                    targetTab = tab;
                    foreach (var panel in tab.Panels)
                    {
                        if (panel.Source.Title.Equals(TargetPanelName))
                            break;
                    }
                }
            }

            if (targetTab == null)
            {
                System.Windows.MessageBox.Show("Вкладка уже добавлена");
                return;
            }
            
            Wizard.Layouts = new ObjectIdCollection(); // stores the newly-created layouts

            RibbonLabel labelBlockName = new RibbonLabel
            {
                ToolTip = "Имя блока для печати  ",
                IsToolTipEnabled = true,
                Height = 22,
                ShowImage = true,
                Size = RibbonItemSize.Standard,
                Image = Extensions.GetBitmap(Properties.Resources.icon_22)
            };
            RibbonLabel labelPrefix = new RibbonLabel
            {
                ToolTip = "Имя атрибута - префикс  ",
                IsToolTipEnabled = true,
                ShowImage = true,
                Size = RibbonItemSize.Standard,
                Image = Extensions.GetBitmap(Properties.Resources.icon_20),
                Height = 22
            };
            RibbonLabel labelPostfix = new RibbonLabel
            {
                ToolTip = "Имя атрибута - постфикс  ",
                IsToolTipEnabled = true,
                Height = 22,
                ShowImage = true,
                Size = RibbonItemSize.Standard,
                Image = Extensions.GetBitmap(Properties.Resources.icon_19)
            };
            tbBlockName = new RibbonTextBox
            {
                Id = "tbBlockName",
                ToolTip = "Имя блока для печати",
                IsToolTipEnabled = true,
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new Ribbon.CommandHandlers.GenericTextboxCommandHandler(),
                Width = 100,
                Height = 22,
                Size = RibbonItemSize.Large,
                IsEnabled = false,
                Text = "",
            };
            tbPrefix = new RibbonTextBox
            {
                Id = "tbPrefix",
                ToolTip = "Имя атрибута - префикс  ",
                IsToolTipEnabled = true,
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new Ribbon.CommandHandlers.GenericTextboxCommandHandler(),
                Width = 100,
                Height = 22,
                Size = RibbonItemSize.Large,
                IsEnabled = false,
                Text = ""
            };
            tbPostfix = new RibbonTextBox
            {
                ToolTip = "Имя атрибута - постфикс  ",
                IsToolTipEnabled = true,
                Id = "tbPostfix",
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new Ribbon.CommandHandlers.GenericTextboxCommandHandler(),
                Width = 100,
                Height = 22,
                Size = RibbonItemSize.Large,
                IsEnabled = false,
                Text = ""
            };
            btnChooseBlock = new Autodesk.Windows.RibbonButton
            {
                CommandHandler = new Ribbon.CommandHandlers.ButtonChoosePlotObjCommandHandler(),
                Text = "Выбрать\nобъекты\nпечати",
                ShowText = true,
                LargeImage = Extensions.GetBitmap(Properties.Resources.icon_12),
                Size = RibbonItemSize.Large,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                Width = 65,
                MinWidth = 65
            };
            btnCreateLayouts = new Autodesk.Windows.RibbonButton
            {
                CommandHandler = new Ribbon.CommandHandlers.ButtonCreateLayoutsCommandHandler(),
                Text = "Создать\nлисты",
                ShowText = true,
                LargeImage = Extensions.GetBitmap(Properties.Resources.icon_15),
                Size = RibbonItemSize.Large,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                Width = 65,
                MinWidth = 65,
                
            };
            btnEraseLayouts = new Autodesk.Windows.RibbonButton
            {
                CommandHandler = new Ribbon.CommandHandlers.ButtonEraseLayoutsCommandHandler(),
                Text = "Очистить",
                ShowText = true,
                LargeImage = Extensions.GetBitmap(Properties.Resources.icon_16),
                Size = RibbonItemSize.Large,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                Width = 65,
                MinWidth = 65
            };
            btnMultiPlot = new Autodesk.Windows.RibbonButton
            {
                CommandHandler = new Ribbon.CommandHandlers.ButtonMultiPlotCommandHandler(),
                Text = "Печать",
                ShowText = true,
                LargeImage = Extensions.GetBitmap(Properties.Resources.icon_18),
                Size = RibbonItemSize.Large,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                MinWidth = 65
            };
            btnLayoutSettings = new Autodesk.Windows.RibbonButton
            {
                CommandHandler = new Ribbon.CommandHandlers.ButtonLayoutSettingsCommandHandler(),
                Text = "Настройки\nпечати",
                ShowText = true,
                LargeImage = Extensions.GetBitmap(Properties.Resources.icon_26),
                Size = RibbonItemSize.Large,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                Width = 65,
                MinWidth = 65
            };
            
            Ribbon.LayoutSettings.SetDefaults();

            RibbonRowPanel row1 = new RibbonRowPanel();
            row1.Items.Add(labelBlockName);
            row1.Items.Add(new RibbonRowBreak());
            row1.Items.Add(labelPrefix);
            row1.Items.Add(new RibbonRowBreak());
            row1.Items.Add(labelPostfix);

            RibbonRowPanel row2 = new RibbonRowPanel();
            row2.Items.Add(tbBlockName);
            row2.Items.Add(new RibbonRowBreak());
            row2.Items.Add(tbPrefix);
            row2.Items.Add(new RibbonRowBreak());
            row2.Items.Add(tbPostfix);

            Autodesk.Windows.RibbonPanelSource panelSource = new Autodesk.Windows.RibbonPanelSource()
            {
                Title = "Печать блоков"
            };
            Autodesk.Windows.RibbonPanel plotWizardPanel = new RibbonPanel
            {
                Source = panelSource,
                Id = "plotwizard"
            };
            panelSource.Items.Add(btnChooseBlock);
            panelSource.Items.Add(row1);
            panelSource.Items.Add(row2);
            panelSource.Items.Add(btnCreateLayouts);
            panelSource.Items.Add(btnEraseLayouts);
            panelSource.Items.Add(new RibbonSeparator());
            panelSource.Items.Add(btnLayoutSettings);
            panelSource.Items.Add(new RibbonSeparator());
            panelSource.Items.Add(btnMultiPlot);

            targetTab.Panels.Add(plotWizardPanel);
            targetTab.IsActive = true;
        }
    }
}
