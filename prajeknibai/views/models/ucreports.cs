using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using prajeknibai.controller;

namespace prajeknibai.views.models
{
    public partial class ucreports : UserControl
    {
        private FlowLayoutPanel _recentSalesHost;

        public ucreports()
        {
            InitializeComponent();
            Load += ucreports_Load;
            Resize += ucreports_Resize;
        }

        private void ucreports_Load(object sender, EventArgs e)
        {
            LoadReportData();
            BeginInvoke(new Action(LayoutReports));
        }

        private void ucreports_Resize(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutReports));
        }

        private void LoadReportData()
        {
            try
            {
                var summary = SalesRepository.GetSalesSummary();
                var recentSales = SalesRepository.GetRecentSales(8);

                guna2HtmlLabel2.Text = FormatCurrency(summary.TotalRevenue);
                guna2HtmlLabel5.Text = summary.TotalSales.ToString(CultureInfo.InvariantCulture);
                guna2HtmlLabel6.Text = "Average Sale";
                guna2HtmlLabel7.Text = FormatCurrency(summary.AverageSale);
                guna2HtmlLabel7.ForeColor = Color.FromArgb(0, 24, 64);
                guna2HtmlLabel9.Text = "Recent Sales";

                HideStaticCategoryControls();
                EnsureRecentSalesHost();
                _recentSalesHost.Controls.Clear();

                foreach (var sale in recentSales)
                {
                    _recentSalesHost.Controls.Add(CreateSalePanel(sale));
                }

                LayoutReports();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load reports.\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HideStaticCategoryControls()
        {
            foreach (Control control in pnlCategory.Controls)
            {
                if (control != guna2PictureBox2 && control != guna2HtmlLabel9)
                {
                    control.Visible = false;
                }
            }
        }

        private void EnsureRecentSalesHost()
        {
            if (_recentSalesHost != null)
            {
                return;
            }

            _recentSalesHost = new FlowLayoutPanel
            {
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                Location = new Point(20, 60),
                Size = new Size(880, 350),
                WrapContents = false
            };

            pnlCategory.Controls.Add(_recentSalesHost);
            _recentSalesHost.BringToFront();
        }

        private Guna.UI2.WinForms.Guna2Panel CreateSalePanel(RecentSale sale)
        {
            var panel = new Guna.UI2.WinForms.Guna2Panel
            {
                BorderRadius = 10,
                BorderThickness = 1,
                Size = new Size(850, 70)
            };

            var titleLabel = new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0),
                Location = new Point(20, 10),
                Text = $"Sale #{sale.Id}"
            };

            var detailsLabel = new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.Gray,
                Location = new Point(20, 38),
                Text = $"{sale.CreatedAt:g} • {sale.ItemCount} items • {sale.PaymentMethod}"
            };

            var totalLabel = new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0),
                Location = new Point(730, 22),
                Tag = "TotalLabel",
                Text = FormatCurrency(sale.Total)
            };

            panel.Controls.Add(totalLabel);
            panel.Controls.Add(detailsLabel);
            panel.Controls.Add(titleLabel);
            return panel;
        }

        private void LayoutReports()
        {
            if (_recentSalesHost == null)
            {
                return;
            }

            _recentSalesHost.Size = new Size(Math.Max(300, pnlCategory.ClientSize.Width - 40), Math.Max(120, pnlCategory.ClientSize.Height - 80));

            foreach (Control control in _recentSalesHost.Controls)
            {
                if (control is not Guna.UI2.WinForms.Guna2Panel row)
                {
                    continue;
                }

                row.Size = new Size(_recentSalesHost.ClientSize.Width - 20, row.Height);

                foreach (Control child in row.Controls)
                {
                    if (Equals(child.Tag, "TotalLabel"))
                    {
                        child.Location = new Point(row.Width - child.Width - 20, child.Location.Y);
                    }
                }
            }
        }

        private static string FormatCurrency(decimal amount)
        {
            return string.Concat("$", amount.ToString("0.00", CultureInfo.InvariantCulture));
        }

        private void panelcards_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
