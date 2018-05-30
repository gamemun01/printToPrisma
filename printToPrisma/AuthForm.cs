using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace printToPrisma
{
    public partial class AuthForm : Form
    {
        public AuthForm()
        {
            InitializeComponent();
        }
        
        private bool loginAD(String user, String password)
        {
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, textBox3.Text))
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

        private void button2_Click(object sender, EventArgs e)
        {
            Settings set = new Settings();
            set.Show();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (loginAD(textBox1.Text, textBox2.Text))
            {
                try
                {
                    Settings settingForm = new Settings();
                    settingForm.openGroupBox = "granted!";
                    settingForm.Show();

                    this.Close();
                    MessageBox.Show("Logged in");
                }
                catch (Exception er)
                {

                    MessageBox.Show(er.Message);
                }
            }
            else
            {
                MessageBox.Show("Please try Again!");
            }
        }
    }
}
