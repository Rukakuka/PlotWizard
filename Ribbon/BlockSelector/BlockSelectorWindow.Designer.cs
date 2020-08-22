namespace PlotWizard.Ribbon
{
    partial class BlockSelectorWindow
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlockSelectorWindow));
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("+X, +Y", 4);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("+X, -Y", 1);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("+Y, +X", 0);
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("+Y, -X", 5);
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("-X, +Y", 3);
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("-X, -Y", 6);
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("-Y, +X", 7);
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("-Y, -X", 2);
            this.listBoxPrefix = new System.Windows.Forms.ListBox();
            this.listBoxPostfix = new System.Windows.Forms.ListBox();
            this.labelPrefix = new System.Windows.Forms.Label();
            this.labelPostfix = new System.Windows.Forms.Label();
            this.buttonOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.imageListSortingOrder = new System.Windows.Forms.ImageList(this.components);
            this.listViewSortingOrder = new System.Windows.Forms.ListView();
            this.labelChoosedBlockPrompt = new System.Windows.Forms.Label();
            this.textBoxChoosedBlockName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // listBoxPrefix
            // 
            this.listBoxPrefix.FormattingEnabled = true;
            this.listBoxPrefix.Location = new System.Drawing.Point(12, 66);
            this.listBoxPrefix.Name = "listBoxPrefix";
            this.listBoxPrefix.Size = new System.Drawing.Size(123, 303);
            this.listBoxPrefix.TabIndex = 0;
            // 
            // listBoxPostfix
            // 
            this.listBoxPostfix.FormattingEnabled = true;
            this.listBoxPostfix.Location = new System.Drawing.Point(154, 66);
            this.listBoxPostfix.Name = "listBoxPostfix";
            this.listBoxPostfix.Size = new System.Drawing.Size(123, 303);
            this.listBoxPostfix.TabIndex = 1;
            // 
            // labelPrefix
            // 
            this.labelPrefix.AutoSize = true;
            this.labelPrefix.Location = new System.Drawing.Point(12, 46);
            this.labelPrefix.Name = "labelPrefix";
            this.labelPrefix.Size = new System.Drawing.Size(100, 13);
            this.labelPrefix.TabIndex = 2;
            this.labelPrefix.Text = "Атрибут - префикс";
            // 
            // labelPostfix
            // 
            this.labelPostfix.AutoSize = true;
            this.labelPostfix.Location = new System.Drawing.Point(154, 46);
            this.labelPostfix.Name = "labelPostfix";
            this.labelPostfix.Size = new System.Drawing.Size(105, 13);
            this.labelPostfix.TabIndex = 3;
            this.labelPostfix.Text = "Атрибут - постфикс";
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(297, 345);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(149, 24);
            this.buttonOk.TabIndex = 4;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(297, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(152, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Порядок сортировки блоков";
            // 
            // imageListSortingOrder
            // 
            this.imageListSortingOrder.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSortingOrder.ImageStream")));
            this.imageListSortingOrder.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListSortingOrder.Images.SetKeyName(0, "icon_axes01.png");
            this.imageListSortingOrder.Images.SetKeyName(1, "icon_axes02.png");
            this.imageListSortingOrder.Images.SetKeyName(2, "icon_axes03.png");
            this.imageListSortingOrder.Images.SetKeyName(3, "icon_axes04.png");
            this.imageListSortingOrder.Images.SetKeyName(4, "icon_axes05.png");
            this.imageListSortingOrder.Images.SetKeyName(5, "icon_axes06.png");
            this.imageListSortingOrder.Images.SetKeyName(6, "icon_axes07.png");
            this.imageListSortingOrder.Images.SetKeyName(7, "icon_axes08.png");
            // 
            // listViewSortingOrder
            // 
            this.listViewSortingOrder.HideSelection = false;
            listViewItem1.StateImageIndex = 0;
            listViewItem2.StateImageIndex = 0;
            listViewItem3.StateImageIndex = 0;
            listViewItem4.StateImageIndex = 0;
            listViewItem5.StateImageIndex = 0;
            listViewItem6.StateImageIndex = 0;
            listViewItem7.StateImageIndex = 0;
            listViewItem8.StateImageIndex = 0;
            this.listViewSortingOrder.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5,
            listViewItem6,
            listViewItem7,
            listViewItem8});
            this.listViewSortingOrder.Location = new System.Drawing.Point(297, 66);
            this.listViewSortingOrder.MultiSelect = false;
            this.listViewSortingOrder.Name = "listViewSortingOrder";
            this.listViewSortingOrder.Size = new System.Drawing.Size(149, 151);
            this.listViewSortingOrder.SmallImageList = this.imageListSortingOrder;
            this.listViewSortingOrder.TabIndex = 16;
            this.listViewSortingOrder.UseCompatibleStateImageBehavior = false;
            this.listViewSortingOrder.View = System.Windows.Forms.View.SmallIcon;
            // 
            // labelChoosedBlockPrompt
            // 
            this.labelChoosedBlockPrompt.AutoSize = true;
            this.labelChoosedBlockPrompt.Location = new System.Drawing.Point(12, 22);
            this.labelChoosedBlockPrompt.Name = "labelChoosedBlockPrompt";
            this.labelChoosedBlockPrompt.Size = new System.Drawing.Size(96, 13);
            this.labelChoosedBlockPrompt.TabIndex = 17;
            this.labelChoosedBlockPrompt.Text = "Выбранный блок:";
            // 
            // textBoxChoosedBlockName
            // 
            this.textBoxChoosedBlockName.Location = new System.Drawing.Point(114, 19);
            this.textBoxChoosedBlockName.Name = "textBoxChoosedBlockName";
            this.textBoxChoosedBlockName.ReadOnly = true;
            this.textBoxChoosedBlockName.Size = new System.Drawing.Size(335, 20);
            this.textBoxChoosedBlockName.TabIndex = 18;
            this.textBoxChoosedBlockName.Text = "(Нет)";
            // 
            // BlockSelectorWindow
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 389);
            this.Controls.Add(this.textBoxChoosedBlockName);
            this.Controls.Add(this.labelChoosedBlockPrompt);
            this.Controls.Add(this.listViewSortingOrder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.labelPostfix);
            this.Controls.Add(this.labelPrefix);
            this.Controls.Add(this.listBoxPostfix);
            this.Controls.Add(this.listBoxPrefix);
            this.Name = "BlockSelectorWindow";
            this.Text = "Настройка объектов печати";
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
        private System.Windows.Forms.ImageList imageListSortingOrder;
        private System.Windows.Forms.ListView listViewSortingOrder;
        private System.Windows.Forms.Label labelChoosedBlockPrompt;
        private System.Windows.Forms.TextBox textBoxChoosedBlockName;
    }
}