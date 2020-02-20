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
    public static class Extensions
    {
        /// <summary>
        /// Reverses the order of the X and Y properties of a Point2d.
        /// </summary>
        /// <param name="flip">Boolean indicating whether to reverse or not.</param>
        /// <returns>The original Point2d or the reversed version.</returns>
        public static Point2d Swap(this Point2d pt, bool flip = true)
        {
            return flip ? new Point2d(pt.Y, pt.X) : pt;
        }
        /// <summary>
        /// Pads a Point2d with a zero Z value, returning a Point3d.
        /// </summary>
        /// <param name="pt">The Point2d to pad.</param>
        /// <returns>The padded Point3d.</returns>
        public static Point3d Pad(this Point2d pt)
        {
            return new Point3d(pt.X, pt.Y, 0);
        }
        /// <summary>
        /// Strips a Point3d down to a Point2d by simply ignoring the Z ordinate.
        /// </summary>
        /// <param name="pt">The Point3d to strip.</param>
        /// <returns>The stripped Point2d.</returns>
        public static Point2d Strip(this Point3d pt)
        {
            return new Point2d(pt.X, pt.Y);
        }
        /// <summary>
        /// Creates a layout with the specified name and optionally makes it current.
        /// </summary>
        /// <param name="name">The name of the viewport.</param>
        /// <param name="select">Whether to select it.</param>
        /// <returns>The ObjectId of the newly created viewport.</returns>
        public static Extents2d Strip(this Extents3d ex)
        {
            return new Extents2d(ex.MinPoint.X, ex.MinPoint.Y, ex.MaxPoint.X, ex.MaxPoint.Y);
        }
        public static ObjectId CreateAndMakeLayoutCurrent(
          this LayoutManager lm, string name, bool select = true
        )
        {
            // First try to get the layout
            var id = lm.GetLayoutId(name);
            // If it doesn't exist, we create it
            if (!id.IsValid)
            {
                id = lm.CreateLayout(name);
            }
            // And finally we select it
            if (select)
            {
                lm.CurrentLayout = name;
            }
            return id;
        }
        /// <summary>
        /// Applies an action to the specified viewport from this layout.
        /// Creates a new viewport if none is found withthat number.
        /// </summary>
        /// <param name="tr">The transaction to use to open the viewports.</param>
        /// <param name="vpNum">The number of the target viewport.</param>
        /// <param name="f">The action to apply to each of the viewports.</param>
        public static void ApplyToViewport(
          this Layout lay, Transaction tr, int vpNum, Action<Viewport> f
        )
        {
            var vpIds = lay.GetViewports();
            Viewport vp = null;
            foreach (ObjectId vpId in vpIds)
            {
                var vp2 = tr.GetObject(vpId, OpenMode.ForWrite) as Viewport;
                if (vp2 != null && vp2.Number == vpNum)
                {
                    // We have found our viewport, so call the action
                    vp = vp2;
                    break;
                }
            }
            if (vp == null)
            {
                // We have not found our viewport, so create one
                var btr =
                  (BlockTableRecord)tr.GetObject(
                    lay.BlockTableRecordId, OpenMode.ForWrite
                  );
                vp = new Viewport();
                // Add it to the database
                btr.AppendEntity(vp);
                tr.AddNewlyCreatedDBObject(vp, true);
                // Turn it - and its grid - on
                vp.On = true;
                vp.GridOn = true;
            }
            // Finally we call our function on it
            f(vp);
        }
        /// <summary>
        /// Apply plot settings to the provided layout.
        /// </summary>
        /// <param name="pageSize">The canonical media name for our page size.</param>
        /// <param name="styleSheet">The pen settings file (ctb or stb).</param>
        /// <param name="devices">The name of the output device.</param>
        public static void SetPlotSettings(
          this Layout lay, string pageSize, string styleSheet, string device
        )
        {
            using (var ps = new PlotSettings(lay.ModelType))
            {
                ps.CopyFrom(lay);
                
                var psv = PlotSettingsValidator.Current;
                // Set the device
                var devs = psv.GetPlotDeviceList();
                psv.SetPlotRotation(ps,PlotRotation.Degrees000);
                if (devs.Contains(device))
                {
                    psv.SetPlotConfigurationName(ps, device, null);
                    psv.RefreshLists(ps);
                }
                // Set the media name/size
                var mns = psv.GetCanonicalMediaNameList(ps);
                if (mns.Contains(pageSize))
                {
                    psv.SetCanonicalMediaName(ps, pageSize);
                }
                // Set the pen settings
                var ssl = psv.GetPlotStyleSheetList();
                if (ssl.Contains(styleSheet))
                {
                    psv.SetCurrentStyleSheet(ps, styleSheet);
                }
                
                // Copy the PlotSettings data back to the Layout
                var upgraded = false;
                if (!lay.IsWriteEnabled)
                {
                    lay.UpgradeOpen();
                    upgraded = true;
                }
                lay.CopyFrom(ps);
                if (upgraded)
                {
                    lay.DowngradeOpen();
                }
            }
        }

        /// <summary>
            /// Determine the maximum possible size for this layout.
            /// </summary>
            /// <returns>The maximum extents of the viewport on this layout.</returns>
        public static Extents2d GetMaximumExtents(this Layout lay)
        {
            // If the drawing template is imperial, we need to divide by
            // 1" in mm (25.4)
            var div = lay.PlotPaperUnits == PlotPaperUnit.Inches ? 25.4 : 1.0;
            // We need to flip the axes if the plot is rotated by 90 or 270 deg
            var doIt =
              lay.PlotRotation == PlotRotation.Degrees090 ||
              lay.PlotRotation == PlotRotation.Degrees270;
            // Get the extents in the correct units and orientation
            var min = lay.PlotPaperMargins.MinPoint.Swap(doIt) / div;
            var max =
              (lay.PlotPaperSize.Swap(doIt) -
               lay.PlotPaperMargins.MaxPoint.Swap(doIt).GetAsVector()) / div;
            return new Extents2d(min, max);
        }
        /// <summary>
        /// Sets the size of the viewport according to the provided extents.
        /// </summary>
        /// <param name="ext">The extents of the viewport on the page.</param>
        /// <param name="fac">Optional factor to provide padding.</param>
        public static void ResizeViewport(
          this Viewport vp, Extents2d ext, double fac = 1.0
        )
        {
            vp.Width = (ext.MaxPoint.X - ext.MinPoint.X) * fac;
            vp.Height = (ext.MaxPoint.Y - ext.MinPoint.Y) * fac;
            vp.CenterPoint =
              (Point2d.Origin + (ext.MaxPoint - ext.MinPoint) * 0.5).Pad();
        }
        /// <summary>
        /// Sets the view in a viewport to contain the specified model extents.
        /// </summary>
        /// <param name="ext">The extents of the content to fit the viewport.</param>
        /// <param name="fac">Optional factor to provide padding.</param>
        public static void FitContentToViewport(
          this Viewport vp, Extents3d ext, double fac = 1.0
        )
        {
            // Let's zoom to just larger than the extents
            vp.ViewCenter =
              (ext.MinPoint + ((ext.MaxPoint - ext.MinPoint) * 0.5)).Strip();
            // Get the dimensions of our view from the database extents
            var hgt = ext.MaxPoint.Y - ext.MinPoint.Y;
            var wid = ext.MaxPoint.X - ext.MinPoint.X;
            // We'll compare with the aspect ratio of the viewport itself
            // (which is derived from the page size)
            var aspect = vp.Width / vp.Height;
            // If our content is wider than the aspect ratio, make sure we
            // set the proposed height to be larger to accommodate the
            // content
            if (wid / hgt > aspect)
            {
                hgt = wid / aspect;
            }
            // Set the height so we're exactly at the extents
            vp.ViewHeight = hgt;
            // Set a custom scale to zoom out slightly (could also
            // vp.ViewHeight *= 1.1, for instance)
            vp.CustomScale *= fac;
        }
    }
    public class PlotObject
    {
        public String label { get; set; }
        public String sheet { get; set; }
        public Extents3d extents { get; set; }
    }

    public class LayoutCommand
    {
        public void CreateMyLayout(string pageSize, string styleSheet, string plotter, PlotObject plotObject)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;
            var db = doc.Database;
            var ed = doc.Editor;
            var ext = new Extents2d();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                // Create and select a new layout tab
                var id = LayoutManager.Current.CreateAndMakeLayoutCurrent(plotObject.label);
                // Open the created layout
                var lay = (Layout)tr.GetObject(id, OpenMode.ForWrite);
                // Make some settings on the layout and get its extents
                lay.SetPlotSettings(
                  //"ANSI_B_(11.00_x_17.00_Inches)",
                  //"monochrome.ctb", //"acad.ctb",
                  //"DWF6 ePlot.pc3"
                  pageSize, styleSheet, plotter
                );

                ext = Extensions.Strip(plotObject.extents); //lay.GetMaximumExtents();
                ed.WriteMessage(lay.PlotRotation.ToString());
                lay.ApplyToViewport(
                  tr, 2,
                  vp =>
                  {
                      // Size the viewport according to the extents calculated when
                      // we set the PlotSettings (device, page size, etc.)
                      // Use the standard 10% margin around the viewport
                      // (found by measuring pixels on screenshots of Layout1, etc.)
                      vp.ResizeViewport(ext, 0.95);
                      // Adjust the view so that the model contents fit
                      if (ValidDbExtents(db.Extmin, db.Extmax))
                      {
                          vp.FitContentToViewport(plotObject.extents, 1);//(new Extents3d(db.Extmin, db.Extmax), 0.98);
                      }
                      
                      // Finally we lock the view to prevent meddling
                      vp.Locked = true;
                  }
                );
                // Commit the transaction
                tr.Commit();
            }
            // Zoom so that we can see our new layout, again with a little padding
            //ed.Command("_.ZOOM", "_E");
            //ed.Command("_.ZOOM", ".7X");
            //ed.Regen();
        }
        // Returns whether the provided DB extents - retrieved from
        // Database.Extmin/max - are "valid" or whether they are the default
        // invalid values (where the min's coordinates are positive and the
        // max coordinates are negative)
        private bool ValidDbExtents(Point3d min, Point3d max)
        {
            return
              !(min.X > 0 && min.Y > 0 && min.Z > 0 &&
                max.X < 0 && max.Y < 0 && max.Z < 0);
        }
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

        private const string MyBlock_Name = "ListSPDSF6_A42";
        private const string MyBlockAttr_Label = "ЧЕРТНИЗ";
        private const string MyBLockAttr_Sheet = "ЛИСТ";
        private const string MyPlotter = "DWG To PDF.pc3";
        private const string MyPageSize = "ISO_full_bleed_A4_(297.00_x_210.00_MM)";
        private const string MyPageStyle = "acad.ctb";
        static public void MultiSheetPlot()
        {
            Document doc =
              Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            Transaction tr =
              db.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt =
                  (BlockTable)tr.GetObject(
                    db.BlockTableId,
                    OpenMode.ForRead
                  );
                PlotInfo pi = new PlotInfo();
                PlotInfoValidator piv =
                  new PlotInfoValidator();
                piv.MediaMatchingPolicy =
                  MatchingPolicy.MatchEnabled;
                // A PlotEngine does the actual plotting
                // (can also create one for Preview)
                if (PlotFactory.ProcessPlotState ==
                    ProcessPlotState.NotPlotting)
                {
                    PlotEngine pe =
                      PlotFactory.CreatePublishEngine();
                    using (pe)
                    {
                        // Create a Progress Dialog to provide info
                        // and allow thej user to cancel
                        PlotProgressDialog ppd =
                          new PlotProgressDialog(false, 1, true);
                        using (ppd)
                        {
                            ObjectIdCollection layoutsToPlot =
                              new ObjectIdCollection();
                            foreach (ObjectId btrId in bt)
                            {
                                BlockTableRecord btr =
                                  (BlockTableRecord)tr.GetObject(
                                    btrId,
                                    OpenMode.ForRead
                                  );
                                if (btr.IsLayout &&
                                    btr.Name.ToUpper() !=
                                      BlockTableRecord.ModelSpace.ToUpper())
                                {
                                    layoutsToPlot.Add(btrId);
                                }
                            }
                            int numSheet = 1;
                            foreach (ObjectId btrId in layoutsToPlot)
                            {
                                BlockTableRecord btr =
                                  (BlockTableRecord)tr.GetObject(
                                    btrId,
                                    OpenMode.ForRead
                                  );
                                Layout lo =
                                  (Layout)tr.GetObject(
                                    btr.LayoutId,
                                    OpenMode.ForRead
                                  );
                                // We need a PlotSettings object
                                // based on the layout settings
                                // which we then customize
                                PlotSettings ps =
                                  new PlotSettings(lo.ModelType);
                                ps.CopyFrom(lo);
                                // The PlotSettingsValidator helps
                                // create a valid PlotSettings object
                                PlotSettingsValidator psv =
                                  PlotSettingsValidator.Current;
                                // We'll plot the extents, centered and
                                // scaled to fit
                                psv.SetPlotType(
                                  ps,
                                Autodesk.AutoCAD.DatabaseServices.PlotType.Extents
                                );
                                psv.SetUseStandardScale(ps, true);
                                psv.SetStdScaleType(ps, StdScaleType.ScaleToFit);
                                psv.SetPlotCentered(ps, true);
                                // We'll use the standard DWFx PC3, as
                                // this supports multiple sheets
                                psv.SetPlotConfigurationName(
                                  ps,
                                  "DWG To PDF.pc3",
                                  "ISO_full_bleed_A4_(297.00_x_210.00_MM)"
                                );
                                // We need a PlotInfo object
                                // linked to the layout
                                pi.Layout = btr.LayoutId;
                                // Make the layout we're plotting current
                                LayoutManager.Current.CurrentLayout =
                                  lo.LayoutName;
                                // We need to link the PlotInfo to the
                                // PlotSettings and then validate it
                                pi.OverrideSettings = ps;
                                piv.Validate(pi);
                                if (numSheet == 1)
                                {
                                    ppd.set_PlotMsgString(
                                      PlotMessageIndex.DialogTitle,
                                      "Custom Plot Progress"
                                    );
                                    ppd.set_PlotMsgString(
                                      PlotMessageIndex.CancelJobButtonMessage,
                                      "Cancel Job"
                                    );
                                    ppd.set_PlotMsgString(
                                      PlotMessageIndex.CancelSheetButtonMessage,
                                      "Cancel Sheet"
                                    );
                                    ppd.set_PlotMsgString(
                                      PlotMessageIndex.SheetSetProgressCaption,
                                      "Sheet Set Progress"
                                    );
                                    ppd.set_PlotMsgString(
                                      PlotMessageIndex.SheetProgressCaption,
                                      "Sheet Progress"
                                    );
                                    ppd.LowerPlotProgressRange = 0;
                                    ppd.UpperPlotProgressRange = 100;
                                    ppd.PlotProgressPos = 0;
                                    // Let's start the plot, at last
                                    ppd.OnBeginPlot();
                                    ppd.IsVisible = true;
                                    pe.BeginPlot(ppd, null);
                                    // We'll be plotting a single document
                                    String MySavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                                    pe.BeginDocument(
                                      pi,
                                      doc.Name,
                                      null,
                                      1,
                                      true, // Let's plot to file
                                      MySavePath + "\\list.pdf"
                                    );
                                }
                                // Which may contain multiple sheets
                                ppd.StatusMsgString =
                                  "Plotting " +
                                  doc.Name.Substring(
                                    doc.Name.LastIndexOf("\\") + 1
                                  ) +
                                  " - sheet " + numSheet.ToString() +
                                  " of " + layoutsToPlot.Count.ToString();
                                ppd.OnBeginSheet();
                                ppd.LowerSheetProgressRange = 0;
                                ppd.UpperSheetProgressRange = 100;
                                ppd.SheetProgressPos = 0;
                                PlotPageInfo ppi = new PlotPageInfo();
                                pe.BeginPage(
                                  ppi,
                                  pi,
                                  (numSheet == layoutsToPlot.Count),
                                  null
                                );
                                pe.BeginGenerateGraphics(null);
                                ppd.SheetProgressPos = 50;
                                pe.EndGenerateGraphics(null);
                                // Finish the sheet
                                pe.EndPage(null);
                                ppd.SheetProgressPos = 100;
                                ppd.OnEndSheet();
                                numSheet++;
                            }
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
                    ed.WriteMessage(
                      "\nAnother plot is in progress."
                    );
                }
            }
        }

        [Rt.CommandMethod("Multiplot", Rt.CommandFlags.Modal)]
        public static void MyPlot()
        {
            Ap.Document doc = acad.DocumentManager.MdiActiveDocument;
            if (doc == null || doc.IsDisposed)
                return;
            Ed.Editor ed = doc.Editor;
            Db.Database db = doc.Database;
            using (doc.LockDocument())
            {
                String MySavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                List<PlotObject> plotObjects = GetBlockReferenceBoundaries(MyBlock_Name);

                LayoutCommand lc = new LayoutCommand();
                //foreach (var plotObject in plotObjects)
                //{
                //    // create a single layout for each block reference
                //    lc.CreateMyLayout(MyPageSize, MyPageStyle, MyPlotter, plotObject);
                //}

                lc.CreateMyLayout(MyPageSize, MyPageStyle, MyPlotter, plotObjects[0]);
                
                ed.WriteMessage($"Viewport boundaries: Xmax={plotObjects[0].extents.MaxPoint.X}," +
                    $"Ymax={plotObjects[0].extents.MaxPoint.Y}," +
                    $"Xmin={plotObjects[0].extents.MinPoint.X}," +
                    $"Ymin={plotObjects[0].extents.MinPoint.Y}, ");
                

                ed.WriteMessage("\nThe \"{0}\" file created.\n", "");
            }
        }
        private static List<PlotObject> GetBlockReferenceBoundaries(String targetBlock)
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
                        obj.extents = bounds.Value; // Extensions.Strip(bounds.Value);
                    }                    

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
                    plotObjects.Add(obj);
                }
                tr.Commit();
            }
            System.Windows.MessageBox.Show(plotObjects.Count.ToString() + " block references were found in current modelspace.");
            return plotObjects;
        }
        public static void PlotBlocks(List<PlotObject> plotObjects,
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
                                        if (String.Compare(btr.Name, MyBlock_Name) == 0) ;
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