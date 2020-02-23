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
        public void CreateMyLayout(String pageSize, String styleSheet, String plotter, PlotObject plotObject)
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

                String layoutName = null;
                layoutName = (plotObject.label + " Лист " + plotObject.sheet).Trim();

                //if (!String.IsNullOrEmpty(plotObject.label) && !String.IsNullOrEmpty(plotObject.sheet))
                // System.Windows.MessageBox.Show($"Атрибут блока не содержит символов.\nВхождение блока  пропущено.");

                if (String.IsNullOrEmpty(layoutName))
                    return;

                ObjectId id;
                string overridedLayoutName = layoutName;
                int i = 1;
                while (CheckForDuplicates(overridedLayoutName))
                {
                    overridedLayoutName = layoutName + $" ({i.ToString()})";
                    i++;
                }

                id = LayoutManager.Current.CreateAndMakeLayoutCurrent(overridedLayoutName);

                // Open the created layout
                if (id != null)
                {
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

                }
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
        public static bool CheckForDuplicates(string layoutName)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            bool duplicate = false;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary layoutDict = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForWrite) as DBDictionary;
                
                // Iterate dictionary entries.
                foreach (DBDictionaryEntry de in layoutDict)
                {
                    string name = de.Key;
                    if (name.Equals(layoutName))
                    {
                        duplicate = true;
                        break;
                    }
                }
                tr.Commit();
            }
            return duplicate;
        }
        public static void EraseAllLayouts()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc == null || doc.IsDisposed)
                return;

            Database db = doc.Database;
            LayoutManager lm = LayoutManager.Current;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary layoutDict = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForWrite) as DBDictionary;

                // Iterate dictionary entries.
                foreach (DBDictionaryEntry de in layoutDict)
                {
                    string layoutName = de.Key;
                    if (layoutName != "Model" && layoutName != "Лист1")
                    {
                        try
                        {
                            lm.DeleteLayout(layoutName); // Delete layout.
                        }
                        catch (Exception e)
                        {
                            System.Windows.MessageBox.Show($"Tried to delete layout with name '{layoutName}' \n" + e.ToString());
                        }
                    }
                }
                tr.Commit();
            }
        }
    }
}
