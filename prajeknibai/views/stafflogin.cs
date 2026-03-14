using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using prajeknibai.controller;

namespace prajeknibai.views.models
{
    public partial class stafflogin : Form
    {
        public stafflogin()
        {
            InitializeComponent();
            guna2Button1.Click += guna2Button1_Click;
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            Load += stafflogin_Load;
            Shown += stafflogin_Shown;
            Resize += stafflogin_Resize;
            ClientSizeChanged += stafflogin_ClientSizeChanged;
        }

        private void stafflogin_Load(object sender, EventArgs e)
        {
            guna2TextBox2.UseSystemPasswordChar = true;
            guna2Button1.Anchor = AnchorStyles.None;
            guna2TextBox1.Anchor = AnchorStyles.None;
            guna2TextBox2.Anchor = AnchorStyles.None;
            guna2HtmlLabel1.Anchor = AnchorStyles.None;
            guna2HtmlLabel2.Anchor = AnchorStyles.None;
            guna2HtmlLabel3.Anchor = AnchorStyles.None;
            linkLabel1.Anchor = AnchorStyles.None;
            BeginInvoke(new Action(LayoutStaffLoginControls));
        }

        private void stafflogin_Shown(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutStaffLoginControls));
        }

        private void stafflogin_Resize(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutStaffLoginControls));
        }

        private void stafflogin_ClientSizeChanged(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutStaffLoginControls));
        }

        private void LayoutStaffLoginControls()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            SuspendLayout();

            guna2Panel1.Width = Math.Max(420, Math.Min(628, ClientSize.Width / 2));

            var leftAreaWidth = guna2Panel1.Left;
            var inputWidth = Math.Min(320, Math.Max(250, leftAreaWidth - 220));
            var titleY = Math.Max(85, ClientSize.Height / 7);
            var labelLeft = Math.Max(35, (leftAreaWidth - inputWidth) / 2 - 110);
            var inputLeft = labelLeft + 155;
            var usernameY = titleY + 105;
            var passwordY = usernameY + 95;

            guna2HtmlLabel3.Location = new Point(Math.Max(30, (leftAreaWidth - guna2HtmlLabel3.Width) / 2), titleY);

            guna2HtmlLabel1.Location = new Point(labelLeft, usernameY + 10);
            guna2TextBox1.Location = new Point(inputLeft, usernameY);
            guna2TextBox1.Size = new Size(inputWidth, guna2TextBox1.Height);

            guna2HtmlLabel2.Location = new Point(labelLeft, passwordY + 10);
            guna2TextBox2.Location = new Point(inputLeft, passwordY);
            guna2TextBox2.Size = new Size(inputWidth, guna2TextBox2.Height);

            linkLabel1.Location = new Point(guna2TextBox2.Left + (guna2TextBox2.Width - linkLabel1.Width) / 2, guna2TextBox2.Bottom + 10);
            guna2Button1.Location = new Point((leftAreaWidth - guna2Button1.Width) / 2, linkLabel1.Bottom + 55);

            ResumeLayout();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            var username = guna2TextBox1.Text.Trim();
            var password = guna2TextBox2.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter a username.", "Staff Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter your password.", "Staff Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var user = UserRepository.AuthenticateStaff(username, password);
                if (user == null)
                {
                    MessageBox.Show("Invalid staff credentials.", "Staff Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                AppSession.CurrentStaffName = user.FullName;

                var dashboard = new prajeknibai.views.staffdashboard();
                dashboard.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to continue to staff dashboard.\n\n{ex.Message}", "Staff Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var adminLogin = new prajeknibai.loginadmin();
            adminLogin.Show();
            Close();
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
