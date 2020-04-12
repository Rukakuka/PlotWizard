using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PlotWizard.Ribbon
{
    public partial class AttributeSelector : Form
    {
        internal string Prefix { get; private set; }
        internal string Postfix { get; private set; }

        public AttributeSelector(List<string> attrCollection)
        {
            InitializeComponent();
            FillListBoxes(attrCollection);
        }
        private void ButtonOk_Click(object sender, EventArgs e)
        {
            Wizard.Prefix = this.Prefix;
            Wizard.Postfix = this.Postfix;
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
    }
}
