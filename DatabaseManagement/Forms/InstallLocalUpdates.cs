using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using DatabaseManagement.Helpers;

namespace DatabaseManagement.Forms
{
    public partial class InstallLocalUpdates : Form
    {
        public InstallLocalUpdates(/*Guid currentApplication, /*Version currentProgramVersion,*/ string connectionString, string[] updateFiles, string updateFilesExtension)
        {
            InitializeComponent();
            //this.CurrentApplication = currentApplication;
            //this.CurrentProgramVersion = currentProgramVersion;
            this.ConnectionString = connectionString;
            this.DataAccess = new DatabaseManagement.Services.DataAccess(connectionString);
            this.UpdateFilesWithPath = updateFiles;
            this.UpdateFilesExtension = updateFilesExtension;
        }

        private string _UpdateFilesExtension;
        /// <summary>
        /// بدون نقطه
        /// </summary>
        public string UpdateFilesExtension
        {
            get
            {
                return _UpdateFilesExtension;
            }
            set
            {
                if (value.StartsWith("."))
                    _UpdateFilesExtension = value.Substring(1);
                else
                    _UpdateFilesExtension = value;
            }
        }

        private string _ConnectionString;
        public string ConnectionString
        {
            get
            {
                return _ConnectionString;
            }
            set
            {
                _ConnectionString = value;
            }
        }

        private string[] _UpdateFilesWithPath;
        public string[] UpdateFilesWithPath
        {
            get
            {
                return _UpdateFilesWithPath;
            }
            set
            {
                _UpdateFilesWithPath = value;
            }
        }

        //private Guid _CurrentApplication;
        //public Guid CurrentApplication
        //{
        //    get
        //    {
        //        return _CurrentApplication;
        //    }
        //    set
        //    {
        //        _CurrentApplication = value;
        //    }
        //}

        //Version _CurrentProgramVersion;
        //public Version CurrentProgramVersion
        //{
        //    get
        //    {
        //        return _CurrentProgramVersion;
        //    }
        //    set
        //    {
        //        _CurrentProgramVersion = value;
        //    }
        //}

        DatabaseManagement.Services.DataAccess _DataAccess;
        public DatabaseManagement.Services.DataAccess DataAccess
        {
            get
            {
                return _DataAccess;
            }
            set
            {
                _DataAccess = value;
            }
        }

        public virtual void InitDatabaseUpdate()
        {
            string query = "SET ANSI_NULLS ON";
            query += Environment.NewLine + "GO" + Environment.NewLine;
            query += Environment.NewLine + "SET QUOTED_IDENTIFIER ON" + Environment.NewLine;
            query += Environment.NewLine + "GO" + Environment.NewLine;
            query += Environment.NewLine + "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DatabaseUpdate]') AND type in (N'U'))" + Environment.NewLine;
            query += Environment.NewLine + "BEGIN" + Environment.NewLine;
            query += Environment.NewLine + "CREATE TABLE [dbo].[DatabaseUpdate](" + Environment.NewLine;
            query += Environment.NewLine + "	[ID] [int] IDENTITY(1,1) NOT NULL," + Environment.NewLine;
            query += Environment.NewLine + "	[InstalledUpdate] [nvarchar](50) NOT NULL," + Environment.NewLine;
            query += Environment.NewLine + " CONSTRAINT [PK_DatabaseUpdate] PRIMARY KEY CLUSTERED " + Environment.NewLine;
            query += Environment.NewLine + "(" + Environment.NewLine;
            query += Environment.NewLine + "	[ID] ASC" + Environment.NewLine;
            query += Environment.NewLine + ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]" + Environment.NewLine;
            query += Environment.NewLine + ") ON [PRIMARY]" + Environment.NewLine;
            query += Environment.NewLine + "END" + Environment.NewLine;
            query += Environment.NewLine + "GO" + Environment.NewLine;
            string error;
            if (!SqlHelper.RunQueryWithRollback(query, this.ConnectionString, out error))
                throw new Exception(error);
        }

        public virtual string GetDatabaseUpdate(string installedUpdate)
        {
            DataTable dt = null;
            string query = "SELECT * FROM [DatabaseUpdate] WHERE InstalledUpdate = @InstalledUpdate";
            try
            {
                dt = this.DataAccess.GetData(query, "@InstalledUpdate", installedUpdate);
            }
            catch
            {
                InitDatabaseUpdate();
                dt = this.DataAccess.GetData(query, "@InstalledUpdate", installedUpdate);
            }
            if (dt != null)
            {
                if (dt.Rows.Count == 0)
                {
                    //InitDatabaseUpdate();
                    return null;
                }
                else
                {
                    List<string> list = new List<string>();
                    foreach (DataRow row in dt.Rows)
                    {
                        list.Add(row["InstalledUpdate"].ToString());
                    }
                    //list.Sort(StringComparer.Ordinal);
                    return list[0];
                }
            }
            else
                return null;
        }

        public void CheckForUpdate()
        {
            this.ShowDialog();
        }

        //
        // Summary:
        //     Shows the form as a modal dialog box with the currently active window set
        //     as its owner.
        //
        // Returns:
        //     One of the System.Windows.Forms.DialogResult values.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     The form specified in the owner parameter is the same as the form being shown.
        //
        //   System.InvalidOperationException:
        //     The form being shown is already visible.  -or- The form being shown is disabled.
        //      -or- The form being shown is not a top-level window.  -or- The form being
        //     shown as a dialog box is already a modal form.  -or- The current process
        //     is not running in user interactive mode (for more information, see System.Windows.Forms.SystemInformation.UserInteractive).
        public new System.Windows.Forms.DialogResult ShowDialog()
        {
            return this.ShowDialog(null);
        }

        //
        // Summary:
        //     Shows the form as a modal dialog box with the specified owner.
        //
        // Parameters:
        //   owner:
        //     Any object that implements System.Windows.Forms.IWin32Window that represents
        //     the top-level window that will own the modal dialog box.
        //
        // Returns:
        //     One of the System.Windows.Forms.DialogResult values.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     The form specified in the owner parameter is the same as the form being shown.
        //
        //   System.InvalidOperationException:
        //     The form being shown is already visible.  -or- The form being shown is disabled.
        //      -or- The form being shown is not a top-level window.  -or- The form being
        //     shown as a dialog box is already a modal form.  -or- The current process
        //     is not running in user interactive mode (for more information, see System.Windows.Forms.SystemInformation.UserInteractive).
        public new System.Windows.Forms.DialogResult ShowDialog(IWin32Window owner)
        {
            //string databaseVersion = GetDatabaseUpdate();
            //if (this.CurrentProgramVersion.CompareTo(databaseVersion) > 0)
            //{
            //List<string> updateFiles = System.IO.Directory.GetFiles(this.UpdateFilesPath, "*." + this.UpdateFilesExtension, System.IO.SearchOption.TopDirectoryOnly).ToList();
            //if (updateFiles.Length == 0)
            //    return System.Windows.Forms.DialogResult.OK;
            List<string> updateFiles = UpdateFilesWithPath.ToList();
            for (int i = 0; i < updateFiles.Count; i++)
            {
                //updateFiles[i] = updateFiles[i].Replace(".k.", ".600.");
                //updateFiles[i] = updateFiles[i].Replace(".a.", ".700.");
                //updateFiles[i] = updateFiles[i].Replace(".m.", ".800.");
                //updateFiles[i] = updateFiles[i].Replace(".f.", ".900.");
                updateFiles[i] = Path.GetFileName(updateFiles[i]);
            }
            //for (int i = updateFiles.Count - 1; i >= 0; i--)
            //{
            //    if (!Version.TryParse(updateFiles[i], out databaseVersion))
            //    {
            //        updateFiles.RemoveAt(i);
            //    }
            //}
            updateFiles.Sort(StringComparer.Ordinal);
            //this.UpdateFiles.AddRange(updateFiles);
            this.UpdateFiles.Clear();
            foreach (string item in updateFiles)
            {
                //if (char.IsDigit(item[0]))
                //{
                this.UpdateFiles.Add(item);
                //}
            }
            //if (this.UpdateFiles.Count == 0)
            //    return System.Windows.Forms.DialogResult.OK;
            if (owner == null)
                return base.ShowDialog();
            else
                return base.ShowDialog(owner);
            //}
            //else
            //{
            //    return System.Windows.Forms.DialogResult.OK;
            //}
        }

        private List<string> _UpdateFiles = new List<string>();
        public List<string> UpdateFiles
        {
            get
            {
                return _UpdateFiles;
            }
            set
            {
                _UpdateFiles = value;
            }
        }

        //public bool IsCurrectVersion(string version)
        //{
        //    if (version == null)
        //        return false;
        //    string[] strArray = version.Split(new char[1] { '.' });
        //    int length = strArray.Length;
        //    if (length < 2 || length > 4)
        //        return false;
        //    int _Major = int.Parse(strArray[0], (IFormatProvider)CultureInfo.InvariantCulture);
        //    if (_Major < 0)
        //        return false;
        //    int _Minor = int.Parse(strArray[1], (IFormatProvider)CultureInfo.InvariantCulture);
        //    if (_Minor < 0)
        //        return false;
        //    int num = length - 2;
        //    if (num <= 0)
        //        return true;
        //    int _Build = int.Parse(strArray[2], (IFormatProvider)CultureInfo.InvariantCulture);
        //    if (_Build < 0)
        //        return false;
        //    if (num - 1 <= 0)
        //        return true;
        //    int _Revision = int.Parse(strArray[3], (IFormatProvider)CultureInfo.InvariantCulture);
        //    if (_Revision < 0)
        //        return false;
        //    return true;
        //}

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Refresh();
            if (this.UpdateFiles.Count == 0)
            {
                MessageBox.Show("No update file found.");
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                return;
            }
            //int successCount, errorCount;
            //InstallUpdates(out  successCount, out  errorCount);
            backgroundWorker.RunWorkerAsync();
            this.Refresh();
            //if (successCount == 0 && errorCount == 0)
            //    this.Close();
            //else if (errorCount == 0)
            //{
            //    SetMessage("به روز رسانی با موفقیت به پایان رسید");
            //    timerClose.Start();
            //}
        }

        //private void InstallUpdates(out int successCount, out int errorCount)
        //{

        //}

        private void SetStatus(string text)
        {
            //lblStatus.Text = "";
            //foreach (char ch in text)
            //{
            //    lblStatus.Text += ch.ToString();
            //    lblStatus.Refresh();
            //    System.Threading.Thread.Sleep(5);
            //}
            //lblStatus.Refresh();
            listBoxStatus.Items.Add(text);
            listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
            listBoxStatus.Refresh();
            //System.Threading.Thread.Sleep(1000);
        }

        private void SetMessage(string text)
        {
            //lblStatus.ForeColor = Color.Blue;
            SetStatus(text);
        }

        private void SetError(string text)
        {
            //lblStatus.ForeColor = Color.Red;
            SetStatus("Error --> " + text);
        }

        int closeTime = 6;

        public int CloseTime
        {
            get { return closeTime; }
            set
            {
                closeTime = value;
                lblTitle.Text = "Update completed successfully." + Environment.NewLine + value.ToString();
            }
        }

        //private void timerClose_Tick(object sender, EventArgs e)
        //{
        //    CloseTime -= 1;
        //    if (CloseTime == 0)
        //    {
        //        timerClose.Stop();
        //        this.Close();
        //    }
        //}

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //successCount = 0;
            int errorCount = 0;
            foreach (string updateFile in this.UpdateFiles)
            {
                backgroundWorker.ReportProgress(-1, "Checking version " + updateFile + " ...");
                string installedUpdate = GetDatabaseUpdate(updateFile);
                if (string.IsNullOrEmpty(installedUpdate))
                {
                    backgroundWorker.ReportProgress(-1, "Updating database to version " + updateFile + " ...");
                    this.DataAccess.Connection.Open();
                    System.Data.SqlClient.SqlTransaction tr = this.DataAccess.Connection.BeginTransaction();
                    this.DataAccess.Transaction = tr;

                    //string filePath = System.IO.Path.Combine(this.UpdateFilesPath, updateFile /*+ "." + this.UpdateFilesExtension*/);
                    string filePath = UpdateFilesWithPath[this.UpdateFiles.IndexOf(updateFile)];
                    string query = System.IO.File.ReadAllText(filePath);
                    try
                    {
                        SqlHelper.RunQuery(query, this.DataAccess);
                        this.DataAccess.Execute("INSERT INTO [DatabaseUpdate] ([InstalledUpdate]) Values (@InstalledUpdate)", "@InstalledUpdate", updateFile);
                    }
                    catch (Exception ex)
                    {
                        while (ex.InnerException != null)
                        {
                            ex = ex.InnerException;
                        }
                        tr.Rollback();
                        this.DataAccess.Connection.Close();
                        errorCount++;
                        backgroundWorker.ReportProgress(-2, "Error executing version update script " + updateFile + " " + ex.Message);
                        e.Result = errorCount;
                        return;
                    }

                    tr.Commit();
                    this.DataAccess.Connection.Close();
                    //successCount++;
                    backgroundWorker.ReportProgress(-1, "Update script " + updateFile + " installed successfully.");
                    backgroundWorker.ReportProgress(-1, "");
                }
                else
                {
                    backgroundWorker.ReportProgress(-1, "Update " + updateFile + " is already installed");
                    backgroundWorker.ReportProgress(-1, "");
                }
            }
            e.Result = errorCount;
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == -1)
                SetMessage(e.UserState.ToString());
            else if (e.ProgressPercentage == -2)
                SetError(e.UserState.ToString());
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                SetMessage("Update operation canceled.");
            else if (e.Error != null)
                SetError("An error occurred while performing the update operation.");
            else
            {
                if (((int)e.Result) == 0)
                {
                    SetMessage("The update operation completed successfully.");
                    //timerClose.Start();
                }
            }
        }
    }
}
