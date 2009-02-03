using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Drawing;
using System.Windows.Forms;

namespace NHibernateMappingGenerator
{
    public partial class App : Form
    {
        public App()
        {
            InitializeComponent();
            dbTableDetailsGridView.AutoGenerateColumns = true;
            tablesComboBox.SelectedIndexChanged += TablesSelectedIndexChanged;
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
                var conn = new OracleConnection(connStrTextBox.Text);
                conn.Open();
                using (conn)
                {
                    var dbTable = new ColumnDetails();
                    OracleCommand tableCommand = conn.CreateCommand();
                    tableCommand.CommandText = "select * from user_tab_cols where table_name = '" + selectedTableName + "'";
                    OracleDataReader oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
                    while (oracleDataReader.Read())
                    {
                        string columnName = oracleDataReader.GetString(1);
                        string dataType = oracleDataReader.GetString(2);
                        dbTable.Add(new ColumnDetail(columnName, dataType));
                    }
                    dbTableDetailsGridView.DataSource = dbTable;
                }
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

        private void PopulateTablesAndSequences()
        {
            try
            {
                var conn = new OracleConnection(connStrTextBox.Text);
                conn.Open();
                using (conn)
                {
                    var tables = new List<string>();
                    OracleCommand tableCommand = conn.CreateCommand();
                    tableCommand.CommandText = "select * from user_tables";
                    OracleDataReader oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
                    while(oracleDataReader.Read())
                    {
                        string tableName = oracleDataReader.GetString(0);
                        tables.Add(tableName);
                    }
                    tablesComboBox.Items.AddRange(tables.ToArray());
                    tablesComboBox.SelectedIndex = 0;
                    
                    var sequences = new List<string>();
                    OracleCommand seqCommand = conn.CreateCommand();
                    seqCommand.CommandText = "select * from user_sequences";
                    OracleDataReader seqReader = seqCommand.ExecuteReader(CommandBehavior.CloseConnection);
                    while(seqReader.Read())
                    {
                        string tableName = seqReader.GetString(0);
                        sequences.Add(tableName);
                    }
                    sequencesComboBox.Items.AddRange(sequences.ToArray());
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
                var generator = new MappingGenerator(folderTextBox.Text, tablesComboBox.SelectedItem.ToString(), nameSpaceTextBox.Text, assemblyNameTextBox.Text, sequencesComboBox.SelectedItem.ToString(), (ColumnDetails) dbTableDetailsGridView.DataSource);
                generator.GenerateMappingFile();
                generator.GenerateCodeFile();
                errorLabel.Text = "Generated all files successfully.";
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
            }
        }


    }
}
