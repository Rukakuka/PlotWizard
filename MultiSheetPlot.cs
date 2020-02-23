using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.PlottingServices;

namespace PrintWizard
{
    public static class MultiSheetPlot
    {
        public static void MultiSheetPlotter(String pageSize, String plotter, String outputFileName, ObjectIdCollection layouts)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                PlotInfo pi = new PlotInfo();
                PlotInfoValidator piv = new PlotInfoValidator();
                piv.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;
                // A PlotEngine does the actual plotting
                // (can also create one for Preview)
                if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
                {
                    PlotEngine pe = PlotFactory.CreatePublishEngine();
                    using (pe)
                    {
                        // Collect all the paperspace layouts
                        // for plotting
                        //ObjectIdCollection layoutsToPlot = new ObjectIdCollection();
                        //foreach (ObjectId btrId in bt)
                        //{
                        //    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
                        //    if (btr.IsLayout &&
                        //        btr.Name.ToUpper() != BlockTableRecord.ModelSpace.ToUpper())
                        //    {
                        //        layoutsToPlot.Add(btrId);
                        //    }
                        //}

                        if (layouts == null)
                            return;
                        if (layouts.IsDisposed)
                            return;
                        if (layouts.Count == 0)
                        {
                            System.Windows.MessageBox.Show("Нет листов для печати");
                            return;
                        }
                        // Create a Progress Dialog to provide info
                        // and allow thej user to cancel

                        PlotProgressDialog ppd = new PlotProgressDialog(false, layouts.Count, true);
                        using (ppd)
                        {
                            int numSheet = 1;
                            foreach (ObjectId btrId in layouts)
                            {
                                //-------------------------------------------------->BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);

                                Layout lo = (Layout)tr.GetObject(btrId, OpenMode.ForRead);
                                // We need a PlotSettings object
                                // based on the layout settings
                                // which we then customize
                                PlotSettings ps = new PlotSettings(lo.ModelType);
                                ps.CopyFrom(lo);
                                // The PlotSettingsValidator helps
                                // create a valid PlotSettings object
                                PlotSettingsValidator psv = PlotSettingsValidator.Current;
                                // We'll plot the extents, centered and
                                // scaled to fit
                                psv.SetPlotType(ps, Autodesk.AutoCAD.DatabaseServices.PlotType.Extents);
                                psv.SetUseStandardScale(ps, true);
                                psv.SetStdScaleType(ps, StdScaleType.ScaleToFit);
                                psv.SetPlotCentered(ps, true);
                                // We'll use the standard DWFx PC3, as
                                // this supports multiple sheets
                                psv.SetPlotConfigurationName(ps, plotter, pageSize);
                                // We need a PlotInfo object
                                // linked to the layout
                                pi.Layout = btrId;                         //---------------------------> btr.LayoutId;
                                // Make the layout we're plotting current
                                LayoutManager.Current.CurrentLayout = lo.LayoutName;
                                // We need to link the PlotInfo to the
                                // PlotSettings and then validate it
                                pi.OverrideSettings = ps;
                                piv.Validate(pi);
                                if (numSheet == 1)
                                {
                                    ppd.set_PlotMsgString(PlotMessageIndex.DialogTitle, "Custom Plot Progress");
                                    ppd.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");
                                    ppd.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Cancel Sheet");
                                    ppd.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
                                    ppd.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Sheet Progress");
                                    ppd.LowerPlotProgressRange = 0;
                                    ppd.UpperPlotProgressRange = 100;
                                    ppd.PlotProgressPos = 0;
                                    // Let's start the plot, at last
                                    ppd.OnBeginPlot();
                                    ppd.IsVisible = true;
                                    pe.BeginPlot(ppd, null);
                                    // We'll be plotting a single document

                                    pe.BeginDocument(
                                      pi,
                                      doc.Name,
                                      null,
                                      1,
                                      true, // Let's plot to file
                                      outputFileName
                                    );
                                }
                                // Which may contain multiple sheets
                                ppd.StatusMsgString =
                                  "Plotting " +
                                  doc.Name.Substring(
                                    doc.Name.LastIndexOf("\\") + 1) +
                                  " - sheet " + numSheet.ToString() +
                                  " of " + layouts.Count.ToString();
                                ppd.OnBeginSheet();
                                ppd.LowerSheetProgressRange = 0;
                                ppd.UpperSheetProgressRange = 100;
                                ppd.SheetProgressPos = 0;
                                PlotPageInfo ppi = new PlotPageInfo();
                                pe.BeginPage(
                                  ppi,
                                  pi,
                                  (numSheet == layouts.Count),
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
                    ed.WriteMessage("\nОтмена. Принтер занят (другая печать в процессе).\n");
                }
                tr.Commit();
                ed.WriteMessage($"\nПечать завершена. \n");
            }
        }
    }
}
