using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Rt = Autodesk.AutoCAD.Runtime;

namespace PrintWizard
{
    public class LayoutCommands
    {
        public void CreateMyLayout(String pageSize, String styleSheet, String plotter, PlotObject plotObject, String layoutName)
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
                //ed.WriteMessage(plotObject.label);
                var id = LayoutManager.Current.CreateAndMakeLayoutCurrent(layoutName);
                // Open the created layout
                var lay = (Layout)tr.GetObject(id, OpenMode.ForWrite);
                // Make some settings on the layout and get its extents
                lay.SetPlotSettings(
                  //"ANSI_B_(11.00_x_17.00_Inches)",
                  //"monochrome.ctb", //"acad.ctb",
                  //"DWF6 ePlot.pc3"
                  pageSize,
                  styleSheet,
                  plotter
                );

                ext = Extensions.Strip(plotObject.extents); //lay.GetMaximumExtents();

                lay.ApplyToViewport(
                  tr, 2,
                  vp =>
                  {
                      // Size the viewport according to the extents calculated when
                      // we set the PlotSettings (device, page size, etc.)
                      // Use the standard 10% margin around the viewport
                      // (found by measuring pixels on screenshots of Layout1, etc.)
                      vp.ResizeViewport(ext, 0.98);
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

        [Rt.CommandMethod("ERASE_ALL_LAYOUTS", Rt.CommandFlags.Modal)]
        public static void EraseAllLayouts()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // ACAD_LAYOUT dictionary.
                DBDictionary layoutDict = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
                // Iterate dictionary entries.
                foreach (DBDictionaryEntry de in layoutDict)
                {
                    string layoutName = de.Key;
                    if (layoutName != "Model")
                    {
                        LayoutManager.Current.DeleteLayout(layoutName); // Delete layout.
                    }
                }
                tr.Commit();
            }
        }
    }
}
