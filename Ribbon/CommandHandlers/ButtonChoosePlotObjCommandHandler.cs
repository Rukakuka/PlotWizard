using System;
using System.Collections.Generic;
using Autodesk.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Ap = Autodesk.AutoCAD.ApplicationServices;
using Ed = Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace PlotWizard.Ribbon.CommandHandlers
{
    internal class ButtonChoosePlotObjCommandHandler : System.Windows.Input.ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object param)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            if (!(parameter is RibbonCommandItem))
                throw new Exception();

            Ap.Document doc = Ap.Core.Application.DocumentManager.MdiActiveDocument;
            if (doc == null || doc.IsDisposed)
                throw new Exception();

            Ed.Editor ed = doc.Editor;

            Ed.PromptPointOptions ppo = new Ed.PromptPointOptions("\nУкажите первый угол области печати: ")
            {
                AllowNone = false
            };
            Ed.PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status != Ed.PromptStatus.OK)
                return;
            Point3d first = ppr.Value;
            Ed.PromptCornerOptions pco = new Ed.PromptCornerOptions("\nУкажите противоположный угол области печати: ", first);
            ppr = ed.GetCorner(pco);
            if (ppr.Status != Ed.PromptStatus.OK)
                return;
            Point3d second = ppr.Value;
            
            Ed.PromptEntityOptions peo = new Ed.PromptEntityOptions("\nВыберите экземпляр вхождения блоков для печати:");
            peo.SetRejectMessage("\nВыбранный объект не является вхождением блока.\n");
            peo.AddAllowedClass(typeof(BlockReference), false);
            Ed.PromptEntityResult resource = ed.GetEntity(peo);

            if (resource.Status != Ed.PromptStatus.OK)
            {
                ed.WriteMessage("\nОтмена.\n");
                return;
            }

            ObjectId objId = resource.ObjectId;

            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                BlockReference br = tr.GetObject(objId, OpenMode.ForRead) as BlockReference;
                BlockSelectorSettings.TargetBlockName = br.Name;

                List<string> attrCollection = ParseAttributes(tr, br.AttributeCollection);

                Ribbon.BlockSelectorWindow blockSelector = new BlockSelectorWindow(attrCollection);

                if (blockSelector.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    BlockSelectorSettings.FirstCornerPoint = new Point3d(first.X > second.X ? first.X : second.X, first.Y > second.Y ? first.Y : second.Y, 0);
                    BlockSelectorSettings.SecondCornerPoint = new Point3d(first.X < second.X ? first.X : second.X, first.Y < second.Y ? first.Y : second.Y, 0);
                    RefreshRibbonBlockFields();
                }
                else
                {
                    BlockSelectorSettings.FirstCornerPoint = BlockSelectorSettings.SecondCornerPoint  = new Point3d(0, 0, 0);
                    BlockSelectorSettings.TargetBlockName = null;
                }
                blockSelector.Dispose();
                tr.Commit(); 
            }
        } 
        private void RefreshRibbonBlockFields()
        {
            RibbonTab tab = FindRibbonTabByName(RibbonCommands.TargetTabName);

            if (tab == null)
            {
                System.Windows.MessageBox.Show($"Tab not found: {RibbonCommands.TargetTabName}");
                return;
            }
            RibbonTextBox textbox = tab.FindItem("tbBlockName") as RibbonTextBox;
            if (textbox != null)
            {
                textbox.TextValue = BlockSelectorSettings.TargetBlockName;
            }
             textbox = tab.FindItem("tbPrefix") as RibbonTextBox;
            if (textbox != null)
            {
                textbox.TextValue = BlockSelectorSettings.Prefix;
            }
            textbox = tab.FindItem("tbPostfix") as RibbonTextBox;
            if (textbox != null)
            {
                textbox.TextValue = BlockSelectorSettings.Postfix;
            }
        }
        private RibbonTab FindRibbonTabByName(string name)
        {
            Autodesk.Windows.RibbonControl ribbon = ComponentManager.Ribbon;
            foreach (var tab in ribbon.Tabs)
            {
                if (tab.Title.Equals(name))
                    return tab;
            }
            return null;
        }
        private List<string> ParseAttributes(Transaction tr, AttributeCollection ac)
        {
            List<string> collection = new List<string>();
            foreach (ObjectId obj in ac)
            {
                var attr = tr.GetObject(obj, OpenMode.ForRead) as AttributeReference;
                if (attr == null)
                    continue;
                collection.Add(attr.Tag);
            }
            return collection;
        }
    }
}
