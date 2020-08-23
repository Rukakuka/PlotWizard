using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PlotWizard.Ribbon
{
    public partial class BlockSelectorWindow : Form
    {
        public BlockSelectorWindow(List<string> attrCollection)
        {
            InitializeComponent();
            FillListBoxes(attrCollection);
            if (listViewSortingOrder.Items.Count != 0)
                listViewSortingOrder.Items[0].Selected = true;
            listViewSortingOrder.Select();
            listViewSortingOrder.Enabled = false;

            textBoxChoosedBlockName.ReadOnly = false;
            textBoxChoosedBlockName.Text = BlockSelectorSettings.TargetBlockName;
            textBoxChoosedBlockName.ReadOnly = true;
        }
        private void ButtonOk_Click(object sender, EventArgs e)
        {
            BlockSelectorSettings.Prefix = listBoxPrefix.Text.Equals("(Нет)") ? BlockSelectorSettings.defaultPrefix : listBoxPrefix.Text;
            BlockSelectorSettings.Postfix = listBoxPostfix.Text.Equals("(Нет)") ? BlockSelectorSettings.defaultPostfix : listBoxPostfix.Text;

            if (listViewSortingOrder.SelectedIndices.Count != 0)
                BlockSelectorSettings.SortingOrder = SortingOrder.ToSortingOrder(listViewSortingOrder.SelectedIndices[0]);
            else
                BlockSelectorSettings.SortingOrder = BlockSelectorSettings.defaultSortingOrder;

            this.DialogResult = DialogResult.OK;
            Close();
        }
        private void FillListBoxes(List<string> attrCollection)
        {
            listBoxPrefix.Items.Add("(Нет)");
            listBoxPostfix.Items.Add("(Нет)");
            if (attrCollection != null && attrCollection.Count > 0)
            {
                foreach (var attr in attrCollection)
                {
                    listBoxPrefix.Items.Add(attr);
                    listBoxPostfix.Items.Add(attr);
                }
            }
            listBoxPrefix.SelectedIndex = 0;
            listBoxPostfix.SelectedIndex = 0;
        }
    }
}
