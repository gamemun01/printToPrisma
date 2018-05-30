using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using printToPrisma.Properties;

namespace printToPrisma
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }
        public string openGroupBox { get; set; }
        private void button2_Click(object sender, EventArgs e)
        {
            //Form1 fm = new Form1();
            //fm.Show();
            this.Close();
        }
        
        private void Settings_Load(object sender, EventArgs e)
        {
            if (openGroupBox == "granted!")
            {
                groupBox3.Visible = true;
            }
            else
            {
                groupBox3.Visible = false;
            }
            textBox4.Text = Properties.Settings.Default["dbserver"].ToString();
            textBox6.Text = Properties.Settings.Default["dbuser"].ToString();
            textBox7.Text = Properties.Settings.Default["dbpassword"].ToString();
            textBox8.Text = Properties.Settings.Default["domain"].ToString();
            textBox9.Text = Properties.Settings.Default["resenduser1"].ToString();
            textBox10.Text = Properties.Settings.Default["resenduser2"].ToString();


            textBox1.Text = Properties.Settings.Default["server"].ToString();
            textBox2.Text = Properties.Settings.Default["username"].ToString();
            textBox3.Text = Properties.Settings.Default["password"].ToString();
            textBox5.Text = Properties.Settings.Default["destination"].ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //Form1 fm = new Form1();
            //fm.Show();

            Properties.Settings.Default["server"] = textBox1.Text;
            Properties.Settings.Default["username"] = textBox2.Text;
            Properties.Settings.Default["password"] = textBox3.Text;
            Properties.Settings.Default["destination"] = textBox5.Text;

            Properties.Settings.Default["dbserver"] = textBox4.Text;
            Properties.Settings.Default["dbuser"] = textBox6.Text;
            Properties.Settings.Default["dbpassword"] = textBox7.Text;
            Properties.Settings.Default["domain"] = textBox8.Text;
            Properties.Settings.Default["resenduser1"] = textBox9.Text;
            Properties.Settings.Default["resenduser2"] = textBox10.Text;

            Properties.Settings.Default.Save();
            this.Close();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            AuthForm af = new AuthForm();
            af.Show();
            this.Close();

            //groupBox3.Visible = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            groupBox3.Visible = false;
        }
    }
}
