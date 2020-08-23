
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

using acad = Autodesk.AutoCAD.ApplicationServices.Application;
using Ap = Autodesk.AutoCAD.ApplicationServices;
using Ed = Autodesk.AutoCAD.EditorInput;
using Rt = Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;

[assembly: Rt.CommandClass(typeof(PlotWizard.WizardInitializer))]

namespace PlotWizard
{
    public static class WizardInitializer
    {
        [Rt.CommandMethod("PLOTWIZARD", Rt.CommandFlags.Modal)]
        public static void Plotwizard()
        {
            new Ribbon.RibbonCommands().AddMyRibbonPanel();
        }
    }
    public static class Wizard
    {        
        public static ObjectIdCollection Layouts { get; set; }
        public static void CreateLayouts()
        {
            Document doc = acad.DocumentManager.MdiActiveDocument;

            if (doc == null || doc.IsDisposed)
                return;

            using (doc.LockDocument())
            {
                List <PlotObject> plotObjects = GetPlotObjects(Ribbon.BlockSelectorSettings.TargetBlockName,
                                                               Ribbon.BlockSelectorSettings.Prefix,
                                                               Ribbon.BlockSelectorSettings.Postfix,
                                                               Ribbon.BlockSelectorSettings.FirstCornerPoint,
                                                               Ribbon.BlockSelectorSettings.SecondCornerPoint,
                                                               Ribbon.BlockSelectorSettings.SortingOrder);
                LayoutCommands lc = new LayoutCommands();
                Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("BACKGROUNDPLOT", 0);
                
                foreach (var plotObject in plotObjects)
                {
                    ObjectId lay = lc.CreateMyLayout(Ribbon.LayoutSettings.PageSize.Value,
                                                     Ribbon.LayoutSettings.ViewportScaling,
                                                     Ribbon.LayoutSettings.ContentScaling,
                                                     "acad.ctb",
                                                     Ribbon.LayoutSettings.PlotterType,
                                                     plotObject);
                    if (!lay.IsNull)
                    {
                        Layouts.Add(lay);
                    }
                }
                doc.Editor.WriteMessage($"Создано {plotObjects.Count.ToString()} листа(-ов).\n");
            }
            //doc.Editor.Regen();
        }
        public static void MultiPlot()
        {
            try
            {
                var doc = acad.DocumentManager.MdiActiveDocument;

                if (doc == null)
                    return;
                if (Layouts == null || Layouts.IsDisposed || Layouts.Count == 0)
                {
                    System.Windows.MessageBox.Show("Нет страниц для печати. Пропущено.");
                    return;
                }

                Autodesk.AutoCAD.PlottingServices.PlotConfigManager.SetCurrentConfig(Ribbon.LayoutSettings.PlotterType);

                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Вывод в файл",
                    Filter = $"{Autodesk.AutoCAD.PlottingServices.PlotConfigManager.CurrentConfig.DeviceName}|*" +
                                $"{Autodesk.AutoCAD.PlottingServices.PlotConfigManager.CurrentConfig.DefaultFileExtension}",
                    FileName = GetInitialFilename(Layouts)
                };

                bool? result = saveFileDialog.ShowDialog();

                if (!result.HasValue || !result.Value)
                {
                    doc.Editor.WriteMessage("\nОтмена.\n");
                    return;
                }
                ObjectIdCollection AllLayouts = Layouts;

                MultiSheetPlot.MultiSheetPlotter(Ribbon.LayoutSettings.PageSize.Value,
                                                 Ribbon.LayoutSettings.PlotterType,
                                                 saveFileDialog.FileName,
                                                 AllLayouts);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
        }
        public static void EraseAllLayouts()
        {
            LayoutCommands.EraseAllLayouts();
            Layouts.Clear();
        }
        private static string GetInitialFilename(ObjectIdCollection layouts)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            string filename = "";
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                if (layouts != null && !layouts.IsDisposed && layouts.Count > 0)
                {
                    try
                    {
                        var obj = layouts[layouts.Count-1];
                        if (db.TryGetObjectId(obj.Handle, out obj))
                        {
                            Layout layout = tr.GetObject(obj, OpenMode.ForRead) as Layout;
                            if (String.IsNullOrEmpty(layout.LayoutName))
                            {
                                throw new Exception();
                            }
                            filename = layout.LayoutName;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    catch (Exception e)
                    {
                        System.Windows.MessageBox.Show("Невозможно сформировать имя файла для печати по умолчанию\nУказанная страница отсуствует в БД чертежа.");
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Невозможно сформировать имя файла для печати по умолчанию\nНет страниц для печати.");
                }
                tr.Commit();
            }
            return filename;
        }
        private static List<PlotObject> GetPlotObjects(string targetBlockName,
                                                       string prefix,
                                                       string postfix,
                                                       Point3d maxCornerPoint,
                                                       Point3d minCornerPoint,
                                                       SortingOrder sortingOrder)
        {
            Document doc = acad.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            // create a collection where stored block entries references
            List<PlotObject> plotObjects = new List<PlotObject>();
            // start transaction
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // get table of all drawing's blocks
                var blockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (blockTable == null)
                    return plotObjects;
                // get table of all blocks references in current modelspace
                var blockTableRecord = tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                if (blockTableRecord == null)
                    return plotObjects;
                // iterate through all blocks' references in current modelspace
                foreach (ObjectId blockRecord in blockTableRecord)
                {                    
                    // get single block reference
                    var block = tr.GetObject(blockRecord, OpenMode.ForRead) as BlockReference;
                    if (block == null)
                        continue;
                    if (!block.Name.Equals(targetBlockName, StringComparison.CurrentCultureIgnoreCase))
                        continue;
                    
                    var bounds = block.Bounds;
                    PlotObject obj = new PlotObject();
                    if (bounds.HasValue)
                        obj.Extents = bounds.Value; // Extensions.Strip(bounds.Value);
                    
                    if (obj.Extents.MaxPoint.X <= maxCornerPoint.X &&
                        obj.Extents.MaxPoint.Y <= maxCornerPoint.Y &&
                        obj.Extents.MinPoint.X >= minCornerPoint.X &&
                        obj.Extents.MinPoint.Y >= minCornerPoint.Y)
                    {
                        // find attributes to create the name of file according to it's attribute values
                        foreach (ObjectId attributeId in block.AttributeCollection)
                        {
                            var attribute = tr.GetObject(attributeId, OpenMode.ForRead) as AttributeReference;
                            if (attribute == null)
                                continue;
                            if (attribute.Tag.Equals(prefix, StringComparison.CurrentCultureIgnoreCase))
                            {
                                obj.Prefix = attribute.TextString;
                            }
                            if (attribute.Tag.Equals(postfix, StringComparison.CurrentCultureIgnoreCase))
                                obj.Postfix = attribute.TextString;
                        }
                        plotObjects.Add(obj);
                    }
                }
                plotObjects = SortPlotObjectsByCoordinates(plotObjects, sortingOrder);
                tr.Commit();
            }
            return plotObjects;
        }

        private static List<PlotObject> SortPlotObjectsByCoordinates(List<PlotObject> plotObjects, SortingOrder order)
        {

            // TODO : implement sorting order

            var sortedList = new List<PlotObject>();
            int position;

            int plotObjectsCount = plotObjects.Count;
            for (int i = 0; i < plotObjectsCount; i++)
            {
                double minX = Double.MaxValue;
                double minY = Double.MaxValue;
                position = -1;
                for (int j = 0; j < plotObjects.Count; j++)
                {
                    // check what Y is minimal
                    if (Math.Abs(Math.Abs(plotObjects[j].Extents.MinPoint.Y) - Math.Abs(minY)) > 1e-3)
                    {
                        if (plotObjects[j].Extents.MinPoint.Y < minY)
                        {
                            minX = plotObjects[j].Extents.MinPoint.X;
                            minY = plotObjects[j].Extents.MinPoint.Y;
                            position = j;
                        }
                    }
                    // objects are aligned through X-axis - check what X is minimal
                    else if (Math.Abs(Math.Abs(plotObjects[j].Extents.MinPoint.X) - Math.Abs(minX)) > 1e-3)
                    {
                        if (plotObjects[j].Extents.MinPoint.X < minX)
                        {
                            minX = plotObjects[j].Extents.MinPoint.X;
                            minY = plotObjects[j].Extents.MinPoint.Y;
                            position = j;
                        }
                    }
                    //  objects are aligned through X-axis & Y-axis => same position of objects
                    else
                    {
                        minX = plotObjects[j].Extents.MinPoint.X;
                        minY = plotObjects[j].Extents.MinPoint.Y;
                        position = j;
                    }
                }

                if (plotObjects.Count == 0)
                    break;
                if (position != -1)
                {
                    sortedList.Add(plotObjects[position]);
                    plotObjects.RemoveAt(position);
                }

            }
            return sortedList;
        }
    }
}