using prajeknibai.views;
using prajeknibai.controller;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace prajeknibai
{
    public partial class loginadmin : Form
    {
        public loginadmin()
        {
            InitializeComponent();

            DoubleBuffered = true;
            ResizeRedraw = true;

            Load += loginadmin_Load;
            Resize += loginadmin_Resize;
            Shown += loginadmin_Shown;
            ClientSizeChanged += loginadmin_ClientSizeChanged;
            guna2Panel1.SizeChanged += loginadmin_LeftPanelSizeChanged;
        }

        private void loginadmin_Load(object sender, EventArgs e)
        {
            guna2TextBox2.UseSystemPasswordChar = true;
            guna2Button1.Anchor = AnchorStyles.None;
            guna2TextBox1.Anchor = AnchorStyles.None;
            guna2TextBox2.Anchor = AnchorStyles.None;
            guna2HtmlLabel1.Anchor = AnchorStyles.None;
            guna2HtmlLabel2.Anchor = AnchorStyles.None;
            guna2HtmlLabel3.Anchor = AnchorStyles.None;
            linkLabel1.Anchor = AnchorStyles.None;
            BeginInvoke(new Action(LayoutLoginControls));
        }

        private void loginadmin_Resize(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutLoginControls));
        }

        private void loginadmin_Shown(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutLoginControls));
        }

        private void loginadmin_ClientSizeChanged(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutLoginControls));
        }

        private void loginadmin_LeftPanelSizeChanged(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutLoginControls));
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void LayoutLoginControls()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            SuspendLayout();

            guna2Panel1.Width = Math.Max(420, Math.Min(590, ClientSize.Width / 2));

            var rightAreaLeft = guna2Panel1.Right;
            var rightAreaWidth = ClientSize.Width - rightAreaLeft;
            var inputWidth = Math.Min(320, Math.Max(250, rightAreaWidth - 320));
            const int labelWidth = 150;
            const int controlGap = 20;
            var groupWidth = labelWidth + controlGap + inputWidth;
            var groupLeft = rightAreaLeft + Math.Max(40, (rightAreaWidth - groupWidth) / 2);
            var titleY = Math.Max(85, ClientSize.Height / 7);
            var usernameY = titleY + 105;
            var passwordY = usernameY + 95;

            guna2HtmlLabel3.Location = new Point(rightAreaLeft + Math.Max(40, (rightAreaWidth - guna2HtmlLabel3.Width) / 2), titleY);

            guna2HtmlLabel1.Location = new Point(groupLeft, usernameY + 10);
            guna2TextBox1.Location = new Point(groupLeft + labelWidth + controlGap, usernameY);
            guna2TextBox1.Size = new Size(inputWidth, guna2TextBox1.Height);

            guna2HtmlLabel2.Location = new Point(groupLeft, passwordY + 10);
            guna2TextBox2.Location = new Point(groupLeft + labelWidth + controlGap, passwordY);
            guna2TextBox2.Size = new Size(inputWidth, guna2TextBox2.Height);

            linkLabel1.Location = new Point(guna2TextBox2.Left + (guna2TextBox2.Width - linkLabel1.Width) / 2, guna2TextBox2.Bottom + 10);
            guna2Button1.Location = new Point(rightAreaLeft + Math.Max(40, (rightAreaWidth - guna2Button1.Width) / 2), linkLabel1.Bottom + 55);

            ResumeLayout();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            var username = guna2TextBox1.Text.Trim();
            var password = guna2TextBox2.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter an admin username.", "Admin Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter your password.", "Admin Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = UserRepository.AuthenticateAdmin(username, password);
            if (user == null)
            {
                MessageBox.Show("Invalid admin credentials.", "Admin Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            AppSession.CurrentAdminName = user.FullName;
            AppSession.CurrentAdminRole = user.Role;
            AppSession.CurrentStaffName = string.Empty;

            Form nextForm = string.Equals(user.Role, "Super Admin", StringComparison.OrdinalIgnoreCase)
                ? new SuperAdminForm()
                : new admindashboard();

            nextForm.Show();
            Hide();
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var staffLogin = new prajeknibai.views.models.stafflogin();
            staffLogin.Show();
            Hide();
        }
    }
}
