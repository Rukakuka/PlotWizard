using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.PlottingServices;

namespace PrintWizard
{
    public static class MultiSheetPlot
    {
        public static void MultiSheetPlotter(String pageSize, String plotter, String outputFileName, ObjectIdCollection layoutsToPlot)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            Transaction tr = db.TransactionManager.StartTransaction();

            PlotInfo plotInfo = new PlotInfo();
            PlotInfoValidator plotInfoValidator = new PlotInfoValidator
            {
                MediaMatchingPolicy = MatchingPolicy.MatchEnabled
            };

            if (PlotFactory.ProcessPlotState != ProcessPlotState.NotPlotting)
            {
                ed.WriteMessage("\nОтмена. Принтер занят (другая печать в процессе).\n");
                tr.Commit();
            }

            PlotEngine plotEngine = PlotFactory.CreatePublishEngine();

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

            if (layoutsToPlot == null || layoutsToPlot.IsDisposed)
            {
                System.Windows.MessageBox.Show("\nОбъект печати пуст Пропущено.\n");
                tr.Commit();
                return;
            }
            if (layoutsToPlot.Count == 0)
            {
                System.Windows.MessageBox.Show("\nКоличество листов для печати равно нулю. Пропущено.\n");
                tr.Commit();
                return;
            }

            int sheetCount = 0;
            foreach (ObjectId btrId in layoutsToPlot)
            {
                try
                {
                    _ = tr.GetObject(btrId, OpenMode.ForRead) as Layout;
                }
                catch (Exception e)
                { // catch erased layout or whatever shit
                    layoutsToPlot.RemoveAt(sheetCount);
                    char[] r = { '(', ')' };
                    ed.WriteMessage($"\nЛист c id '{btrId.ToString().Trim(r)}' удалён или создан с ошибками. Удалено из очереди печати.\n");
                }
                sheetCount++;
            }

            PlotProgressDialog plotProcessDialog = new PlotProgressDialog(false, sheetCount, true);

            int numSheet = 1;
            foreach (ObjectId btrId in layoutsToPlot)
            {
                Layout layout = new Layout();
                try
                {
                    layout = tr.GetObject(btrId, OpenMode.ForRead) as Layout;
                }
                catch (Exception e)
                {
                    ed.WriteMessage($"\nЛист c id '{btrId.ToString()}' отсуствует в базе данных чертежа. Выполнен переход к следующему листу в очереди.\n");
                    continue;
                }

                PlotSettings plotSettings = new PlotSettings(layout.ModelType);
                plotSettings.CopyFrom(layout);

                PlotSettingsValidator plotSettingsValidator = PlotSettingsValidator.Current;

                plotSettingsValidator.SetPlotType(plotSettings, Autodesk.AutoCAD.DatabaseServices.PlotType.Extents);
                plotSettingsValidator.SetUseStandardScale(plotSettings, true);
                plotSettingsValidator.SetStdScaleType(plotSettings, StdScaleType.ScaleToFit);
                plotSettingsValidator.SetPlotCentered(plotSettings, true);

                plotSettingsValidator.SetPlotConfigurationName(plotSettings, plotter, pageSize);

                plotInfo.Layout = btrId;                         

                LayoutManager.Current.CurrentLayout = layout.LayoutName;

                plotInfo.OverrideSettings = plotSettings;
                plotInfoValidator.Validate(plotInfo);
                if (numSheet == 1)
                {
                    plotProcessDialog.set_PlotMsgString(PlotMessageIndex.DialogTitle, "Custom Plot Progress");
                    plotProcessDialog.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");
                    plotProcessDialog.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Cancel Sheet");
                    plotProcessDialog.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
                    plotProcessDialog.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Sheet Progress");
                    plotProcessDialog.LowerPlotProgressRange = 0;
                    plotProcessDialog.UpperPlotProgressRange = 100;
                    plotProcessDialog.PlotProgressPos = 0;

                    plotProcessDialog.OnBeginPlot();
                    plotProcessDialog.IsVisible = true;
                    plotEngine.BeginPlot(plotProcessDialog, null);

                    plotEngine.BeginDocument(plotInfo, doc.Name, null, 1, true, outputFileName);
                }

                plotProcessDialog.StatusMsgString = "Plotting " + doc.Name.Substring(doc.Name.LastIndexOf("\\") + 1) + " - sheet " + numSheet.ToString() + " of " + layoutsToPlot.Count.ToString();
                plotProcessDialog.OnBeginSheet();
                plotProcessDialog.LowerSheetProgressRange = 0;
                plotProcessDialog.UpperSheetProgressRange = 100;
                plotProcessDialog.SheetProgressPos = 0;

                PlotPageInfo plotPageInfo = new PlotPageInfo();
                plotEngine.BeginPage(plotPageInfo, plotInfo, (numSheet == layoutsToPlot.Count), null);

                plotEngine.BeginGenerateGraphics(null);
                plotProcessDialog.SheetProgressPos = 50;
                plotEngine.EndGenerateGraphics(null);
                plotEngine.EndPage(null);
                plotProcessDialog.SheetProgressPos = 100;
                plotProcessDialog.OnEndSheet();
                numSheet++;

                plotProcessDialog.PlotProgressPos = (int)Math.Floor((double)numSheet * 100 / layoutsToPlot.Count);
            }
            plotEngine.EndDocument(null);
            plotProcessDialog.PlotProgressPos = 100;
            plotProcessDialog.OnEndPlot();
            plotEngine.EndPlot(null);

            tr.Commit();
            ed.WriteMessage($"\nПечать завершена. \n");
        }
    }
}

