
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
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;


namespace PrintWizard
{
    public class NotifyingTextBox : RibbonTextBox
    {
        public NotifyingTextBox()
        {
            // Register our focus-related event handlers
            EventManager.RegisterClassHandler(typeof(System.Windows.Controls.TextBox), System.Windows.Controls.TextBox.LostKeyboardFocusEvent, new RoutedEventHandler(OnLostFocus));
        }
        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Autodesk.Private.Windows.TextBoxWithImage tb)
            {
                if (tb.Text == null)
                {
                    tb.Text = "";
                }
                string filename = Extensions.PurgeString(tb.Text);
                RibbonCommands.fileName = filename;
                PlotWizard.MyFileName = RibbonCommands.fileName;
                tb.Text = filename;
            }
        }
    }
    public class TextboxCommandHandler : System.Windows.Input.ICommand
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
                        RibbonCommands.blockName = tb.TextValue;
                        break;
                    case "tbAttrLabel":
                        RibbonCommands.attrLabelName = tb.TextValue;
                        break;
                    case "tbAttrSheet":
                        RibbonCommands.attrSheetName = tb.TextValue;
                        break;
                    case "tbViewportScaling":
                        try
                        {
                            double sc = double.Parse(tb.TextValue, CultureInfo.InvariantCulture);
                            sc = Extensions.Clamp(sc,0,1);
                            tb.TextValue = sc.ToString();
                            RibbonCommands.viewportScaling = sc;
                            PlotWizard.MyViewportScaling = RibbonCommands.viewportScaling;
                        }
                        catch (System.Exception e)
                        {
                            tb.TextValue = RibbonCommands.viewportScaling.ToString();
                        }
                        break;
                    case "tbContentScaling":
                        try
                        {
                            double sc = double.Parse(tb.TextValue, CultureInfo.InvariantCulture);
                            sc = Extensions.Clamp(sc, 0, (double)Int32.MaxValue);
                            tb.TextValue = sc.ToString();
                            RibbonCommands.contentScaling = sc;
                            PlotWizard.MyContentScaling = RibbonCommands.contentScaling;
                        }
                        catch (System.Exception e)
                        {
                            tb.TextValue = RibbonCommands.contentScaling.ToString();
                        }
                        break;
                }
            }
        }
    }
    public class ButtonCommandHandler : System.Windows.Input.ICommand
    {
#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67
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

    public class ButtonChooseBlockCommandHandler : System.Windows.Input.ICommand
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

                        RibbonCommands.blockName = br.Name;
                        Autodesk.Windows.RibbonControl ribbon = ComponentManager.Ribbon;

                        foreach (var tab in ribbon.Tabs)
                        {
                            if (tab.Title.Equals("Вывод"))
                            {

                                var tb = tab.FindItem("tbBlockName") as RibbonTextBox;
                                if (tb is RibbonTextBox)
                                {
                                    tb.TextValue = RibbonCommands.blockName;
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

                                RibbonCommands.attrLabelName = AttributesSelector._attrLabel;
                                RibbonCommands.attrSheetName = AttributesSelector._attrSheet;
                                tb = tab.FindItem("tbAttrLabel") as RibbonTextBox;
                                if (tb is RibbonTextBox)
                                    tb.TextValue = RibbonCommands.attrLabelName;
                                tb = tab.FindItem("tbAttrSheet") as RibbonTextBox;
                                if (tb is RibbonTextBox)
                                    tb.TextValue = RibbonCommands.attrSheetName;
                                break;
                            }
                        }
                        PlotWizard.MyBlock_Name = RibbonCommands.blockName;
                        PlotWizard.MyBlockAttr_Label = RibbonCommands.attrLabelName;
                        PlotWizard.MyBLockAttr_Sheet = RibbonCommands.attrSheetName;
                        tr.Commit();
                    }
                }
            }
        }
        private partial class AttributesSelector : Form
        {
            public static string _attrLabel;
            public static string _attrSheet;
            public static List<string> _attrCollection = new List<string>();
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

                lbAttributesLabel.SelectedIndexChanged += new System.EventHandler(lbAttributesLabel_SelectedIndexChanged);
                lbAttributesSheet.SelectedIndexChanged += new System.EventHandler(lbAttributesSheet_SelectedIndexChanged);
                buttonOk.Click += new System.EventHandler(buttonOk_Click);

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
            private void buttonOk_Click(object sender, EventArgs e)
            {
                if (!String.IsNullOrEmpty(_attrLabel))
                    PlotWizard.MyBlockAttr_Label = _attrLabel;
                if (!String.IsNullOrEmpty(_attrSheet))
                    PlotWizard.MyBLockAttr_Sheet = _attrSheet;
                Close();
            }
            private void lbAttributesLabel_SelectedIndexChanged(object sender, EventArgs e)
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
            private void lbAttributesSheet_SelectedIndexChanged(object sender, EventArgs e)
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
    public class RibbonCommands : IExtensionApplication
    {
        public static string blockName;
        public static string attrLabelName;
        public static string attrSheetName;
        public static double viewportScaling;
        public static double contentScaling;
        public static string fileName;

        private NotifyingTextBox tbFileName;
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
        private void comboPlotterType_SelectedIndexChanged(object o, RibbonPropertyChangedEventArgs args)
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
                    if (select)
                    {
                        select = false;
                        comboSheetSize.Current = btn;
                    }
                }
            }
        }
        private void comboSheetSize_SelectedIndexChanged(object o, RibbonPropertyChangedEventArgs args)
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
                Text = "Имя блока для печати  ",
                Height = 22
            };
            RibbonLabel labelAttrLabelName = new RibbonLabel
            {
                Text = "Имя атрибута - чертеж  ",
                Height = 22,
            };
            
            RibbonLabel labelAttrSheetName = new RibbonLabel
            {
                Text = "Имя атрибута - лист  ",
                Height = 22,
            };

            RibbonLabel labelFileName = new RibbonLabel
            {
                Text = "Имя файла  ",
                Height = 22,
            };
            RibbonLabel labelPlotterType = new RibbonLabel
            {
                Text = "Плоттер  ",
                Height = 22,
            };
            RibbonLabel labelSheetSize = new RibbonLabel
            {
                Text = "Размер листа  ",
                Height = 22,
            };

            RibbonLabel labelViewportScaling = new RibbonLabel
            {
                Text = "Масштабирование видового окна  ",
                Height = 22,
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

            tbFileName = new NotifyingTextBox
            {
                Id = "tbFileName",
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new TextboxCommandHandler(),
                Height = 22,
                Width = 250,
                Size = RibbonItemSize.Large,
                TextValue = ""
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
            comboPlotterType.CurrentChanged += comboPlotterType_SelectedIndexChanged;

            comboSheetSize = new RibbonCombo
            {
                Id = "comboSheetSize",
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
            comboSheetSize.CurrentChanged += comboSheetSize_SelectedIndexChanged;

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
            row3.Items.Add(labelFileName);
            row3.Items.Add(new RibbonRowBreak());
            row3.Items.Add(labelPlotterType);
            row3.Items.Add(new RibbonRowBreak());
            row3.Items.Add(labelSheetSize);

            RibbonRowPanel row4 = new RibbonRowPanel();
            row4.Items.Add(tbFileName);
            row4.Items.Add(new RibbonRowBreak());
            row4.Items.Add(comboPlotterType);
            row4.Items.Add(new RibbonRowBreak());
            row4.Items.Add(comboSheetSize);

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
            panelSource.Items.Add(row4);
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
