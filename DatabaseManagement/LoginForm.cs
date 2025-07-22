using DatabaseManagement.Models;
using DatabaseManagement.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DatabaseManagement
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private static LoginForm _Instance;
        public static LoginForm Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new LoginForm();
                if (_Instance.IsDisposed)
                    _Instance = new LoginForm();
                return _Instance;
            }
        }

        public DataTable databaseList = new DataTable();
        public bool connected = false;
        private List<Connection> Connections = new List<Connection>();

        private void Form_Load(object sender, EventArgs e)
        {
            string connectionsFilePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Connections.json");
            if (File.Exists(connectionsFilePath))
            {
                try
                {
                    string x = File.ReadAllText(connectionsFilePath);
                    this.Connections = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Connection>>(x);
                    comboBoxServer.Items.Clear();
                    foreach (Connection c in this.Connections)
                    {
                        comboBoxServer.Items.Add(c.Server);
                    }
                }
                catch (Exception)
                {
                }
            }
            comboBoxServer.Text = Properties.Settings.Default.Server;
            comboBoxAuthentication.SelectedIndex = Properties.Settings.Default.Authentication;
            comboBox_Authentication_SelectedIndexChanged(sender, EventArgs.Empty);
            if (comboBoxAuthentication.SelectedIndex == 1)
                txtUsername.Text = Properties.Settings.Default.UserName;
            if (!backgroundWorkerServers.IsBusy)
                backgroundWorkerServers.RunWorkerAsync();
        }

        private void comboBox_Authentication_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxAuthentication.SelectedIndex == 0)
            {
                txtUsername.Text = Environment.MachineName + @"\" + Environment.UserName;
                txtUsername.Enabled = false;
                txtPassword.Enabled = false;
            }
            else
            {
                txtUsername.Text = "";
                txtUsername.Enabled = true;
                txtPassword.Enabled = true;
            }
        }

        bool User_Instance = false;

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!(this.ActiveControl == btnConnect || this.ActiveControl == txtPassword))
            {
                SendKeys.Send("\t");
                return;
            }
            if (comboBoxServer.Text == "")
            {
                MessageBox.Show("Enter the server name");
                comboBoxServer.Focus();
                comboBoxServer.SelectAll();
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            if (comboBoxAuthentication.SelectedIndex == 0)
            {
                if (User_Instance)
                    Program.ConnectionString = string.Format(@"Data Source={0};Initial Catalog={1};Integrated Security=True;User Instance=True;Connect Timeout=60", comboBoxServer.Text, txtInitialCatalog.Text);
                else
                    Program.ConnectionString = string.Format(@"Data Source={0};Initial Catalog={1};Integrated Security=True;Connect Timeout=60", comboBoxServer.Text, txtInitialCatalog.Text);
            }
            else if (comboBoxAuthentication.SelectedIndex == 1)
            {
                Program.ConnectionString = string.Format(@"Data Source={0};Initial Catalog={3};User ID={1};Password={2};Connect Timeout=60", comboBoxServer.Text, txtUsername.Text, txtPassword.Text, txtInitialCatalog.Text);
            }
            DataAccess.Instance = new DataAccess(Program.ConnectionString);
            try
            {
                if (Program.ShowAllDatabases)
                    databaseList = DataAccess.Instance.GetData("SELECT * FROM [sys].[databases]");
                else
                    databaseList = DataAccess.Instance.GetData("SELECT * FROM [sys].[databases] WHERE [name]=@name", "@name", txtInitialCatalog.Text);
                connected = true;
                Properties.Settings.Default.Server = comboBoxServer.Text;
                Properties.Settings.Default.Authentication = comboBoxAuthentication.SelectedIndex;
                Properties.Settings.Default.UserName = txtUsername.Text;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                databaseList = null;
                connected = false;
                MessageBox.Show("Error connecting to database" + "\n\n" + ex.Message);
                return;
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
            if (comboBoxAuthentication.SelectedIndex == 0)
            {
                Program.LoginName = Environment.MachineName + @"\" + Environment.UserName;
            }
            else if (comboBoxAuthentication.SelectedIndex == 1)
            {
                Program.LoginName = txtUsername.Text.Trim();
            }
            var existingConnection = this.Connections.Where(x => x.Server == comboBoxServer.SelectedItem.ToString()).FirstOrDefault();
            if (existingConnection != null)
            {
                existingConnection.Server = comboBoxServer.SelectedItem.ToString();
                existingConnection.Authentication = comboBoxAuthentication.SelectedItem.ToString();
                existingConnection.Username = txtUsername.Text.Trim();
                if (checkBoxRememberPass.Checked)
                {
                    existingConnection.Password = txtPassword.Text.Trim();
                }
                existingConnection.Database = txtInitialCatalog.Text.Trim();
            }
            else
            {
                Connection newConnection = new Connection();
                newConnection.Server = comboBoxServer.SelectedItem.ToString();
                newConnection.Authentication = comboBoxAuthentication.SelectedItem.ToString();
                newConnection.Username = txtUsername.Text.Trim();
                if (checkBoxRememberPass.Checked)
                {
                    newConnection.Password = txtPassword.Text.Trim();
                }
                newConnection.Database = txtInitialCatalog.Text.Trim();
                Connections.Add(newConnection);
            }
            var newJson = Newtonsoft.Json.JsonConvert.SerializeObject(this.Connections, Newtonsoft.Json.Formatting.Indented);
            string connectionsFilePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Connections.json");
            File.WriteAllText(connectionsFilePath, newJson);
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form_login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.Control && e.Shift && e.KeyCode == Keys.U)
            {
                User_Instance = !User_Instance;
                MessageBox.Show("User Instance=" + User_Instance.ToString());
            }
            if (e.Alt && e.Control && e.Shift && e.KeyCode == Keys.S)
            {
                Program.ShowAllDatabases = !Program.ShowAllDatabases;
                MessageBox.Show("Show All Databases=" + Program.ShowAllDatabases.ToString());
            }
        }

        private List<string> _DataSources;
        public List<string> DataSources
        {
            get
            {
                if (_DataSources == null)
                    _DataSources = new List<string>();
                return _DataSources;
            }
            set
            {
                _DataSources = value;
            }
        }

        private void backgroundWorkerServers_DoWork(object sender, DoWorkEventArgs e)
        {
            backgroundWorkerServers.ReportProgress(0);
            DataTable dt = System.Data.Sql.SqlDataSourceEnumerator.Instance.GetDataSources();
            if (backgroundWorkerServers.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            foreach (DataRow item in dt.Rows)
            {
                string name = item["ServerName"].ToString() + ((item["InstanceName"] == DBNull.Value || item["InstanceName"] == null || (item["InstanceName"] is string && ((string)item["InstanceName"]) == "")) ? ("") : ("\\" + item["InstanceName"].ToString()));
                this.DataSources.Add(name.Replace(Environment.MachineName, "(local)"));
            }
        }

        private void backgroundWorkerServers_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblServersRefresh.Text = "...";
        }

        private void backgroundWorkerServers_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;
            if (e.Error != null)
                MessageBox.Show(e.Error.Message);
            lblServersRefresh.Text = "";
            foreach (var item in this.DataSources)
            {
                if (comboBoxServer.Items.Contains(item) == false)
                    comboBoxServer.Items.Insert(0, item);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (backgroundWorkerServers.IsBusy)
                backgroundWorkerServers.CancelAsync();
        }

        private void comboBoxServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            var c = this.Connections.Where(x => x.Server == comboBoxServer.SelectedItem.ToString()).FirstOrDefault();
            if (c != null)
            {
                comboBoxAuthentication.SelectedItem = c.Authentication;
                txtUsername.Text = c.Username;
                txtPassword.Text = c.Password;
                txtInitialCatalog.Text = c.Database;
                checkBoxRememberPass.Checked = true;
            }
        }
    }
}
