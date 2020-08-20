
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;

namespace PlotWizard
{
    internal class RibbonCommands : IExtensionApplication
    {
        public  const string TargetTabName = "Вывод";
        private const string TargetPanelName = "Печать блоков";
        public static string BlockName { get; set; }
        public static string Prefix { get; set; }
        public static string Postfix { get; set; }
        public static double ViewportScaling { get; set; }
        public static double ContentScaling { get; set; }

        private RibbonTextBox tbViewportScaling; // <--------------
        private RibbonTextBox tbContentScaling;  // <--------------

        private RibbonTextBox tbBlockName;
        private RibbonTextBox tbPrefix;
        private RibbonTextBox tbPostfix;

        private RibbonCombo comboPlotterType;
        private RibbonCombo comboSheetSize;

        private RibbonButton btnChooseBlock;
        private RibbonButton btnCreateLayouts;
        private RibbonButton btnEraseLayouts;
        private RibbonButton btnMultiPlot;

        // Функции Initialize() и Terminate() необходимы, чтобы реализовать интерфейс IExtensionApplication
        public void Initialize() { }
        public void Terminate() { }
        private void ComboPlotterType_SelectedIndexChanged(object o, RibbonPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                Autodesk.AutoCAD.PlottingServices.PlotConfigManager.SetCurrentConfig((args.NewValue as RibbonButton).Text);

                Wizard.Plotter = (args.NewValue as RibbonButton).Text;

                comboSheetSize.Items.Clear();

                bool select = true;
                foreach (var sheetSize in Extensions.GetMediaNameList())
                {
                    Autodesk.Windows.RibbonButton btn = new Autodesk.Windows.RibbonButton
                    {
                        Text = sheetSize.Key.ToString(),
                        ShowText = true
                    };
                    comboSheetSize.Items.Add(btn);
                    
                    // select first sheetSize in list
                    if (select)
                    {
                        select = false;
                        comboSheetSize.Current = btn;
                    }
                }
            }
        }
        private void ComboSheetSize_SelectedIndexChanged(object o, RibbonPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                Wizard.PageSize = Extensions.GetMediaNameList()[(args.NewValue as RibbonButton).Text];
            }
        }        
        public void AddMyRibbonPanel()
        {
            Autodesk.Windows.RibbonControl ribbon = ComponentManager.Ribbon;
            foreach (var tab in ribbon.Tabs)
            {
                if (tab.Title.Equals(TargetTabName))
                {
                    foreach (var panel in tab.Panels)
                    {
                        if (panel.Source.Title.Equals(TargetPanelName))
                        {
                            System.Windows.MessageBox.Show("Вкладка уже добавлена");
                            return;
                        }
                    }
                }
            }

            Autodesk.AutoCAD.PlottingServices.PlotConfig plotConfig = Autodesk.AutoCAD.PlottingServices.PlotConfigManager.SetCurrentConfig(Wizard.Plotter);

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

            RibbonLabel labelPlotterType = new RibbonLabel
            {
                ToolTip = "Плоттер  ",
                IsToolTipEnabled = true,
                Height = 22,
                ShowImage = true,
                Size = RibbonItemSize.Standard,
                Image = Extensions.GetBitmap(Properties.Resources.icon_24)
            };
            RibbonLabel labelSheetSize = new RibbonLabel
            {
                ToolTip = "Размер листа  ",
                IsToolTipEnabled = true,
                Height = 22,
                ShowImage = true,
                Size = RibbonItemSize.Standard,
                Image = Extensions.GetBitmap(Properties.Resources.icon_23)
            };

            RibbonLabel labelViewportScaling = new RibbonLabel
            {
                Text = "Масштабирование видового окна  ",
                Height = 22,
                ShowImage = true,
                Image = Extensions.GetBitmap(Properties.Resources.icon_17),
                LargeImage = Extensions.GetBitmap(Properties.Resources.icon_17)
            };

            RibbonLabel labelContentScaling = new RibbonLabel
            {
                Text = "Масштабирование содержимого ",
                Height = 22,
                Image = Extensions.GetBitmap(Properties.Resources.icon_17),
                LargeImage = Extensions.GetBitmap(Properties.Resources.icon_17)
            };

            tbViewportScaling = new RibbonTextBox
            {
                Id = "tbViewportScaling",
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new PlotWizard.Ribbon.CommandHandlers.TextboxCommandHandler(),
                Height = 22,
                Width = 50,
                MinWidth = 50,
                Size = RibbonItemSize.Large,
                TextValue = Wizard.ViewportScaling.ToString()
            };

            tbContentScaling = new RibbonTextBox
            {
                Id = "tbContentScaling",
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new Ribbon.CommandHandlers.TextboxCommandHandler(),
                Height = 22,
                Width = 50,
                MinWidth = 50,
                Size = RibbonItemSize.Large,
                TextValue = Wizard.ContentScaling.ToString(),
            };

            tbBlockName = new RibbonTextBox
            {
                Id = "tbBlockName",
                ToolTip = "Имя блока для печати",
                IsToolTipEnabled = true,
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new Ribbon.CommandHandlers.TextboxCommandHandler(),
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
                CommandHandler = new Ribbon.CommandHandlers.TextboxCommandHandler(),
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
                CommandHandler = new Ribbon.CommandHandlers.TextboxCommandHandler(),
                Width = 100,
                Height = 22,
                Size = RibbonItemSize.Large,
                IsEnabled = false,
                Text = ""
            };

            comboPlotterType = new RibbonCombo
            {
                Id = "comboPlotterType",
                ToolTip = "Плоттер  ",
                IsToolTipEnabled = true,
                Width = 250,
                Height = 22,
                Size = RibbonItemSize.Large,
            };
            foreach (var plotter in Extensions.GetPlotterNameList())
            {
                Autodesk.Windows.RibbonButton btn = new Autodesk.Windows.RibbonButton
                {
                    Text = plotter,
                    ShowText = true
                };
                comboPlotterType.Items.Add(btn);
                if (plotter.Equals(Wizard.Plotter))
                {
                    comboPlotterType.Current = btn;
                }
            }            
            comboPlotterType.CurrentChanged += ComboPlotterType_SelectedIndexChanged;

            comboSheetSize = new RibbonCombo
            {
                Id = "comboSheetSize",
                ToolTip = "Размер листа  ",
                IsToolTipEnabled = true,
                Width = 250,
                Height = 22,
                Size = RibbonItemSize.Large,
            };
            foreach (var sheetSize in Extensions.GetMediaNameList())
            {
                Autodesk.Windows.RibbonButton btn = new Autodesk.Windows.RibbonButton
                {
                    Text = sheetSize.Key.ToString(),
                    ShowText = true
                };
                comboSheetSize.Items.Add(btn);
                if (sheetSize.Value.Equals(Wizard.PageSize, StringComparison.InvariantCultureIgnoreCase))
                {
                    comboSheetSize.Current = btn;
                }
            }            
            comboSheetSize.CurrentChanged += ComboSheetSize_SelectedIndexChanged;

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
                CommandParameter = "CREATELAYOUTS",
                CommandHandler = new Ribbon.CommandHandlers.ButtonCommandHandler(),
                Text = "Создать\nлисты",
                ShowText = true,
                LargeImage = Extensions.GetBitmap(Properties.Resources.icon_15),
                Size = RibbonItemSize.Large,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                Width = 65,
                MinWidth = 65
            };

            btnEraseLayouts = new Autodesk.Windows.RibbonButton
            {
                CommandParameter = "ERASEALLLAYOUTS",
                CommandHandler = new Ribbon.CommandHandlers.ButtonCommandHandler(),
                Text = "Удалить\nлисты",
                ShowText = true,
                LargeImage = Extensions.GetBitmap(Properties.Resources.icon_16),
                Size = RibbonItemSize.Large,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                Width = 65,
                MinWidth = 65
            };

            btnMultiPlot = new Autodesk.Windows.RibbonButton
            {
                CommandParameter = "MULTIPLOT",
                CommandHandler = new Ribbon.CommandHandlers.ButtonCommandHandler(),
                Text = "Печать",
                ShowText = true,
                LargeImage = Extensions.GetBitmap(Properties.Resources.icon_18),
                Size = RibbonItemSize.Large,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                MinWidth = 65
            };

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

            RibbonRowPanel row3 = new RibbonRowPanel();
            row3.Items.Add(labelPlotterType);
            row3.Items.Add(comboPlotterType);
            row3.Items.Add(new RibbonRowBreak());
            row3.Items.Add(labelSheetSize);
            row3.Items.Add(comboSheetSize);
            row3.Items.Add(new RibbonRowBreak());
            row3.Items.Add(new RibbonLabel { Text = "", Height = 22 });
            RibbonRowPanel row01 = new RibbonRowPanel();
            row01.Items.Add(labelViewportScaling);
            row01.Items.Add(new RibbonRowBreak());
            row01.Items.Add(labelContentScaling);

            RibbonRowPanel row02 = new RibbonRowPanel();
            row02.Items.Add(tbViewportScaling);
            row02.Items.Add(new RibbonRowBreak());
            row02.Items.Add(tbContentScaling);

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
            panelSource.Items.Add(row3);
            
            panelSource.Items.Add(btnMultiPlot);
            panelSource.Items.Add(new RibbonPanelBreak());
            panelSource.Items.Add(row01);
            panelSource.Items.Add(row02);

            foreach (var tab in ribbon.Tabs)
            {
                if (tab.Title.Equals(TargetTabName)) 
                {
                    tab.Panels.Add(plotWizardPanel);
                    tab.IsActive = true;
                    break;
                }
            }
        }
    }
}
