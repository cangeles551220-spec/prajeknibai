using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using prajeknibai.controller;

namespace prajeknibai.views
{
    public partial class staffdashboard : Form
    {
        private IReadOnlyList<AppProduct> _products = Array.Empty<AppProduct>();
        private string _paymentMethod = "Cash";
        private Guna.UI2.WinForms.Guna2Button _cashButton;
        private Guna.UI2.WinForms.Guna2Button _cardButton;
        private Guna.UI2.WinForms.Guna2Button _viewStockButton;

        public staffdashboard()
        {
            InitializeComponent();
            Load += staffdashboard_Load;
            Shown += staffdashboard_Shown;
            Resize += staffdashboard_Resize;
            guna2ComboBox1.SelectedIndexChanged += cmbProducts_SelectedIndexChanged;
            numquantity.ValueChanged += numquantity_ValueChanged;
            guna2Button1.Click += guna2Button1_Click;
            EnsurePaymentButtons();
            EnsureViewStockButton();
        }

        private void EnsureViewStockButton()
        {
            if (_viewStockButton != null)
            {
                return;
            }

            _viewStockButton = new Guna.UI2.WinForms.Guna2Button
            {
                AutoRoundedCorners = true,
                BorderRadius = 27,
                FillColor = Color.FromArgb(128, 255, 255),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9F),
                Size = new Size(174, 56),
                Text = "View Stock"
            };

            _viewStockButton.Click += (_, __) => ShowViewStock();
            guna2Panel1.Controls.Add(_viewStockButton);
            _viewStockButton.BringToFront();
        }

        private void ShowViewStock()
        {
            using var form = new Form
            {
                Text = "View Stock",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(900, 650)
            };

            var control = new models.ucviewstock
            {
                Dock = DockStyle.Fill
            };

            form.Controls.Add(control);
            form.ShowDialog(this);
        }

        private void staffdashboard_Load(object sender, EventArgs e)
        {
            LoadDashboardData();
            BeginInvoke(new Action(LayoutStaffDashboard));
        }

        private void staffdashboard_Shown(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutStaffDashboard));
        }

        private void staffdashboard_Resize(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutStaffDashboard));
        }

        private void LoadDashboardData()
        {
            try
            {
                guna2HtmlLabel24.Text = string.IsNullOrWhiteSpace(AppSession.CurrentStaffName)
                    ? "Hello, Staff"
                    : $"Hello, {AppSession.CurrentStaffName}";

                _products = ProductRepository.GetProducts();
                guna2ComboBox1.DataSource = _products.ToList();
                guna2ComboBox1.DisplayMember = nameof(AppProduct.Name);
                guna2ComboBox1.ValueMember = nameof(AppProduct.Sku);
                guna2ComboBox1.SelectedIndex = -1;

                guna2HtmlLabel2.Text = _products.Count.ToString(CultureInfo.InvariantCulture);
                guna2HtmlLabel4.Text = SalesRepository.GetTodaySalesCount().ToString(CultureInfo.InvariantCulture);
                var lowStockCount = _products.Count(product => product.Stock <= 5);
                guna2HtmlLabel6.Text = lowStockCount.ToString(CultureInfo.InvariantCulture);
                guna2Panel7.FillColor = lowStockCount > 0 ? Color.MistyRose : Color.Honeydew;
                guna2HtmlLabel6.ForeColor = lowStockCount > 0 ? Color.Crimson : Color.SeaGreen;

                UpdateTotalAmount();
                UpdateRecentSales();
                UpdatePaymentButtons();
                BeginInvoke(new Action(LayoutStaffDashboard));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load staff dashboard.\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnsurePaymentButtons()
        {
            if (_cashButton != null)
            {
                return;
            }

            salecontainer.Location = new Point(19, 328);
            salecontainer.Size = new Size(589, 40);
            salecontainer.FillColor = Color.Transparent;

            _cashButton = new Guna.UI2.WinForms.Guna2Button
            {
                BorderRadius = 10,
                Location = new Point(0, 0),
                Size = new Size(285, 40),
                Text = "Cash"
            };

            _cardButton = new Guna.UI2.WinForms.Guna2Button
            {
                BorderRadius = 10,
                Location = new Point(304, 0),
                Size = new Size(285, 40),
                Text = "Card"
            };

            _cashButton.Click += PaymentButton_Click;
            _cardButton.Click += PaymentButton_Click;

            salecontainer.Controls.Add(_cashButton);
            salecontainer.Controls.Add(_cardButton);
        }

        private void LayoutStaffDashboard()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            SuspendLayout();

            guna2Button2.Location = new Point(guna2Panel1.Width - guna2Button2.Width - 15, guna2Button2.Location.Y);

            if (_viewStockButton != null)
            {
                _viewStockButton.Location = new Point(guna2Button2.Left - _viewStockButton.Width - 12, guna2Button2.Location.Y);
            }

            var leftWidth = Math.Max(520, Math.Min(640, ClientSize.Width / 2));
            guna2Panel2.Width = leftWidth;
            guna2Panel3.Width = ClientSize.Width - leftWidth;

            LayoutStaffSummaryCards();
            LayoutStaffRecentSales();
            LayoutRecordSalesPanel();

            ResumeLayout();
        }

        private void LayoutStaffSummaryCards()
        {
            const int margin = 8;
            const int gap = 8;

            guna2Panel4.Location = new Point(4, 3);
            guna2Panel4.Size = new Size(guna2Panel2.ClientSize.Width - 8, 161);

            var cardWidth = (guna2Panel4.Width - (margin * 2) - (gap * 2)) / 3;
            guna2Panel5.Location = new Point(margin, 12);
            guna2Panel5.Size = new Size(cardWidth, 124);

            guna2Panel6.Location = new Point(guna2Panel5.Right + gap, 12);
            guna2Panel6.Size = new Size(cardWidth, 124);

            guna2Panel7.Location = new Point(guna2Panel6.Right + gap, 12);
            guna2Panel7.Size = new Size(cardWidth, 124);
        }

        private void LayoutStaffRecentSales()
        {
            guna2Panel8.Location = new Point(4, 170);
            guna2Panel8.Size = new Size(guna2Panel2.ClientSize.Width - 8, guna2Panel2.ClientSize.Height - 174 - 12);

            var rowWidth = guna2Panel8.ClientSize.Width - 24;
            LayoutRecentSaleRow(guna2Panel9, guna2Panel10, guna2HtmlLabel10, 79, rowWidth);
            LayoutRecentSaleRow(guna2Panel11, guna2Panel12, guna2HtmlLabel13, 156, rowWidth);
            LayoutRecentSaleRow(guna2Panel13, guna2Panel14, guna2HtmlLabel16, 234, rowWidth);
            LayoutRecentSaleRow(guna2Panel15, guna2Panel16, guna2HtmlLabel19, 309, rowWidth);
        }

        private void LayoutRecordSalesPanel()
        {
            guna2Panel17.Size = new Size(guna2Panel3.ClientSize.Width - 24, guna2Panel17.Height);
            pnlTotal.Size = new Size(guna2Panel17.Width - 42, pnlTotal.Height);
            numquantity.Size = new Size(guna2Panel17.Width - 37, numquantity.Height);
            guna2ComboBox1.Size = new Size(guna2Panel17.Width - 31, guna2ComboBox1.Height);
            salecontainer.Location = new Point(19, 328);
            salecontainer.Size = new Size(guna2Panel17.Width - 42, 40);
            guna2Button1.Size = new Size(guna2Panel17.Width - 42, guna2Button1.Height);
            guna2HtmlLabel23.Location = new Point(pnlTotal.Width - guna2HtmlLabel23.Width - 18, guna2HtmlLabel23.Location.Y);

            if (_cashButton != null && _cardButton != null)
            {
                var paymentWidth = (salecontainer.Width - 20) / 2;
                _cashButton.Size = new Size(paymentWidth, 40);
                _cardButton.Location = new Point(paymentWidth + 20, 0);
                _cardButton.Size = new Size(paymentWidth, 40);
            }
        }

        private static void LayoutRecentSaleRow(Control row, Control divider, Control amountLabel, int top, int width)
        {
            row.Location = new Point(8, top);
            row.Size = new Size(width, row.Height);
            divider.Size = new Size(row.Width - 24, divider.Height);
            amountLabel.Location = new Point(row.Width - amountLabel.Width - 14, amountLabel.Location.Y);
        }

        private void PaymentButton_Click(object sender, EventArgs e)
        {
            _paymentMethod = sender == _cardButton ? "Card" : "Cash";
            UpdatePaymentButtons();
        }

        private void UpdatePaymentButtons()
        {
            if (_cashButton == null || _cardButton == null)
            {
                return;
            }

            var selectedFill = Color.FromArgb(37, 99, 235);
            var defaultFill = Color.White;
            var selectedForeColor = Color.White;
            var defaultForeColor = Color.FromArgb(37, 99, 235);

            _cashButton.FillColor = _paymentMethod == "Cash" ? selectedFill : defaultFill;
            _cashButton.ForeColor = _paymentMethod == "Cash" ? selectedForeColor : defaultForeColor;
            _cardButton.FillColor = _paymentMethod == "Card" ? selectedFill : defaultFill;
            _cardButton.ForeColor = _paymentMethod == "Card" ? selectedForeColor : defaultForeColor;
        }

        private void UpdateRecentSales()
        {
            var recentSales = SalesRepository.GetRecentSales(4).ToList();

            UpdateRecentSalePanel(guna2Panel9, guna2HtmlLabel8, guna2HtmlLabel9, guna2HtmlLabel10, recentSales.ElementAtOrDefault(0));
            UpdateRecentSalePanel(guna2Panel11, guna2HtmlLabel11, guna2HtmlLabel12, guna2HtmlLabel13, recentSales.ElementAtOrDefault(1));
            UpdateRecentSalePanel(guna2Panel13, guna2HtmlLabel14, guna2HtmlLabel15, guna2HtmlLabel16, recentSales.ElementAtOrDefault(2));
            UpdateRecentSalePanel(guna2Panel15, guna2HtmlLabel17, guna2HtmlLabel18, guna2HtmlLabel19, recentSales.ElementAtOrDefault(3));
        }

        private void UpdateRecentSalePanel(Control panel, Control titleLabel, Control detailsLabel, Control amountLabel, RecentSale sale)
        {
            if (sale == null)
            {
                panel.Visible = false;
                return;
            }

            panel.Visible = true;
            titleLabel.Text = $"Sale #{sale.Id}";
            detailsLabel.Text = $"Qty: {sale.ItemCount} • {sale.CreatedAt:t}";
            amountLabel.Text = FormatCurrency(sale.Total);
        }

        private void numquantity_ValueChanged(object sender, EventArgs e)
        {
            UpdateTotalAmount();
        }

        private void UpdateTotalAmount()
        {
            if (guna2ComboBox1.SelectedItem is not AppProduct product)
            {
                guna2HtmlLabel23.Text = FormatCurrency(0m);
                return;
            }

            var total = product.Price * numquantity.Value;
            guna2HtmlLabel23.Text = FormatCurrency(total);
        }

        private static string FormatCurrency(decimal amount)
        {
            return string.Concat("$", amount.ToString("0.00", CultureInfo.InvariantCulture));
        }

        private void guna2Panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel9_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cmbProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTotalAmount();
        }

        private void guna2HtmlLabel25_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            if (guna2ComboBox1.SelectedItem is not AppProduct product)
            {
                MessageBox.Show("Please choose a product first.", "Record Sales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var quantity = (int)numquantity.Value;
            if (quantity <= 0)
            {
                MessageBox.Show("Quantity must be at least 1.", "Record Sales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (product.Stock < quantity)
            {
                MessageBox.Show($"Not enough stock for '{product.Name}'. Available: {product.Stock}.", "Record Sales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var subtotal = product.Price * quantity;
                var tax = Math.Round(subtotal * 0.10m, 2);
                var total = subtotal + tax;

                SalesRepository.SaveSale(
                    new[]
                    {
                        new SaleLine
                        {
                            ProductName = product.Name,
                            Sku = product.Sku,
                            UnitPrice = product.Price,
                            Quantity = quantity
                        }
                    },
                    subtotal,
                    tax,
                    total,
                    _paymentMethod);

                MessageBox.Show("Sale recorded successfully.", "Record Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                numquantity.Value = 1;
                LoadDashboardData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to record sale.\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            var staffLogin = new prajeknibai.views.models.stafflogin();
            staffLogin.Show();
            this.Close();
        }

        private void linkLabelAdmin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var adminLogin = new prajeknibai.loginadmin();
            adminLogin.Show();
            this.Close();
        }

        private void numquantity_ValueChanged_1(object sender, EventArgs e)
        {

        }

        private void guna2Panel17_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2HtmlLabel24_Click(object sender, EventArgs e)
        {

        }
    }
}
