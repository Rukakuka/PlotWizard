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
            if (LayoutSettings.Current.AutoOpenFile == null)
                checkBoxAutoOpenFile.Checked = LayoutSettings.Default.AutoOpenFile;
            else
                checkBoxAutoOpenFile.Checked = (bool)LayoutSettings.Current.AutoOpenFile;
        }

        private void FillNumericalUpDown()
        {
            numericUpDownContentScaling.Value = LayoutSettings.Current.ContentScaling == -1 ? (decimal)LayoutSettings.Default.ContentScaling : (decimal)LayoutSettings.Current.ContentScaling;
            numericUpDownViewportScaling.Value = LayoutSettings.Current.ViewportScaling == -1 ? (decimal)LayoutSettings.Default.ViewportScaling : (decimal)LayoutSettings.Current.ViewportScaling;
        }
        private void FillPlotterComboBox()
        {
            foreach (string plotter in Extensions.GetPlotterNameList())
            {
                this.comboBoxPlotterType.Items.Add(plotter);

                if (String.IsNullOrEmpty(LayoutSettings.Current.PlotterType) && plotter.Equals(LayoutSettings.Default.PlotterType))
                    comboBoxPlotterType.SelectedItem = LayoutSettings.Default.PlotterType;
                else if (plotter.Equals(LayoutSettings.Current.PlotterType))
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
                if (((String.IsNullOrEmpty(LayoutSettings.Current.PageSize.Value) || String.IsNullOrWhiteSpace(LayoutSettings.Current.PageSize.Value))
                    && pageSize.Value.Equals(LayoutSettings.Default.PageSize.Value))
                    || pageSize.Value.Equals(LayoutSettings.Current.PageSize.Value))
                {
                    comboBoxPageSize.SelectedItem = pageSize.Key;
                }
            }
        }
        private void buttonOk_Click(object sender, EventArgs e)
        {
            // applying changed settings
            LayoutSettings.Current.PageSize = new KeyValuePair<string, string>(comboBoxPageSize.SelectedItem.ToString(), Extensions.GetMediaNameList(comboBoxPlotterType.SelectedItem.ToString())[comboBoxPageSize.SelectedItem.ToString()]);
            LayoutSettings.Current.PlotterType = comboBoxPlotterType.SelectedItem.ToString();
            LayoutSettings.Current.ContentScaling = (double)numericUpDownContentScaling.Value;
            LayoutSettings.Current.ViewportScaling = (double)numericUpDownViewportScaling.Value;
            LayoutSettings.Current.AutoOpenFile = checkBoxAutoOpenFile.Checked;
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
