using System;
using System.Collections.Generic;
using Autodesk.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using acad = Autodesk.AutoCAD.ApplicationServices.Application;
using Ap = Autodesk.AutoCAD.ApplicationServices;
using Ed = Autodesk.AutoCAD.EditorInput;

namespace PrintWizard
{
    
    internal class ButtonChooseBlockCommandHandler : System.Windows.Input.ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object param)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            if (!(parameter is RibbonCommandItem))
                throw new TypeAccessException();

            Ap.Document doc = acad.DocumentManager.MdiActiveDocument;
            if (doc == null || doc.IsDisposed)
                throw new ArgumentNullException();
            Ed.Editor ed = doc.Editor;

            using (doc.LockDocument())
            {

                Ed.PromptEntityOptions peo = new Ed.PromptEntityOptions("\nВыберите экземпляр вхождения блока:");

                peo.SetRejectMessage("\nВыбранный объект не является вхождением блока.\n");
                peo.AddAllowedClass(typeof(BlockReference), false);
                Ed.PromptEntityResult resource = ed.GetEntity(peo);

                if (resource.Status != Ed.PromptStatus.OK)
                {
                    ed.WriteMessage("\nОтмена.\n");
                    return;
                }

                ObjectId objId = resource.ObjectId;
                Database db = doc.Database;

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockReference br = tr.GetObject(objId, OpenMode.ForRead) as BlockReference;
                    ed.WriteMessage($"\nВыбран блок '{br.Name}'.\n");

                    RibbonCommands.BlockName = br.Name;

                    RibbonTab tab = FindRibbonTab(RibbonCommands.TargetTab);

                    if (tab == null)
                        throw new ArgumentNullException($"Tab not found: {RibbonCommands.TargetTab}");

                    var textbox = tab.FindItem("tbBlockName") as RibbonTextBox;
                    textbox.TextValue = RibbonCommands.BlockName;

                    List<string> attrCollection = ParseAttributes(tr, br.AttributeCollection);
                    
                    Ribbon.AttributeSelector attrSelector = new Ribbon.AttributeSelector(attrCollection);
                    attrSelector.ShowDialog();

                    RibbonCommands.AttrLabelName = attrSelector.Prefix;
                    RibbonCommands.AttrSheetName = attrSelector.Postfix;

                    textbox = tab.FindItem("tbAttrLabel") as RibbonTextBox;
                    textbox.TextValue = RibbonCommands.AttrLabelName;
                    textbox = tab.FindItem("tbAttrSheet") as RibbonTextBox;
                    textbox.TextValue = RibbonCommands.AttrSheetName;

                    PlotWizard.MyBlockName = RibbonCommands.BlockName;
                    PlotWizard.MyBlockAttrLabel = RibbonCommands.AttrLabelName;
                    PlotWizard.MyBLockAttrSheet = RibbonCommands.AttrSheetName;
                    tr.Commit();
                }
            }
        }
        private RibbonTab FindRibbonTab(string name)
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
            foreach (ObjectId obj in ac )
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
