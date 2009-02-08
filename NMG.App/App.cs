using System;
using System.ComponentModel;
using System.Windows.Forms;
using NMG.Core;
using NMG.Service;

namespace NHibernateMappingGenerator
{
    public partial class App : Form
    {
        public App()
        {
            InitializeComponent();            
            tablesComboBox.SelectedIndexChanged += TablesSelectedIndexChanged;
            serverTypeComboBox.SelectedIndexChanged += ServerTypeSelected;
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
        }

        private void App_Closing(object sender, CancelEventArgs e)
        {
            var applicationSettings = new ApplicationSettings(connStrTextBox.Text, (ServerType) serverTypeComboBox.SelectedItem, nameSpaceTextBox.Text, assemblyNameTextBox.Text);
            applicationSettings.Save();
        }

        private void ServerTypeSelected(object sender, EventArgs e)
        {
            bool isOracleSelected = ((ServerType)serverTypeComboBox.SelectedItem == ServerType.Oracle);
            connStrTextBox.Text = isOracleSelected ? StringConstants.ORACLE_CONN_STR_TEMPLATE : StringConstants.SQL_CONN_STR_TEMPLATE;
            sequencesComboBox.Enabled = isOracleSelected;
        }

        private void BindData()
        {
            serverTypeComboBox.DataSource = Enum.GetValues(typeof(ServerType));
            serverTypeComboBox.SelectedIndex = 0;

            columnName.DataPropertyName = "ColumnName";
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
            var selectedTableName = (string) tablesComboBox.SelectedItem;
            try
            {
                var dbController = GetDbController();
                dbTableDetailsGridView.DataSource = dbController.GetTableDetails(selectedTableName);
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
            }
        }

        private void connectBtn_Click(object sender, EventArgs e)
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

        private DBController GetDbController()
        {
            string connectionStr = connStrTextBox.Text;
            DBController dbController;
            if ((ServerType) serverTypeComboBox.SelectedItem == ServerType.Oracle)
            {
                dbController = new OracleDBController(connectionStr);
            }
            else
            {
                dbController = new SqlServerDBController(connectionStr);
            }
            return dbController;
        }

        private void PopulateTablesAndSequences()
        {
            var dbController = GetDbController();
            try
            {
                tablesComboBox.Items.AddRange(dbController.GetTables().ToArray());
                bool hasTables = tablesComboBox.Items.Count > 0;
                tablesComboBox.Enabled = hasTables;
                if (hasTables)
                {
                    tablesComboBox.SelectedIndex = 0;
                }

                sequencesComboBox.Items.AddRange(dbController.GetSequences().ToArray());
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

        private void generateButton_Click(object sender, EventArgs e)
        {
            if(tablesComboBox.SelectedItem == null || dbTableDetailsGridView.DataSource == null)
            {
                errorLabel.Text = "Please select a table above to generate the mapping files.";
            }

            try
            {
                errorLabel.Text = "Generating " + tablesComboBox.SelectedItem + " mapping file ...";
                var serverType = (ServerType) serverTypeComboBox.SelectedItem;
                string sequence = string.Empty;
                if(sequencesComboBox.SelectedItem != null)
                {
                    sequence = sequencesComboBox.SelectedItem.ToString();
                }
                var controller = new MappingController(serverType, folderTextBox.Text, tablesComboBox.SelectedItem.ToString(), nameSpaceTextBox.Text, assemblyNameTextBox.Text, sequence, (ColumnDetails)dbTableDetailsGridView.DataSource);
                controller.Generate();
                errorLabel.Text = "Generated all files successfully.";
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
            }
        }
    }
}
