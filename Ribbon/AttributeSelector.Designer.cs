namespace PlotWizard.Ribbon
{
    partial class AttributeSelector
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
            this.SuspendLayout();
            // 
            // listBoxPrefix
            // 
            this.listBoxPrefix.FormattingEnabled = true;
            this.listBoxPrefix.Location = new System.Drawing.Point(12, 41);
            this.listBoxPrefix.Name = "listBoxPrefix";
            this.listBoxPrefix.Size = new System.Drawing.Size(123, 290);
            this.listBoxPrefix.TabIndex = 0;
            this.listBoxPrefix.SelectedIndexChanged += new System.EventHandler(this.ListBox_SelectedIndexChanged);
            // 
            // listBoxPostfix
            // 
            this.listBoxPostfix.FormattingEnabled = true;
            this.listBoxPostfix.Location = new System.Drawing.Point(149, 41);
            this.listBoxPostfix.Name = "listBoxPostfix";
            this.listBoxPostfix.Size = new System.Drawing.Size(123, 290);
            this.listBoxPostfix.TabIndex = 1;
            this.listBoxPostfix.SelectedIndexChanged += new System.EventHandler(this.ListBox_SelectedIndexChanged);
            // 
            // labelPrefix
            // 
            this.labelPrefix.AutoSize = true;
            this.labelPrefix.Location = new System.Drawing.Point(13, 13);
            this.labelPrefix.Name = "labelPrefix";
            this.labelPrefix.Size = new System.Drawing.Size(100, 13);
            this.labelPrefix.TabIndex = 2;
            this.labelPrefix.Text = "Атрибут - префикс";
            // 
            // labelPostfix
            // 
            this.labelPostfix.AutoSize = true;
            this.labelPostfix.Location = new System.Drawing.Point(146, 13);
            this.labelPostfix.Name = "labelPostfix";
            this.labelPostfix.Size = new System.Drawing.Size(105, 13);
            this.labelPostfix.TabIndex = 3;
            this.labelPostfix.Text = "Атрибут - постфикс";
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(197, 337);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 4;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // AttributeSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 371);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.labelPostfix);
            this.Controls.Add(this.labelPrefix);
            this.Controls.Add(this.listBoxPostfix);
            this.Controls.Add(this.listBoxPrefix);
            this.Name = "AttributeSelector";
            this.Text = " Выберите атрибуты блока...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxPrefix;
        private System.Windows.Forms.ListBox listBoxPostfix;
        private System.Windows.Forms.Label labelPrefix;
        private System.Windows.Forms.Label labelPostfix;
        private System.Windows.Forms.Button buttonOk;
    }
}