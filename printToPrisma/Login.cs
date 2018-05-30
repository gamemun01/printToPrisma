using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace printToPrisma
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        static string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        static string[] words = userName.Split('\\');
        string user = words[1];
        string machine = words[0];


        //MySqlConnection conMySQL = new MySqlConnection("datasource=127.0.0.1; port=3306; username=root; password=Tokyo88");
        MySqlConnection conMySQL = new MySqlConnection("datasource=" + Properties.Settings.Default["dbserver"].ToString() + "; port=3306; username=" + Properties.Settings.Default["dbuser"].ToString() + "; password=" + Properties.Settings.Default["dbpassword"].ToString() + "");
        static string connectionString = @"Data Source = kitchen.db; Version = 3";
        SQLiteConnection con = new SQLiteConnection(connectionString);
        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss");
        }
        String timeStamp = GetTimestamp(DateTime.Now);
        private bool loginAD(String user, String password)
        {
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, Properties.Settings.Default["domain"].ToString()))
            {
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
        private void insertRecord(String no, String filename, String sDate, String hash, String status, String reason)
        {

            try
            {
                SQLiteCommand insert = new SQLiteCommand();
                insert.CommandText = @"INSERT INTO reprinted_log (no, filename, send_date, hash, status, reason) VALUES (@no, @filename, @send_date, @hash, @status, @reason)";
                insert.Connection = con;
                insert.Parameters.Add(new SQLiteParameter("@no", no));
                insert.Parameters.Add(new SQLiteParameter("@filename", filename));
                insert.Parameters.Add(new SQLiteParameter("@send_date", sDate));
                insert.Parameters.Add(new SQLiteParameter("@hash", hash));
                insert.Parameters.Add(new SQLiteParameter("@status", status));
                insert.Parameters.Add(new SQLiteParameter("@reason", reason));


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
        public void insertRecordMySQL(String filename, String send_date, String user, String machine, String size, String modified, String hash, String reason)
        {
            string insertQuery = "INSERT INTO cpc.reprinted_log(filename, send_date, user, machine, size, modified, hash, reason) VALUES ('" + filename + "', '" + send_date + "','" + user + "','" + machine + "','" + size + "','" + modified + "','" + hash + "','" + reason + "')";
            conMySQL.Open();
            MySqlCommand command = new MySqlCommand(insertQuery, conMySQL);
            command.ExecuteNonQuery();
            conMySQL.Close();
        }
        public string GetSha1Hash(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                SHA1 sha = new SHA1Managed();
                return BitConverter.ToString(sha.ComputeHash(fs));
            }
        }       
        public string MyProperty { get; set; }
        public string SelectRegularFolder { get; set; }
        

        

        
        
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select reason");
            }
            else
            {
                if (textBox1.Text == Properties.Settings.Default["resenduser1"].ToString() || textBox1.Text == Properties.Settings.Default["resenduser2"].ToString())
                {
                    if (loginAD(textBox1.Text, textBox2.Text))
                    {
                        try
                        {
                            if (sftp.UploadSFTPFile(Properties.Settings.Default["server"].ToString(), Properties.Settings.Default["username"].ToString(), Properties.Settings.Default["password"].ToString(), MyProperty, Properties.Settings.Default["destination"].ToString() + "/" + SelectRegularFolder, 22))
                            {
                                FileInfo f = new FileInfo(MyProperty);
                                long s1 = f.Length;
                                var num = s1;
                                var ci = new CultureInfo("it-IT");

                                insertRecordMySQL(Path.GetFileName(MyProperty), timeStamp, textBox1.Text, machine, num.ToString("#,##0", ci), System.IO.File.GetLastWriteTime(MyProperty).ToString(), GetSha1Hash(MyProperty), comboBox1.SelectedItem.ToString());
                                DialogResult result = MessageBox.Show("Sent!", "Sent!", MessageBoxButtons.OK);
                                if (result == DialogResult.OK)
                                {
                                    this.Close();
                                }
                            }
                        }
                        catch (Exception er)
                        {
                            MessageBox.Show("Please Check Prisma Server ... " + er.Message.ToString());
                        }
                    }
                    else
                    {
                        MessageBox.Show("Wrong Password!");
                    }
                }
                else
                {
                    MessageBox.Show("Username not granted!");
                }
            }   
        }
        private void Login_Load(object sender, EventArgs e)
        {
            con.Open();
            richTextBox1.Text = MyProperty;
            label5.Text = SelectRegularFolder;
    }

        
    }
}
