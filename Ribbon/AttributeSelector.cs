using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (attrCollection != null)
                FillListBoxes(attrCollection);
            listBoxPrefix.SelectedIndex = 0;
            listBoxPostfix.SelectedIndex = 0;
        }
        private void ButtonOk_Click(object sender, EventArgs e)
        {
            Wizard.Prefix = this.Prefix;
            Wizard.Postfix = this.Postfix;
            Close();
        }
        private void FillListBoxes(List<string> list)
        {
            listBoxPrefix.Items.Add("Нет");
            listBoxPostfix.Items.Add("Нет");
            foreach (var attr in list)
            {
                listBoxPrefix.Items.Add(attr);
                listBoxPostfix.Items.Add(attr);
            }
        }
        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb;
            if (sender is ListBox)
            {
                lb = (ListBox)sender;
                string text;
                text = lb.Text.Equals("Нет") ? "" : lb.Text;
                  
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
