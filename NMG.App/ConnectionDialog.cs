using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NMG.Core.Domain;
using NMG.Core.Util;

namespace NHibernateMappingGenerator
{
    public partial class ConnectionDialog : Form
    {
        private Connection _connection;
        public Connection Connection
        {
            get { return _connection; }
            set { _connection = value; BindData(); }
        }

        public ConnectionDialog()
        {
            InitializeComponent();

            Load += OnConnectionDialogLoad;

            PopulateServerTypes();

            serverTypeComboBox.SelectedIndexChanged += OnServerTypeSelectedIndexChanged;
        }

        private void OnConnectionDialogLoad(object sender, EventArgs e)
        {
            // If no connection has been passed in create a new one
            if (Connection == null)
            {
                Connection = CreateNewConnection();
            }
        }

        private void OnDeleteButtonClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
        }

        private void OnAddButtonClick(object sender, EventArgs e)
        {
            Connection = CreateNewConnection();
        }

        private void OnServerTypeSelectedIndexChanged(object sender, EventArgs e)
        {
            if (serverTypeComboBox.SelectedIndex == -1)
            {
                // Nothing selected
                return;
            }

            // Set a default connection string if user changes server type.
            var serverType = (ServerType)serverTypeComboBox.SelectedItem;
            Connection.Name = nameTextBox.Text;
            Connection.Type = serverType;
            Connection.ConnectionString = GetDefaultConnectionStringForServerType(serverType);
            BindData();
        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            CaptureConnection();
        }

        private Connection CreateNewConnection(ServerType serverType = ServerType.SqlServer)
        {
            // Default connection settings.
            var connectionString = GetDefaultConnectionStringForServerType(serverType);

            return new Connection
                       {
                           Id = Guid.NewGuid(),
                           Name = "New Connection",
                           ConnectionString = connectionString,
                           Type = serverType
                       };
        }

        private string GetDefaultConnectionStringForServerType(ServerType serverType)
        {
            switch (serverType)
            {
                case ServerType.Oracle:
                    return StringConstants.ORACLE_CONN_STR_TEMPLATE;
                case ServerType.SqlServer:
                    return StringConstants.SQL_CONN_STR_TEMPLATE;
                case ServerType.MySQL:
                    return StringConstants.MYSQL_CONN_STR_TEMPLATE;
                case ServerType.SQLite:
                    return StringConstants.SQLITE_CONN_STR_TEMPLATE;
                case ServerType.Sybase:
                    return StringConstants.SYBASE_CONN_STR_TEMPLATE;
                default:
                    return StringConstants.POSTGRESQL_CONN_STR_TEMPLATE;
            }
        }

        private void BindData()
        {
            serverTypeComboBox.SelectedIndexChanged -= OnServerTypeSelectedIndexChanged;

            nameTextBox.Text = Connection.Name;
            serverTypeComboBox.SelectedItem = Connection.Type;
            connectionStringTextBox.Text = Connection.ConnectionString;

            serverTypeComboBox.SelectedIndexChanged += OnServerTypeSelectedIndexChanged;
        }

        private void CaptureConnection()
        {
            Connection.Name = nameTextBox.Text;
            Connection.Type = (ServerType)serverTypeComboBox.SelectedItem;
            Connection.ConnectionString = connectionStringTextBox.Text;
        }

        private void PopulateServerTypes()
        {
            serverTypeComboBox.DataSource = Enum.GetValues(typeof(ServerType));
            serverTypeComboBox.SelectedIndex = 0;
        }

    }
}
