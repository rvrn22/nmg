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
            dbTableDetailsGridView.AutoGenerateColumns = true;
            tablesComboBox.SelectedIndexChanged += TablesSelectedIndexChanged;
            serverTypeComboBox.SelectedIndexChanged += ServerTypeSelected;
            BindData();
        }

        private void ServerTypeSelected(object sender, EventArgs e)
        {
            bool isOracleSelected = ((ServerType)serverTypeComboBox.SelectedItem == ServerType.Oracle);
            sequencesComboBox.Enabled = isOracleSelected;
        }

        private void BindData()
        {
            serverTypeComboBox.Items.Add(ServerType.Oracle);
            serverTypeComboBox.Items.Add(ServerType.SqlServer2005);
            serverTypeComboBox.Items.Add(ServerType.SqlServer2008);
            serverTypeComboBox.SelectedIndex = 0;
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
                var dbController = new OracleDBController(connStrTextBox.Text);
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
            if((ServerType)serverTypeComboBox.SelectedItem != ServerType.Oracle)
            {
                MessageBox.Show("Only Oracle Server is currently supported.", "DB Support Error", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                return;
            }
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

        private void PopulateTablesAndSequences()
        {
            try
            {
                var dbController = new OracleDBController(connStrTextBox.Text);
                tablesComboBox.Items.AddRange(dbController.GetTables().ToArray());
                tablesComboBox.SelectedIndex = 0;


                sequencesComboBox.Items.AddRange(dbController.GetSequences().ToArray());
                sequencesComboBox.SelectedIndex = 0;
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
                string folderPath = folderTextBox.Text;
                if(!folderPath.EndsWith("\\"))
                {
                    folderPath += "\\";
                }
                var generator = new OracleMappingGenerator(folderPath, tablesComboBox.SelectedItem.ToString(), nameSpaceTextBox.Text, assemblyNameTextBox.Text, sequencesComboBox.SelectedItem.ToString(), (ColumnDetails) dbTableDetailsGridView.DataSource);
                var codeGenerator = new CodeGenerator(folderPath, tablesComboBox.SelectedItem.ToString(), nameSpaceTextBox.Text, assemblyNameTextBox.Text, sequencesComboBox.SelectedItem.ToString(), (ColumnDetails) dbTableDetailsGridView.DataSource);
                generator.Generate();
                codeGenerator.Generate();
                errorLabel.Text = "Generated all files successfully.";
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
            }
        }


    }
}
