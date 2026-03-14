using System;
using System.Drawing;
using System.Windows.Forms;
using prajeknibai.controller;
using prajeknibai.views.models;

namespace prajeknibai
{
    public partial class SuperAdminForm : Form
    {
        public SuperAdminForm()
        {
            InitializeComponent();
            Shown += SuperAdminForm_Shown;
            Resize += SuperAdminForm_Resize;
        }

        private void SuperAdminForm_Load(object sender, EventArgs e)
        {
            var displayName = string.IsNullOrWhiteSpace(AppSession.CurrentAdminName)
                ? "Super Admin"
                : AppSession.CurrentAdminName;

            lblTitle.Text = string.IsNullOrWhiteSpace(AppSession.CurrentAdminName)
                ? "Super Admin"
                : $"Super Admin - {AppSession.CurrentAdminName}";
            lblProfileName.Text = displayName;
            lblProfileRole.Text = string.IsNullOrWhiteSpace(AppSession.CurrentAdminRole)
                ? "Super Admin"
                : AppSession.CurrentAdminRole;

            LoadSystemSales();
            BeginInvoke(new Action(LayoutSuperAdminShell));
        }

        private void SuperAdminForm_Shown(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutSuperAdminShell));
        }

        private void SuperAdminForm_Resize(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutSuperAdminShell));
        }

        private void btnSystemSales_Click(object sender, EventArgs e)
        {
            LoadSystemSales();
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            LoadBackupModule();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            AppSession.CurrentAdminName = string.Empty;
            AppSession.CurrentAdminRole = string.Empty;

            var login = new loginadmin();
            login.Show();
            Close();
        }

        private void LoadSystemSales()
        {
            SetActiveModuleButton(btnSystemSales);
            panelMain.Controls.Clear();

            var module = new SystemSalesModule
            {
                Dock = DockStyle.Fill
            };

            panelMain.Controls.Add(module);
        }

        private void LoadBackupModule()
        {
            SetActiveModuleButton(btnBackup);
            panelMain.Controls.Clear();

            var module = new BackupModule
            {
                Dock = DockStyle.Fill
            };

            panelMain.Controls.Add(module);
        }

        private void SetActiveModuleButton(Control activeButton)
        {
            btnSystemSales.FillColor = Color.PaleTurquoise;
            btnSystemSales.ForeColor = Color.Black;
            btnSystemSales.BorderColor = Color.Transparent;
            btnBackup.FillColor = Color.PaleTurquoise;
            btnBackup.ForeColor = Color.Black;
            btnBackup.BorderColor = Color.Transparent;

            if (activeButton == btnSystemSales)
            {
                btnSystemSales.FillColor = Color.FromArgb(37, 99, 235);
                btnSystemSales.ForeColor = Color.White;
                btnSystemSales.BorderColor = Color.FromArgb(20, 35, 60);
            }
            else if (activeButton == btnBackup)
            {
                btnBackup.FillColor = Color.FromArgb(37, 99, 235);
                btnBackup.ForeColor = Color.White;
                btnBackup.BorderColor = Color.FromArgb(20, 35, 60);
            }
        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }

        private void LayoutSuperAdminShell()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            SuspendLayout();

            btnLogout.Location = new Point(btnLogout.Left, panelSide.ClientSize.Height - panelProfile.Height - btnLogout.Height - 32);
            panelProfile.Location = new Point(panelProfile.Left, panelSide.ClientSize.Height - panelProfile.Height - 18);
            lblTitle.Location = new Point(Math.Max(28, (panelTop.ClientSize.Width - lblTitle.Width) / 2), lblTitle.Location.Y);

            ResumeLayout();
        }
    }
}
