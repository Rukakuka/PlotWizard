// ============================================================================
// PlotRegionToPDF.cs
// © Andrey Bushman, 2014
// ============================================================================
// The PLOTREGION command plot a Region object to PDF.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

using acad = Autodesk.AutoCAD.ApplicationServices.Application;
using Ap = Autodesk.AutoCAD.ApplicationServices;
using Db = Autodesk.AutoCAD.DatabaseServices;
using Ed = Autodesk.AutoCAD.EditorInput;
using Rt = Autodesk.AutoCAD.Runtime;
using Gm = Autodesk.AutoCAD.Geometry;
using Us = Autodesk.AutoCAD.DatabaseServices.SymbolUtilityServices;
using Pt = Autodesk.AutoCAD.PlottingServices;

[assembly: Rt.CommandClass(typeof(PrintWizard.PlotWizard))]

namespace PrintWizard
{
    internal class PlotObject
    {
        public String Label { get; set; }
        public String Sheet { get; set; }
        public Extents3d Extents { get; set; }
    }
    internal static class UnmanagedApi
    {
        [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedTrans")]
        internal static extern int AcedTrans(double[] point, IntPtr fromRb, IntPtr toRb, int disp, double[] result);
    }
    public static class PlotWizard
    {        
        public static string MyBlockName { get; set; }
        public static string MyBlockAttrLabel { get; set; }
        public static string MyBLockAttrSheet { get; set; }
        public static double MyViewportScaling { get; set; }
        public static double MyContentScaling { get; set; }
        public static string MyPlotter { get; set; }
        public static string MyPageSize { get; set; }
        private static ObjectIdCollection Layouts { get; set; } = new ObjectIdCollection();
        private const string MyPageStyle = "acad.ctb";

        [Rt.CommandMethod("PLOTWIZARD", Rt.CommandFlags.Modal)]
        public static void Plotwizard()
        {
            MyPlotter = "DWG To PDF.pc3";
            MyPageSize = "ISO_full_bleed_A4_(210.00_x_297.00_MM)";
            MyContentScaling = 1.003;
            MyViewportScaling = 1;
            AddMyRibbonPanel();
        }

        [Rt.CommandMethod("CREATELAYOUTS", Rt.CommandFlags.Modal)]
        public static void CreateLayouts()
        {
            Ap.Document doc = acad.DocumentManager.MdiActiveDocument;
            if (doc == null || doc.IsDisposed)
                return;
            Ed.Editor ed = doc.Editor;

            using (doc.LockDocument())
            {
                List<PlotObject> plotObjects = GetBlockReferencesBoundaries(MyBlockName);
                LayoutCommands lc = new LayoutCommands();
                Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("BACKGROUNDPLOT", 0);

                foreach (var plotObject in plotObjects)
                {
                    ObjectId lay = lc.CreateMyLayout(MyPageSize, MyViewportScaling, MyContentScaling, MyPageStyle, MyPlotter, plotObject);
                    if (!lay.IsNull)
                    {
                        Layouts.Add(lay);
                    }
                }
                ed.WriteMessage($"Создано {plotObjects.Count.ToString()} листа(-ов).\n");
            }
            ed.Regen();
        }

        [Rt.CommandMethod("MULTIPLOT", Rt.CommandFlags.Modal)]
        public static void MultiPlot()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Вывод в файл",
                Filter =  $"{Autodesk.AutoCAD.PlottingServices.PlotConfigManager.CurrentConfig.DeviceName}|*" +
                            $"{Autodesk.AutoCAD.PlottingServices.PlotConfigManager.CurrentConfig.DefaultFileExtension}"
            };
            bool? result = saveFileDialog.ShowDialog();

            if (!result.HasValue || !result.Value)
            {
                doc.Editor.WriteMessage("\nОтмена.\n");
                return;
            }
            MultiSheetPlot.MultiSheetPlotter(MyPageSize, MyPlotter, saveFileDialog.FileName, Layouts);
        }

        [Rt.CommandMethod("ERASEALLLAYOUTS", Rt.CommandFlags.Modal)]
        public static void EraseAllLayouts()
        {
            LayoutCommands.EraseAllLayouts();
            Layouts.Clear();
        }

        private static void AddMyRibbonPanel()
        {
            RibbonCommands rbCommands = new RibbonCommands();
            rbCommands.AddMyRibbonPanel();
        }

        private static List<PlotObject> GetBlockReferencesBoundaries(String targetBlock)
        {
            Document doc = acad.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            // create a collection where stored needed block references
            List<PlotObject> plotObjects = new List<PlotObject>();
            // start transaction
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // get table of all drawing's blocks
                var blockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (blockTable == null)
                    return plotObjects;
                // get table of all drawing's blocks references in current modelspace
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
                    if (!block.Name.Equals(targetBlock, StringComparison.CurrentCultureIgnoreCase))
                        continue;
                    
                    Extents3d? bounds = block.Bounds;
                    PlotObject obj = new PlotObject();
                    if (bounds.HasValue)
                    {
                        obj.Extents = bounds.Value; // Extensions.Strip(bounds.Value);
                    }                    

                    // find attributes to create the name of file according to it's attribute values
                    foreach (ObjectId attributeId in block.AttributeCollection)
                    {
                        var attribute = tr.GetObject(attributeId, OpenMode.ForRead) as AttributeReference;
                        if (attribute == null)
                            continue;
                        if (attribute.Tag.Contains(MyBlockAttrLabel))
                        {
                            obj.Label = attribute.TextString;
                        }
                        if (attribute.Tag.Equals(MyBLockAttrSheet, StringComparison.CurrentCultureIgnoreCase))
                            obj.Sheet = attribute.TextString;
                    }
                    plotObjects.Add(obj);
                }
                tr.Commit();
            }
            return plotObjects;
        }
#pragma warning disable IDE0051 // Remove unused private members
        private static void PlotBlocks(List<PlotObject> plotObjects,
                                                String plotterType,
                                                String sheetType,
                                                String outputPath)
#pragma warning restore IDE0051
        {
            if (plotObjects == null || plotObjects.Count == 0)
                throw new ArgumentException($"Plot object is null - no blocks with name '{MyBlockName}' are found ");
            if (String.IsNullOrEmpty(plotterType))
                throw new ArgumentNullException("Plotter type is empty");
            if (String.IsNullOrEmpty(sheetType))
                throw new ArgumentNullException("Sheet type is empty");
            if (String.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException("Output file name is invalid");

            Ap.Document doc = acad.DocumentManager.MdiActiveDocument;
            if (doc == null || doc.IsDisposed)
                throw new ArgumentNullException("Failed to get Active Mdi document");
            Ed.Editor ed = doc.Editor;
            Db.Database db = doc.Database;
            using (doc.LockDocument())
            {
                using (Db.Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    Db.ObjectId modelId = Us.GetBlockModelSpaceId(db);
                    Db.BlockTableRecord model = tr.GetObject(modelId, Db.OpenMode.ForRead) as Db.BlockTableRecord;
                    Db.Layout layout = tr.GetObject(model.LayoutId, Db.OpenMode.ForRead) as Db.Layout;
                        
                    using (Pt.PlotInfo pi = new Pt.PlotInfo())
                    {
                        pi.Layout = model.LayoutId;
                        using (Db.PlotSettings ps = new Db.PlotSettings(layout.ModelType))
                        {
                            ps.CopyFrom(layout);
                            Db.PlotSettingsValidator psv = Db.PlotSettingsValidator.Current;
                            Db.ResultBuffer rbFrom = new Db.ResultBuffer(new Db.TypedValue(5003, 0));
                            Db.ResultBuffer rbTo = new Db.ResultBuffer(new Db.TypedValue(5003, 2));
                            double[] firres = new double[] { 0, 0, 0 };
                            double[] secres = new double[] { 0, 0, 0 };
                            Gm.Point3d bottomLeft_3d = new Gm.Point3d(0, 0, 0);
                            Gm.Point3d topRight_3d = new Gm.Point3d(297, 210, 0);

                            UnmanagedApi.AcedTrans(bottomLeft_3d.ToArray(), rbFrom.UnmanagedObject, rbTo.UnmanagedObject, 0, firres);
                            UnmanagedApi.AcedTrans(topRight_3d.ToArray(), rbFrom.UnmanagedObject, rbTo.UnmanagedObject, 0, secres);

                            Db.Extents2d extents = new Db.Extents2d(firres[0], firres[1], secres[0], secres[1]);
                            psv.SetPlotWindowArea(ps, extents);
                            psv.SetZoomToPaperOnUpdate(ps, true);
                            psv.SetPlotType(ps, Db.PlotType.Window);
                            psv.SetUseStandardScale(ps, true);
                            psv.SetStdScaleType(ps, Db.StdScaleType.ScaleToFit);
                            psv.SetPlotCentered(ps, true);
                            psv.SetPlotRotation(ps, Db.PlotRotation.Degrees000);
                            // We'll use the standard DWF PC3, as
                            // for today we're just plotting to file
                            psv.SetPlotConfigurationName(ps, plotterType, sheetType);
                            // We need to link the PlotInfo to the
                            // PlotSettings and then validate it
                            pi.OverrideSettings = ps;
                            Pt.PlotInfoValidator piv = new Pt.PlotInfoValidator
                            {
                                MediaMatchingPolicy = Pt.MatchingPolicy.MatchEnabled
                            };
                            piv.Validate(pi);
                            // A PlotEngine does the actual plotting
                            // (can also create one for Preview)
                            if (Pt.PlotFactory.ProcessPlotState == Pt.ProcessPlotState.NotPlotting)
                            {
                                using (Pt.PlotEngine pe = Pt.PlotFactory.CreatePublishEngine())
                                {
                                    ObjectIdCollection layoutsToPlot = new ObjectIdCollection();
                                    // Create a Progress Dialog to provide info
                                    // and allow thej user to cancel
                                    foreach (ObjectId btrId in bt)
                                    {
                                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
                                        if (String.Compare(btr.Name, MyBlockName) == 0) ;
                                        layoutsToPlot.Add(btrId);
                                    }

                                    int numSheet = 1;
                                    using (Pt.PlotProgressDialog ppd = new Pt.PlotProgressDialog(false, 1, true))
                                    {
                                        ppd.set_PlotMsgString(
                                        Pt.PlotMessageIndex.DialogTitle, "Custom Plot Progress");
                                        ppd.set_PlotMsgString(Pt.PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");
                                        ppd.set_PlotMsgString(Pt.PlotMessageIndex.CancelSheetButtonMessage, "Cancel Sheet");
                                        ppd.set_PlotMsgString(Pt.PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
                                        ppd.set_PlotMsgString(Pt.PlotMessageIndex.SheetProgressCaption, "Sheet Progress");
                                        ppd.LowerPlotProgressRange = 0;
                                        ppd.UpperPlotProgressRange = 100;
                                        ppd.PlotProgressPos = 0;
                                        // Let's start the plot, at last
                                        ppd.OnBeginPlot();
                                        ppd.IsVisible = true;
                                        pe.BeginPlot(ppd, null);
                                        // We'll be plotting a single document
                                        pe.BeginDocument(pi, doc.Name, null, 1, true,
                                         // Let's plot to file
                                         outputPath + "\\1.pdf");
                                        // Which contains a single sheet
                                        ppd.OnBeginSheet();
                                        ppd.LowerSheetProgressRange = 0;
                                        ppd.UpperSheetProgressRange = 100;
                                        ppd.SheetProgressPos = 0;
                                        Pt.PlotPageInfo ppi = new Pt.PlotPageInfo();
                                        pe.BeginPage(ppi, pi, true, null);
                                        pe.BeginGenerateGraphics(null);
                                        pe.EndGenerateGraphics(null);
                                        // Finish the sheet
                                        pe.EndPage(null);
                                        ppd.SheetProgressPos = 100;
                                        ppd.OnEndSheet();
                                        // Finish the document
                                        pe.EndDocument(null);
                                        // And finish the plot
                                        ppd.PlotProgressPos = 100;
                                        ppd.OnEndPlot();
                                        pe.EndPlot(null);
                                    }
                                }
                            }
                            else
                            {
                                ed.WriteMessage("\nAnother plot is in progress.");
                            }
                        }
                    }
                    tr.Commit();
                }
            }
        }
    }
}