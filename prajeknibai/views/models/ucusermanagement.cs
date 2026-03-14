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
    public partial class ucusermanagement : UserControl
    {
        private bool _usersLoaded;

        public ucusermanagement()
        {
            InitializeComponent();
            btnAddUser.Click += btnAddUser_Click;
            flowLayoutPanel1.AutoScroll = true;
            Load += ucusermanagement_Load;
            Resize += ucusermanagement_Resize;
            txtPassword.UseSystemPasswordChar = true;
            txtConfirmPassword.UseSystemPasswordChar = true;
        }

        private void ucusermanagement_Load(object sender, EventArgs e)
        {
            if (_usersLoaded)
            {
                return;
            }

            _usersLoaded = true;
            LoadUsersFromDatabase();
            BeginInvoke(new Action(LayoutUserManagement));
        }

        private void ucusermanagement_Resize(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutUserManagement));
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            var fullName = txtFullName.Text.Trim();
            var username = txtUsername.Text.Trim();
            var role = cmbRole.SelectedItem?.ToString()?.Trim();
            var password = txtPassword.Text;
            var confirmPassword = txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(fullName)
                || string.IsNullOrWhiteSpace(username)
                || string.IsNullOrWhiteSpace(role)
                || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter full name, username, role, and password.", "Add User", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Add User", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                MessageBox.Show("Password and confirm password do not match.", "Add User", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!UserRepository.AddUser(fullName, username, role, password))
                {
                    MessageBox.Show("That username already exists in the database.", "Add User", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                LoadUsersFromDatabase();
                txtFullName.Clear();
                txtUsername.Clear();
                txtPassword.Clear();
                txtConfirmPassword.Clear();
                cmbRole.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save user.\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadUsersFromDatabase()
        {
            try
            {
                var users = UserRepository.GetUsers();

                flowLayoutPanel1.Controls.Clear();
                foreach (var user in users)
                {
                    flowLayoutPanel1.Controls.Add(CreateUserPanel(user.FullName, user.Username, user.Role));
                }

                LayoutUserManagement();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load users.\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Guna.UI2.WinForms.Guna2Panel CreateUserPanel(string fullName, string username, string role)
        {
            var panel = new Guna.UI2.WinForms.Guna2Panel
            {
                BackColor = Color.WhiteSmoke,
                Size = new Size(850, 70),
                Tag = username
            };

            var statusIndicator = new Guna.UI2.WinForms.Guna2CirclePictureBox
            {
                FillColor = Color.LawnGreen,
                Location = new Point(19, 21),
                Size = new Size(25, 25),
                BackColor = Color.Transparent
            };

            var nameLabel = new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0),
                Location = new Point(65, 12),
                Text = fullName.ToUpperInvariant()
            };

            var detailsLabel = new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0),
                ForeColor = Color.Gray,
                Location = new Point(65, 38),
                Text = string.Concat(role, " • @", username)
            };

            var statusButton = new Guna.UI2.WinForms.Guna2Button
            {
                BorderRadius = 15,
                Enabled = false,
                FillColor = Color.LightGreen,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Green,
                Location = new Point(740, 20),
                Size = new Size(91, 30),
                Text = "ACTIVE"
            };

            panel.Controls.Add(statusIndicator);
            panel.Controls.Add(statusButton);
            panel.Controls.Add(detailsLabel);
            panel.Controls.Add(nameLabel);

            return panel;
        }

        private void LayoutUserManagement()
        {
            var width = Math.Max(320, flowLayoutPanel1.ClientSize.Width - 20);

            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is not Guna.UI2.WinForms.Guna2Panel panel)
                {
                    continue;
                }

                panel.Size = new Size(width, panel.Height);

                foreach (Control child in panel.Controls)
                {
                    if (child is Guna.UI2.WinForms.Guna2Button button && string.Equals(button.Text, "ACTIVE", StringComparison.OrdinalIgnoreCase))
                    {
                        button.Location = new Point(panel.Width - button.Width - 20, button.Location.Y);
                    }
                }
            }
        }

        private void guna2HtmlLabel3_Click(object sender, EventArgs e)
        {


        }

        private void guna2HtmlLabel6_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel9_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel11_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel10_Click(object sender, EventArgs e)
        {

        }

        private void flowusers_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtFullName_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
