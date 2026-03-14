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

namespace prajeknibai.views.models
{
    public partial class ucrecordsales : UserControl
    {
        private const decimal TaxRate = 0.10m;
        private List<Guna.UI2.WinForms.Guna2Panel> _productPanels;
        private readonly Dictionary<string, CartItem> _cartItems = new Dictionary<string, CartItem>(StringComparer.OrdinalIgnoreCase);
        private string _paymentMethod = "Cash";

        public ucrecordsales()
        {
            InitializeComponent();
            Resize += ucrecordsales_Resize;

            // Some designer versions don't automatically wire the Load event for UserControls.
            // Ensure our initialization (including Cash/Card button handlers) always runs.
            Load += ucrecordsales_Load;

            guna2Button1.Click -= PaymentButton_Click;
            guna2Button1.Click += PaymentButton_Click;
            guna2Button2.Click -= PaymentButton_Click;
            guna2Button2.Click += PaymentButton_Click;

            UpdatePaymentButtons();
        }

        private void ucrecordsales_Load(object sender, EventArgs e)
        {
            guna2Button4.Click -= guna2Button4_Click;
            guna2Button4.Click += guna2Button4_Click;
            guna2Button3.Click -= guna2Button3_Click;
            guna2Button3.Click += guna2Button3_Click;
            guna2Button1.Click -= PaymentButton_Click;
            guna2Button1.Click += PaymentButton_Click;
            guna2Button2.Click -= PaymentButton_Click;
            guna2Button2.Click += PaymentButton_Click;
            LoadProductsFromDatabase();
            UpdatePaymentButtons();
            UpdateSummary(0m);
            BeginInvoke(new Action(LayoutRecordSalesControl));
        }

        private void ucrecordsales_Resize(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutRecordSalesControl));
        }

        private void LoadProductsFromDatabase()
        {
            try
            {
                var products = ProductRepository.GetProducts();
                flowProducts.Controls.Clear();
                _productPanels = new List<Guna.UI2.WinForms.Guna2Panel>();

                foreach (var product in products)
                {
                    var panel = CreateProductPanel(product);
                    flowProducts.Controls.Add(panel);
                    _productPanels.Add(panel);
                    RegisterClickHandlers(panel);
                }

                LayoutRecordSalesControl();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load products.\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Guna.UI2.WinForms.Guna2Panel CreateProductPanel(AppProduct product)
        {
            var panel = new Guna.UI2.WinForms.Guna2Panel
            {
                Size = new Size(165, 105),
                Tag = new ProductInfo
                {
                    Name = product.Name,
                    Sku = product.Sku,
                    Price = product.Price,
                    Stock = product.Stock
                }
            };

            var nameLabel = new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(12, 12),
                Text = product.Name
            };

            var priceLabel = new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(12, 40),
                Text = FormatCurrency(product.Price)
            };

            var stockLabel = new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                ForeColor = Color.FromArgb(110, 120, 135),
                Location = new Point(12, 67),
                Text = $"Stock: {product.Stock}"
            };

            panel.Controls.Add(stockLabel);
            panel.Controls.Add(priceLabel);
            panel.Controls.Add(nameLabel);

            return panel;
        }

        private void RegisterClickHandlers(Control control)
        {
            control.Cursor = Cursors.Hand;
            control.Click -= ProductControl_Click;
            control.Click += ProductControl_Click;

            foreach (Control child in control.Controls)
            {
                RegisterClickHandlers(child);
            }
        }

        private void ProductControl_Click(object sender, EventArgs e)
        {
            var panel = FindProductPanel(sender as Control);
            if (panel?.Tag is ProductInfo product)
            {
                AddToCart(product);
            }
        }

        private void AddToCart(ProductInfo product)
        {
            if (string.IsNullOrWhiteSpace(product.Sku))
            {
                return;
            }

            var currentQuantity = _cartItems.TryGetValue(product.Sku, out var cartItem) ? cartItem.Quantity : 0;
            if (product.Stock <= currentQuantity)
            {
                MessageBox.Show($"Not enough stock for '{product.Name}'.", "Record Sales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_cartItems.TryGetValue(product.Sku, out var existingItem))
            {
                existingItem.Quantity++;
            }
            else
            {
                _cartItems[product.Sku] = new CartItem
                {
                    Name = product.Name,
                    Sku = product.Sku,
                    UnitPrice = product.Price,
                    Quantity = 1
                };
            }

            UpdateSummary(GetCartSubtotal());
        }

        private decimal GetCartSubtotal()
        {
            decimal subtotal = 0m;

            foreach (var item in _cartItems.Values)
            {
                subtotal += item.UnitPrice * item.Quantity;
            }

            return subtotal;
        }

        private Guna.UI2.WinForms.Guna2Panel FindProductPanel(Control control)
        {
            while (control != null)
            {
                if (control is Guna.UI2.WinForms.Guna2Panel panel && panel.Parent == flowProducts)
                {
                    return panel;
                }

                control = control.Parent;
            }

            return null;
        }

        private void PaymentButton_Click(object sender, EventArgs e)
        {
            _paymentMethod = sender == guna2Button2 ? "Card" : "Cash";
            UpdatePaymentButtons();
        }

        private void UpdatePaymentButtons()
        {
            var selectedFill = Color.FromArgb(37, 99, 235);
            var defaultFill = Color.White;
            var selectedForeColor = Color.White;
            var defaultForeColor = Color.FromArgb(37, 99, 235);

            guna2Button1.FillColor = _paymentMethod == "Cash" ? selectedFill : defaultFill;
            guna2Button1.ForeColor = _paymentMethod == "Cash" ? selectedForeColor : defaultForeColor;
            guna2Button2.FillColor = _paymentMethod == "Card" ? selectedFill : defaultFill;
            guna2Button2.ForeColor = _paymentMethod == "Card" ? selectedForeColor : defaultForeColor;
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            if (_cartItems.Count == 0)
            {
                MessageBox.Show("Please select at least one product before completing the sale.", "Record Sales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var subtotal = GetCartSubtotal();
                var tax = Math.Round(subtotal * TaxRate, 2);
                var total = subtotal + tax;

                SalesRepository.SaveSale(
                    _cartItems.Values.Select(item => new SaleLine
                    {
                        ProductName = item.Name,
                        Sku = item.Sku,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity
                    }).ToList(),
                    subtotal,
                    tax,
                    total,
                    _paymentMethod);

                MessageBox.Show("Sale saved successfully.", "Record Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _cartItems.Clear();
                LoadProductsFromDatabase();
                UpdateSummary(0m);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save sale.\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSummary(decimal subtotal)
        {
            var tax = Math.Round(subtotal * TaxRate, 2);
            var total = subtotal + tax;

            guna2HtmlLabel35.Text = FormatCurrency(subtotal);
            guna2HtmlLabel37.Text = FormatCurrency(tax);
            guna2HtmlLabel39.Text = FormatCurrency(total);
        }

        private string FormatCurrency(decimal amount)
        {
            return string.Concat("$", amount.ToString("0.00", CultureInfo.InvariantCulture));
        }

        private void LayoutRecordSalesControl()
        {
            if (_productPanels == null || _productPanels.Count == 0)
            {
                return;
            }

            var availableWidth = Math.Max(360, flowProducts.ClientSize.Width - 24);
            var columns = Math.Max(2, Math.Min(4, availableWidth / 190));
            var panelWidth = Math.Max(150, (availableWidth - ((columns - 1) * 12)) / columns);

            foreach (var productPanel in _productPanels)
            {
                productPanel.Margin = new Padding(0, 0, 12, 12);
                productPanel.Size = new Size(panelWidth, productPanel.Height);
            }
        }

        private void guna2GroupBox1_Click(object sender, EventArgs e)
        {

        }

        private void guna2GroupBox3_Click(object sender, EventArgs e)
        {

        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_productPanels == null)
            {
                return;
            }

            var keyword = txtSearch.Text.Trim();
            foreach (var panel in _productPanels)
            {
                var product = panel.Tag as ProductInfo;
                panel.Visible = string.IsNullOrWhiteSpace(keyword)
                    || (product?.Name?.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            _cartItems.Clear();
            UpdateSummary(0m);
        }

        private void btnViewStock_Click(object sender, EventArgs e)
        {
            using var dialog = new Form
            {
                Text = "View Stock",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable,
                MinimizeBox = false,
                MaximizeBox = true,
                Size = new Size(900, 600)
            };

            var view = new ucviewstock
            {
                Dock = DockStyle.Fill
            };

            dialog.Controls.Add(view);
            dialog.ShowDialog(FindForm());
        }

        private sealed class CartItem
        {
            public string Name { get; set; }
            public string Sku { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
        }

        private sealed class ProductInfo
        {
            public string Name { get; set; }
            public string Sku { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
        }

        private void guna2GroupBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
