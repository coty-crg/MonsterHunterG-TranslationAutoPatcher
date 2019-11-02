namespace TestApp
{
    partial class Form1
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
            this.SelectFolderButton = new System.Windows.Forms.Button();
            this.SelectFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.PatchProgressBar = new System.Windows.Forms.ProgressBar();
            this.SelectISOText = new System.Windows.Forms.TextBox();
            this.OutputFileButton = new System.Windows.Forms.Button();
            this.OutputFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.OutputFileText = new System.Windows.Forms.TextBox();
            this.ApplyPatchButton = new System.Windows.Forms.Button();
            this.SelectPatchFileButton = new System.Windows.Forms.Button();
            this.PatchFileData = new System.Windows.Forms.TextBox();
            this.SelectPatchFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.OutputText = new System.Windows.Forms.TextBox();
            this.OpenISODialog = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.ImgBurnPathTextBox = new System.Windows.Forms.TextBox();
            this.LogLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.CleanupCheckbox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.KeepOldArchivesCheckBox = new System.Windows.Forms.CheckBox();
            this.DeepSearchCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // SelectFolderButton
            // 
            this.SelectFolderButton.Location = new System.Drawing.Point(14, 12);
            this.SelectFolderButton.Name = "SelectFolderButton";
            this.SelectFolderButton.Size = new System.Drawing.Size(145, 57);
            this.SelectFolderButton.TabIndex = 0;
            this.SelectFolderButton.Text = "Select Input ISO";
            this.SelectFolderButton.UseVisualStyleBackColor = true;
            this.SelectFolderButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // SelectFolderDialog
            // 
            this.SelectFolderDialog.HelpRequest += new System.EventHandler(this.folderBrowserDialog1_HelpRequest_1);
            // 
            // PatchProgressBar
            // 
            this.PatchProgressBar.Location = new System.Drawing.Point(14, 490);
            this.PatchProgressBar.Name = "PatchProgressBar";
            this.PatchProgressBar.Size = new System.Drawing.Size(292, 23);
            this.PatchProgressBar.TabIndex = 1;
            this.PatchProgressBar.Click += new System.EventHandler(this.PatchProgressBar_Click);
            // 
            // SelectISOText
            // 
            this.SelectISOText.Location = new System.Drawing.Point(14, 75);
            this.SelectISOText.Name = "SelectISOText";
            this.SelectISOText.Size = new System.Drawing.Size(145, 20);
            this.SelectISOText.TabIndex = 2;
            this.SelectISOText.TextChanged += new System.EventHandler(this.SelectFolderText_TextChanged);
            // 
            // OutputFileButton
            // 
            this.OutputFileButton.Location = new System.Drawing.Point(162, 12);
            this.OutputFileButton.Name = "OutputFileButton";
            this.OutputFileButton.Size = new System.Drawing.Size(144, 57);
            this.OutputFileButton.TabIndex = 3;
            this.OutputFileButton.Text = "Select Output ISO";
            this.OutputFileButton.UseVisualStyleBackColor = true;
            this.OutputFileButton.Click += new System.EventHandler(this.OutputFileButton_Click);
            // 
            // OutputFileDialog
            // 
            this.OutputFileDialog.FileName = "patched.ISO";
            // 
            // OutputFileText
            // 
            this.OutputFileText.Location = new System.Drawing.Point(162, 75);
            this.OutputFileText.Name = "OutputFileText";
            this.OutputFileText.Size = new System.Drawing.Size(144, 20);
            this.OutputFileText.TabIndex = 4;
            // 
            // ApplyPatchButton
            // 
            this.ApplyPatchButton.AutoSize = true;
            this.ApplyPatchButton.Enabled = false;
            this.ApplyPatchButton.Location = new System.Drawing.Point(12, 203);
            this.ApplyPatchButton.Name = "ApplyPatchButton";
            this.ApplyPatchButton.Size = new System.Drawing.Size(193, 125);
            this.ApplyPatchButton.TabIndex = 5;
            this.ApplyPatchButton.Text = "PATCH";
            this.ApplyPatchButton.UseVisualStyleBackColor = true;
            this.ApplyPatchButton.Click += new System.EventHandler(this.ApplyPatchButton_Click);
            // 
            // SelectPatchFileButton
            // 
            this.SelectPatchFileButton.Location = new System.Drawing.Point(14, 140);
            this.SelectPatchFileButton.Name = "SelectPatchFileButton";
            this.SelectPatchFileButton.Size = new System.Drawing.Size(142, 57);
            this.SelectPatchFileButton.TabIndex = 7;
            this.SelectPatchFileButton.Text = "Select Patch File (*.csv)";
            this.SelectPatchFileButton.UseVisualStyleBackColor = true;
            this.SelectPatchFileButton.Click += new System.EventHandler(this.SelectPatchFileButton_Click);
            // 
            // PatchFileData
            // 
            this.PatchFileData.AllowDrop = true;
            this.PatchFileData.Location = new System.Drawing.Point(164, 140);
            this.PatchFileData.Multiline = true;
            this.PatchFileData.Name = "PatchFileData";
            this.PatchFileData.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.PatchFileData.Size = new System.Drawing.Size(142, 57);
            this.PatchFileData.TabIndex = 8;
            // 
            // SelectPatchFileDialog
            // 
            this.SelectPatchFileDialog.FileName = "patch.txt";
            // 
            // OutputText
            // 
            this.OutputText.Location = new System.Drawing.Point(14, 347);
            this.OutputText.Multiline = true;
            this.OutputText.Name = "OutputText";
            this.OutputText.ReadOnly = true;
            this.OutputText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.OutputText.Size = new System.Drawing.Size(292, 121);
            this.OutputText.TabIndex = 9;
            this.OutputText.TextChanged += new System.EventHandler(this.OutputText_TextChanged);
            // 
            // OpenISODialog
            // 
            this.OpenISODialog.FileName = "file.ISO";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 98);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "ImgBurn Path";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // ImgBurnPathTextBox
            // 
            this.ImgBurnPathTextBox.Location = new System.Drawing.Point(14, 114);
            this.ImgBurnPathTextBox.Name = "ImgBurnPathTextBox";
            this.ImgBurnPathTextBox.Size = new System.Drawing.Size(292, 20);
            this.ImgBurnPathTextBox.TabIndex = 11;
            this.ImgBurnPathTextBox.Text = "C:/Program Files (x86)/ImgBurn/ImgBurn.exe";
            // 
            // LogLabel
            // 
            this.LogLabel.AutoSize = true;
            this.LogLabel.Location = new System.Drawing.Point(14, 331);
            this.LogLabel.Name = "LogLabel";
            this.LogLabel.Size = new System.Drawing.Size(54, 13);
            this.LogLabel.TabIndex = 12;
            this.LogLabel.Text = "output log";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 471);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Patching progress";
            // 
            // CleanupCheckbox
            // 
            this.CleanupCheckbox.AutoSize = true;
            this.CleanupCheckbox.Location = new System.Drawing.Point(216, 219);
            this.CleanupCheckbox.Name = "CleanupCheckbox";
            this.CleanupCheckbox.Size = new System.Drawing.Size(90, 17);
            this.CleanupCheckbox.TabIndex = 16;
            this.CleanupCheckbox.Text = "Cleanup After";
            this.CleanupCheckbox.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(213, 203);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Options";
            // 
            // KeepOldArchivesCheckBox
            // 
            this.KeepOldArchivesCheckBox.AutoSize = true;
            this.KeepOldArchivesCheckBox.Location = new System.Drawing.Point(216, 243);
            this.KeepOldArchivesCheckBox.Name = "KeepOldArchivesCheckBox";
            this.KeepOldArchivesCheckBox.Size = new System.Drawing.Size(111, 17);
            this.KeepOldArchivesCheckBox.TabIndex = 18;
            this.KeepOldArchivesCheckBox.Text = "Keep old archives";
            this.KeepOldArchivesCheckBox.UseVisualStyleBackColor = true;
            // 
            // DeepSearchCheckbox
            // 
            this.DeepSearchCheckbox.AutoSize = true;
            this.DeepSearchCheckbox.Location = new System.Drawing.Point(216, 266);
            this.DeepSearchCheckbox.Name = "DeepSearchCheckbox";
            this.DeepSearchCheckbox.Size = new System.Drawing.Size(89, 17);
            this.DeepSearchCheckbox.TabIndex = 19;
            this.DeepSearchCheckbox.Text = "Deep Search";
            this.DeepSearchCheckbox.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 521);
            this.Controls.Add(this.DeepSearchCheckbox);
            this.Controls.Add(this.KeepOldArchivesCheckBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.CleanupCheckbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.LogLabel);
            this.Controls.Add(this.ImgBurnPathTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.OutputText);
            this.Controls.Add(this.PatchFileData);
            this.Controls.Add(this.SelectPatchFileButton);
            this.Controls.Add(this.ApplyPatchButton);
            this.Controls.Add(this.OutputFileText);
            this.Controls.Add(this.OutputFileButton);
            this.Controls.Add(this.SelectISOText);
            this.Controls.Add(this.PatchProgressBar);
            this.Controls.Add(this.SelectFolderButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(340, 560);
            this.MinimumSize = new System.Drawing.Size(340, 560);
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "ISO Translation Patcher";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button SelectFolderButton;
        private System.Windows.Forms.FolderBrowserDialog SelectFolderDialog;
        private System.Windows.Forms.ProgressBar PatchProgressBar;
        private System.Windows.Forms.TextBox SelectISOText;
        private System.Windows.Forms.Button OutputFileButton;
        private System.Windows.Forms.SaveFileDialog OutputFileDialog;
        private System.Windows.Forms.TextBox OutputFileText;
        private System.Windows.Forms.Button ApplyPatchButton;
        private System.Windows.Forms.Button SelectPatchFileButton;
        private System.Windows.Forms.TextBox PatchFileData;
        private System.Windows.Forms.OpenFileDialog SelectPatchFileDialog;
        private System.Windows.Forms.TextBox OutputText;
        private System.Windows.Forms.OpenFileDialog OpenISODialog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ImgBurnPathTextBox;
        private System.Windows.Forms.Label LogLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox CleanupCheckbox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox KeepOldArchivesCheckBox;
        private System.Windows.Forms.CheckBox DeepSearchCheckbox;
    }
}

