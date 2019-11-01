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
            this.Header = new System.Windows.Forms.Label();
            this.SelectPatchFileButton = new System.Windows.Forms.Button();
            this.PatchFileData = new System.Windows.Forms.TextBox();
            this.SelectPatchFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.OutputText = new System.Windows.Forms.TextBox();
            this.OpenISODialog = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // SelectFolderButton
            // 
            this.SelectFolderButton.Location = new System.Drawing.Point(12, 49);
            this.SelectFolderButton.Name = "SelectFolderButton";
            this.SelectFolderButton.Size = new System.Drawing.Size(142, 89);
            this.SelectFolderButton.TabIndex = 0;
            this.SelectFolderButton.Text = "Select Input Folder";
            this.SelectFolderButton.UseVisualStyleBackColor = true;
            this.SelectFolderButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // SelectFolderDialog
            // 
            this.SelectFolderDialog.HelpRequest += new System.EventHandler(this.folderBrowserDialog1_HelpRequest_1);
            // 
            // PatchProgressBar
            // 
            this.PatchProgressBar.Location = new System.Drawing.Point(12, 360);
            this.PatchProgressBar.Name = "PatchProgressBar";
            this.PatchProgressBar.Size = new System.Drawing.Size(292, 23);
            this.PatchProgressBar.TabIndex = 1;
            this.PatchProgressBar.Click += new System.EventHandler(this.PatchProgressBar_Click);
            // 
            // SelectISOText
            // 
            this.SelectISOText.Location = new System.Drawing.Point(12, 144);
            this.SelectISOText.Name = "SelectISOText";
            this.SelectISOText.Size = new System.Drawing.Size(142, 20);
            this.SelectISOText.TabIndex = 2;
            this.SelectISOText.TextChanged += new System.EventHandler(this.SelectFolderText_TextChanged);
            // 
            // OutputFileButton
            // 
            this.OutputFileButton.Location = new System.Drawing.Point(162, 49);
            this.OutputFileButton.Name = "OutputFileButton";
            this.OutputFileButton.Size = new System.Drawing.Size(142, 89);
            this.OutputFileButton.TabIndex = 3;
            this.OutputFileButton.Text = "Select Output File";
            this.OutputFileButton.UseVisualStyleBackColor = true;
            this.OutputFileButton.Click += new System.EventHandler(this.OutputFileButton_Click);
            // 
            // OutputFileDialog
            // 
            this.OutputFileDialog.FileName = "patched.ISO";
            // 
            // OutputFileText
            // 
            this.OutputFileText.Location = new System.Drawing.Point(162, 144);
            this.OutputFileText.Name = "OutputFileText";
            this.OutputFileText.Size = new System.Drawing.Size(142, 20);
            this.OutputFileText.TabIndex = 4;
            // 
            // ApplyPatchButton
            // 
            this.ApplyPatchButton.Enabled = false;
            this.ApplyPatchButton.Location = new System.Drawing.Point(12, 265);
            this.ApplyPatchButton.Name = "ApplyPatchButton";
            this.ApplyPatchButton.Size = new System.Drawing.Size(292, 89);
            this.ApplyPatchButton.TabIndex = 5;
            this.ApplyPatchButton.Text = "Apply Patch";
            this.ApplyPatchButton.UseVisualStyleBackColor = true;
            this.ApplyPatchButton.Click += new System.EventHandler(this.ApplyPatchButton_Click);
            // 
            // Header
            // 
            this.Header.AutoSize = true;
            this.Header.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Header.Location = new System.Drawing.Point(23, 13);
            this.Header.Name = "Header";
            this.Header.Size = new System.Drawing.Size(268, 33);
            this.Header.TabIndex = 6;
            this.Header.Text = "Translation Patcher";
            this.Header.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SelectPatchFileButton
            // 
            this.SelectPatchFileButton.Location = new System.Drawing.Point(12, 170);
            this.SelectPatchFileButton.Name = "SelectPatchFileButton";
            this.SelectPatchFileButton.Size = new System.Drawing.Size(142, 89);
            this.SelectPatchFileButton.TabIndex = 7;
            this.SelectPatchFileButton.Text = "Select Patch File";
            this.SelectPatchFileButton.UseVisualStyleBackColor = true;
            this.SelectPatchFileButton.Click += new System.EventHandler(this.SelectPatchFileButton_Click);
            // 
            // PatchFileData
            // 
            this.PatchFileData.AllowDrop = true;
            this.PatchFileData.Location = new System.Drawing.Point(160, 170);
            this.PatchFileData.Multiline = true;
            this.PatchFileData.Name = "PatchFileData";
            this.PatchFileData.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.PatchFileData.Size = new System.Drawing.Size(142, 89);
            this.PatchFileData.TabIndex = 8;
            // 
            // SelectPatchFileDialog
            // 
            this.SelectPatchFileDialog.FileName = "patch.txt";
            // 
            // OutputText
            // 
            this.OutputText.Enabled = false;
            this.OutputText.Location = new System.Drawing.Point(12, 389);
            this.OutputText.Multiline = true;
            this.OutputText.Name = "OutputText";
            this.OutputText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.OutputText.Size = new System.Drawing.Size(292, 154);
            this.OutputText.TabIndex = 9;
            this.OutputText.TextChanged += new System.EventHandler(this.OutputText_TextChanged);
            // 
            // OpenISODialog
            // 
            this.OpenISODialog.FileName = "file.ISO";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 555);
            this.Controls.Add(this.OutputText);
            this.Controls.Add(this.PatchFileData);
            this.Controls.Add(this.SelectPatchFileButton);
            this.Controls.Add(this.Header);
            this.Controls.Add(this.ApplyPatchButton);
            this.Controls.Add(this.OutputFileText);
            this.Controls.Add(this.OutputFileButton);
            this.Controls.Add(this.SelectISOText);
            this.Controls.Add(this.PatchProgressBar);
            this.Controls.Add(this.SelectFolderButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(332, 594);
            this.MinimumSize = new System.Drawing.Size(332, 594);
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
        private System.Windows.Forms.Label Header;
        private System.Windows.Forms.Button SelectPatchFileButton;
        private System.Windows.Forms.TextBox PatchFileData;
        private System.Windows.Forms.OpenFileDialog SelectPatchFileDialog;
        private System.Windows.Forms.TextBox OutputText;
        private System.Windows.Forms.OpenFileDialog OpenISODialog;
    }
}

