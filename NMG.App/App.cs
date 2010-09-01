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
        }

        protected override void OnLoad(EventArgs e)
        {
            var applicationSettings = ApplicationSettings.Load();
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
            bool isOracleSelected = ((ServerType) serverTypeComboBox.SelectedItem == ServerType.Oracle);
            connStrTextBox.Text = isOracleSelected ? StringConstants.ORACLE_CONN_STR_TEMPLATE : StringConstants.SQL_CONN_STR_TEMPLATE;
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

        private void TablesSelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            entityNameTextBox.Text = tablesComboBox.SelectedItem.ToString();
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
            var selectedTable = (Table) tablesComboBox.SelectedItem;
            try
            {
                var metadataReader = MetadataFactory.GetReader((ServerType)serverTypeComboBox.SelectedItem, connStrTextBox.Text);
                dbTableDetailsGridView.AutoGenerateColumns = true;
                dbTableDetailsGridView.DataSource = metadataReader.GetTableDetails(selectedTable);
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
                //tablesComboBox.Items.AddRange(metadataReader.GetTables().ToArray());
                var tables = metadataReader.GetTables();
                tablesComboBox.DataSource = tables;
                var hasTables = tables.Count > 0;
                tablesComboBox.Enabled = hasTables;
                if (hasTables)
                {
                    tablesComboBox.SelectedIndex = 0;
                }

                //sequencesComboBox.Items.AddRange(metadataReader.GetSequences().ToArray());
                //bool hasSequences = sequencesComboBox.Items.Count > 0;
                //sequencesComboBox.Enabled = hasSequences;
                //if (hasSequences)
                //{
                //    sequencesComboBox.SelectedIndex = 0;
                //}
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
                //throw (ex);
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
                var table = (Table)selectedItem;
                //var columnDetails = (Column) dbTableDetailsGridView.DataSource;
                Generate(table);
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

                    foreach (var item in tablesComboBox.Items)
                    {
                        var table = (Table)item;
                        var metadataReader = MetadataFactory.GetReader(serverType, connStrTextBox.Text);
                        table.Columns = metadataReader.GetTableDetails(table);
                        Generate(table);
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

        private void Generate(Table table)
        {
            var applicationPreferences = GetApplicationPreferences(table.Name);
            var applicationController = new ApplicationController(applicationPreferences, table);
            applicationController.Generate();
        }

        private Language LanguageSelected
        {
            get { return vbRadioButton.Checked ? Language.VB : Language.CSharp; }
        }

        public bool IsFluent
        {
            get { return fluentMappingOption.Checked; }
        }

        public bool IsCastle
        {
            get { return castleMappingOption.Checked; }
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

            var applicationPreferences = new ApplicationPreferences
                                             {
                                                 ServerType = (ServerType) serverTypeComboBox.SelectedItem,
                                                 FolderPath = folderTextBox.Text,
                                                 TableName = tableName,
                                                 NameSpace = nameSpaceTextBox.Text,
                                                 AssemblyName = assemblyNameTextBox.Text,
                                                 Sequence = sequence,
                                                 Language = LanguageSelected,
                                                 FieldNamingConvention = GetFieldNamingConvention(),
                                                 FieldGenerationConvention = GetFieldGenerationConvention(),
                                                 Prefix = prefixTextBox.Text,
                                                 IsFluent = IsFluent,
                                                 IsCastle = IsCastle,
                                                 ConnectionString = connStrTextBox.Text
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