using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using acad = Autodesk.AutoCAD.ApplicationServices.Application;
using WApplication = System.Windows.Forms;

namespace PrintWizard
{
    // class for implementing TextBox callbacks'
    public class TextBoxMarkingData : System.ComponentModel.INotifyPropertyChanged
    {
        public string markingName;
        public event PropertyChangedEventHandler PropertyChanged;
        public TextBoxMarkingData() { }
        public TextBoxMarkingData(String stringData)
        {
            markingName = stringData;
        }

        public String MarkingProperty
        {
            get 
            {
                return markingName; 
            }
            set
            {
                markingName = value;
                OnPropertyChanged("MarkingProperty");
            }
        }
        private void OnPropertyChanged(string info)
        {
            System.ComponentModel.PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(info));
                ManageMarking.markingName = markingName;
                //WApplication.MessageBox.Show("Marking changed", "Marking message", WApplication.MessageBoxButtons.OK, WApplication.MessageBoxIcon.Information);               
            }
        }
    }
    public class ButtonPlaceBlocksCommandHandler : System.Windows.Input.ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object param)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            /*
            RibbonCommandItem ribbonItem = parameter as RibbonCommandItem;
            if (ribbonItem != null)             
             */
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

    public static class ManageMarking
    {
        public static string[] _inlet_machines_types = {"1T1", "3T1", "1T2", "3T2", "1F1", "3F1", "1F2", "3F2", "3R1", "3R2", "1R1", "1R2" };
        public static string[] _inlet_automation_types = { "F", "FD", "F010", "RHF", "RF" };

        public static string[] _outlet_machines_types = { "1TE1", "3TE1", "1TE2", "3TE2", "1FE1", "3FE1", "1FE2", "3FE2", "3RE1", "3RE2" };
        public static string[] _outlet_automation_types = { "FE", "FED", "FE010", "RHFE", "RFE" };

        public static string[] _recuperator_machines_types = { "1PG1", "3PG1", "1RR1", "3RR1"};
        public static string[] _recuperator_automation_types = { "RG", "RR" }; // RX, RXC as common blocks

        public static string[] _waterheater_machines_types = { "1P1", "3P1", "1P2", "3P2"};
        public static string[] _waterheater_automation_types = { "HW", "HWP", "HWA" };

        public static string[] _humidifier_machines_types = { "1PW1", "3PW1" };
        public static string[] _humidifier_automation_types = { "WP", "WS", "WF" };

        public static string[] _sensor_list = { "T0", "T1", "T2" };

        public static string markingName = "";
        public static List<string> blocksToPlace = new List<string>();

        public static void SplitMarking() {
            // example marking SSM-S-XX.XX.XX-XX.XX.XX

            blocksToPlace.Clear();

            if (markingName.Length == 0)
            {
                System.Windows.MessageBox.Show("Маркировка отсутствует!", "Ошибка маркировки");
                return;
            }
            string[] strList = markingName.Split('-');
            if (strList.Length != 4)
            {
                System.Windows.MessageBox.Show("Маркировка задана в неверном формате!" +
                    "\nНеобходимый формат 'SSM-S-XX.XX.XX-XX.XX.XX'", "Ошибка маркировки");
                return;
            }            
            
            List<string[]> automation_types = new List<string[]> { _inlet_automation_types,
                                                              _outlet_automation_types,
                                                              _recuperator_automation_types,
                                                              _waterheater_automation_types,
                                                              _humidifier_automation_types};

            List<string[]> machines_types = new List<string[]> { _inlet_machines_types,
                                                              _outlet_machines_types,
                                                              _recuperator_machines_types,
                                                              _waterheater_machines_types,
                                                              _humidifier_machines_types};
            int i = 0;
            foreach (string[] _automation_type in automation_types)
            {                
                string type = "";
                foreach (string t in _automation_type)
                {
                    if (strList[2].Contains(t))
                    {
                        type = (type + t + ".");
                    }
                }

                if (type.Length > 0)
                {
                    type = type.Remove(type.Length - 1); // remove last
                    type += "-";
                    foreach (string t in machines_types[i])
                    {
                        if (strList[3].Contains(t))
                        {
                            type += t;
                        }

                    }
                    blocksToPlace.Add(type);
                }
                i++;
            }

            List<string> exceptionList = new List<string>();
            foreach (string[] _automation_type in automation_types)
                exceptionList.AddRange(_automation_type);

            // Check if any string match the string in exception list -> do not write to blocksToPlace.
            // Affects complex blocks splitted by '.' separator such as F.FD.F010-3F1 e.g.
            // inlets, outlets, recuperators, humidifiers, water heaters
            
            foreach (string str in strList[2].Split('.'))
            {
                bool del = false;
                foreach (string ex in exceptionList)
                {
                    if (str.CompareTo(ex) == 0)
                    {
                        del = true; break;
                    }
                }
                if (!del)
                    blocksToPlace.Add(str);
            }

            foreach (string str in _sensor_list)
            {
                if (blocksToPlace.Contains(str))
                {
                    int _stype = Commands._tsensor_type_dict[Commands._tsensor_type_user];
                    blocksToPlace.Remove(str);
                    switch (_stype)
                    {                        
                        case 0:                            
                            Commands._tsensor_type_blockname = "";
                            break;
                        case 1:
                            Commands._tsensor_type_blockname = str + "SMH";                           
                            break;
                        case 2:
                            Commands._tsensor_type_blockname = str;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
    
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

    public class Commands : IExtensionApplication
    {
        // Функции Initialize() и Terminate() необходимы, чтобы реализовать интерфейс IExtensionApplication
        public void Initialize() { }
        public void Terminate() { }

        public static TextBoxMarkingData _textBoxMarkingData = new TextBoxMarkingData("");
        public static bool _added = false;
        public static string _power_source_type = null;
        public static string[] _power_source_list = {"-","1P", "3P",
                                                     "1P-1P (1C)", "1P-3P (1C)", "3P-1P (1C)", "3P-3P (1C)",
                                                     "1P-1P (ATS)","3P-3P (ATS)",
                                                     "1P-1P (1C,ATS)", "1P-3P(1C,ATS)", "3P-1P(1C,ATS)", "3P-3P(1С,ATS)" };   
        
        public static string[] _tsensor_list = { "-", "Раздельные GND (SMH4)", "Общий GND (др.)" };
        public static string _tsensor_type_blockname = "";
        public static string _tsensor_type_user = _tsensor_list[1];
        public static Dictionary<string, int> _tsensor_type_dict = new Dictionary<string, int> {{ _tsensor_list[0], 0 },
                                                                                                { _tsensor_list[1], 1 },
                                                                                                { _tsensor_list[2], 2 }};
        [CommandMethod("CreateRibbonPanel")]
        public void CreateRibbonPanel()
        {
           
 // Панель 1 - Маркировка

            Autodesk.Windows.RibbonPanelSource panelSource1 = new Autodesk.Windows.RibbonPanelSource();
            RibbonPanel panel1 = new RibbonPanel
            {
                Source = panelSource1
            };
            panelSource1.Title = "Маркировка";

            // создаем элементы
            Autodesk.Windows.RibbonTextBox textBoxMarking = new RibbonTextBox();

            // создаем bind для вызова функций в случае изменения текста внутри textbox
            System.Windows.Data.Binding myBinding = new System.Windows.Data.Binding("MarkingProperty")
            {
                Source = _textBoxMarkingData,
                Mode = System.Windows.Data.BindingMode.TwoWay
            };
            textBoxMarking.TextValueBinding = myBinding;
            _added = true;

            textBoxMarking.Width = 150;
            textBoxMarking.Size = RibbonItemSize.Large;

            Autodesk.Windows.RibbonButton markButton = new Autodesk.Windows.RibbonButton
            {
                Text = "Маркировка: ",
                ShowText = true,
                ShowImage = true,
                Image = Images.GetBitmap(Properties.Resources.icon_05),
                LargeImage = Images.GetBitmap(Properties.Resources.icon_05)
            };

            RibbonRowPanel panel1Row = new RibbonRowPanel();
            panel1Row.Items.Add(markButton);
            panel1Row.Items.Add(new RibbonRowBreak());
            panel1Row.Items.Add(textBoxMarking);
            panelSource1.Items.Add(panel1Row);

// Панель 2 - Источник питания

            Autodesk.Windows.RibbonPanelSource panelSource2 = new Autodesk.Windows.RibbonPanelSource
            {
                Title = "Питание"
            };
            RibbonPanel panel2 = new RibbonPanel
            {
                Source = panelSource2
            };

            // создаем выпадающий список - выбор конфигурации вентиляторов
            Autodesk.Windows.RibbonCombo comboBoxPowerSource = new RibbonCombo();
            foreach (string str in _power_source_list)
            {
                Autodesk.Windows.RibbonButton btn = new Autodesk.Windows.RibbonButton
                {
                    Text = str,
                    ShowText = true
                };
                comboBoxPowerSource.Items.Add(btn);
            }
            comboBoxPowerSource.CurrentChanged += ComboBoxPowerSource_CurrentChanged;
            comboBoxPowerSource.Width = 150;

            Autodesk.Windows.RibbonButton powerTypeButton = new Autodesk.Windows.RibbonButton
            {
                Text = "Тип ввода питания: ",
                ShowText = true,
                ShowImage = true,
                Image = Images.GetBitmap(Properties.Resources.icon_04),
                LargeImage = Images.GetBitmap(Properties.Resources.icon_04)
            };

            RibbonRowPanel panel2Row = new RibbonRowPanel();
            panel2Row.Items.Add(powerTypeButton);   
            panel2Row.Items.Add(new RibbonRowBreak());
            panel2Row.Items.Add(comboBoxPowerSource);     
            panelSource2.Items.Add(panel2Row);

 // Панель 5 - вывод
            Autodesk.Windows.RibbonPanelSource panelSource5 = new Autodesk.Windows.RibbonPanelSource
            {
                Title = "Вывод"
            };
            RibbonPanel panel5 = new RibbonPanel
            {
                Source = panelSource5
            };
            Autodesk.Windows.RibbonButton btnPlaceBlocks = new Autodesk.Windows.RibbonButton
            {
                CommandParameter = "AddBlock",// привязываем нажатие кнопки к кастомному событию
                CommandHandler = new ButtonPlaceBlocksCommandHandler(),
                Text = "Разместить\nблоки",
                ShowText = true,
                LargeImage = Images.GetBitmap(Properties.Resources.icon_02),
                Size = RibbonItemSize.Large,
                Orientation = System.Windows.Controls.Orientation.Vertical
            };
            
            panelSource5.Items.Add(btnPlaceBlocks);

 // Панель 7 - датчики
            Autodesk.Windows.RibbonPanelSource panelSource7 = new Autodesk.Windows.RibbonPanelSource
            {
                Title = "Датчики температуры"
            };
            RibbonPanel panel7 = new RibbonPanel
            {
                Source = panelSource7
            };
            Autodesk.Windows.RibbonCombo comboBoxTemperatureSensor = new RibbonCombo();
            foreach (string str in _tsensor_list)
            {
                Autodesk.Windows.RibbonButton btn = new Autodesk.Windows.RibbonButton
                {
                    Text = str,
                    ShowText = true
                };
                comboBoxTemperatureSensor.Items.Add(btn);
            }
            comboBoxTemperatureSensor.CurrentChanged += ComboBoxTemperatureSensor_CurrentChanged;
            comboBoxTemperatureSensor.Width = 160;

            Autodesk.Windows.RibbonButton temperatureSensorTypeButton = new Autodesk.Windows.RibbonButton
            {
                Text = "Тип датчиков:",
                ShowText = true,
                ShowImage = true,
                Image = Images.GetBitmap(Properties.Resources.icon_10),
                LargeImage = Images.GetBitmap(Properties.Resources.icon_10)
            };

            RibbonRowPanel panel7Row = new RibbonRowPanel();
            panel7Row.Items.Add(temperatureSensorTypeButton);
            panel7Row.Items.Add(new RibbonRowBreak());
            panel7Row.Items.Add(comboBoxTemperatureSensor);
            panelSource7.Items.Add(panel7Row);

 // Общее - создаем вкладку
            RibbonTab newTab = new RibbonTab
            {
                Title = "Print Wizard",
                Id = "_PrintWizard"
            };

            newTab.Panels.Add(panel1);
            newTab.Panels.Add(panel2);
            newTab.Panels.Add(panel7);
            newTab.Panels.Add(panel5);            

            Autodesk.Windows.RibbonControl ribbon = ComponentManager.Ribbon;
            ribbon.Tabs.Add(newTab);
            newTab.IsActive = true;
        }        
        public static void ComboBoxTemperatureSensor_CurrentChanged(object o, RibbonPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
                _tsensor_type_user = (args.NewValue as RibbonButton).Text;                
        }
        public static void ComboBoxPowerSource_CurrentChanged(object o, RibbonPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
                _power_source_type = (args.NewValue as RibbonButton).Text;
        }       

        [CommandMethod("AddBlock")]
        static public void AddBlock()
        {

            List<string> missedBlocks = new List<string>();
            ManageMarking.SplitMarking();

            Database database = acad.DocumentManager.MdiActiveDocument.Database;

            if (ManageMarking.blocksToPlace.Count == 0)
            {
                System.Windows.MessageBox.Show("0 блоков выбрано для размещения!");
                return;
            }
            if (_power_source_type == null)
            {
                System.Windows.MessageBox.Show("Не выбран тип ввода питания!");
                return;
            }

            string _frameName = "ListSPDSF6_A42"; // standart GOST A4

            int sheet_counter = 1; // store the sheets counter
            int left_offset = 20; // 20 mm - standart left offset for A4 frame
            int right_offset = 5; // 5 mm - standart right offset for A4 frame
            int sheet_width = 297;
            
            int delta = left_offset; // store the cumulative sum offset for each new added block

            List<string> list = new List<string> { _power_source_type };
            list.AddRange(ManageMarking.blocksToPlace);
            list.Add(_tsensor_type_blockname);
            
            
            Transaction transaction = database.TransactionManager.StartTransaction();


            //Get the block database definition
            BlockTable blockTable = database.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
            BlockTableRecord blockDef;
            BlockTableRecord modelSpace;
            Point3d point;
            // Add circuit diagram blocks
            foreach (string _blockName in list)
            {
                if (blockTable.Has(_blockName))
                {
                    //Also open modelspace - we'll be adding our BlockReference to it
                    //Create new BlockReference, and link it to our block definition
                    blockDef = blockTable[_blockName].GetObject(OpenMode.ForRead) as BlockTableRecord;
                    modelSpace = blockTable[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForWrite) as BlockTableRecord;
                    point = new Point3d(delta, 5, 0);

                    using (BlockReference blockRef =
                        new BlockReference(point, blockDef.ObjectId))
                    {
                        int width;
                        Extents3d? bounds = blockRef.Bounds;
                        if (bounds.HasValue)
                        {
                            Extents3d ext = bounds.Value;
                            width = (int)(ext.MaxPoint.X - ext.MinPoint.X);
                            if (((delta + width) > (sheet_width * sheet_counter - right_offset)))
                            {
                                delta = sheet_width * sheet_counter + left_offset;
                                sheet_counter++;
                                blockRef.Position = new Point3d(delta, 5, 0);
                            }
                            delta += width;
                        }

                        modelSpace.AppendEntity(blockRef);
                        transaction.AddNewlyCreatedDBObject(blockRef, true);

                        // iterate through block attributes
                        foreach (ObjectId id in blockDef)
                        {
                            DBObject obj = id.GetObject(OpenMode.ForRead);
                            AttributeDefinition attDef = obj as AttributeDefinition;
                            if ((attDef != null) && (!attDef.Constant))
                            {
                                // create new instances of atribute in block and add to transaction
                                using (AttributeReference attRef = new AttributeReference())
                                {
                                    attRef.SetAttributeFromBlock(attDef, blockRef.BlockTransform);
                                    blockRef.AttributeCollection.AppendAttribute(attRef);
                                    transaction.AddNewlyCreatedDBObject(attRef, true);
                                }
                            }

                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(_blockName) && !(_blockName.CompareTo("-") == 0))
                    {
                        missedBlocks.Add(_blockName);
                    }
                }
            }

            delta = 0;
            // Add frames
            if (blockTable.Has(_frameName))
            {
                blockDef = blockTable[_frameName].GetObject(OpenMode.ForRead) as BlockTableRecord;
                modelSpace = blockTable[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForWrite) as BlockTableRecord;

                for (int sheet = 1; sheet <= sheet_counter; sheet++)
                {                    
                    point = new Point3d(delta, 0, 0);
                    using (BlockReference blockRef =
                            new BlockReference(point, blockDef.ObjectId))
                    {
                        int width;
                        Extents3d? bounds;
                        bounds = blockRef.Bounds;
                        if (bounds.HasValue)
                        {
                            Extents3d ext = bounds.Value;
                            width = (int)(ext.MaxPoint.X - ext.MinPoint.X);
                            delta += width;
                        }

                        modelSpace.AppendEntity(blockRef);
                        transaction.AddNewlyCreatedDBObject(blockRef, true);

                        foreach (ObjectId id in blockDef)
                        {
                            DBObject obj = id.GetObject(OpenMode.ForRead);
                            AttributeDefinition attDef = obj as AttributeDefinition;
                            if ((attDef != null) && (!attDef.Constant))
                            {
                                using (AttributeReference attRef = new AttributeReference())
                                {
                                    attRef.SetAttributeFromBlock(attDef, blockRef.BlockTransform);
                                    if (attRef.Tag == "ЧЕРТНИЗ")
                                    {
                                        attRef.TextString = ManageMarking.markingName;
                                        attRef.WidthFactor = 1.4 - 0.01 * ManageMarking.markingName.Length;
                                        if (attRef.WidthFactor > 1)
                                            attRef.WidthFactor = 1;
                                        if (attRef.WidthFactor < 0.4)
                                            attRef.WidthFactor = 0.4;
                                    }
                                    if (attRef.Tag == "ЛИСТ")
                                        attRef.TextString = sheet.ToString();

                                    blockRef.AttributeCollection.AppendAttribute(attRef);
                                    transaction.AddNewlyCreatedDBObject(attRef, true);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                missedBlocks.Add(_frameName);
            }

            // Commit transaction - add blocks to workspace
            
            if (missedBlocks.Count!=0)
            {
                System.Windows.MessageBox.Show($"Блоки" +
                    $"{"\n" + String.Join("\n", missedBlocks) + "\n"}" +
                    $"не содержатся в библиотеке блоков текущего чертежа",
                    "Ошибка");
            }
            transaction.Commit();
        }

        [CommandMethod("EditBlockText")]
        public void EditBlockText() // Search for blocks of defined type and change_attribute(attr_name, attr_value)
        {
            // получаем ссылки на документ и его БД
            Document doc = acad.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            // поле документа "Editor" понадобится нам для вывода сообщений в окно консоли AutoCAD
            //Editor ed = doc.Editor;

            // имя создаваемого блока
            const string blockName = "NULL";
            const string attrName = "NULL";
            // начинаем транзакцию
            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                var acBlockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (acBlockTable == null) return;

                var acBlockTableRecord = tr.GetObject(acBlockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                if (acBlockTableRecord == null) return;

                foreach (var blkId in acBlockTableRecord)
                {
                    var acBlock = tr.GetObject(blkId, OpenMode.ForRead) as BlockReference;
                    if (acBlock == null) continue;
                    if (!acBlock.Name.Equals(blockName, StringComparison.CurrentCultureIgnoreCase)) continue;
                    foreach (ObjectId attId in acBlock.AttributeCollection)
                    {
                        var acAtt = tr.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                        if (acAtt == null) continue;

                        if (!acAtt.Tag.Equals(attrName, StringComparison.CurrentCultureIgnoreCase)) continue;

                        acAtt.UpgradeOpen();
                        WApplication.MessageBox.Show("Sucess!");
                        acAtt.TextString = "NOVAYA_PODPIS";
                    }
                }
                tr.Commit();
            }
        }
        // place new methods here
        //...
    }
    // place new class here
    //...
}