using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace printToPrisma
{
    public partial class ReprintLog : Form
    {
        public ReprintLog()
        {
            InitializeComponent();
        }
        static string connectionString = @"Data Source = kitchen.db; Version = 3";
        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss");
        }
        String timeStamp = GetTimestamp(DateTime.Now);
        SQLiteConnection con = new SQLiteConnection(connectionString);
        string connStr = "datasource=" + Properties.Settings.Default["dbserver"].ToString() + "; port=3306; username=" + Properties.Settings.Default["dbuser"].ToString() + "; password=" + Properties.Settings.Default["dbpassword"].ToString() + "";
        private void ReprintLog_Load(object sender, EventArgs e)
        {
            con.Open();
            showPrintedMySQL();
        }
        public void showPrinted()
        {
            SQLiteCommand openRePrint = new SQLiteCommand();
            openRePrint.CommandText = @"SELECT * FROM reprinted_log";
            openRePrint.Connection = con;
            openRePrint.ExecuteNonQuery();

            DataSet dt = new DataSet();
            SQLiteDataAdapter da = new SQLiteDataAdapter(openRePrint.CommandText, con);
            da.Fill(dt);
            dataGridView1.DataSource = dt.Tables[0].DefaultView;


            // Column 0
            DataGridViewColumn cFilename = dataGridView1.Columns[0];
            cFilename.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth0 = cFilename.Width;
            cFilename.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth0 += 30;
            this.dataGridView1.Columns[0].Width = colWidth0;

            // Column 1
            DataGridViewColumn cPath = dataGridView1.Columns[1];
            cPath.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth1 = cPath.Width;
            cPath.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth1 += 30;
            this.dataGridView1.Columns[1].Width = colWidth1;

            // Column 2
            DataGridViewColumn cHash = dataGridView1.Columns[2];
            cHash.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth2 = cHash.Width;
            cHash.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth2 += 30;
            this.dataGridView1.Columns[2].Width = colWidth2;

            // Column 3
            DataGridViewColumn cStatus = dataGridView1.Columns[3];
            cStatus.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            int colWidth3 = cStatus.Width;
            cStatus.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colWidth3 += 30;
            this.dataGridView1.Columns[3].Width = colWidth3;
        }
        public void showPrintedMySQL()
        {
            string query = "SELECT no, filename, send_date, user, machine, size, modified, reason FROM cpc.reprinted_log"; // set query to fetch data "Select * from  tabelname";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                {
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    dataGridView1.DataSource = ds.Tables[0];
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            con.Close();
            this.Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = GetTimestamp(DateTime.Now) + "_Reprinted.csv";
            saveFileDialog1.Filter = "Comma Delimiter (*.csv)|*.csv|All files (*.*)|*.*";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                String sql = "SELECT * FROM cpc.reprinted_log";
                String pathCSV = saveFileDialog1.FileName;

                using (DataTable dbTbl = new DataTable("Printed"))
                {
                    using (MySqlDataAdapter dbAdptr = new MySqlDataAdapter(sql, connStr))
                    {
                        dbAdptr.Fill(dbTbl);
                    }
                    StringBuilder builder = new StringBuilder();
                    foreach (DataRow dr in dbTbl.Rows)
                    {
                        foreach (object item in dr.ItemArray)
                        {
                            builder.AppendFormat("\"{0}\", ", item.ToString());
                        }
                        builder.Replace(", ", Environment.NewLine, builder.Length - 2, 2);
                    }
                    File.WriteAllText(pathCSV, builder.ToString());
                }

            }
        }
    }
}
