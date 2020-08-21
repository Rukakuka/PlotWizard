using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PlotWizard.Ribbon
{
    public partial class AttributeSelectorWindow : Form
    {
        public AttributeSelectorWindow(List<string> attrCollection)
        {
            InitializeComponent();
            FillListBoxes(attrCollection);
            //FillDataGridSortingOrder(); // <----------- Implement
        }
        private void FillDataGridSortingOrder()
        {
            dataGridSortingOrder.Rows.Add(new object[] { "+Y, +X", Properties.Resources.icon_axes01 });
            dataGridSortingOrder.Rows.Add(new object[] { "+X, -Y", Properties.Resources.icon_axes02 });
            dataGridSortingOrder.Rows.Add(new object[] { "-Y, -X", Properties.Resources.icon_axes03 });
            dataGridSortingOrder.Rows.Add(new object[] { "-X, +Y", Properties.Resources.icon_axes04 });
            dataGridSortingOrder.Rows.Add(new object[] { "+X, +Y", Properties.Resources.icon_axes05 });
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            AttributeSelectorSettings.Prefix = listBoxPrefix.Text.Equals("None") ? AttributeSelectorSettings.defaultPrefix : listBoxPrefix.Text;
            AttributeSelectorSettings.Postfix = listBoxPostfix.Text.Equals("None") ? AttributeSelectorSettings.defaultPostfix : listBoxPostfix.Text;
            //AttributeSelectorSettings.SortingOrder = ;

            Close();
        }
        private void FillListBoxes(List<string> list)
        {
            listBoxPrefix.Items.Add("None");
            listBoxPostfix.Items.Add("None");
            if (list != null && list.Count > 0)
            {
                foreach (var attr in list)
                {
                    listBoxPrefix.Items.Add(attr);
                    listBoxPostfix.Items.Add(attr);
                }
            }
            listBoxPrefix.SelectedIndex = 0;
            listBoxPostfix.SelectedIndex = 0;
        }
        /*
        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is ListBox lb)
            {
                string text = lb.Text.Equals("None") ? " " : lb.Text;

                switch (lb.Name)
                {
                    case "listBoxPrefix":
                        Prefix = text;
                        break;
                    case "listBoxPostfix":
                        Postfix = text;
                        break;
                }
            }
        }
        */
    }
}
