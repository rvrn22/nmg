using System;
using System.Drawing;
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
            oracleType.DataPropertyName = "MappedType";
            oracleType.DataSource = new DotNetTypes();
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
                errorLabel.ForeColor = Color.Tomato;
            }
        }

        private void connectBtn_Click(object sender, EventArgs e)
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
            DBController dbController = GetDbController();
            try
            {
                tablesComboBox.Items.AddRange(dbController.GetTables().ToArray());
                if (tablesComboBox.Items.Count > 0)
                {
                    tablesComboBox.SelectedIndex = 0;
                }

                sequencesComboBox.Items.AddRange(dbController.GetSequences().ToArray());
                if (sequencesComboBox.Items.Count > 0)
                {
                    sequencesComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
                errorLabel.ForeColor = Color.Tomato;
            }
        }

        private void folderSelectButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.ShowDialog();
            folderTextBox.Text = folderBrowserDialog.SelectedPath;
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
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
