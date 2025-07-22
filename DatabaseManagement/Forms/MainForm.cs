﻿using DatabaseManagement.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DatabaseManagement
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private static MainForm _Instance;
        public static MainForm Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new MainForm();
                if (_Instance.IsDisposed)
                    _Instance = new MainForm();
                return _Instance;
            }
        }

        string _UserName = "";
        string UserName
        {
            set
            {
                _UserName = value;
            }
            get
            {
                return _UserName;
            }
        }
        string _ServerName = "";
        string ServerName
        {
            set
            {
                _ServerName = value;
            }
            get
            {
                return _ServerName;
            }
        }
        bool _Connected;
        bool Connected
        {
            set
            {
                _Connected = value;
                if (value == true)
                {
                    lblTitle.ForeColor = Color.Green;
                    lblTitle.Text = "Connection not established.";
                    lblTitle.Text = ServerName + "  -  " + UserName;
                }
                else
                {
                    mainListView.Items.Clear();
                    DatabaseList = new DataTable();
                    lblTitle.ForeColor = Color.Red;
                    lblTitle.Text = "Connection not established.";
                }
            }
            get
            {
                return _Connected;
            }
        }
        DataTable _DatabaseList = new DataTable();
        internal DataTable DatabaseList
        {
            set
            {
                _DatabaseList = value;
                mainListView.Items.Clear();
                lblStatus.Text = "";
                if (value != null)
                {
                    foreach (DataRow r in value.Rows)
                    {
                        mainListView.Items.Add(r["name"].ToString(), r["name"].ToString(), 0);
                    }
                    lblStatus.Text = "Number of databases = " + DatabaseList.Rows.Count.ToString();
                }
            }
            get
            {
                return _DatabaseList;
            }
        }
        private void MainForm_Shown(object sender, EventArgs e)
        {
            Connect();
        }

        private void MenuConnect_Click(object sender, EventArgs e)
        {
            Connect();
        }

        private void Connect()
        {
            if (LoginForm.Instance.ShowDialog() == DialogResult.OK)
            {
                DatabaseList = LoginForm.Instance.databaseList;
                UserName = LoginForm.Instance.txtUsername.Text;
                ServerName = LoginForm.Instance.comboBoxServer.Text;
                Connected = LoginForm.Instance.connected;
            }
        }

        private void tsb_Refresh_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetToolstripLocation();
            SetDateTime();
            if (!(Properties.Settings.Default.Main_Size.Height == 0 && Properties.Settings.Default.Main_Size.Width == 0))
                this.Size = Properties.Settings.Default.Main_Size;
            if (!(Properties.Settings.Default.Main_Location.X == 0 && Properties.Settings.Default.Main_Location.Y == 0))
                this.Location = Properties.Settings.Default.Main_Location;
            if (Properties.Settings.Default.View == 1)
                menuViewList_Click(sender, EventArgs.Empty);
        }

        private void SetDateTime()
        {
            lblDate.Text = persian_Calendar.GetDayOfWeek(DateTime.Now) + " " + persian_Calendar.GetDateWithMonthName(DateTime.Now, " ");
            lblTime.Text = DateTime.Now.ToLongTimeString();
        }

        private void toolStripButton_dis_Click(object sender, EventArgs e)
        {
            if (!Connected)
            {
                MessageBox.Show("Connection not established.");
                return;
            }
            DatabaseList = null;
            Connected = false;
            string ConnectionString = DataAccess.Instance.Connection.ConnectionString;
            DataAccess.Instance.Dispose();
            DataAccess.Instance = new DataAccess(ConnectionString);
        }

        private void tsb_attach_Click(object sender, EventArgs e)
        {
            if (!Connected)
            {
                MessageBox.Show("Connection not established.");
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            AttachForm f = new AttachForm();
            if (f.ShowDialog() == DialogResult.OK)
                RefreshData();
            this.Cursor = Cursors.Default;
        }

        private void RefreshData()
        {
            if (!Connected)
            {
                MessageBox.Show("Connection not established.");
                return;
            }
            try
            {
                this.Cursor = Cursors.WaitCursor;
                if (Program.ShowAllDatabases)
                    DatabaseList = DataAccess.Instance.GetData("SELECT * FROM sys.databases");
                else
                    DatabaseList = DataAccess.Instance.GetData("SELECT * FROM [sys].[databases] WHERE [name]=@name", "@name", DataAccess.Instance.Connection.Database);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void tsb_detach_Click(object sender, EventArgs e)
        {
            if (!Connected)
            {
                MessageBox.Show("Connection not established.");
                return;
            }
            if (mainListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("No database selected.");
                return;
            }
            else if (mainListView.SelectedItems.Count == 1)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    string cmd = string.Format(@"USE [master]; EXEC master.dbo.sp_detach_db @dbname = N'{0}', @keepfulltextindexfile=N'true';", mainListView.SelectedItems[0].Name);
                    DataAccess.Instance.Execute(cmd);
                    RefreshData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:" + "\n\n" + ex.Message);
                    return;
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
                Program.LoginName = "";
            }
        }

        private void tsb_backup_Click(object sender, EventArgs e)
        {
            if (!Connected)
            {
                MessageBox.Show("Connection not established.");
                return;
            }
            if (mainListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("No database selected.");
                return;
            }
            else if (mainListView.SelectedItems.Count == 1)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    saveFileDialog.Filter = "Backup File (*.bak)|*.bak|All Files|*.*";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string s = string.Format(@"BACKUP DATABASE [{0}] TO  DISK = N'{1}' WITH NOFORMAT, INIT,  NAME = N'{0} Full Backup', SKIP, STATS = 10", mainListView.SelectedItems[0].Name, saveFileDialog.FileName);
                        DataAccess.Instance.Execute(s);
                        RefreshData();
                        lblStatusStrip.Text = "Backup completed successfully.";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error backing up database" + "\n\n" + ex.Message);
                    return;
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void tsb_restore_Click(object sender, EventArgs e)
        {
            if (!Connected)
            {
                MessageBox.Show("Connection not established.");
                return;
            }
            if (mainListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("No database selected.");
                return;
            }
            else if (mainListView.SelectedItems.Count == 1)
            {
                openFileDialog.Filter = "Backup File (*.bak)|*.bak|All Files|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;

                        System.Data.DataTable backupFiles = DataAccess.Instance.GetData(string.Format("RESTORE FILELISTONLY FROM DISK = N'{0}'", openFileDialog.FileName));
                        string backupDataFile = backupFiles.Rows[0]["LogicalName"].ToString();
                        string backupLogFile = backupFiles.Rows[1]["LogicalName"].ToString();

                        System.Data.SqlClient.SqlConnectionStringBuilder csb = new System.Data.SqlClient.SqlConnectionStringBuilder(Program.ConnectionString);
                        csb.InitialCatalog = mainListView.SelectedItems[0].Name;
                        string script = "SELECT * FROM [sys].[database_files]";
                        DataAccess da = new DataAccess(csb.ConnectionString);
                        System.Data.DataTable databaseFiles = da.GetData(script);
                        string physicalDataFile = databaseFiles.Rows[0]["physical_name"].ToString();
                        string physicalLogFile = databaseFiles.Rows[1]["physical_name"].ToString();
                        da.Dispose();
                        DataAccess.Instance.Command.CommandTimeout = 5 * 60;
                        DataAccess.Instance.Execute(string.Format(@"ALTER DATABASE [{0}] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE", mainListView.SelectedItems[0].Name));
                        DataAccess.Instance.Execute(string.Format(@"RESTORE DATABASE [{0}] FROM  DISK = N'{1}' WITH  FILE = 1,  MOVE N'{2}' TO N'{3}',  MOVE N'{4}' TO N'{5}',  NOUNLOAD,  REPLACE, STATS = 10", mainListView.SelectedItems[0].Name, openFileDialog.FileName, backupDataFile, physicalDataFile, backupLogFile, physicalLogFile));
                        DataAccess.Instance.Execute(string.Format(@"ALTER DATABASE [{0}] SET MULTI_USER", mainListView.SelectedItems[0].Name));
                        RefreshData();
                        lblStatusStrip.Text = "Restore completed successfully.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error:" + "\n\n" + ex.Message);
                        return;
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (mainListView.SelectedItems.Count == 0)
            {
                menu_delete.Visible = false;
                menu_detach.Visible = false;
                menu_backup.Visible = false;
                menu_restore.Visible = false;
                menuDatabaseProperty.Visible = false;
                menu_new.Visible = true;
                menu_attach.Visible = true;
                menu_refresh.Visible = true;
            }
            else
            {
                menu_detach.Visible = true;
                menu_backup.Visible = true;
                menu_delete.Visible = true;
                menu_restore.Visible = true;
                menuDatabaseProperty.Visible = true;
                menu_new.Visible = false;
                menu_attach.Visible = false;
                menu_refresh.Visible = false;
            }
        }

        private void menu_attach_Click(object sender, EventArgs e)
        {
            tsb_attach_Click(sender, e);
        }

        private void menu_detach_Click(object sender, EventArgs e)
        {
            tsb_detach_Click(sender, e);
        }

        private void menu_backup_Click(object sender, EventArgs e)
        {
            tsb_backup_Click(sender, e);
        }

        private void menu_restore_Click(object sender, EventArgs e)
        {
            tsb_restore_Click(sender, e);
        }

        private void menu_refresh_Click(object sender, EventArgs e)
        {
            tsb_Refresh_Click(sender, e);
        }

        private void Form_main_Resize(object sender, EventArgs e)
        {
            SetToolstripLocation();
        }

        private void SetToolstripLocation()
        {
            toolStripMain.Left = this.Width - toolStripMain.Width;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SetDateTime();
        }

        private void menuViewIcon_Click(object sender, EventArgs e)
        {
            mainListView.View = View.LargeIcon;
            menuViewIcon.Checked = true;
            menuViewList.Checked = false;
        }

        private void menuViewList_Click(object sender, EventArgs e)
        {
            mainListView.View = View.List;
            menuViewIcon.Checked = false;
            menuViewList.Checked = true;
        }

        private void tsbNew_Click(object sender, EventArgs e)
        {
            NewDatabaseForm f = new NewDatabaseForm();
            if (f.ShowDialog() == DialogResult.OK)
                RefreshData();
        }

        private void tsbDelete_Click(object sender, EventArgs e)
        {
            if (!Connected)
            {
                MessageBox.Show("Connection not established.");
                return;
            }
            if (mainListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("No database selected.");
                return;
            }
            if (MessageBox.Show("Delete the selected database?", "Delete database", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign) != DialogResult.Yes)
                return;
            else if (mainListView.SelectedItems.Count == 1)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    string s = string.Format(@"EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'{0}' ;ALTER DATABASE [{0}] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE; USE [master]; DROP DATABASE [{0}]", mainListView.SelectedItems[0].Name);
                    DataAccess.Instance.Execute(s);
                    RefreshData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting database" + "\n\n" + ex.Message);
                    return;
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void menuNew_Click(object sender, EventArgs e)
        {
            tsbNew_Click(sender, e);
        }

        private void menuDelete_Click(object sender, EventArgs e)
        {
            tsbDelete_Click(sender, e);
        }

        private void tsbAbout_Click(object sender, EventArgs e)
        {
            AboutBox f = new AboutBox();
            f.ShowDialog();
        }

        private void menuCloseConnections_Click(object sender, EventArgs e)
        {
            if (!Connected)
            {
                MessageBox.Show("Connection not established.");
                return;
            }
            if (mainListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("No database selected.");
                return;
            }
            else if (mainListView.SelectedItems.Count == 1)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    DataAccess.Instance.Execute(string.Format("ALTER DATABASE [{0}] SET  SINGLE_USER", mainListView.SelectedItems[0].Name));
                    DataAccess.Instance.Execute(string.Format("ALTER DATABASE [{0}] SET  MULTI_USER", mainListView.SelectedItems[0].Name));
                    RefreshData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error disconnecting database connections" + "\n\n" + ex.Message);
                    return;
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.View = menuViewIcon.Checked ? 0 : 1;
            if (this.WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.Main_Size = this.Size;
                Properties.Settings.Default.Main_Location = this.Location;
                Properties.Settings.Default.Save();
            }
        }

        private void tsbExecute_Click(object sender, EventArgs e)
        {
            if (!Connected)
            {
                MessageBox.Show("Connection not established.");
                return;
            }
            QueryForm f;
            if (mainListView.SelectedItems.Count == 0)
                f = new QueryForm();
            else
                f = new QueryForm(mainListView.SelectedItems[0].Name);
            f.ShowDialog();
            RefreshData();
        }

        private void toolStripMenuItemDatabaseProperty_Click(object sender, EventArgs e)
        {
            if (!Connected)
            {
                MessageBox.Show("Connection not established.");
                return;
            }
            if (mainListView.SelectedItems.Count != 0)
            {
                DatabasePropertiesForm f = new DatabasePropertiesForm(mainListView.SelectedItems[0].Name);
                f.ShowDialog(this);
            }
        }

        private void mainListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            toolStripMenuItemDatabaseProperty_Click(sender, EventArgs.Empty);
        }

        private void btnInstallUpdate_Click(object sender, EventArgs e)
        {
            if (!Connected)
            {
                MessageBox.Show("Connection not established.");
                return;
            }
            if (mainListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("No database selected.");
                return;
            }
            if (openUpdateFilesDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            System.Data.SqlClient.SqlConnectionStringBuilder csb = new System.Data.SqlClient.SqlConnectionStringBuilder(DatabaseManagement.Services.DataAccess.ConnectionString);
            csb.InitialCatalog = mainListView.SelectedItems[0].Name;
            ZeroAndOne.Sql.UI.InstallLocalUpdates f = new ZeroAndOne.Sql.UI.InstallLocalUpdates(csb.ConnectionString, openUpdateFilesDialog.FileNames, "sql");
            f.CheckForUpdate();
        }
    }
}
