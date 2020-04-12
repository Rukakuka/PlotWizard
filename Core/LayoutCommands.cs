using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PlotWizard
{
    internal class LayoutCommands
    {
        internal ObjectId CreateMyLayout(String pageSize, 
            double viewportScaling, 
            double contentScaling, 
            String styleSheet, 
            String plotter, 
            PlotObject plotObject)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return new ObjectId();

            var db = doc.Database;
            var ed = doc.Editor;
            var ext = new Extents2d();

            using (var tr = db.TransactionManager.StartTransaction())
            {
                String layoutName = plotObject.Prefix + " " + plotObject.Postfix;
                layoutName = Extensions.PurgeString(layoutName.Trim());

                if (String.IsNullOrEmpty(layoutName))
                {
                    //ed.WriteMessage("\nИмя листа не содержит символов. Пропущено.\n");
                    //return new ObjectId();
                    layoutName = "Layout";
                }
                
                // Consecutevly check if there is already a list with the same name, else add (1), (2) etc. to the name
                string overridedLayoutName = layoutName;
                int i = 1;
                while (CheckForDuplicates(overridedLayoutName))
                {
                    overridedLayoutName = layoutName + $" ({i.ToString()})";
                    i++;
                }
                ObjectId id = LayoutManager.Current.CreateAndMakeLayoutCurrent(overridedLayoutName);
                ObjectId layoutId = new ObjectId();

                // Open the created layout
                if (id != null)
                {
                    var lay = tr.GetObject(id, OpenMode.ForWrite) as Layout;
                    // Make some settings on the layout and get its extents
                    lay.SetPlotSettings(pageSize, styleSheet, plotter);

                    //ext = Extensions.Strip(plotObject.extents);   
                    ext = lay.GetMaximumExtents(); 

                    lay.ApplyToViewport(tr, 2, vp => // lambda
                    {
                        // Size the viewport according to the extents calculated when we set the PlotSettings (device, page size, etc.)
                        vp.ResizeViewport(ext, Extensions.Clamp(viewportScaling, 0, 1));
                        // Adjust the view so that the model contents fit
                        if (ValidDbExtents(db.Extmin, db.Extmax))
                        {
                            vp.FitContentToViewport(plotObject.Extents, Extensions.Clamp(contentScaling, 0, (double)Int32.MaxValue));//(new Extents3d(db.Extmin, db.Extmax), 0.98);
                        }
                        // Finally we lock the view to prevent meddling
                        vp.Locked = true;
                    }
                    );
                    layoutId = lay.Id;
                }
                // Commit the transaction
                tr.Commit();
                return layoutId;
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
        private static bool CheckForDuplicates(string layoutName)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary laytDict = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForWrite) as DBDictionary;
                
                // Iterate dictionary entries.
                foreach (DBDictionaryEntry layName in laytDict)
                {
                    if (layName.Key.Equals(layoutName))
                        return true;
                }
                tr.Commit();
            }
            return false;
        }
        internal static void EraseAllLayouts()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null || doc.IsDisposed)
                return;

            Database db = doc.Database;
            LayoutManager lm = LayoutManager.Current;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary layoutDictionary = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForWrite) as DBDictionary;

                // Iterate dictionary entries.
                foreach (DBDictionaryEntry layout in layoutDictionary)
                {
                    string layoutName = layout.Key;
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
