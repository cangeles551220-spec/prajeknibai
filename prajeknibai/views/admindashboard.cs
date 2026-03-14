using prajeknibai.views.models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.Reflection;
using prajeknibai.controller;

namespace prajeknibai
{
    public partial class admindashboard : Form
    {
        public admindashboard()
        {
            InitializeComponent();
            Load += admindashboard_Load;
            Shown += admindashboard_Shown;
            Resize += admindashboard_Resize;
            panelmain.SizeChanged += admindashboard_PanelmainSizeChanged;
        }

        private void admindashboard_Load(object sender, EventArgs e)
        {
            btnUserMenu.Text = string.IsNullOrWhiteSpace(AppSession.CurrentAdminName)
                ? "admin user"
                : AppSession.CurrentAdminName;
            pnlUserDropdown.Visible = false;
            btnSuperAdmin.Visible = string.Equals(AppSession.CurrentAdminRole, "Admin", StringComparison.OrdinalIgnoreCase);
            BeginInvoke(new Action(LayoutDashboardHome));
        }

        private void admindashboard_Shown(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutDashboardHome));
        }

        private void admindashboard_Resize(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutDashboardHome));
        }

        private void admindashboard_PanelmainSizeChanged(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutDashboardHome));
        }

        private void LoadPage(Func<UserControl> createPage)
        {
            try
            {
                panelmain.Controls.Clear();

                UserControl page = createPage();
                page.Dock = DockStyle.Fill;
                panelmain.Controls.Add(page);
            }
            catch (NullReferenceException)
            {
                this.Controls.Add(createPage());
            }
        }

        private UserControl CreateUserControlInstance(string targetName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray();
                }

                foreach (var t in types)
                {
                    if (t == null) continue;

                    if (string.Equals(t.Name, targetName, StringComparison.OrdinalIgnoreCase)
                        && typeof(UserControl).IsAssignableFrom(t))
                    {
                        try
                        {
                            var inst = (UserControl)Activator.CreateInstance(t);
                            inst.Dock = DockStyle.Fill;
                            return inst;
                        }
                        catch
                        {
                          
                        }
                    }
                }
            }

            return new UserControl { Dock = DockStyle.Fill };
        }

        private UserControl CreateUcReportsInstance()
        {
            return CreateUserControlInstance("ucreports");
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            LoadPage(CreateUcReportsInstance);
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            try
            {
                panelmain.Controls.Clear();
                flowcard.Dock = DockStyle.Top;
                recentSalesPanel.Dock = DockStyle.Fill;
                panelmain.Controls.Add(recentSalesPanel);
                panelmain.Controls.Add(flowcard);
                BeginInvoke(new Action(LayoutDashboardHome));
            }
            catch (NullReferenceException)
            {
                
                this.Controls.Add(recentSalesPanel);
                this.Controls.Add(flowcard);
                BeginInvoke(new Action(LayoutDashboardHome));
            }
        }

        private void LayoutDashboardHome()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            SuspendLayout();

            btnUserMenu.Location = new Point(guna2Panel1.Width - btnUserMenu.Width - 22, btnUserMenu.Top);
            pnlUserDropdown.Location = new Point(btnUserMenu.Left, btnUserMenu.Bottom + 4);
            pnlUserDropdown.Width = btnUserMenu.Width;
            guna2HtmlLabel1.Location = new Point((guna2Panel1.Width - guna2HtmlLabel1.Width) / 2, guna2HtmlLabel1.Top);

            if (panelmain.Controls.Contains(flowcard) && panelmain.Controls.Contains(recentSalesPanel))
            {
                const int outerMargin = 10;
                const int sectionGap = 12;
                const int cardGap = 14;

                flowcard.SuspendLayout();
                flowcard.Location = new Point(outerMargin, outerMargin);
                flowcard.Size = new Size(panelmain.ClientSize.Width - (outerMargin * 2), 170);
                flowcard.Padding = new Padding(0, 10, 0, 0);

                var summaryCards = new[] { cardsales, guna2Panel4, guna2Panel5, guna2Panel6 };
                var cardWidth = (flowcard.Width - (cardGap * (summaryCards.Length - 1))) / summaryCards.Length;
                for (var index = 0; index < summaryCards.Length; index++)
                {
                    var card = summaryCards[index];
                    card.Margin = Padding.Empty;
                    card.Location = new Point(0, 10);
                    card.Size = new Size(cardWidth, 140);
                    card.Margin = new Padding(0, 0, index == summaryCards.Length - 1 ? 0 : cardGap, 0);
                }
                flowcard.ResumeLayout(true);
                flowcard.PerformLayout();

                recentSalesPanel.Location = new Point(outerMargin, flowcard.Bottom + sectionGap);
                recentSalesPanel.Size = new Size(panelmain.ClientSize.Width - (outerMargin * 2), panelmain.ClientSize.Height - recentSalesPanel.Top - outerMargin);

                LayoutSalesRows();
            }

            ResumeLayout();
        }

        private void LayoutSalesRows()
        {
            const int leftPadding = 12;
            var rowWidth = recentSalesPanel.ClientSize.Width - (leftPadding * 2);

            if (rowWidth <= 0)
            {
                return;
            }

            LayoutSalesRow(guna2Panel7, guna2Panel8, guna2HtmlLabel17, 60, rowWidth);
            LayoutSalesRow(guna2Panel9, guna2Panel10, guna2HtmlLabel18, 126, rowWidth);
            LayoutSalesRow(guna2Panel11, guna2Panel12, guna2HtmlLabel21, 192, rowWidth);

            pnlLowStockAlert.Location = new Point(leftPadding, 270);
            pnlLowStockAlert.Size = new Size(rowWidth, pnlLowStockAlert.Height);
        }

        private static void LayoutSalesRow(Control row, Control divider, Control amountLabel, int top, int width)
        {
            row.Location = new Point(12, top);
            row.Size = new Size(width, row.Height);
            divider.Size = new Size(row.Width - 20, divider.Height);
            amountLabel.Location = new Point(row.Width - amountLabel.Width - 20, amountLabel.Location.Y);
        }

        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {

        }

        private void recentSalesPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            LoadPage(CreateUcSupplierInstance);
        }

        private void guna2Button6_Click_LoadPayroll(object sender, EventArgs e)
        {
            // Backwards-compatible named method if designer wires different name; forward to main handler.
            guna2Button6_Click(sender, e);
        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            LoadPage(CreateUcUserManagementInstance);
        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {
            pnlUserDropdown.Visible = !pnlUserDropdown.Visible;
        }

        private void guna2Button8_Click_1(object sender, EventArgs e)
        {
            pnlUserDropdown.Visible = false;
            LoadPage(CreateUcUserManagementInstance);
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            LoadPage(CreateUcRecordSalesInstance);
        }

   
        private void guna2Button4_Click(object sender, EventArgs e)
        {
            LoadPage(CreateUcPayrollInstance);
        }

        
        private UserControl CreateUcPayrollInstance()
        {
            return CreateUserControlInstance("ucpayroll");
        }

 
        private UserControl CreateUcRecordSalesInstance()
        {
            return CreateUserControlInstance("ucrecordsales");
        }

       
        private UserControl CreateUcSupplierInstance()
        {
            return CreateUserControlInstance("ucsupplier");
        }

        private UserControl CreateUcUserManagementInstance()
        {
            return CreateUserControlInstance("ucusermanagement");
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            LoadPage(CreateUcProductManagementInstance);
        }

        // Create an instance of `ucProductManagement` if present at runtime.
        // Falls back to a plain UserControl if the type isn't found.
        private UserControl CreateUcProductManagementInstance()
        {
            return CreateUserControlInstance("ucProductManagement");
        }

        // When the Log Out button is clicked, return to the login window.
        private void guna2Button9_Click(object sender, EventArgs e)
        {
            AppSession.CurrentAdminName = string.Empty;
            AppSession.CurrentAdminRole = string.Empty;
            var login = new loginadmin();
            login.Show();
            this.Close();
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnSuperAdmin_Click(object sender, EventArgs e)
        {
            pnlUserDropdown.Visible = false;
            SuperAdminForm frm = new SuperAdminForm();
            frm.Show();
            this.Hide();
        }

        private void guna2Panel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private UserControl CreateUcViewStockInstance()
        {
            return CreateUserControlInstance("ucviewstock");
        }
    }
}
