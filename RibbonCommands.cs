﻿
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.Drawing;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using acad = Autodesk.AutoCAD.ApplicationServices.Application;
using Ap = Autodesk.AutoCAD.ApplicationServices;
using Db = Autodesk.AutoCAD.DatabaseServices;
using Ed = Autodesk.AutoCAD.EditorInput;
using System.Windows.Forms;
using System.Globalization;
using System.Windows;
using System.IO;

namespace PrintWizard
{
    class A
    {
        public int Number {
            get { return Number; }
            set { Number = value; System.Console.WriteLine(""); }
        }
    }
    internal class TextboxCommandHandler : System.Windows.Input.ICommand
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
                            sc = Extensions.Clamp(sc,0,1);
                            tb.TextValue = sc.ToString();
                            RibbonCommands.ViewportScaling = sc;
                            PlotWizard.MyViewportScaling = RibbonCommands.ViewportScaling;
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
                            PlotWizard.MyContentScaling = RibbonCommands.ContentScaling;
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

    internal class ButtonChooseBlockCommandHandler : System.Windows.Input.ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object param)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            if (parameter is RibbonCommandItem)
            {
                Ap.Document doc = acad.DocumentManager.MdiActiveDocument;
                if (doc == null || doc.IsDisposed)
                    return;
                Ed.Editor ed = doc.Editor;

                using (doc.LockDocument())
                {

                    Ed.PromptEntityOptions peo = new Ed.PromptEntityOptions("\nВыберите экземпляр вхождения блока:");

                    peo.SetRejectMessage("\nВыбранный объект не является вхождением блока.\n");
                    peo.AddAllowedClass(typeof(Db.BlockReference), false);

                    Ed.PromptEntityResult res = ed.GetEntity(peo);

                    if (res.Status != Ed.PromptStatus.OK)
                    {
                        ed.WriteMessage("\nОтмена.\n");
                        return;
                    }

                    Db.ObjectId objId = res.ObjectId;
                    Db.Database db = doc.Database;

                    using (Db.Transaction tr = db.TransactionManager.StartTransaction())
                    {

                        BlockReference br = tr.GetObject(objId, Db.OpenMode.ForRead) as BlockReference;
                        ed.WriteMessage($"\nВыбран блок '{br.Name}'.\n");

                        RibbonCommands.BlockName = br.Name;
                        Autodesk.Windows.RibbonControl ribbon = ComponentManager.Ribbon;

                        foreach (var tab in ribbon.Tabs)
                        {
                            if (tab.Title.Equals("Вывод"))
                            {

                                var tb = tab.FindItem("tbBlockName") as RibbonTextBox;
                                if (tb is RibbonTextBox)
                                {
                                    tb.TextValue = RibbonCommands.BlockName;
                                }

                                List<string> attrCollection = new List<string>();
                                foreach (ObjectId obj in br.AttributeCollection)
                                {
                                    var attr = tr.GetObject(obj, OpenMode.ForRead) as AttributeReference;
                                    if (attr == null) 
                                            continue;
                                    attrCollection.Add(attr.Tag);
                                }

                                AttributesSelector attrSelector = new AttributesSelector(attrCollection);
                                attrSelector.ShowDialog();

                                RibbonCommands.AttrLabelName = AttributesSelector._attrLabel;
                                RibbonCommands.AttrSheetName = AttributesSelector._attrSheet;

                                tb = tab.FindItem("tbAttrLabel") as RibbonTextBox;
                                if (tb is RibbonTextBox)
                                    tb.TextValue = RibbonCommands.AttrLabelName;
                                tb = tab.FindItem("tbAttrSheet") as RibbonTextBox;
                                if (tb is RibbonTextBox)
                                    tb.TextValue = RibbonCommands.AttrSheetName;
                                break;
                            }
                        }
                        PlotWizard.MyBlockName = RibbonCommands.BlockName;
                        PlotWizard.MyBlockAttrLabel = RibbonCommands.AttrLabelName;
                        PlotWizard.MyBLockAttrSheet = RibbonCommands.AttrSheetName;
                        tr.Commit();
                    }
                }
            }
        }
        private partial class AttributesSelector : Form
        {
            internal static string _attrLabel;
            internal static string _attrSheet;
            private static List<string> _attrCollection = new List<string>();
            public AttributesSelector(List<string> attrCollection)
            {
                if (attrCollection != null)
                {
                    _attrCollection = attrCollection;
                }
                this.InitializeComponent();
            }

            private System.Windows.Forms.ListBox lbAttributesLabel;
            private System.Windows.Forms.ListBox lbAttributesSheet;
            private System.Windows.Forms.Label labelAttributesLabel;
            private System.Windows.Forms.Label labelAttributesSheet;
            private System.Windows.Forms.Button buttonOk;
            private void InitializeComponent()
            {
                SuspendLayout();

                labelAttributesLabel = new System.Windows.Forms.Label
                {
                    Text = "Атрибут блока -\nчертеж",
                    Location = new System.Drawing.Point(10, 10),
                    Size = new System.Drawing.Size(150, 35)
                };
                
                labelAttributesSheet = new System.Windows.Forms.Label
                {
                    Text = "Атрибут блока -\nлист",
                    Location = new System.Drawing.Point(160, 10),
                    Size = new System.Drawing.Size(150, 35)
                };

                lbAttributesLabel = new System.Windows.Forms.ListBox
                {
                    Location = new System.Drawing.Point(10, 50),
                    Size = new System.Drawing.Size(140, 200),
                };

                lbAttributesSheet = new System.Windows.Forms.ListBox
                {
                    Location = new System.Drawing.Point(160, 50),
                    Size = new System.Drawing.Size(140, 200),
                };
                lbAttributesLabel.Items.Add("Нет");
                lbAttributesSheet.Items.Add("Нет");

                foreach (var _attr in _attrCollection)
                {
                    lbAttributesLabel.Items.Add(_attr);
                    lbAttributesSheet.Items.Add(_attr);
                }

                lbAttributesLabel.SelectedIndex = 0;
                lbAttributesSheet.SelectedIndex = 0;

                buttonOk = new System.Windows.Forms.Button
                {
                    Location = new System.Drawing.Point(225, 280),
                    Size = new System.Drawing.Size(75, 20),
                    Text = "OK"
                };

                lbAttributesLabel.SelectedIndexChanged += new System.EventHandler(ListboxAttributesLabel_SelectedIndexChanged);
                lbAttributesSheet.SelectedIndexChanged += new System.EventHandler(ListboxAttributesSheet_SelectedIndexChanged);
                buttonOk.Click += new System.EventHandler(ButtonOk_Click);

                AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                ClientSize = new System.Drawing.Size(310, 320);
                Controls.Add(lbAttributesLabel);
                Controls.Add(lbAttributesSheet);
                Controls.Add(labelAttributesLabel);
                Controls.Add(labelAttributesSheet);
                Controls.Add(buttonOk);

                //System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AttributesSelector));
                Icon = Icon.FromHandle(Properties.Resources.icon_12.GetHicon());
                Text = "Выберите атрибуты блока...";
    
                PerformLayout();    
            }
            private void ButtonOk_Click(object sender, EventArgs e)
            {
                if (!String.IsNullOrEmpty(_attrLabel))
                    PlotWizard.MyBlockAttrLabel = _attrLabel;
                if (!String.IsNullOrEmpty(_attrSheet))
                    PlotWizard.MyBLockAttrSheet = _attrSheet;
                Close();
            }
            private void ListboxAttributesLabel_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (!this.lbAttributesLabel.Text.Equals("Нет"))
                {
                    _attrLabel = this.lbAttributesLabel.Text;
                }
                else
                {
                    _attrLabel = " ";
                }
            }
            private void ListboxAttributesSheet_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (!this.lbAttributesSheet.Text.Equals("Нет"))
                {
                    _attrSheet = this.lbAttributesSheet.Text;
                }
                else
                {
                    _attrSheet = " ";
                }
            }
        }
    }
    internal class RibbonCommands : IExtensionApplication
    {
        public static string BlockName { get; set; }
        public static string AttrLabelName { get; set; }
        public static string AttrSheetName { get; set; }
        public static double ViewportScaling { get; set; }
        public static double ContentScaling { get; set; }

        private Autodesk.Windows.RibbonTextBox tbViewportScaling;
        private Autodesk.Windows.RibbonTextBox tbContentScaling;
        private Autodesk.Windows.RibbonTextBox tbBlockName;
        private Autodesk.Windows.RibbonTextBox tbAttrLabel;
        private Autodesk.Windows.RibbonTextBox tbAttrSheet;

        private Autodesk.Windows.RibbonCombo comboPlotterType;
        private Autodesk.Windows.RibbonCombo comboSheetSize;

        private Autodesk.Windows.RibbonButton btnChooseBlock;
        private Autodesk.Windows.RibbonButton btnCreateLayouts;
        private Autodesk.Windows.RibbonButton btnEraseLayouts;
        private Autodesk.Windows.RibbonButton btnMultiPlot;

        // Функции Initialize() и Terminate() необходимы, чтобы реализовать интерфейс IExtensionApplication
        public void Initialize() { }
        public void Terminate() { }
        private void ComboPlotterType_SelectedIndexChanged(object o, RibbonPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                Autodesk.AutoCAD.PlottingServices.PlotConfigManager.SetCurrentConfig((args.NewValue as RibbonButton).Text);

                PlotWizard.MyPlotter = (args.NewValue as RibbonButton).Text;

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
                PlotWizard.MyPageSize = Extensions.GetMediaNameList()[(args.NewValue as RibbonButton).Text];
            }
        }        
        public void AddMyRibbonPanel()
        {
            Autodesk.AutoCAD.PlottingServices.PlotConfig plotConfig = Autodesk.AutoCAD.PlottingServices.PlotConfigManager.SetCurrentConfig(PlotWizard.MyPlotter);

            RibbonLabel labelBlockName = new RibbonLabel
            {
                ToolTip = "Имя блока для печати",
                IsToolTipEnabled = true,
                Height = 22,
                ShowImage = true,
                Size = RibbonItemSize.Standard,
                Image = Extensions.GetBitmap(Properties.Resources.icon_22)
            };
            RibbonLabel labelAttrLabelName = new RibbonLabel
            {
                ToolTip = "Имя атрибута - чертеж",
                IsToolTipEnabled = true,
                ShowImage = true,
                Size = RibbonItemSize.Standard,
                Image = Extensions.GetBitmap(Properties.Resources.icon_20),
                Height = 22
            };
            
            RibbonLabel labelAttrSheetName = new RibbonLabel
            {
                ToolTip = "Имя атрибута - лист  ",
                IsToolTipEnabled = true,
                Height = 22,
                ShowImage = true,
                Size = RibbonItemSize.Standard,
                Image = Extensions.GetBitmap(Properties.Resources.icon_19)
            };

            RibbonLabel labelFileName = new RibbonLabel
            {
                ToolTip = "Имя файла  ",
                IsToolTipEnabled = true,
                Height = 22,
                ShowImage = true,
                Size = RibbonItemSize.Standard,
                Image = Extensions.GetBitmap(Properties.Resources.icon_21)
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
                CommandHandler = new TextboxCommandHandler(),
                Height = 22,
                Width = 50,
                MinWidth = 50,
                Size = RibbonItemSize.Large,
                TextValue = PlotWizard.MyViewportScaling.ToString()
            };

            tbContentScaling = new RibbonTextBox
            {
                Id = "tbContentScaling",
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new TextboxCommandHandler(),
                Height = 22,
                Width = 50,
                MinWidth = 50,
                Size = RibbonItemSize.Large,
                TextValue = PlotWizard.MyContentScaling.ToString(),
            };

            tbBlockName = new RibbonTextBox
            {
                Id = "tbBlockName",
                ToolTip = "Имя блока для печати",
                IsToolTipEnabled = true,
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new TextboxCommandHandler(),
                Width = 100,
                Height = 22,
                Size = RibbonItemSize.Large,
                IsEnabled = false,
                Text = "",
            };

            tbAttrLabel = new RibbonTextBox
            {
                Id = "tbAttrLabel",
                ToolTip = "Имя атрибута - чертеж",
                IsToolTipEnabled = true,
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new TextboxCommandHandler(),
                Width = 100,
                Height = 22,
                Size = RibbonItemSize.Large,
                IsEnabled = false,
                Text = ""
            };

            tbAttrSheet = new RibbonTextBox
            {
                ToolTip = "Имя атрибута - лист  ",
                IsToolTipEnabled = true,
                Id = "tbAttrSheet",
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new TextboxCommandHandler(),
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
                if (plotter.Equals(PlotWizard.MyPlotter))
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
                if (sheetSize.Value.Equals(PlotWizard.MyPageSize, StringComparison.InvariantCultureIgnoreCase))
                {
                    comboSheetSize.Current = btn;
                }
            }            
            comboSheetSize.CurrentChanged += ComboSheetSize_SelectedIndexChanged;

            btnChooseBlock = new Autodesk.Windows.RibbonButton
            {
                CommandHandler = new ButtonChooseBlockCommandHandler(),
                Text = "Выбрать\nблок",
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
                CommandHandler = new ButtonCommandHandler(),
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
                CommandHandler = new ButtonCommandHandler(),
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
                CommandHandler = new ButtonCommandHandler(),
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
            row1.Items.Add(labelAttrLabelName);
            row1.Items.Add(new RibbonRowBreak());
            row1.Items.Add(labelAttrSheetName);
            
            RibbonRowPanel row2 = new RibbonRowPanel();
            row2.Items.Add(tbBlockName);
            row2.Items.Add(new RibbonRowBreak());
            row2.Items.Add(tbAttrLabel);
            row2.Items.Add(new RibbonRowBreak());
            row2.Items.Add(tbAttrSheet);

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

            Autodesk.Windows.RibbonControl ribbon = ComponentManager.Ribbon;
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
