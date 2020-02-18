// ============================================================================
// PlotRegionToPDF.cs
// © Andrey Bushman, 2014
// ============================================================================
// The PLOTREGION command plot a Region object to PDF.
// ============================================================================
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

[assembly: Rt.CommandClass(typeof(PrintWizard.MultiPlotter))]

namespace PrintWizard
{
    public class CustomObjectId
    {
        public String label { get; set; }
        public String sheet { get; set; }
        public ObjectId plotObject { get; set; }
    }
    public static class MultiPlotter
    {
#if AUTOCAD_NEWER_THAN_2012
    const String acedTransOwner = "accore.dll";
#else
        const String acedTransOwner = "accore.dll";
#endif

#if AUTOCAD_NEWER_THAN_2014
    const String acedTrans_x86_Prefix = "_";
#else
        const String acedTrans_x86_Prefix = "";
#endif

        const String acedTransName = "acedTrans";

        [DllImport(acedTransOwner, CallingConvention = CallingConvention.Cdecl,
                EntryPoint = acedTrans_x86_Prefix + acedTransName)]
        static extern Int32 acedTrans_x86(Double[] point, IntPtr fromRb,
          IntPtr toRb, Int32 disp, Double[] result);

        [DllImport(acedTransOwner, CallingConvention = CallingConvention.Cdecl,
                EntryPoint = acedTransName)]
        static extern Int32 acedTrans_x64(Double[] point, IntPtr fromRb,
          IntPtr toRb, Int32 disp, Double[] result);

        public static Int32 acedTrans(Double[] point, IntPtr fromRb, IntPtr toRb,
          Int32 disp, Double[] result)
        {
            if (IntPtr.Size == 4)
                return acedTrans_x86(point, fromRb, toRb, disp, result);
            else
                return acedTrans_x64(point, fromRb, toRb, disp, result);
        }

        private const string MyBlock_Name = "ListSPDSF6_A42"; // standart GOST A4
        private const string MyBlockAttr_Label = "ЧЕРТНИЗ";
        private const string MyBLockAttr_Sheet = "ЛИСТ";

        [Rt.CommandMethod("PlotWizard", Rt.CommandFlags.Modal)]
        public static void MyPlot()
        {
            Ap.Document doc = acad.DocumentManager.MdiActiveDocument;
            if (doc == null || doc.IsDisposed)
                return;

            Ed.Editor ed = doc.Editor;
            Db.Database db = doc.Database;

            using (doc.LockDocument())
            {
                /*============= PLOT BY REGION ==============*/
                /*
                Ed.PromptEntityOptions peo = new Ed.PromptEntityOptions(
                 "\nSelect a region"
                 );

                peo.SetRejectMessage("\nIt is not a region."
                  );
                peo.AddAllowedClass(typeof(Db.Region), false);

                Ed.PromptEntityResult per = ed.GetEntity(peo);

                if (per.Status != Ed.PromptStatus.OK)
                {
                    ed.WriteMessage("\nCommand canceled.\n");
                    return;
                }

                Db.ObjectId regionId = per.ObjectId;
                */
                /*============= USER FILE NAME INPUT DIALOG ==============*/
                /*
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32
                  .SaveFileDialog();
                saveFileDialog.Title =
                  "PDF file name";
                saveFileDialog.Filter = "PDF-files|*.pdf";
                bool? result = saveFileDialog.ShowDialog();

                if (!result.HasValue || !result.Value)
                {
                    ed.WriteMessage("\nCommand canceled.");
                    return;
                }

                String pdfFileName = saveFileDialog.FileName;
                */

                String MySavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                List<CustomObjectId> collection = LoadBlocks();

                //foreach (var plot in plotinfo)
                //{

                //String pdfFileName = MySavePath + plot.label + " Лист " + plot.sheet;

                PlotBlocks(collection,
                    "DWG To PDF.pc3",
                    "ISO_full_bleed_A4_(297.00_x_210.00_MM)",
                    MySavePath);

                ed.WriteMessage("\nThe \"{0}\" file created.\n", "");
                //}
                /*
                PlotRegion(regionId, "DWG To PDF.pc3",
                  "ISO_A4_(210.00_x_297.00_MM)", pdfFileName);
                  */

            }
        }
        private static List<CustomObjectId> LoadBlocks()
        {
            Document doc = acad.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            // create a collection where stored needed block references
            List<CustomObjectId> collection = new List<CustomObjectId>();

            // start transaction
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // get table of all drawing's blocks
                var blockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (blockTable == null)
                    return collection;

                // get table of all drawing's blocks references in current modelspace
                var blockTableRecord = tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                if (blockTableRecord == null)
                    return collection;

                // iterate through all blocks' references in current modelspace
                foreach (ObjectId blockRecord in blockTableRecord)
                {
                    CustomObjectId obj = new CustomObjectId();
                    // get single block reference
                    var block = tr.GetObject(blockRecord, OpenMode.ForRead) as BlockReference;
                    if (block == null)
                        continue;
                    if (!block.Name.Equals(MyBlock_Name, StringComparison.CurrentCultureIgnoreCase))
                        continue;
                    /*
                    Extents3d? bounds = block.Bounds;
                    plotObject pinfo = new plotObject();

                    if (bounds.HasValue)
                    {
                        Extents3d ext = bounds.Value;
                        pinfo.xmax = ext.MaxPoint.X;
                        pinfo.ymax = ext.MaxPoint.Y;
                        pinfo.xmin = ext.MinPoint.X;
                        pinfo.ymin = ext.MinPoint.Y;
                    }
                    */

                    obj.plotObject = block.Id;

                    // find attributes to create the name of file according to it's attribute values
                    foreach (ObjectId attributeId in block.AttributeCollection)
                    {
                        var attribute = tr.GetObject(attributeId, OpenMode.ForRead) as AttributeReference;
                        if (attribute == null) continue;

                        if (attribute.Tag.Contains(MyBlockAttr_Label))
                            obj.label = attribute.TextString;
                        if (attribute.Tag.Equals(MyBLockAttr_Sheet, StringComparison.CurrentCultureIgnoreCase))
                            obj.sheet = attribute.TextString;
                    }

                    collection.Add(obj);
                }
                tr.Commit();
            }

            System.Windows.MessageBox.Show(collection.Count.ToString() + " block references were found in current modelspace.");
            return collection;
        }

        public static void PlotBlocks(List<CustomObjectId> plotObjects,
            String plotterType,
            String sheetType,
            String outputPath)
        {
            if (plotObjects == null || plotObjects.Count == 0)
                throw new ArgumentException($"Plot object is null - no blocks with name '{MyBlock_Name}' are found ");

            if (String.IsNullOrEmpty(plotterType))
                throw new ArgumentNullException("Plotter type is empty");

            if (String.IsNullOrEmpty(sheetType))
                throw new ArgumentNullException("Sheet type is empty");

            if (String.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException("Output file name is invalid");

            Db.Database previewDb = Hs.WorkingDatabase;
            Db.Database db = null;
            Ap.Document doc = acad.DocumentManager.MdiActiveDocument;
            if (doc == null || doc.IsDisposed)
                throw new ArgumentNullException("Failed to get Active Mdi document");
            Ed.Editor ed = doc.Editor;
            
            db = doc.Database;
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

                            acedTrans(bottomLeft_3d.ToArray(), rbFrom.UnmanagedObject,
                              rbTo.UnmanagedObject, 0, firres);
                            acedTrans(topRight_3d.ToArray(), rbFrom.UnmanagedObject,
                              rbTo.UnmanagedObject, 0, secres);

                            Db.Extents2d extents = new Db.Extents2d(
                                firres[0],
                                firres[1],
                                secres[0],
                                secres[1]
                              );
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
                            Pt.PlotInfoValidator piv = new Pt.PlotInfoValidator();
                            piv.MediaMatchingPolicy = Pt.MatchingPolicy.MatchEnabled;
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
                                        if(String.Compare(btr.Name, MyBlock_Name) == 0);
                                            layoutsToPlot.Add(btrId);
                                    }
                                    ed.WriteMessage(layoutsToPlot.Count.ToString());
                                    
                                    int numSheet = 1;

                                    using (Pt.PlotProgressDialog ppd = new Pt.PlotProgressDialog(false, 1, true))
                                    {
                                        ppd.set_PlotMsgString(
                                        Pt.PlotMessageIndex.DialogTitle, "Custom Plot Progress");

                                        ppd.set_PlotMsgString(
                                          Pt.PlotMessageIndex.CancelJobButtonMessage,
                                          "Cancel Job");

                                        ppd.set_PlotMsgString(
                                        Pt.PlotMessageIndex.CancelSheetButtonMessage,
                                        "Cancel Sheet");

                                        ppd.set_PlotMsgString(
                                        Pt.PlotMessageIndex.SheetSetProgressCaption,
                                        "Sheet Set Progress");

                                        ppd.set_PlotMsgString(
                                          Pt.PlotMessageIndex.SheetProgressCaption,
                                         "Sheet Progress");

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
        //Ed.Editor ed = doc.Editor;
        //try
        //{
        //    db = doc.Database;
        //    using (doc.LockDocument())
        //    {
        //        using (Db.Transaction tr = db.TransactionManager.StartTransaction())
        //        {
        //            BlockTable blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        //            PlotInfo plotInfo = new PlotInfo();
        //            PlotInfoValidator plotInfoValidator = new PlotInfoValidator();
        //            plotInfoValidator.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;
        //            // A PlotEngine does the actual plotting
        //            // (can also create one for Preview)

        //            if (Pt.PlotFactory.ProcessPlotState == Pt.ProcessPlotState.NotPlotting)
        //            {
        //                using (PlotEngine plotEngine = PlotFactory.CreatePublishEngine())
        //                {
        //                    // Create a Progress Dialog to provide info
        //                    // and allow thej user to cancel

        //                    using (Pt.PlotProgressDialog ppd = new Pt.PlotProgressDialog(false, plotObjects.Count, true))
        //                    {
        //                        int numSheet = 1;
        //                        ObjectIdCollection layoutsToPlot = new ObjectIdCollection();

        //                        foreach (ObjectId btrId in blockTable)
        //                        { 
        //                            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);

        //                            if (btr.IsLayout &&
        //                                btr.Name.ToUpper() != BlockTableRecord.ModelSpace.ToUpper())
        //                            {
        //                                layoutsToPlot.Add(btrId);
        //                            }
        //                        }
        //                        ed.WriteMessage(layoutsToPlot.Count.ToString() + " blocks found");
        //                        //foreach (CustomObjectId plotObj in plotObjects)
        //                        foreach (ObjectId plotObj in layoutsToPlot)
        //                        {
        //                            BlockTableRecord btr = tr.GetObject(plotObj, OpenMode.ForRead) as BlockTableRecord;
        //                            Layout lo = (Layout)tr.GetObject( btr.LayoutId, OpenMode.ForRead );
        //                            // We need a PlotSettings object
        //                            // based on the layout settings
        //                            // which we then customize
        //                            PlotSettings ps = new PlotSettings(lo.ModelType);
        //                            ps.CopyFrom(lo);
        //                            // The PlotSettingsValidator helps
        //                            // create a valid PlotSettings object

        //                            PlotSettingsValidator psv =
        //                              PlotSettingsValidator.Current;
        //                            // We'll plot the extents, centered and
        //                            // scaled to fit
        //                            psv.SetPlotType( ps, Autodesk.AutoCAD.DatabaseServices.PlotType.Extents );
        //                            psv.SetUseStandardScale(ps, true);
        //                            psv.SetStdScaleType(ps, StdScaleType.ScaleToFit);
        //                            psv.SetPlotCentered(ps, true);
        //                            // We'll use the standard DWFx PC3, as
        //                            // this supports multiple sheets
        //                            psv.SetPlotConfigurationName(
        //                              ps,
        //                              plotterType,
        //                              sheetType
        //                            );
        //                            // We need a PlotInfo object
        //                            // linked to the layout
        //                            plotInfo.Layout = btr.LayoutId;

        //                            // Make the layout we're plotting current
        //                            LayoutManager.Current.CurrentLayout = lo.LayoutName;
        //                            // We need to link the PlotInfo to the
        //                            // PlotSettings and then validate it

        //                            plotInfo.OverrideSettings = ps;
        //                            plotInfoValidator.Validate(plotInfo);

        //                            if (numSheet == 1)
        //                            {
        //                                ppd.set_PlotMsgString( PlotMessageIndex.DialogTitle, "Custom Plot Progress");
        //                                ppd.set_PlotMsgString( PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");
        //                                ppd.set_PlotMsgString( PlotMessageIndex.CancelSheetButtonMessage,"Cancel Sheet");
        //                                ppd.set_PlotMsgString( PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
        //                                ppd.set_PlotMsgString( PlotMessageIndex.SheetProgressCaption, "Sheet Progress");
        //                                ppd.LowerPlotProgressRange = 0;
        //                                ppd.UpperPlotProgressRange = 100;
        //                                ppd.PlotProgressPos = 0;

        //                                // Let's start the plot, at last
        //                                ppd.OnBeginPlot();
        //                                ppd.IsVisible = true;
        //                                plotEngine.BeginPlot(ppd, null);
        //                                // We'll be plotting a single document
        //                                plotEngine.BeginDocument( plotInfo, doc.Name, null, 1, true,
        //                                    outputPath + "\\" + "labelname" + "Лист " + "sheet .pdf");
        //                            }
        //                            // Which may contain multiple sheets
        //                            ppd.StatusMsgString = "Plotting " + doc.Name.Substring(doc.Name.LastIndexOf("\\") + 1) +
        //                              " - sheet " + numSheet.ToString() + " of " + plotObjects.Count.ToString();
        //                            ppd.OnBeginSheet();
        //                            ppd.LowerSheetProgressRange = 0;
        //                            ppd.UpperSheetProgressRange = 100;
        //                            ppd.SheetProgressPos = 0;
        //                            PlotPageInfo ppi = new PlotPageInfo();
        //                            plotEngine.BeginPage(
        //                              ppi,
        //                              plotInfo,
        //                              (numSheet == plotObjects.Count),
        //                              null
        //                            );
        //                            plotEngine.BeginGenerateGraphics(null);
        //                            ppd.SheetProgressPos = 50;
        //                            plotEngine.EndGenerateGraphics(null);
        //                            // Finish the sheet
        //                            plotEngine.EndPage(null);
        //                            ppd.SheetProgressPos = 100;
        //                            ppd.OnEndSheet();
        //                            numSheet++;
        //                        }
        //                        // Finish the document
        //                        plotEngine.EndDocument(null);
        //                        // And finish the plot
        //                        ppd.PlotProgressPos = 100;
        //                        ppd.OnEndPlot();
        //                        plotEngine.EndPlot(null);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                ed.WriteMessage("\nAnother plot is in progress.");
        //            }
        //        }
        //    }
        //}
        //finally
        //{
        //    Hs.WorkingDatabase = previewDb;
        //}

        //tr.Commit();
        // This method helps to get correct "boundary box" for the regions which 
        // created through the splines. Written by Alexander Rivilis.
        public static void GetVisualBoundary(this Db.Region region, double delta,
            ref Gm.Point2d minPoint, ref Gm.Point2d maxPoint)
        {
            using (Gm.BoundBlock3d boundBlk = new Gm.BoundBlock3d())
            {
                using (Br.Brep brep = new Br.Brep(region))
                {
                    foreach (Br.Edge edge in brep.Edges)
                    {
                        using (Gm.Curve3d curve = edge.Curve)
                        {
                            Gm.ExternalCurve3d curve3d = curve as Gm.ExternalCurve3d;

                            if (curve3d != null && curve3d.IsNurbCurve)
                            {
                                using (Gm.NurbCurve3d nurbCurve = curve3d.NativeCurve
                                  as Gm.NurbCurve3d)
                                {
                                    Gm.Interval interval = nurbCurve.GetInterval();
                                    for (double par = interval.LowerBound; par <=
                                      interval.UpperBound; par += (delta * 2.0))
                                    {
                                        Gm.Point3d p = nurbCurve.EvaluatePoint(par);
                                        if (!boundBlk.IsBox)
                                            boundBlk.Set(p, p);
                                        else
                                            boundBlk.Extend(p);
                                    }
                                }
                            }
                            else
                            {
                                if (!boundBlk.IsBox)
                                {
                                    boundBlk.Set(edge.BoundBlock.GetMinimumPoint(),
                                      edge.BoundBlock.GetMaximumPoint());
                                }
                                else
                                {
                                    boundBlk.Extend(edge.BoundBlock.GetMinimumPoint());
                                    boundBlk.Extend(edge.BoundBlock.GetMaximumPoint());
                                }
                            }
                        }
                    }
                }
                boundBlk.Swell(delta);

                minPoint = new Gm.Point2d(boundBlk.GetMinimumPoint().X,
                  boundBlk.GetMinimumPoint().Y);
                maxPoint = new Gm.Point2d(boundBlk.GetMaximumPoint().X,
                  boundBlk.GetMaximumPoint().Y);
            }
        }

        // This code based on Kean Walmsley's article:
        // http://through-the-interface.typepad.com/through_the_interface/2007/10/plotting-a-wind.html
        public static void PlotRegion(Db.ObjectId regionId, String pcsFileName,
      String mediaName, String outputFileName)
        {

            if (regionId.IsNull)
                throw new ArgumentException("regionId.IsNull == true");
            if (!regionId.IsValid)
                throw new ArgumentException("regionId.IsValid == false");

            if (regionId.ObjectClass.Name != "AcDbRegion")
                throw new ArgumentException("regionId.ObjectClass.Name != AcDbRegion");

            if (pcsFileName == null)
                throw new ArgumentNullException("pcsFileName");
            if (pcsFileName.Trim() == String.Empty)
                throw new ArgumentException("pcsFileName.Trim() == String.Empty");

            if (mediaName == null)
                throw new ArgumentNullException("mediaName");
            if (mediaName.Trim() == String.Empty)
                throw new ArgumentException("mediaName.Trim() == String.Empty");

            if (outputFileName == null)
                throw new ArgumentNullException("outputFileName");
            if (outputFileName.Trim() == String.Empty)
                throw new ArgumentException("outputFileName.Trim() == String.Empty");

            Db.Database previewDb = Hs.WorkingDatabase;
            Db.Database db = null;
            Ap.Document doc = acad.DocumentManager.MdiActiveDocument;
            if (doc == null || doc.IsDisposed)
                return;

            Ed.Editor ed = doc.Editor;
            try
            {
                if (regionId.Database != null && !regionId.Database.IsDisposed)
                {
                    Hs.WorkingDatabase = regionId.Database;
                    db = regionId.Database;
                }
                else
                {
                    db = doc.Database;
                }

                using (doc.LockDocument())
                {
                    using (Db.Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        Db.Region region = tr.GetObject(regionId,
                        Db.OpenMode.ForRead) as Db.Region;

                        Db.Extents3d extends = region.GeometricExtents;
                        Db.ObjectId modelId = Us.GetBlockModelSpaceId(db);
                        Db.BlockTableRecord model = tr.GetObject(modelId,
                        Db.OpenMode.ForRead) as Db.BlockTableRecord;

                        Db.Layout layout = tr.GetObject(model.LayoutId,
                        Db.OpenMode.ForRead) as Db.Layout;

                        using (Pt.PlotInfo pi = new Pt.PlotInfo())
                        {
                            pi.Layout = model.LayoutId;

                            using (Db.PlotSettings ps = new Db.PlotSettings(layout.ModelType)
                              )
                            {

                                ps.CopyFrom(layout);

                                Db.PlotSettingsValidator psv = Db.PlotSettingsValidator
                                  .Current;

                                Gm.Point2d bottomLeft = Gm.Point2d.Origin;
                                Gm.Point2d topRight = Gm.Point2d.Origin;

                                region.GetVisualBoundary(0.1, ref bottomLeft,
                                  ref topRight);

                                Gm.Point3d bottomLeft_3d = new Gm.Point3d(bottomLeft.X,
                                  bottomLeft.Y, 0);
                                Gm.Point3d topRight_3d = new Gm.Point3d(topRight.X, topRight.Y,
                                  0);

                                Db.ResultBuffer rbFrom = new Db.ResultBuffer(new Db.TypedValue(
                                  5003, 0));
                                Db.ResultBuffer rbTo = new Db.ResultBuffer(new Db.TypedValue(
                                  5003, 2));

                                double[] firres = new double[] { 0, 0, 0 };
                                double[] secres = new double[] { 0, 0, 0 };

                                acedTrans(bottomLeft_3d.ToArray(), rbFrom.UnmanagedObject,
                                  rbTo.UnmanagedObject, 0, firres);
                                acedTrans(topRight_3d.ToArray(), rbFrom.UnmanagedObject,
                                  rbTo.UnmanagedObject, 0, secres);

                                Db.Extents2d extents = new Db.Extents2d(
                                    firres[0],
                                    firres[1],
                                    secres[0],
                                    secres[1]
                                  );

                                psv.SetZoomToPaperOnUpdate(ps, true);

                                psv.SetPlotWindowArea(ps, extents);
                                psv.SetPlotType(ps, Db.PlotType.Window);
                                psv.SetUseStandardScale(ps, true);
                                psv.SetStdScaleType(ps, Db.StdScaleType.ScaleToFit);
                                psv.SetPlotCentered(ps, true);
                                psv.SetPlotRotation(ps, Db.PlotRotation.Degrees000);

                                // We'll use the standard DWF PC3, as
                                // for today we're just plotting to file
                                psv.SetPlotConfigurationName(ps, pcsFileName, mediaName);

                                // We need to link the PlotInfo to the
                                // PlotSettings and then validate it
                                pi.OverrideSettings = ps;
                                Pt.PlotInfoValidator piv = new Pt.PlotInfoValidator();
                                piv.MediaMatchingPolicy = Pt.MatchingPolicy.MatchEnabled;
                                piv.Validate(pi);

                                // A PlotEngine does the actual plotting
                                // (can also create one for Preview)
                                if (Pt.PlotFactory.ProcessPlotState == Pt.ProcessPlotState
                                  .NotPlotting)
                                {
                                    using (Pt.PlotEngine pe = Pt.PlotFactory.CreatePublishEngine()
                                      )
                                    {
                                        // Create a Progress Dialog to provide info
                                        // and allow thej user to cancel

                                        using (Pt.PlotProgressDialog ppd =
                                          new Pt.PlotProgressDialog(false, 1, true))
                                        {
                                            ppd.set_PlotMsgString(
                                            Pt.PlotMessageIndex.DialogTitle, "Custom Plot Progress");

                                            ppd.set_PlotMsgString(
                                              Pt.PlotMessageIndex.CancelJobButtonMessage,
                                              "Cancel Job");

                                            ppd.set_PlotMsgString(
                                            Pt.PlotMessageIndex.CancelSheetButtonMessage,
                                            "Cancel Sheet");

                                            ppd.set_PlotMsgString(
                                            Pt.PlotMessageIndex.SheetSetProgressCaption,
                                            "Sheet Set Progress");

                                            ppd.set_PlotMsgString(
                                              Pt.PlotMessageIndex.SheetProgressCaption,
                                             "Sheet Progress");

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
                                             outputFileName);
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
            finally
            {
                Hs.WorkingDatabase = previewDb;
            }
        }
    }
}