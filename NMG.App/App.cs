using System;
using System.ComponentModel;
using System.Windows.Forms;
using NMG.Core;
using NMG.Core.Domain;
using NMG.Core.Util;

namespace NHibernateMappingGenerator
{
    public partial class App : Form
    {
        public App()
        {
            InitializeComponent();
            tablesComboBox.SelectedIndexChanged += TablesSelectedIndexChanged;
            serverTypeComboBox.SelectedIndexChanged += ServerTypeSelected;
            dbTableDetailsGridView.DataError += DataError;
            BindData();
            tablesComboBox.Enabled = false;
            sequencesComboBox.Enabled = false;
            Closing += App_Closing;
            var applicationSettings = ApplicationSettings.Load();
            if (applicationSettings != null)
            {
                connStrTextBox.Text = applicationSettings.ConnectionString;
                serverTypeComboBox.SelectedItem = applicationSettings.ServerType;
                nameSpaceTextBox.Text = applicationSettings.NameSpace;
                assemblyNameTextBox.Text = applicationSettings.AssemblyName;
            }
            sameAsDBRadioButton.Checked = true;
            prefixLabel.Visible = prefixTextBox.Visible = false;
            cSharpRadioButton.Checked = true;
            hbmMappingOption.Checked = true;
        }

        private void DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            errorLabel.Text = "Error in column " + e.ColumnIndex + ". Detail : " + e.Exception.Message;
        }

        private void App_Closing(object sender, CancelEventArgs e)
        {
            var applicationSettings = new ApplicationSettings(connStrTextBox.Text, (ServerType) serverTypeComboBox.SelectedItem, nameSpaceTextBox.Text,
                                                              assemblyNameTextBox.Text);
            applicationSettings.Save();
        }

        private void ServerTypeSelected(object sender, EventArgs e)
        {
            bool isOracleSelected = ((ServerType) serverTypeComboBox.SelectedItem == ServerType.Oracle);
            connStrTextBox.Text = isOracleSelected ? StringConstants.ORACLE_CONN_STR_TEMPLATE : StringConstants.SQL_CONN_STR_TEMPLATE;
        }

        private void BindData()
        {
            serverTypeComboBox.DataSource = Enum.GetValues(typeof (ServerType));
            serverTypeComboBox.SelectedIndex = 0;

            columnName.DataPropertyName = "ColumnName";
            isPrimaryKey.DataPropertyName = "IsPrimaryKey";
            columnDataType.DataPropertyName = "DataType";
            cSharpType.DataPropertyName = "MappedType";
            cSharpType.DataSource = new DotNetTypes();
        }

        private void TablesSelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                PopulateTableDetails();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void PopulateTableDetails()
        {
            errorLabel.Text = string.Empty;
            var selectedTableName = (string) tablesComboBox.SelectedItem;
            try
            {
                var metadataReader = MetadataFactory.GetReader((ServerType)serverTypeComboBox.SelectedItem, connStrTextBox.Text);
                dbTableDetailsGridView.DataSource = metadataReader.GetTableDetails(selectedTableName);
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
                tablesComboBox.Items.Clear();
                sequencesComboBox.Items.Clear();
                PopulateTablesAndSequences();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void PopulateTablesAndSequences()
        {
            errorLabel.Text = string.Empty;
            var metadataReader = MetadataFactory.GetReader((ServerType)serverTypeComboBox.SelectedItem, connStrTextBox.Text);
            try
            {
                tablesComboBox.Items.AddRange(metadataReader.GetTables().ToArray());
                bool hasTables = tablesComboBox.Items.Count > 0;
                tablesComboBox.Enabled = hasTables;
                if (hasTables)
                {
                    tablesComboBox.SelectedIndex = 0;
                }

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
                errorLabel.Text = "Please select a table above to generate the mapping files.";
                return;
            }
            try
            {
                errorLabel.Text = "Generating " + selectedItem + " mapping file ...";
                string tableName = selectedItem.ToString();
                var columnDetails = (ColumnDetails) dbTableDetailsGridView.DataSource;
                Generate(tableName, columnDetails);
                errorLabel.Text = "Generated all files successfully.";
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
            }
        }

        private void GenerateAllClicked(object sender, EventArgs e)
        {
            errorLabel.Text = string.Empty;
            if (tablesComboBox.Items == null || tablesComboBox.Items.Count == 0)
            {
                errorLabel.Text = "Please connect to a database to populate the tables first.";
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
                        string tableName = item.ToString();
                        var metadataReader = MetadataFactory.GetReader(serverType, connStrTextBox.Text);
                        var columnDetails = metadataReader.GetTableDetails(tableName);
                        Generate(tableName, columnDetails);
                    }
                    errorLabel.Text = "Generated all files successfully.";
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

        private void Generate(string tableName, ColumnDetails columnDetails)
        {
            var applicationPreferences = GetApplicationPreferences(tableName);
            var applicationController = new ApplicationController(applicationPreferences, columnDetails);
            applicationController.Generate();
        }

        private Language LanguageSelected
        {
            get { return vbRadioButton.Checked ? Language.VB : Language.CSharp; }
        }

        public bool IsFluent
        {
            get { return fluentMappingOption.Checked ? true : false; }
        }

        private void prefixCheckChanged(object sender, EventArgs e)
        {
            prefixLabel.Visible = prefixTextBox.Visible = prefixRadioButton.Checked;
        }

        private ApplicationPreferences GetApplicationPreferences(string tableName)
        {
            var sequence = string.Empty;
            if (sequencesComboBox.SelectedItem != null)
            {
                sequence = sequencesComboBox.SelectedItem.ToString();
            }
            var serverType = (ServerType)serverTypeComboBox.SelectedItem;

            var applicationPreferences = new ApplicationPreferences
                                             {
                                                 ServerType = serverType,
                                                 FolderPath = folderTextBox.Text,
                                                 TableName = tableName,
                                                 NameSpace = nameSpaceTextBox.Text,
                                                 AssemblyName = assemblyNameTextBox.Text,
                                                 Sequence = sequence,
                                                 Language = LanguageSelected,
                                                 FieldNamingConvention = GetFieldNamingConvention(),
                                                 FieldGenerationConvention = GetFieldGenerationConvention()
                                             };

            return applicationPreferences;
        }

        private FieldGenerationConvention GetFieldGenerationConvention()
        {
            var convention = FieldGenerationConvention.Field;
            if (autoPropertyRadioBtn.Checked)
                convention = FieldGenerationConvention.AutoProperty;
            if (propertyRadioBtn.Checked)
                convention = FieldGenerationConvention.Property;
            return convention;
        }

        private FieldNamingConvention GetFieldNamingConvention()
        {
            var convention = FieldNamingConvention.SameAsDatabase;
            if (prefixRadioButton.Checked)
                convention = FieldNamingConvention.Prefixed;
            if (camelCasedRadioButton.Checked)
                convention = FieldNamingConvention.CamelCase;
            return convention;
        }
    }
}