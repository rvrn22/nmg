using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using NMG.Core;
using NMG.Core.Domain;
using NMG.Core.Reader;
using NMG.Core.Util;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NHibernateMappingGenerator
{
    public partial class App : Form
    {
        private IMetadataReader metadataReader;
		private readonly BackgroundWorker worker;
		
        public App()
        {
            InitializeComponent();
            ownersComboBox.SelectedIndexChanged += OwnersSelectedIndexChanged;
//            tablesComboBox.SelectedIndexChanged += TablesSelectedIndexChanged;
            tablesListBox.SelectedIndexChanged += TablesListSelectedIndexChanged;
            serverTypeComboBox.SelectedIndexChanged += ServerTypeSelected;
            dbTableDetailsGridView.DataError += DataError;
            BindData();
//            tablesComboBox.Enabled = false;
            sequencesComboBox.Enabled = false;
            Closing += App_Closing;
			worker = new BackgroundWorker {WorkerSupportsCancellation = true};
        }

        private Language LanguageSelected
        {
            get { return vbRadioButton.Checked ? Language.VB : Language.CSharp; }
        }

        public bool IsNhFluent
        {
            get { return nhFluentMappingStyle.Checked; }
        }

        public bool IsFluent
        {
            get { return fluentMappingOption.Checked; }
        }

        public bool GeneratePartialClasses
        {
            get { return partialClassesCheckBox.Checked; }
        }

        public bool GenerateWCFDataContract
        {
            get { return wcfDataContractCheckBox.Checked; }
        }

        public bool IsCastle
        {
            get { return castleMappingOption.Checked; }
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
				folderTextBox.Text = applicationSettings.FolderPath;
            	textBoxInheritence.Text = applicationSettings.InheritenceAndInterfaces;
            	comboBoxForeignCollection.Text = applicationSettings.ForeignEntityCollectionType;

            	fluentMappingOption.Checked = applicationSettings.IsFluent;
            	nhFluentMappingStyle.Checked = applicationSettings.IsNhFluent;
            	castleMappingOption.Checked = applicationSettings.IsCastle;

				prefixRadioButton.Checked = !string.IsNullOrEmpty(applicationSettings.Prefix);
				prefixTextBox.Text = applicationSettings.Prefix;
				camelCasedRadioButton.Checked = (applicationSettings.FieldNamingConvention == FieldNamingConvention.CamelCase);
				pascalCasedRadioButton.Checked = (applicationSettings.FieldNamingConvention == FieldNamingConvention.PascalCase);
				sameAsDBRadioButton.Checked = (applicationSettings.FieldNamingConvention == FieldNamingConvention.SameAsDatabase);

				sameAsDBRadioButton.Checked = (!prefixRadioButton.Checked && !pascalCasedRadioButton.Checked && !camelCasedRadioButton.Checked);
			}
            else
            {
                autoPropertyRadioBtn.Checked = true;
                sameAsDBRadioButton.Checked = true;
                cSharpRadioButton.Checked = true;
                fluentMappingOption.Checked = true;
            	comboBoxForeignCollection.Text = "IList";
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
                                              IsAutoProperty = autoPropertyRadioBtn.Checked,
											  FolderPath = folderTextBox.Text,
											  InheritenceAndInterfaces = textBoxInheritence.Text,
											  ForeignEntityCollectionType = comboBoxForeignCollection.Text,
											  FieldNamingConvention = GetFieldNamingConvention(),
                                              Prefix = prefixTextBox.Text,
											  IsNhFluent = IsNhFluent,
											  IsCastle = IsCastle
                                          };

            applicationSettings.Save();
        }

        private void ServerTypeSelected(object sender, EventArgs e)
        {
            pOracleOnlyOptions.Hide();

            switch ((ServerType)serverTypeComboBox.SelectedItem)
            {
                case ServerType.Oracle:
                    connStrTextBox.Text = StringConstants.ORACLE_CONN_STR_TEMPLATE;
                    pOracleOnlyOptions.Show();
                    break;
                case ServerType.SqlServer:
                    connStrTextBox.Text = StringConstants.SQL_CONN_STR_TEMPLATE;
                    break;
                case ServerType.MySQL:
                    connStrTextBox.Text = StringConstants.MYSQL_CONN_STR_TEMPLATE;
                    break;
                default:
                    connStrTextBox.Text = StringConstants.POSTGRESQL_CONN_STR_TEMPLATE;
                    break;
            }
        }

        private void BindData()
        {
            serverTypeComboBox.DataSource = Enum.GetValues(typeof (ServerType));
            serverTypeComboBox.SelectedIndex = 0;

            columnName.DataPropertyName = "Name";
            isPrimaryKey.DataPropertyName = "IsPrimaryKey";
            isForeignKey.DataPropertyName = "IsForeignKey";
            isUniqueKey.DataPropertyName = "IsUnique";
            isNullable.DataPropertyName = "IsNullable";
            columnDataType.DataPropertyName = "DataType";
            cSharpType.DataPropertyName = "MappedDataType";
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

        private void TablesListSelectedIndexChanged(object sender, EventArgs e)
        {
			errorLabel.Text = string.Empty;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (tablesListBox.SelectedItems.Count == 1)
                {
                    entityNameTextBox.Text = tablesListBox.SelectedItem.ToString();
                    PopulateTableDetails();
                }
            }
            catch (Exception ex) 
			{
				errorLabel.Text = ex.Message;
			}
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void PopulateTableDetails()
        {
            errorLabel.Text = string.Empty;
            var selectedTable = (Table) tablesListBox.SelectedItem;
            try
            {
                dbTableDetailsGridView.AutoGenerateColumns = true;
                dbTableDetailsGridView.DataSource = metadataReader.GetTableDetails(selectedTable, ownersComboBox.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
            }
        }

        private void connectBtnClicked(object sender, EventArgs e)
        {
			errorLabel.Text = string.Empty;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                tablesListBox.DataSource = null;
                tablesListBox.Items.Clear();
//                tablesComboBox.DataSource = null;
//                tablesComboBox.Items.Clear();
                sequencesComboBox.Items.Clear();

                metadataReader = MetadataFactory.GetReader((ServerType)serverTypeComboBox.SelectedItem,
                                              connStrTextBox.Text);
                PopulateOwners();
                PopulateTablesAndSequences();
                Cursor.Current = Cursors.Default;
            }
            catch (Exception ex)
            {
                errorLabel.Text = ex.Message;
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
//            tablesComboBox.DataBindings.Clear();
            tablesListBox.DataBindings.Clear();

            try
            {
                if (ownersComboBox.SelectedItem == null)
                {
                    return;
                }

                var tables = metadataReader.GetTables(ownersComboBox.SelectedItem.ToString());
//                tablesComboBox.DataSource = tables;
                tablesListBox.DataSource = tables;
                var hasTables = tables.Count > 0;
//                tablesComboBox.Enabled = hasTables;
                tablesListBox.Enabled = hasTables;
                if (hasTables)
                {
//                    tablesComboBox.SelectedIndex = 0;
                    tablesListBox.SelectedIndex = 0;
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
            var selectedItems = tablesListBox.SelectedItems;
            if (selectedItems.Count == 0)
            {
                errorLabel.Text = @"Please select table(s) above to generate the mapping files.";
                return;
            }
            try
            {
                foreach (var selectedItem in selectedItems)
                {
                    errorLabel.Text = string.Format("Generating {0} mapping file ...", selectedItem);
                    var table = (Table)selectedItem;
                    table.EntityName = entityNameTextBox.Text;
                    Generate(table, false);                
                }
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
			var items = tablesListBox.Items;
            if (items.Count == 0)
            {
                errorLabel.Text = @"Please connect to a database to populate the tables first.";
                return;
            }
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    progressBar.Maximum = 100;
                    progressBar.Value = 10;
                    worker.DoWork += DoWork;
                    worker.RunWorkerCompleted += WorkerCompleted;
                    worker.RunWorkerAsync();
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
		
        private void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 100;
            errorLabel.Text = @"Generated all files successfully.";
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            var items = tablesListBox.Items;
            Parallel.ForEach(items.Cast<Table>(), (table, loopState) =>
            {
                if(worker != null && worker.CancellationPending && !loopState.IsStopped)
                {
                    loopState.Stop();
                    loopState.Break();
                    Thread.Sleep(1000);
                }
                //table.Columns = metadataReader.GetTableDetails(table, ownersComboBox.SelectedItem.ToString());
                string name = "";
                if (ownersComboBox.InvokeRequired)
                {
                    ownersComboBox.Invoke(new MethodInvoker(delegate { name = ownersComboBox.SelectedItem.ToString(); }));
                }
                else
                {
                    name = ownersComboBox.SelectedItem.ToString();
                }
                table.Columns = metadataReader.GetTableDetails(table, name);
                Generate(table, true);
            });
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
            object sequenceName = null;
            if (sequencesComboBox.InvokeRequired)
            {
                sequencesComboBox.Invoke(new MethodInvoker(delegate { sequenceName = sequencesComboBox.SelectedItem; }));
            }
            else
            {
                sequenceName = sequencesComboBox.SelectedItem;
            }
            if (sequenceName != null && !all)
            {
                sequence = sequenceName.ToString();
            }

            var folderPath = AddSlashToFolderPath(folderTextBox.Text);
            if(!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            object serverType = null;
            if (serverTypeComboBox.InvokeRequired)
            {
                serverTypeComboBox.Invoke(new MethodInvoker(delegate { serverType = serverTypeComboBox.SelectedItem; }));
            }
            else
            {
                serverType = serverTypeComboBox.SelectedItem;
            }
            var applicationPreferences = new ApplicationPreferences
                                             {
                                                 ServerType = (ServerType)serverType,
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
                                                 IsNhFluent = IsNhFluent,
                                                 IsCastle = IsCastle,
                                                 GeneratePartialClasses = GeneratePartialClasses,
                                                 GenerateWCFDataContract = GenerateWCFDataContract,
                                                 ConnectionString = connStrTextBox.Text,
												 ForeignEntityCollectionType = comboBoxForeignCollection.Text,
												 InheritenceAndInterfaces = textBoxInheritence.Text
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
		
        private void cancelButton_Click(object sender, EventArgs e)
        {
            if(worker != null)
            {
                worker.CancelAsync();
            }
        }

		private void advanceSettingsTabPage_Click(object sender, EventArgs e) {

		}

		
    }
}