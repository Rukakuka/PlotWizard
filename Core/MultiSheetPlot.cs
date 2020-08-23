using System;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.PlottingServices;

namespace PlotWizard
{
    public static class MultiSheetPlot
    {
        public static void MultiSheetPlotter(String pageSize, String plotter, String outputFileName, ObjectIdCollection allLayouts)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null || doc.IsDisposed)
            {
                System.Windows.MessageBox.Show("Чертеж отсуствует или загружен с ошибкой. Отмена.");
                return;
            }
            Editor ed = doc.Editor;

            Database db = doc.Database;
            if (db == null || db.IsDisposed)
            {
                System.Windows.MessageBox.Show("База данных чертежа отсуствует или загружена с ошибкой. Отмена.");
                return;
            }
            using (Transaction tr = db.TransactionManager.StartTransaction())// <--- TODO fix NullReferenceException 
            {
                if (tr == null || tr.IsDisposed)
                {
                    System.Windows.MessageBox.Show("Транзакция отсутствует или загружена с ошибкой. Отмена.");
                    return;
                }
                PlotInfo plotInfo = new PlotInfo();
                PlotInfoValidator plotInfoValidator = new PlotInfoValidator
                {
                    MediaMatchingPolicy = MatchingPolicy.MatchEnabled
                };
                if (PlotFactory.ProcessPlotState != ProcessPlotState.NotPlotting)
                {
                    ed.WriteMessage("\nОтмена. Принтер занят (другая печать в процессе).\n");
                    tr.Commit();
                    return;
                }

                using (PlotEngine plotEngine = PlotFactory.CreatePublishEngine())
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

                    if (allLayouts == null || allLayouts.IsDisposed)
                    {
                        System.Windows.MessageBox.Show("\nОбъект печати пуст Пропущено.\n");
                        plotEngine.Dispose();
                        tr.Commit();
                        return;
                    }
                    if (allLayouts.Count == 0)
                    {
                        System.Windows.MessageBox.Show("\nКоличество листов для печати равно нулю. Пропущено.\n");
                        plotEngine.Dispose();
                        tr.Commit();
                        return;
                    }

                    int sheetCount = 0;
                    ObjectIdCollection layoutsToPlot = new ObjectIdCollection();

                    foreach (ObjectId btrId in allLayouts)
                    {
                        ObjectId obj = btrId;
                        try
                        {
                            if (!db.TryGetObjectId(obj.Handle, out obj))
                            {
                                throw new Exception();
                            }
                        }
                        catch (Exception e) // catch erased layout or whatever shit
                        {
                            char[] r = { '(', ')' };
                            ed.WriteMessage($"\nЛист c id '{btrId.ToString().Trim(r)}' удалён или создан с ошибками. Удалено из очереди печати.\n");
                            continue;
                        }
                        layoutsToPlot.Add(obj);
                        sheetCount++;
                    }

                    ed.WriteMessage($"\nИтого листов к печати: {sheetCount.ToString()}, всего: {allLayouts.Count}, листов создано: {layoutsToPlot.Count}.\n");

                    bool printingError = false;

                    using (PlotProgressDialog plotProcessDialog = new PlotProgressDialog(false, sheetCount, true))
                    {
                        int numSheet = 1;
                        using (doc.LockDocument())
                        {
                            foreach (ObjectId btrId in layoutsToPlot)
                            {
                                Layout layout = tr.GetObject(btrId, OpenMode.ForRead) as Layout;

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

                                    var file = new FileInfo(outputFileName);
                                    if (file.Exists)
                                    {
                                        try
                                        {
                                            file.Delete();
                                        }
                                        catch (Exception e)
                                        {
                                            System.Windows.MessageBox.Show("Файл для печати занят другим процессом. Отмена.");
                                            ed.WriteMessage($"\nОшибка открытия файла для печати. Отмена.\n");
                                            printingError = true;
                                            break;
                                        }
                                    }

                                    plotProcessDialog.OnBeginPlot();
                                    plotProcessDialog.IsVisible = true;
                                    plotEngine.BeginPlot(plotProcessDialog, null);

                                    plotEngine.BeginDocument(plotInfo, doc.Name, null, 1, true, outputFileName);

                                }

                                plotProcessDialog.StatusMsgString = "Plotting " + 
                                                                    doc.Name.Substring(doc.Name.LastIndexOf("\\") + 1) + 
                                                                    " - sheet " + numSheet.ToString() + 
                                                                    " of " + 
                                                                    layoutsToPlot.Count.ToString();

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

                            if (!printingError)
                            {
                                plotEngine.EndDocument(null);
                                plotProcessDialog.PlotProgressPos = 100;
                                plotProcessDialog.OnEndPlot();
                                plotEngine.EndPlot(null);
                            }
                            ed.WriteMessage($"\nПечать завершена. \n");

                            tr.Commit();
                            
                            if ((bool)Ribbon.LayoutSettings.AutoOpenFile && !printingError)
                                System.Diagnostics.Process.Start(outputFileName);
                        }
                    }
                }
            }
        }
    }
}


