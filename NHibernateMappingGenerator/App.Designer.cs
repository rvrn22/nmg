namespace NHibernateMappingGenerator
{
    partial class App
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(App));
            this.connStrTextBox = new System.Windows.Forms.TextBox();
            this.dbConnStrLabel = new System.Windows.Forms.Label();
            this.connectBtn = new System.Windows.Forms.Button();
            this.tablesComboBox = new System.Windows.Forms.ComboBox();
            this.sequencesComboBox = new System.Windows.Forms.ComboBox();
            this.dbTableDetailsGridView = new System.Windows.Forms.DataGridView();
            this.errorLabel = new System.Windows.Forms.Label();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.folderTextBox = new System.Windows.Forms.TextBox();
            this.generateButton = new System.Windows.Forms.Button();
            this.folderSelectButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nameSpaceTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.assemblyNameTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dbTableDetailsGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // connStrTextBox
            // 
            this.connStrTextBox.Location = new System.Drawing.Point(147, 9);
            this.connStrTextBox.Name = "connStrTextBox";
            this.connStrTextBox.Size = new System.Drawing.Size(606, 20);
            this.connStrTextBox.TabIndex = 0;
            this.connStrTextBox.Text = "Data Source=XE; user ID=Sample; Password=password;";
            // 
            // dbConnStrLabel
            // 
            this.dbConnStrLabel.AutoSize = true;
            this.dbConnStrLabel.Location = new System.Drawing.Point(13, 13);
            this.dbConnStrLabel.Name = "dbConnStrLabel";
            this.dbConnStrLabel.Size = new System.Drawing.Size(109, 13);
            this.dbConnStrLabel.TabIndex = 1;
            this.dbConnStrLabel.Text = "DB Connection String";
            // 
            // connectBtn
            // 
            this.connectBtn.Location = new System.Drawing.Point(759, 8);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(75, 23);
            this.connectBtn.TabIndex = 2;
            this.connectBtn.Text = "&Connect";
            this.connectBtn.UseVisualStyleBackColor = true;
            this.connectBtn.Click += new System.EventHandler(this.connectBtn_Click);
            // 
            // tablesComboBox
            // 
            this.tablesComboBox.FormattingEnabled = true;
            this.tablesComboBox.Location = new System.Drawing.Point(16, 63);
            this.tablesComboBox.Name = "tablesComboBox";
            this.tablesComboBox.Size = new System.Drawing.Size(406, 21);
            this.tablesComboBox.TabIndex = 3;
            // 
            // sequencesComboBox
            // 
            this.sequencesComboBox.FormattingEnabled = true;
            this.sequencesComboBox.Location = new System.Drawing.Point(428, 63);
            this.sequencesComboBox.Name = "sequencesComboBox";
            this.sequencesComboBox.Size = new System.Drawing.Size(406, 21);
            this.sequencesComboBox.TabIndex = 4;
            // 
            // dbTableDetailsGridView
            // 
            this.dbTableDetailsGridView.AllowUserToAddRows = false;
            this.dbTableDetailsGridView.AllowUserToDeleteRows = false;
            this.dbTableDetailsGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dbTableDetailsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dbTableDetailsGridView.Location = new System.Drawing.Point(16, 110);
            this.dbTableDetailsGridView.Name = "dbTableDetailsGridView";
            this.dbTableDetailsGridView.ReadOnly = true;
            this.dbTableDetailsGridView.RowHeadersVisible = false;
            this.dbTableDetailsGridView.Size = new System.Drawing.Size(818, 281);
            this.dbTableDetailsGridView.TabIndex = 5;
            // 
            // errorLabel
            // 
            this.errorLabel.AutoSize = true;
            this.errorLabel.Location = new System.Drawing.Point(16, 595);
            this.errorLabel.Name = "errorLabel";
            this.errorLabel.Size = new System.Drawing.Size(0, 13);
            this.errorLabel.TabIndex = 6;
            // 
            // folderTextBox
            // 
            this.folderTextBox.Location = new System.Drawing.Point(16, 424);
            this.folderTextBox.Name = "folderTextBox";
            this.folderTextBox.Size = new System.Drawing.Size(605, 20);
            this.folderTextBox.TabIndex = 7;
            this.folderTextBox.Text = "D:\\Temp\\";
            // 
            // generateButton
            // 
            this.generateButton.Location = new System.Drawing.Point(16, 526);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(75, 23);
            this.generateButton.TabIndex = 8;
            this.generateButton.Text = "&Generate";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // folderSelectButton
            // 
            this.folderSelectButton.Location = new System.Drawing.Point(628, 423);
            this.folderSelectButton.Name = "folderSelectButton";
            this.folderSelectButton.Size = new System.Drawing.Size(75, 23);
            this.folderSelectButton.TabIndex = 9;
            this.folderSelectButton.Text = "&Select";
            this.folderSelectButton.UseVisualStyleBackColor = true;
            this.folderSelectButton.Click += new System.EventHandler(this.folderSelectButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 407);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(363, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Select the folder in which the mapping and domain files would be generated";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 459);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "NameSpace :";
            // 
            // nameSpaceTextBox
            // 
            this.nameSpaceTextBox.Location = new System.Drawing.Point(107, 456);
            this.nameSpaceTextBox.Name = "nameSpaceTextBox";
            this.nameSpaceTextBox.Size = new System.Drawing.Size(364, 20);
            this.nameSpaceTextBox.TabIndex = 12;
            this.nameSpaceTextBox.Text = "Sample.CustomerService.Domain";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 490);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "AssemblyName :";
            // 
            // assemblyNameTextBox
            // 
            this.assemblyNameTextBox.Location = new System.Drawing.Point(107, 483);
            this.assemblyNameTextBox.Name = "assemblyNameTextBox";
            this.assemblyNameTextBox.Size = new System.Drawing.Size(364, 20);
            this.assemblyNameTextBox.TabIndex = 14;
            this.assemblyNameTextBox.Text = "Sample.CustomerService.Domain";
            // 
            // App
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(849, 620);
            this.Controls.Add(this.assemblyNameTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.nameSpaceTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.folderSelectButton);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.folderTextBox);
            this.Controls.Add(this.errorLabel);
            this.Controls.Add(this.dbTableDetailsGridView);
            this.Controls.Add(this.sequencesComboBox);
            this.Controls.Add(this.tablesComboBox);
            this.Controls.Add(this.connectBtn);
            this.Controls.Add(this.dbConnStrLabel);
            this.Controls.Add(this.connStrTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "App";
            this.Text = "NHibernate Mapping Generator";
            ((System.ComponentModel.ISupportInitialize)(this.dbTableDetailsGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox connStrTextBox;
        private System.Windows.Forms.Label dbConnStrLabel;
        private System.Windows.Forms.Button connectBtn;
        private System.Windows.Forms.ComboBox tablesComboBox;
        private System.Windows.Forms.ComboBox sequencesComboBox;
        private System.Windows.Forms.DataGridView dbTableDetailsGridView;
        private System.Windows.Forms.Label errorLabel;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.TextBox folderTextBox;
        private System.Windows.Forms.Button generateButton;
        private System.Windows.Forms.Button folderSelectButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox nameSpaceTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox assemblyNameTextBox;
    }
}

