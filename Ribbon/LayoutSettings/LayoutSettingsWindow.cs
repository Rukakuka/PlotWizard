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
    public partial class LayoutSettingsWindow : Form
    {
        public LayoutSettingsWindow()
        {
            InitializeComponent();
            FillPlotterComboBox();
            FillPageSizeComboBox(comboBoxPlotterType.SelectedItem.ToString());
            FillNumericalUpDown();
            FillCheckBox();
        }
        private void FillCheckBox()
        {
            if (LayoutSettings.AutoOpenFile == null)
                checkBoxAutoOpenFile.Checked = LayoutSettings.defaultAutoOpenFile;
            else
                checkBoxAutoOpenFile.Checked = (bool)LayoutSettings.AutoOpenFile;
        }

        private void FillNumericalUpDown()
        {
            numericUpDownContentScaling.Value = LayoutSettings.ContentScaling == -1 ? (decimal)LayoutSettings.defaultContentScaling : (decimal)LayoutSettings.ContentScaling;
            numericUpDownViewportScaling.Value = LayoutSettings.ViewportScaling == -1 ? (decimal)LayoutSettings.defaultViewportScaling : (decimal)LayoutSettings.ViewportScaling;
        }
        private void FillPlotterComboBox()
        {
            foreach (string plotter in Extensions.GetPlotterNameList())
            {
                this.comboBoxPlotterType.Items.Add(plotter);

                if (String.IsNullOrEmpty(LayoutSettings.PlotterType) && plotter.Equals(LayoutSettings.defaultPlotterType))
                    comboBoxPlotterType.SelectedItem = LayoutSettings.defaultPlotterType;
                else if (plotter.Equals(LayoutSettings.PlotterType))
                        comboBoxPlotterType.SelectedItem = plotter;
            }
            this.comboBoxPlotterType.SelectedIndexChanged += new EventHandler(this.comboBoxPlotterType_SelectedIndexChanged);
        }
        private void FillPageSizeComboBox(string plotter)
        {
            this.comboBoxPageSize.Items.Clear();

            if (String.IsNullOrEmpty(plotter) || String.IsNullOrWhiteSpace(plotter))
            {
                System.Windows.MessageBox.Show("Tried to fill PageSizeComboBox with empty plotter name!");
                throw new ArgumentNullException();
            }

            // key = readable name, value = canonical name
            Dictionary<string, string> media = Extensions.GetMediaNameList(plotter);

            foreach (KeyValuePair<string, string> pageSize in media)
            {
                this.comboBoxPageSize.Items.Add(pageSize.Key);
                if (((String.IsNullOrEmpty(LayoutSettings.PageSize.Value) || String.IsNullOrWhiteSpace(LayoutSettings.PageSize.Value))
                    && pageSize.Value.Equals(LayoutSettings.defaultPageSize.Value))
                    || pageSize.Value.Equals(LayoutSettings.PageSize.Value))
                {
                    comboBoxPageSize.SelectedItem = pageSize.Key;
                }
            }
        }
        private void buttonOk_Click(object sender, EventArgs e)
        {
            // applying changed settings
            LayoutSettings.PageSize = new KeyValuePair<string, string>(comboBoxPageSize.SelectedItem.ToString(), Extensions.GetMediaNameList(comboBoxPlotterType.SelectedItem.ToString())[comboBoxPageSize.SelectedItem.ToString()]);
            LayoutSettings.PlotterType = comboBoxPlotterType.SelectedItem.ToString();
            LayoutSettings.ContentScaling = (double)numericUpDownContentScaling.Value;
            LayoutSettings.ViewportScaling = (double)numericUpDownViewportScaling.Value;
            LayoutSettings.AutoOpenFile = checkBoxAutoOpenFile.Checked;
            LayoutSettings.SaveConfig(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\" + LayoutSettings.configFileName);

            this.Close();
        }
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            // discarding changed settings
            this.Close();
        }
        private void comboBoxPlotterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillPageSizeComboBox(comboBoxPlotterType.SelectedItem.ToString());
            this.comboBoxPageSize.SelectedIndex = 0;
        }
    }
}
