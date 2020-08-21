namespace PlotWizard.Ribbon
{
    partial class AttributeSelectorWindow
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
            this.listBoxPrefix = new System.Windows.Forms.ListBox();
            this.listBoxPostfix = new System.Windows.Forms.ListBox();
            this.labelPrefix = new System.Windows.Forms.Label();
            this.labelPostfix = new System.Windows.Forms.Label();
            this.buttonOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridSortingOrder = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridSortingOrder)).BeginInit();
            this.SuspendLayout();
            // 
            // listBoxPrefix
            // 
            this.listBoxPrefix.FormattingEnabled = true;
            this.listBoxPrefix.Location = new System.Drawing.Point(12, 41);
            this.listBoxPrefix.Name = "listBoxPrefix";
            this.listBoxPrefix.Size = new System.Drawing.Size(123, 290);
            this.listBoxPrefix.TabIndex = 0;
            // 
            // listBoxPostfix
            // 
            this.listBoxPostfix.FormattingEnabled = true;
            this.listBoxPostfix.Location = new System.Drawing.Point(153, 41);
            this.listBoxPostfix.Name = "listBoxPostfix";
            this.listBoxPostfix.Size = new System.Drawing.Size(123, 290);
            this.listBoxPostfix.TabIndex = 1;
            // 
            // labelPrefix
            // 
            this.labelPrefix.AutoSize = true;
            this.labelPrefix.Location = new System.Drawing.Point(12, 13);
            this.labelPrefix.Name = "labelPrefix";
            this.labelPrefix.Size = new System.Drawing.Size(100, 13);
            this.labelPrefix.TabIndex = 2;
            this.labelPrefix.Text = "Атрибут - префикс";
            // 
            // labelPostfix
            // 
            this.labelPostfix.AutoSize = true;
            this.labelPostfix.Location = new System.Drawing.Point(153, 13);
            this.labelPostfix.Name = "labelPostfix";
            this.labelPostfix.Size = new System.Drawing.Size(105, 13);
            this.labelPostfix.TabIndex = 3;
            this.labelPostfix.Text = "Атрибут - постфикс";
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(338, 337);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 4;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(294, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Метод сортировки";
            // 
            // dataGridSortingOrder
            // 
            this.dataGridSortingOrder.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridSortingOrder.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridSortingOrder.Location = new System.Drawing.Point(294, 41);
            this.dataGridSortingOrder.Name = "dataGridSortingOrder";
            this.dataGridSortingOrder.Size = new System.Drawing.Size(128, 290);
            this.dataGridSortingOrder.TabIndex = 7;
            // 
            // AttributeSelectorWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 371);
            this.Controls.Add(this.dataGridSortingOrder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.labelPostfix);
            this.Controls.Add(this.labelPrefix);
            this.Controls.Add(this.listBoxPostfix);
            this.Controls.Add(this.listBoxPrefix);
            this.Name = "AttributeSelectorWindow";
            this.Text = " Выберите атрибуты блока...";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridSortingOrder)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxPrefix;
        private System.Windows.Forms.ListBox listBoxPostfix;
        private System.Windows.Forms.Label labelPrefix;
        private System.Windows.Forms.Label labelPostfix;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dataGridSortingOrder;
    }
}