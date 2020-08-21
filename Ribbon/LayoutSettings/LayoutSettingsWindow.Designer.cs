namespace PlotWizard.Ribbon
{
    partial class LayoutSettingsWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBoxPageSize = new System.Windows.Forms.ComboBox();
            this.comboBoxPlotterType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownViewportScaling = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownContentScaling = new System.Windows.Forms.NumericUpDown();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownViewportScaling)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownContentScaling)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxPageSize
            // 
            this.comboBoxPageSize.FormattingEnabled = true;
            this.comboBoxPageSize.Location = new System.Drawing.Point(151, 11);
            this.comboBoxPageSize.Name = "comboBoxPageSize";
            this.comboBoxPageSize.Size = new System.Drawing.Size(253, 21);
            this.comboBoxPageSize.TabIndex = 0;
            // 
            // comboBoxPlotterType
            // 
            this.comboBoxPlotterType.FormattingEnabled = true;
            this.comboBoxPlotterType.Location = new System.Drawing.Point(151, 38);
            this.comboBoxPlotterType.Name = "comboBoxPlotterType";
            this.comboBoxPlotterType.Size = new System.Drawing.Size(253, 21);
            this.comboBoxPlotterType.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Формат страницы:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Принтер:";
            // 
            // numericUpDownViewportScaling
            // 
            this.numericUpDownViewportScaling.DecimalPlaces = 3;
            this.numericUpDownViewportScaling.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numericUpDownViewportScaling.Location = new System.Drawing.Point(151, 65);
            this.numericUpDownViewportScaling.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericUpDownViewportScaling.Name = "numericUpDownViewportScaling";
            this.numericUpDownViewportScaling.Size = new System.Drawing.Size(94, 20);
            this.numericUpDownViewportScaling.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(133, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Масштаб видового окна:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(128, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Масштаб содержимого:";
            // 
            // numericUpDownContentScaling
            // 
            this.numericUpDownContentScaling.DecimalPlaces = 3;
            this.numericUpDownContentScaling.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numericUpDownContentScaling.Location = new System.Drawing.Point(151, 91);
            this.numericUpDownContentScaling.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericUpDownContentScaling.Name = "numericUpDownContentScaling";
            this.numericUpDownContentScaling.Size = new System.Drawing.Size(94, 20);
            this.numericUpDownContentScaling.TabIndex = 8;
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(329, 126);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 9;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(248, 126);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "Отмена";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // LayoutSettingsWindow
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(421, 161);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.numericUpDownContentScaling);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numericUpDownViewportScaling);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxPlotterType);
            this.Controls.Add(this.comboBoxPageSize);
            this.Name = "LayoutSettingsWindow";
            this.Text = "Параметры вывода";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownViewportScaling)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownContentScaling)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxPageSize;
        private System.Windows.Forms.ComboBox comboBoxPlotterType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownViewportScaling;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownContentScaling;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
    }
}