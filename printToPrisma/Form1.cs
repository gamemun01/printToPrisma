using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Threading.Tasks;
using printToPrisma.Properties;
using System.Collections;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Drawing.Text;
using MySql.Data.MySqlClient;

namespace printToPrisma
{
    public partial class Form1 : Form
    {
        bool ready;
        int totalFileSend;
        int totalPartialFileSend;
        String timeStamp = GetTimestamp(DateTime.Now);

        /*
        bool statusFile;
        string fileName;
        string sha1Hash;
        string source = @"D:\josephaditya\Desktop\WinSCP\upload.txt";
        string destination = @"/home/josephaditya/REGULAR2/";
        string host = "10.10.10.7";
        string username = "root";
        string password = "dlink";
        int port = 22;
        */

        string RegFolder;
        static string connectionString = @"Data Source = kitchen.db; Version = 3";
        SQLiteConnection con = new SQLiteConnection(connectionString);
        PrivateFontCollection modernFont = new PrivateFontCollection();
        static string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        static string[] words = userName.Split('\\');
        string user = words[1];
        string machine = words[0];


        MySqlConnection conMySQL = new MySqlConnection("datasource="+ Properties.Settings.Default["dbserver"].ToString() +"; port=3306; username="+ Properties.Settings.Default["dbuser"].ToString() + "; password="+ Properties.Settings.Default["dbpassword"].ToString() + "");


        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load_1(object sender, EventArgs e)
        {
            cbReg.SelectedIndex = 1;
            // change all font


            FileInfo f = new FileInfo("kitchen.db");
            long s1 = f.Length;

            var num = s1;
            var ci = new CultureInfo("it-IT");

            //label13.Text = num.ToString("#,##0", ci) + " bytes";

            try
            {
                con.Open();
                if (con.State == ConnectionState.Open)
                {
                    label10.Text = "Connected!";
                }
                else
                {
                    label10.Text = "Not Connected!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            label4.Text = Properties.Settings.Default["server"].ToString();
            label6.Text = Properties.Settings.Default["destination"].ToString();
            label18.Text = Properties.Settings.Default["dbserver"].ToString();
            label19.Text = Properties.Settings.Default["domain"].ToString();

            progressBar1.Visible = false;
            label9.Visible = false;
        }
        struct DataParameter
        {
            public int Process;
            public int Delay;
        }
        private DataParameter _inputparameter;
        private DataParameter _inputparameter2;
        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss");
        } 
        public string GetSha1Hash(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                SHA1 sha = new SHA1Managed();
                return BitConverter.ToString(sha.ComputeHash(fs));
            }
        }
        private bool checkHashExist(String hash)
        {
            SQLiteCommand comID = new SQLiteCommand("SELECT count(*) FROM printed_log WHERE hash = '" + hash + "'");
            comID.Connection = con;

            SQLiteDataReader dr = comID.ExecuteReader(CommandBehavior.CloseConnection);
            if (dr.Read())
            {
                if (dr.GetValue(0).ToString() == "0")
                {
                    ready = true;
                }
                else
                {
                    ready = false;
                }
            }
            return ready;
        }
        private bool checkHashExistMySQL(String hash)
        {
            //https://www.codeproject.com/Questions/552323/Howplustoplusstoreplustheplusresultplusofplusaplus

            bool exist;
            string selectQuery = "SELECT count(*) FROM cpc.printed_log WHERE hash = '" + hash + "'";
            conMySQL.Open();
            MySqlCommand command = new MySqlCommand(selectQuery, conMySQL);

            string result = command.ExecuteScalar().ToString();

            if (result == "1")
            {
                conMySQL.Close();
                exist = false;
            }
            else
            {
                conMySQL.Close();
                exist = true;    
            }
            return exist;
        }
        private bool loginAD(String user, String password)
        {
            //https://stackoverflow.com/questions/290548/validate-a-username-and-password-against-active-directory
            //https://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "GONJOTD.COM"))
            {
                // validate the credentials
                bool isValid = pc.ValidateCredentials(user, password);
                if (isValid)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public void insertRecord(String filename, String sDate, String user, String machine, String size, String modified, String hash, String status, String no)
        {
            
            try
            {
                SQLiteCommand insert = new SQLiteCommand();
                insert.CommandText = @"INSERT INTO printed_log (filename, send_date, user, machine, size, modified, hash, status, no) VALUES (@filename, @send_date, @user, @machine, @size, @modified, @hash, @status, @no)";
                insert.Connection = con;
                
                insert.Parameters.Add(new SQLiteParameter("@filename", filename));
                insert.Parameters.Add(new SQLiteParameter("@send_date", sDate));

                insert.Parameters.Add(new SQLiteParameter("@user", user));
                insert.Parameters.Add(new SQLiteParameter("@machine", machine));
                insert.Parameters.Add(new SQLiteParameter("@size", size));
                insert.Parameters.Add(new SQLiteParameter("@modified", modified));

                insert.Parameters.Add(new SQLiteParameter("@hash", hash));
                insert.Parameters.Add(new SQLiteParameter("@status", status));
                insert.Parameters.Add(new SQLiteParameter("@no", no));

                int i = insert.ExecuteNonQuery();

                if (i == 1)
                {
                    //MessageBox.Show("Masuk!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void insertRecordMySQL(String filename, String send_date, String user, String machine, String size, String modified, String hash, String status)
        {
            string insertQuery = "INSERT INTO cpc.printed_log(filename, send_date, user, machine, size, modified, hash, status) VALUES ('"+ filename +"', '" + send_date + "','" + user +"','" + machine +"','" + size +"','" + modified + "','" + hash + "','"+ status + "')";
            conMySQL.Open();
            MySqlCommand command = new MySqlCommand(insertQuery, conMySQL);
            command.ExecuteNonQuery();
            conMySQL.Close();
        }
        public void insertChamber(String filename, String path, String hash, String status)
        {
            try
            {
                SQLiteCommand insert = new SQLiteCommand();
                insert.CommandText = @"INSERT INTO chamber (filename,  path, hash, status) VALUES (@filename,  @path, @hash, @status)";
                insert.Connection = con;
                insert.Parameters.Add(new SQLiteParameter("@filename", filename));
                insert.Parameters.Add(new SQLiteParameter("@path", path));
                insert.Parameters.Add(new SQLiteParameter("@hash", hash));
                insert.Parameters.Add(new SQLiteParameter("@status", status));


                int i = insert.ExecuteNonQuery();

                if (i == 1)
                {
                    //MessageBox.Show("Masuk!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void insertChamberSend(String filename, String sDate, String user, String machine, String size, String modified, String hash, String status, String no)
        {

            try
            {
                SQLiteCommand insert = new SQLiteCommand();
                insert.CommandText = @"INSERT INTO chamber_send (filename, send_date, user, machine, size, modified, hash, status, no) VALUES (@filename, @send_date, @user, @machine, @size, @modified, @hash, @status, @no)";
                insert.Connection = con;

                insert.Parameters.Add(new SQLiteParameter("@filename", filename));
                insert.Parameters.Add(new SQLiteParameter("@send_date", sDate));

                insert.Parameters.Add(new SQLiteParameter("@user", user));
                insert.Parameters.Add(new SQLiteParameter("@machine", machine));
                insert.Parameters.Add(new SQLiteParameter("@size", size));
                insert.Parameters.Add(new SQLiteParameter("@modified", modified));

                insert.Parameters.Add(new SQLiteParameter("@hash", hash));
                insert.Parameters.Add(new SQLiteParameter("@status", status));
                insert.Parameters.Add(new SQLiteParameter("@no", no));

                int i = insert.ExecuteNonQuery();

                if (i == 1)
                {
                    //MessageBox.Show("Masuk!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }      
        public void truncateChamber()
        {
            SQLiteCommand openRePrint = new SQLiteCommand();
            openRePrint.CommandText = @"DELETE FROM chamber";
            openRePrint.Connection = con;
            openRePrint.ExecuteNonQuery();
        }
        public void truncateChamberSend()
        {
            SQLiteCommand openRePrint = new SQLiteCommand();
            openRePrint.CommandText = @"DELETE FROM chamber_send";
            openRePrint.Connection = con;
            openRePrint.ExecuteNonQuery();
        }
        public void showDataChamber()
        {
            SQLiteCommand openRePrint = new SQLiteCommand();
            openRePrint.CommandText = @"SELECT * FROM chamber";
            openRePrint.Connection = con;
            openRePrint.ExecuteNonQuery();
            DataSet dt = new DataSet();
            SQLiteDataAdapter da = new SQLiteDataAdapter(openRePrint.CommandText, con);
            da.Fill(dt);
            dataGridView2.DataSource = dt.Tables[0].DefaultView;


            // Column 0
            DataGridViewColumn cFilename = dataGridView2.Columns[0];
            cFilename.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth0 = cFilename.Width;
            cFilename.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth0 += 30;
            this.dataGridView2.Columns[0].Width = colWidth0;

            // Column 1
            DataGridViewColumn cPath = dataGridView2.Columns[1];
            cPath.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth1 = cPath.Width;
            cPath.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth1 += 30;
            this.dataGridView2.Columns[1].Width = colWidth1;

            // Column 2
            DataGridViewColumn cHash = dataGridView2.Columns[2];
            cHash.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth2 = cHash.Width;
            cHash.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth2 += 30;
            this.dataGridView2.Columns[2].Width = colWidth2;

            // Column 3
            DataGridViewColumn cStatus = dataGridView2.Columns[3];
            cStatus.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth3 = cStatus.Width;
            cStatus.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth3 += 30;
            this.dataGridView2.Columns[3].Width = colWidth3;
        }
        public void showDataChamberSend()
        {
            SQLiteCommand openRePrint = new SQLiteCommand();
            openRePrint.CommandText = @"SELECT filename, send_date, size, modified FROM chamber_send";
            openRePrint.Connection = con;
            openRePrint.ExecuteNonQuery();
            DataSet dt = new DataSet();
            SQLiteDataAdapter da = new SQLiteDataAdapter(openRePrint.CommandText, con);
            da.Fill(dt);
            dataGridView3.DataSource = dt.Tables[0].DefaultView;

            // Column 0
            DataGridViewColumn column = dataGridView3.Columns[0];
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth = column.Width;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth += 30;
            this.dataGridView3.Columns[0].Width = colWidth;

            // Column 1
            DataGridViewColumn column1 = dataGridView3.Columns[1];
            column1.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth1 = column1.Width;
            column1.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth1 += 30;
            this.dataGridView3.Columns[1].Width = colWidth1;

            // Column 2
            DataGridViewColumn column2 = dataGridView3.Columns[2];
            column2.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth2 = column2.Width;
            column2.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth2 += 30;
            this.dataGridView3.Columns[2].Width = colWidth2;

            // Column 3
            DataGridViewColumn column3 = dataGridView3.Columns[3];
            column3.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth3 = column3.Width;
            column3.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth3 += 30;
            this.dataGridView3.Columns[3].Width = colWidth3;

            /*
            // Column 4
            DataGridViewColumn column4 = dataGridView3.Columns[4];
            column4.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth4 = column4.Width;
            column4.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth4 += 30;
            this.dataGridView3.Columns[4].Width = colWidth4;

            // Column 5
            DataGridViewColumn column5 = dataGridView3.Columns[5];
            column5.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth5 = column5.Width;
            column5.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth5 += 30;
            this.dataGridView3.Columns[5].Width = colWidth5;

            // Column 6
            DataGridViewColumn column6 = dataGridView3.Columns[6];
            column6.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth6 = column6.Width;
            column6.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth6 += 30;
            this.dataGridView3.Columns[6].Width = colWidth6;

            // Column 7
            DataGridViewColumn column7 = dataGridView3.Columns[7];
            column7.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth7 = column7.Width;
            column7.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth7 += 30;
            this.dataGridView3.Columns[7].Width = colWidth7;

            // Column 8
            DataGridViewColumn column8 = dataGridView3.Columns[8];
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth8 = column.Width;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth8 += 30;
            this.dataGridView3.Columns[8].Width = colWidth8;
            */
        }
        

        /*
         * WORKER 1 START
         */

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            try
            {
                // START EXECUTION   
                string[] filePaths = Directory.GetFiles(folderBrowserDialog1.SelectedPath, "*.*", SearchOption.TopDirectoryOnly);
                totalFileSend = filePaths.Count() - 1;

                for (int i = 0; i < filePaths.Length; i++)
                {
                    if (checkHashExistMySQL(GetSha1Hash(filePaths[i])))// JIKA SUDAH ADA
                    {
                        try
                        {
                            backgroundWorker1.ReportProgress(i);
                            if (sftp.UploadSFTPFile(Properties.Settings.Default["server"].ToString(), Properties.Settings.Default["username"].ToString(), Properties.Settings.Default["password"].ToString(), filePaths[i], Properties.Settings.Default["destination"].ToString() + "/" + RegFolder + "/", 22))
                            {
                                FileInfo f = new FileInfo(filePaths[i]);
                                long s1 = f.Length;
                                var num = s1;
                                var ci = new CultureInfo("it-IT");
                                
                                insertRecordMySQL(Path.GetFileName(filePaths[i]), timeStamp, user, machine, num.ToString("#,##0", ci), System.IO.File.GetLastWriteTime(filePaths[i]).ToString(), GetSha1Hash(filePaths[i]), "Sent");
                                insertChamberSend(Path.GetFileName(filePaths[i]), timeStamp, user, machine, num.ToString("#,##0", ci), System.IO.File.GetLastWriteTime(filePaths[i]).ToString(), GetSha1Hash(filePaths[i]), "Sent", "");
                            }
                            else
                            {
                                MessageBox.Show("Upload Failed!");
                            }
                        }
                        catch (Exception er)
                        {
                            MessageBox.Show("Check Prisma Server " + er.Message);
                        }
                    }
                    else
                    {
                        insertChamber(Path.GetFileName(filePaths[i]), filePaths[i], GetSha1Hash(filePaths[i]), "Already Printed!, Please contact supervisor.");
                    }
                }
                // STOP EXECUTION
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Maximum = totalFileSend;
            progressBar1.Value = e.ProgressPercentage;
            progressBar1.Visible = true;
            label9.Visible = true;
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int sent = totalFileSend + 1;
            MessageBox.Show(sent.ToString() +  " file(s) has been sent!", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            progressBar1.Value = 0;
            progressBar1.Visible = false;
            label9.Visible = false;
            showDataChamber();
            showDataChamberSend();
        }

        /*
         * WORKER 1 STOP
         */

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings f2 = new Settings();
            f2.Show();

            //this.Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                _inputparameter.Delay = 10;
                _inputparameter.Process = 120;
                backgroundWorker1.RunWorkerAsync(_inputparameter);
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            
            
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                truncateChamber();
                truncateChamberSend();

                label5.Text = folderBrowserDialog1.SelectedPath;
                string[] filePaths = Directory.GetFiles(folderBrowserDialog1.SelectedPath, "*.*", SearchOption.TopDirectoryOnly);
                DataTable table = new DataTable();
                table.Columns.Add("FILE NAME", typeof(string));
                table.Columns.Add("MODIFIED DATE", typeof(string));
                table.Columns.Add("FILE SIZE", typeof(string));
                table.Columns.Add("PATH", typeof(string));
                for (int i = 0; i < filePaths.Length; i++)
                {
                    FileInfo f = new FileInfo(filePaths[i]);
                    long s1 = f.Length;
                    var num = s1;
                    var ci = new CultureInfo("it-IT");
                    table.Rows.Add(Path.GetFileName(filePaths[i]), System.IO.File.GetLastWriteTime(filePaths[i]).ToString(), num.ToString("#,##0", ci) + " bytes", filePaths[i]);
                }

                dataGridView1.DataSource = table;

                // Column 0
                DataGridViewColumn column = dataGridView1.Columns[0];
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                int colWidth = column.Width;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colWidth += 30;
                this.dataGridView1.Columns[0].Width = colWidth;

                // Column 1
                DataGridViewColumn column1 = dataGridView1.Columns[1];
                column1.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                int colWidth1 = column1.Width;
                column1.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colWidth1 += 30;
                this.dataGridView1.Columns[1].Width = colWidth1;

                // Column 2
                DataGridViewColumn column2 = dataGridView1.Columns[2];
                column2.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                int colWidth2 = column2.Width;
                column2.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colWidth2 += 30;
                this.dataGridView1.Columns[2].Width = colWidth2;

                // Column 3
                DataGridViewColumn column3 = dataGridView1.Columns[3];
                column3.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                int colWidth3 = column3.Width;
                column3.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colWidth3 += 30;
                this.dataGridView1.Columns[3].Width = colWidth3;
            }
            showDataChamber();
            showDataChamberSend();
        }
        private void standardLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendLog f3 = new SendLog();
            f3.Show();
        }
        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.contextMenuStrip2.Show(this.dataGridView2, e.Location);
                contextMenuStrip2.Show(Cursor.Position);
            }
        }
        private void dataGridView2_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.contextMenuStrip1.Show(this.dataGridView2, e.Location);
                contextMenuStrip1.Show(Cursor.Position);
            }
        }
        private void reprintToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int rowIndex = dataGridView2.CurrentCell.RowIndex;
            DataGridViewRow row = this.dataGridView2.Rows[rowIndex];
            string a = row.Cells["path"].Value.ToString();
            string b = cbReg.SelectedItem.ToString();

            Login fLogin = new Login();
            fLogin.MyProperty = a;
            fLogin.SelectRegularFolder = b;
            fLogin.Show();
            //this.Hide();

            //MessageBox.Show(a);
        }
        private void reprintLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReprintLog f3 = new ReprintLog();
            f3.Show();
        }      
        private void printSendToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (!backgroundWorker2.IsBusy)
            {
                _inputparameter2.Delay = 10;
                _inputparameter2.Process = 120;
                backgroundWorker2.RunWorkerAsync(_inputparameter2);
            }
        }
        private void cbReg_SelectedIndexChanged(object sender, EventArgs e)
        {
            RegFolder = cbReg.SelectedItem.ToString();
        }

        /*
         * WORKER 2 START
         */

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            Int32 selectedRowCount = dataGridView1.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (selectedRowCount > 0)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                for (int i = 0; i < selectedRowCount; i++)
                {
                    DataGridViewRow row = this.dataGridView1.Rows[dataGridView1.SelectedRows[i].Index];
                    string a = row.Cells["PATH"].Value.ToString();
                    totalPartialFileSend = selectedRowCount - 1;
                    if (checkHashExistMySQL(GetSha1Hash(a))) // JIKA SUDAH ADA
                    {
                        try
                        {
                            backgroundWorker2.ReportProgress(i);
                            if (sftp.UploadSFTPFile(Properties.Settings.Default["server"].ToString(), Properties.Settings.Default["username"].ToString(), Properties.Settings.Default["password"].ToString(), a, Properties.Settings.Default["destination"].ToString() + "/" + RegFolder + "/", 22))
                            {
                                //insertRecord("", a, timeStamp, GetSha1Hash(a), "Sent!");

                                FileInfo f = new FileInfo(a);
                                long s1 = f.Length;
                                var num = s1;
                                var ci = new CultureInfo("it-IT");

                                insertRecordMySQL(Path.GetFileName(a), timeStamp, user, machine, num.ToString("#,##0", ci), System.IO.File.GetLastWriteTime(a).ToString(), GetSha1Hash(a), "Sent");
                                insertChamberSend(Path.GetFileName(a), timeStamp, user, machine, num.ToString("#,##0", ci), System.IO.File.GetLastWriteTime(a).ToString(), GetSha1Hash(a), "Sent", "");
                            }
                            else
                            {
                                MessageBox.Show("Upload Failed!");
                            }
                        }
                        catch (Exception er)
                        {
                            MessageBox.Show("Check Prisma Server.. " + er.Message);
                        }
                    }
                    else
                    {
                        insertChamber(Path.GetFileName(a), a, GetSha1Hash(a), "Duplicate file!, Please contact supervisor.");
                    }
                }
            }
        }
        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Maximum = totalPartialFileSend;
            progressBar1.Value = e.ProgressPercentage;
            progressBar1.Visible = true;
            label9.Visible = true;
        }
        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int sent = totalPartialFileSend + 1;
            MessageBox.Show(sent.ToString() + " file(s) has been sent!", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            progressBar1.Value = 0;
            progressBar1.Visible = false;
            label9.Visible = false;


            showDataChamber();
            showDataChamberSend();
        }

        /*
         * WORKER 2 STOP
         */
    }
}
