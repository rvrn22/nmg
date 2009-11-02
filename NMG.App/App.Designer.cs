using System.Drawing;
using System.Windows.Forms;

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
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnDataType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cSharpType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.isPrimaryKey = new System.Windows.Forms.DataGridViewCheckBoxColumn();
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
            this.serverTypeComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.generateAllBtn = new System.Windows.Forms.Button();
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.basicSettingsTabPage = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.advanceSettingsTabPage = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.fluentMappingOption = new System.Windows.Forms.RadioButton();
            this.hbmMappingOption = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.vbRadioButton = new System.Windows.Forms.RadioButton();
            this.cSharpRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.prefixTextBox = new System.Windows.Forms.TextBox();
            this.prefixRadioButton = new System.Windows.Forms.RadioButton();
            this.prefixLabel = new System.Windows.Forms.Label();
            this.camelCasedRadioButton = new System.Windows.Forms.RadioButton();
            this.sameAsDBRadioButton = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.dbTableDetailsGridView)).BeginInit();
            this.mainTabControl.SuspendLayout();
            this.basicSettingsTabPage.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.advanceSettingsTabPage.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // connStrTextBox
            // 
            this.connStrTextBox.Location = new System.Drawing.Point(142, 21);
            this.connStrTextBox.Name = "connStrTextBox";
            this.connStrTextBox.Size = new System.Drawing.Size(490, 20);
            this.connStrTextBox.TabIndex = 0;
            this.connStrTextBox.Text = "Data Source=XE; user ID=Sample; Password=password;";
            // 
            // dbConnStrLabel
            // 
            this.dbConnStrLabel.AutoSize = true;
            this.dbConnStrLabel.Location = new System.Drawing.Point(8, 25);
            this.dbConnStrLabel.Name = "dbConnStrLabel";
            this.dbConnStrLabel.Size = new System.Drawing.Size(109, 13);
            this.dbConnStrLabel.TabIndex = 1;
            this.dbConnStrLabel.Text = "DB Connection String";
            // 
            // connectBtn
            // 
            this.connectBtn.Location = new System.Drawing.Point(898, 30);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(75, 23);
            this.connectBtn.TabIndex = 2;
            this.connectBtn.Text = "&Connect";
            this.connectBtn.UseVisualStyleBackColor = true;
            this.connectBtn.Click += new System.EventHandler(this.connectBtnClicked);
            // 
            // tablesComboBox
            // 
            this.tablesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tablesComboBox.FormattingEnabled = true;
            this.tablesComboBox.Location = new System.Drawing.Point(11, 72);
            this.tablesComboBox.Name = "tablesComboBox";
            this.tablesComboBox.Size = new System.Drawing.Size(496, 21);
            this.tablesComboBox.TabIndex = 3;
            // 
            // sequencesComboBox
            // 
            this.sequencesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sequencesComboBox.FormattingEnabled = true;
            this.sequencesComboBox.Location = new System.Drawing.Point(513, 72);
            this.sequencesComboBox.Name = "sequencesComboBox";
            this.sequencesComboBox.Size = new System.Drawing.Size(460, 21);
            this.sequencesComboBox.TabIndex = 4;
            // 
            // dbTableDetailsGridView
            // 
            this.dbTableDetailsGridView.AllowUserToAddRows = false;
            this.dbTableDetailsGridView.AllowUserToDeleteRows = false;
            this.dbTableDetailsGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dbTableDetailsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dbTableDetailsGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnName,
            this.columnDataType,
            this.cSharpType,
            this.isPrimaryKey});
            this.dbTableDetailsGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dbTableDetailsGridView.Location = new System.Drawing.Point(3, 16);
            this.dbTableDetailsGridView.Name = "dbTableDetailsGridView";
            this.dbTableDetailsGridView.RowHeadersVisible = false;
            this.dbTableDetailsGridView.Size = new System.Drawing.Size(1038, 294);
            this.dbTableDetailsGridView.TabIndex = 5;
            // 
            // columnName
            // 
            this.columnName.HeaderText = "Column Name";
            this.columnName.Name = "columnName";
            this.columnName.ReadOnly = true;
            // 
            // columnDataType
            // 
            this.columnDataType.HeaderText = "Data Type";
            this.columnDataType.Name = "columnDataType";
            this.columnDataType.ReadOnly = true;
            // 
            // cSharpType
            // 
            this.cSharpType.HeaderText = "C# Type";
            this.cSharpType.Name = "cSharpType";
            this.cSharpType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.cSharpType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // isPrimaryKey
            // 
            this.isPrimaryKey.HeaderText = "Primary Key";
            this.isPrimaryKey.Name = "isPrimaryKey";
            this.isPrimaryKey.ReadOnly = true;
            // 
            // errorLabel
            // 
            this.errorLabel.AutoSize = true;
            this.errorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.errorLabel.ForeColor = System.Drawing.Color.Crimson;
            this.errorLabel.Location = new System.Drawing.Point(150, 530);
            this.errorLabel.Name = "errorLabel";
            this.errorLabel.Size = new System.Drawing.Size(0, 20);
            this.errorLabel.TabIndex = 6;
            // 
            // folderTextBox
            // 
            this.folderTextBox.Location = new System.Drawing.Point(17, 44);
            this.folderTextBox.Name = "folderTextBox";
            this.folderTextBox.Size = new System.Drawing.Size(605, 20);
            this.folderTextBox.TabIndex = 7;
            this.folderTextBox.Text = "D:\\Temp\\";
            // 
            // generateButton
            // 
            this.generateButton.Location = new System.Drawing.Point(17, 158);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(106, 23);
            this.generateButton.TabIndex = 8;
            this.generateButton.Text = "&Generate";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.GenerateClicked);
            // 
            // folderSelectButton
            // 
            this.folderSelectButton.Location = new System.Drawing.Point(629, 43);
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
            this.label1.Location = new System.Drawing.Point(17, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(363, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Select the folder in which the mapping and domain files would be generated";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "NameSpace :";
            // 
            // nameSpaceTextBox
            // 
            this.nameSpaceTextBox.Location = new System.Drawing.Point(108, 81);
            this.nameSpaceTextBox.Name = "nameSpaceTextBox";
            this.nameSpaceTextBox.Size = new System.Drawing.Size(514, 20);
            this.nameSpaceTextBox.TabIndex = 12;
            this.nameSpaceTextBox.Text = "Sample.CustomerService.Domain";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 125);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "AssemblyName :";
            // 
            // assemblyNameTextBox
            // 
            this.assemblyNameTextBox.Location = new System.Drawing.Point(108, 118);
            this.assemblyNameTextBox.Name = "assemblyNameTextBox";
            this.assemblyNameTextBox.Size = new System.Drawing.Size(514, 20);
            this.assemblyNameTextBox.TabIndex = 14;
            this.assemblyNameTextBox.Text = "Sample.CustomerService.Domain";
            // 
            // serverTypeComboBox
            // 
            this.serverTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.serverTypeComboBox.FormattingEnabled = true;
            this.serverTypeComboBox.Location = new System.Drawing.Point(639, 21);
            this.serverTypeComboBox.Name = "serverTypeComboBox";
            this.serverTypeComboBox.Size = new System.Drawing.Size(244, 21);
            this.serverTypeComboBox.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "Select a table";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(513, 53);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(207, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Select the sequence for the selected table";
            // 
            // generateAllBtn
            // 
            this.generateAllBtn.Location = new System.Drawing.Point(516, 158);
            this.generateAllBtn.Name = "generateAllBtn";
            this.generateAllBtn.Size = new System.Drawing.Size(106, 23);
            this.generateAllBtn.TabIndex = 18;
            this.generateAllBtn.Text = "Generate &All";
            this.generateAllBtn.UseVisualStyleBackColor = true;
            this.generateAllBtn.Click += new System.EventHandler(this.GenerateAllClicked);
            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.basicSettingsTabPage);
            this.mainTabControl.Controls.Add(this.advanceSettingsTabPage);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Location = new System.Drawing.Point(0, 0);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(1058, 645);
            this.mainTabControl.TabIndex = 19;
            // 
            // basicSettingsTabPage
            // 
            this.basicSettingsTabPage.Controls.Add(this.groupBox6);
            this.basicSettingsTabPage.Controls.Add(this.groupBox5);
            this.basicSettingsTabPage.Controls.Add(this.groupBox4);
            this.basicSettingsTabPage.Controls.Add(this.errorLabel);
            this.basicSettingsTabPage.Location = new System.Drawing.Point(4, 22);
            this.basicSettingsTabPage.Name = "basicSettingsTabPage";
            this.basicSettingsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.basicSettingsTabPage.Size = new System.Drawing.Size(1050, 619);
            this.basicSettingsTabPage.TabIndex = 1;
            this.basicSettingsTabPage.Text = "Basic";
            this.basicSettingsTabPage.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.folderTextBox);
            this.groupBox5.Controls.Add(this.label1);
            this.groupBox5.Controls.Add(this.generateAllBtn);
            this.groupBox5.Controls.Add(this.folderSelectButton);
            this.groupBox5.Controls.Add(this.assemblyNameTextBox);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.generateButton);
            this.groupBox5.Controls.Add(this.label3);
            this.groupBox5.Controls.Add(this.nameSpaceTextBox);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox5.Location = new System.Drawing.Point(3, 419);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(1044, 197);
            this.groupBox5.TabIndex = 20;
            this.groupBox5.TabStop = false;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.dbConnStrLabel);
            this.groupBox4.Controls.Add(this.sequencesComboBox);
            this.groupBox4.Controls.Add(this.serverTypeComboBox);
            this.groupBox4.Controls.Add(this.connStrTextBox);
            this.groupBox4.Controls.Add(this.tablesComboBox);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.connectBtn);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox4.Location = new System.Drawing.Point(3, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(1044, 103);
            this.groupBox4.TabIndex = 19;
            this.groupBox4.TabStop = false;
            // 
            // advanceSettingsTabPage
            // 
            this.advanceSettingsTabPage.Controls.Add(this.groupBox3);
            this.advanceSettingsTabPage.Controls.Add(this.groupBox2);
            this.advanceSettingsTabPage.Controls.Add(this.groupBox1);
            this.advanceSettingsTabPage.Location = new System.Drawing.Point(4, 22);
            this.advanceSettingsTabPage.Name = "advanceSettingsTabPage";
            this.advanceSettingsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.advanceSettingsTabPage.Size = new System.Drawing.Size(1050, 619);
            this.advanceSettingsTabPage.TabIndex = 2;
            this.advanceSettingsTabPage.Text = "Preferences";
            this.advanceSettingsTabPage.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.fluentMappingOption);
            this.groupBox3.Controls.Add(this.hbmMappingOption);
            this.groupBox3.Location = new System.Drawing.Point(439, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(200, 140);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Mapping Style";
            // 
            // fluentMappingOption
            // 
            this.fluentMappingOption.AutoSize = true;
            this.fluentMappingOption.Location = new System.Drawing.Point(6, 42);
            this.fluentMappingOption.Name = "fluentMappingOption";
            this.fluentMappingOption.Size = new System.Drawing.Size(98, 17);
            this.fluentMappingOption.TabIndex = 5;
            this.fluentMappingOption.TabStop = true;
            this.fluentMappingOption.Text = "Fluent Mapping";
            this.fluentMappingOption.UseVisualStyleBackColor = true;
            // 
            // hbmMappingOption
            // 
            this.hbmMappingOption.AutoSize = true;
            this.hbmMappingOption.Location = new System.Drawing.Point(6, 19);
            this.hbmMappingOption.Name = "hbmMappingOption";
            this.hbmMappingOption.Size = new System.Drawing.Size(82, 17);
            this.hbmMappingOption.TabIndex = 4;
            this.hbmMappingOption.TabStop = true;
            this.hbmMappingOption.Text = ".hbm.xml file";
            this.hbmMappingOption.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.vbRadioButton);
            this.groupBox2.Controls.Add(this.cSharpRadioButton);
            this.groupBox2.Location = new System.Drawing.Point(223, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 140);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Language";
            // 
            // vbRadioButton
            // 
            this.vbRadioButton.AutoSize = true;
            this.vbRadioButton.Location = new System.Drawing.Point(6, 42);
            this.vbRadioButton.Name = "vbRadioButton";
            this.vbRadioButton.Size = new System.Drawing.Size(39, 17);
            this.vbRadioButton.TabIndex = 5;
            this.vbRadioButton.TabStop = true;
            this.vbRadioButton.Text = "VB";
            this.vbRadioButton.UseVisualStyleBackColor = true;
            // 
            // cSharpRadioButton
            // 
            this.cSharpRadioButton.AutoSize = true;
            this.cSharpRadioButton.Location = new System.Drawing.Point(6, 19);
            this.cSharpRadioButton.Name = "cSharpRadioButton";
            this.cSharpRadioButton.Size = new System.Drawing.Size(39, 17);
            this.cSharpRadioButton.TabIndex = 4;
            this.cSharpRadioButton.TabStop = true;
            this.cSharpRadioButton.Text = "C#";
            this.cSharpRadioButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.prefixTextBox);
            this.groupBox1.Controls.Add(this.prefixRadioButton);
            this.groupBox1.Controls.Add(this.prefixLabel);
            this.groupBox1.Controls.Add(this.camelCasedRadioButton);
            this.groupBox1.Controls.Add(this.sameAsDBRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 140);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Generated Property Name";
            // 
            // prefixTextBox
            // 
            this.prefixTextBox.Location = new System.Drawing.Point(47, 93);
            this.prefixTextBox.Name = "prefixTextBox";
            this.prefixTextBox.Size = new System.Drawing.Size(105, 20);
            this.prefixTextBox.TabIndex = 3;
            this.prefixTextBox.Text = "m_";
            // 
            // prefixRadioButton
            // 
            this.prefixRadioButton.AutoSize = true;
            this.prefixRadioButton.Location = new System.Drawing.Point(6, 65);
            this.prefixRadioButton.Name = "prefixRadioButton";
            this.prefixRadioButton.Size = new System.Drawing.Size(63, 17);
            this.prefixRadioButton.TabIndex = 2;
            this.prefixRadioButton.TabStop = true;
            this.prefixRadioButton.Text = "Prefixed";
            this.prefixRadioButton.UseVisualStyleBackColor = true;
            this.prefixRadioButton.CheckedChanged += new System.EventHandler(this.prefixCheckChanged);
            // 
            // prefixLabel
            // 
            this.prefixLabel.AutoSize = true;
            this.prefixLabel.Location = new System.Drawing.Point(6, 96);
            this.prefixLabel.Name = "prefixLabel";
            this.prefixLabel.Size = new System.Drawing.Size(42, 13);
            this.prefixLabel.TabIndex = 2;
            this.prefixLabel.Text = "Prefix : ";
            // 
            // camelCasedRadioButton
            // 
            this.camelCasedRadioButton.AutoSize = true;
            this.camelCasedRadioButton.Location = new System.Drawing.Point(6, 42);
            this.camelCasedRadioButton.Name = "camelCasedRadioButton";
            this.camelCasedRadioButton.Size = new System.Drawing.Size(83, 17);
            this.camelCasedRadioButton.TabIndex = 1;
            this.camelCasedRadioButton.TabStop = true;
            this.camelCasedRadioButton.Text = "Camelcased";
            this.camelCasedRadioButton.UseVisualStyleBackColor = true;
            // 
            // sameAsDBRadioButton
            // 
            this.sameAsDBRadioButton.AutoSize = true;
            this.sameAsDBRadioButton.Location = new System.Drawing.Point(6, 19);
            this.sameAsDBRadioButton.Name = "sameAsDBRadioButton";
            this.sameAsDBRadioButton.Size = new System.Drawing.Size(179, 17);
            this.sameAsDBRadioButton.TabIndex = 0;
            this.sameAsDBRadioButton.TabStop = true;
            this.sameAsDBRadioButton.Text = "Same as database column name";
            this.sameAsDBRadioButton.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(6, 42);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(98, 17);
            this.radioButton1.TabIndex = 5;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Fluent Mapping";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(6, 19);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(82, 17);
            this.radioButton2.TabIndex = 4;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = ".hbm.xml file";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(6, 42);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(98, 17);
            this.radioButton3.TabIndex = 5;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "Fluent Mapping";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Location = new System.Drawing.Point(6, 19);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(82, 17);
            this.radioButton4.TabIndex = 4;
            this.radioButton4.TabStop = true;
            this.radioButton4.Text = ".hbm.xml file";
            this.radioButton4.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.dbTableDetailsGridView);
            this.groupBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox6.Location = new System.Drawing.Point(3, 106);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(1044, 313);
            this.groupBox6.TabIndex = 21;
            this.groupBox6.TabStop = false;
            // 
            // App
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1058, 645);
            this.Controls.Add(this.mainTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "App";
            this.Text = "NHibernate Mapping Generator";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.dbTableDetailsGridView)).EndInit();
            this.mainTabControl.ResumeLayout(false);
            this.basicSettingsTabPage.ResumeLayout(false);
            this.basicSettingsTabPage.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.advanceSettingsTabPage.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.ResumeLayout(false);

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
        private System.Windows.Forms.ComboBox serverTypeComboBox;
        private Label label4;
        private Label label5;
        private Button generateAllBtn;
        private DataGridViewTextBoxColumn columnName;
        private DataGridViewTextBoxColumn columnDataType;
        private DataGridViewComboBoxColumn cSharpType;
        private DataGridViewCheckBoxColumn isPrimaryKey;
        private TabControl mainTabControl;
        private TabPage basicSettingsTabPage;
        private TabPage advanceSettingsTabPage;
        private GroupBox groupBox1;
        private RadioButton sameAsDBRadioButton;
        private RadioButton prefixRadioButton;
        private RadioButton camelCasedRadioButton;
        private TextBox prefixTextBox;
        private Label prefixLabel;
        private GroupBox groupBox2;
        private RadioButton vbRadioButton;
        private RadioButton cSharpRadioButton;
        private GroupBox groupBox3;
        private RadioButton fluentMappingOption;
        private RadioButton hbmMappingOption;
        private GroupBox groupBox4;
        private RadioButton radioButton1;
        private RadioButton radioButton2;
        private GroupBox groupBox5;
        private GroupBox groupBox6;
        private RadioButton radioButton3;
        private RadioButton radioButton4;
    }
}

