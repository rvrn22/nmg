using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using NMG.Core;
using NMG.Core.Domain;
using NMG.Core.Reader;
using NMG.Core.Util;

namespace NHibernateMappingGenerator
{
    public partial class App : Form
    {
        private IMetadataReader metadataReader;

        public App()
        {
            InitializeComponent();
            ownersComboBox.SelectedIndexChanged += OwnersSelectedIndexChanged;
            tablesComboBox.SelectedIndexChanged += TablesSelectedIndexChanged;
            serverTypeComboBox.SelectedIndexChanged += ServerTypeSelected;
            dbTableDetailsGridView.DataError += DataError;
            BindData();
            tablesComboBox.Enabled = false;
            sequencesComboBox.Enabled = false;
            Closing += App_Closing;
        }

        private Language LanguageSelected
        {
            get { return vbRadioButton.Checked ? Language.VB : Language.CSharp; }
        }

        public bool IsFluent
        {
            get { return fluentMappingOption.Checked; }
        }

        public bool GeneratePartialClasses
        {
            get { return partialClassesCheckBox.Checked; }
        }

        public bool IsCastle
        {
            get { return castleMappingOption.Checked; }
        }

        protected override void OnLoad(EventArgs e)
        {
            ApplicationSettings applicationSettings = ApplicationSettings.Load();
            if (applicationSettings != null)
            {
                serverTypeComboBox.SelectedItem = applicationSettings.ServerType;
                connStrTextBox.Text = applicationSettings.ConnectionString;
                nameSpaceTextBox.Text = applicationSettings.NameSpace;
                assemblyNameTextBox.Text = applicationSettings.AssemblyName;
                fluentMappingOption.Checked = applicationSettings.IsFluent;
                cSharpRadioButton.Checked = applicationSettings.Language == Language.CSharp;
                autoPropertyRadioBtn.Checked = applicationSettings.IsAutoProperty;
            }
            else
            {
                autoPropertyRadioBtn.Checked = true;
                sameAsDBRadioButton.Checked = true;
                cSharpRadioButton.Checked = true;
                fluentMappingOption.Checked = true;
            }
            if (!prefixRadioButton.Checked)
            {
                prefixLabel.Visible = prefixTextBox.Visible = false;
            }
        }

        private void DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            errorLabel.Text = string.Format("Error in column {0}. Detail : {1}", e.ColumnIndex, e.Exception.Message);
        }

        private void App_Closing(object sender, CancelEventArgs e)
        {
            var applicationSettings = new ApplicationSettings
                                          {
                                              ConnectionString = connStrTextBox.Text,
                                              ServerType = (ServerType) serverTypeComboBox.SelectedItem,
                                              NameSpace = nameSpaceTextBox.Text,
                                              AssemblyName = assemblyNameTextBox.Text,
                                              Language = cSharpRadioButton.Checked ? Language.CSharp : Language.VB,
                                              IsFluent = fluentMappingOption.Checked,
                                              IsAutoProperty = autoPropertyRadioBtn.Checked
                                          };

            applicationSettings.Save();
        }

        private void ServerTypeSelected(object sender, EventArgs e)
        {
            if ((ServerType)serverTypeComboBox.SelectedItem == ServerType.Oracle)
            {
                connStrTextBox.Text =StringConstants.ORACLE_CONN_STR_TEMPLATE;
            }
            else if((ServerType) serverTypeComboBox.SelectedItem == ServerType.SqlServer){
                connStrTextBox.Text =StringConstants.SQL_CONN_STR_TEMPLATE;
            }
            else{
                connStrTextBox.Text =StringConstants.POSTGRESQL_CONN_STR_TEMPLATE;
            }
            
        }

        private void BindData()
        {
            serverTypeComboBox.DataSource = Enum.GetValues(typeof (ServerType));
            serverTypeComboBox.SelectedIndex = 0;

            columnName.DataPropertyName = "Name";
            isPrimaryKey.DataPropertyName = "IsPrimaryKey";
            columnDataType.DataPropertyName = "DataType";
            cSharpType.DataPropertyName = "MappedType";
            cSharpType.DataSource = new DotNetTypes();
        }

        private void OwnersSelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                PopulateTablesAndSequences();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void TablesSelectedIndexChanged(object sender, EventArgs e)
        {

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                entityNameTextBox.Text = tablesComboBox.SelectedItem.ToString();
                PopulateTableDetails();
            }
            catch (Exception) { }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void PopulateTableDetails()
        {
            errorLabel.Text = string.Empty;
            var selectedTable = (Table) tablesComboBox.SelectedItem;
            try
            {
                dbTableDetailsGridView.AutoGenerateColumns = true;
                dbTableDetailsGridView.DataSource = metadataReader.GetTableDetails(selectedTable,
                                                                                   ownersComboBox.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
            }
        }

        private void connectBtnClicked(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                tablesComboBox.DataSource = null;
                tablesComboBox.Items.Clear();
                sequencesComboBox.Items.Clear();

                
            }
            catch (Exception ex) { }
            finally
            {
                metadataReader = MetadataFactory.GetReader((ServerType)serverTypeComboBox.SelectedItem,
                                                              connStrTextBox.Text);
                PopulateOwners();
                PopulateTablesAndSequences();
                Cursor.Current = Cursors.Default;
            }
        }

        private void PopulateOwners()
        {
            var owners = metadataReader.GetOwners();
            if (owners == null || owners.Count == 0)
            {
                owners = new List<string> { "dbo" };
            }
            ownersComboBox.DataSource = owners;
        }

        private void PopulateTablesAndSequences()
        {
            errorLabel.Text = string.Empty;
            tablesComboBox.DataBindings.Clear();

            try
            {
                if (ownersComboBox.SelectedItem == null)
                {
                    return;
                }

                var tables = metadataReader.GetTables(ownersComboBox.SelectedItem.ToString());
                tablesComboBox.DataSource = tables;
                var hasTables = tables.Count > 0;
                tablesComboBox.Enabled = hasTables;
                if (hasTables)
                {
                    tablesComboBox.SelectedIndex = 0;
                }
                if(metadataReader.GetSequences()!=null)
                sequencesComboBox.Items.AddRange(metadataReader.GetSequences().ToArray());
                bool hasSequences = sequencesComboBox.Items.Count > 0;
                sequencesComboBox.Enabled = hasSequences;
                if (hasSequences)
                {
                    sequencesComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
            }
        }

        private void folderSelectButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.ShowDialog();
            folderTextBox.Text = folderBrowserDialog.SelectedPath;
        }

        private void GenerateClicked(object sender, EventArgs e)
        {
            errorLabel.Text = string.Empty;
            object selectedItem = tablesComboBox.SelectedItem;
            if (selectedItem == null || dbTableDetailsGridView.DataSource == null)
            {
                errorLabel.Text = @"Please select a table above to generate the mapping files.";
                return;
            }
            try
            {
                errorLabel.Text = string.Format("Generating {0} mapping file ...", selectedItem);
                var table = (Table) selectedItem;
                //var columnDetails = (Column) dbTableDetailsGridView.DataSource;
                Generate(table, false);
                errorLabel.Text = @"Generated all files successfully.";
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
            }
        }

        private void GenerateAllClicked(object sender, EventArgs e)
        {
            errorLabel.Text = string.Empty;
            if (tablesComboBox.Items.Count == 0)
            {
                errorLabel.Text = @"Please connect to a database to populate the tables first.";
                return;
            }
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    var serverType = (ServerType) serverTypeComboBox.SelectedItem;

                    foreach (object item in tablesComboBox.Items)
                    {
                        var table = (Table) item;
                        //var metadataReader = MetadataFactory.GetReader(serverType, connStrTextBox.Text);
                        table.Columns = metadataReader.GetTableDetails(table, ownersComboBox.SelectedItem.ToString());
                        Generate(table, true);
                    }
                    errorLabel.Text = @"Generated all files successfully.";
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
            }
        }

        private void Generate(Table table, bool generateAll)
        {
            ApplicationPreferences applicationPreferences = GetApplicationPreferences(table, generateAll);
            var applicationController = new ApplicationController(applicationPreferences, table);
            applicationController.Generate();
        }

        private void prefixCheckChanged(object sender, EventArgs e)
        {
            prefixLabel.Visible = prefixTextBox.Visible = prefixRadioButton.Checked;
        }

        private ApplicationPreferences GetApplicationPreferences(Table tableName, bool all)
        {
            string sequence = string.Empty;

            if (sequencesComboBox.SelectedItem != null && !all)
            {
                sequence = sequencesComboBox.SelectedItem.ToString();
            }

            var folderPath = AddSlashToFolderPath(folderTextBox.Text);
            if(!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            var applicationPreferences = new ApplicationPreferences
                                             {
                                                 ServerType = (ServerType) serverTypeComboBox.SelectedItem,
                                                 FolderPath = folderPath,
                                                 TableName = tableName.Name,
                                                 NameSpace = nameSpaceTextBox.Text,
                                                 AssemblyName = assemblyNameTextBox.Text,
                                                 Sequence = sequence,
                                                 Language = LanguageSelected,
                                                 FieldNamingConvention = GetFieldNamingConvention(),
                                                 FieldGenerationConvention = GetFieldGenerationConvention(),
                                                 Prefix = prefixTextBox.Text,
                                                 IsFluent = IsFluent,
                                                 IsCastle = IsCastle,
                                                 GeneratePartialClasses = GeneratePartialClasses,
                                                 ConnectionString = connStrTextBox.Text,                                                 
                                             };

            return applicationPreferences;
        }

        private FieldGenerationConvention GetFieldGenerationConvention()
        {
            FieldGenerationConvention convention = FieldGenerationConvention.Field;
            if (autoPropertyRadioBtn.Checked)
                convention = FieldGenerationConvention.AutoProperty;
            if (propertyRadioBtn.Checked)
                convention = FieldGenerationConvention.Property;
            return convention;
        }

        private FieldNamingConvention GetFieldNamingConvention()
        {
            FieldNamingConvention convention = FieldNamingConvention.SameAsDatabase;
            if (prefixRadioButton.Checked)
                convention = FieldNamingConvention.Prefixed;
            if (camelCasedRadioButton.Checked)
                convention = FieldNamingConvention.CamelCase;
            if (pascalCasedRadioButton.Checked)
                convention = FieldNamingConvention.PascalCase;
            return convention;
        }

        private static string AddSlashToFolderPath(string folderPath)
        {
            if (!folderPath.EndsWith("\\"))
            {
                folderPath += "\\";
            }
            return folderPath;
        }
    }
}