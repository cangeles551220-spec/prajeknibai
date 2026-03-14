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
    public partial class ucproductmanagement : UserControl
    {
        private bool _productsLoaded;
        private FlowLayoutPanel _productListHost;
        private int? _editingProductId;
        private IReadOnlyList<AppSupplier> _suppliers = Array.Empty<AppSupplier>();

        public ucproductmanagement()
        {
            InitializeComponent();

            DoubleBuffered = true;

            btnAddProduct.Click += btnAddProduct_Click;
            Resize += ucproductmanagement_Resize;

            // Ensure initialization runs even if the designer didn't wire the Load event.
            Load += ucproductmanagement_Load;
        }

        private bool _layoutScheduled;

        private void ucproductmanagement_Load(object sender, EventArgs e)
        {
            if (_productsLoaded)
            {
                return;
            }

            _productsLoaded = true;
            EnsureProductListHost();
            LoadSuppliers();
            LoadProducts();
            BeginInvoke(new Action(LayoutProductManagement));
        }

        private void ucproductmanagement_Resize(object sender, EventArgs e)
        {
            ScheduleLayout();
        }

        private void ScheduleLayout()
        {
            if (_layoutScheduled || !IsHandleCreated)
            {
                return;
            }

            _layoutScheduled = true;
            BeginInvoke(new Action(() =>
            {
                _layoutScheduled = false;
                LayoutProductManagement();
            }));
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            var productName = txtProductName.Text.Trim();
            var sku = txtSKU.Text.Trim();
            var selectedSupplier = cmbSupplier.SelectedValue;

            if (string.IsNullOrWhiteSpace(productName)
                || string.IsNullOrWhiteSpace(sku)
                || selectedSupplier is not int supplierId
                || !decimal.TryParse(txtPrice.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var price)
                || !int.TryParse(txtStock.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stock)
                || stock < 0)
            {
                MessageBox.Show("Please enter product name, supplier, SKU, a valid price, and a valid stock quantity.", "Add Product", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var success = _editingProductId.HasValue
                    ? ProductRepository.UpdateProduct(_editingProductId.Value, productName, sku, price, stock, supplierId)
                    : ProductRepository.AddProduct(productName, sku, price, stock, supplierId);

                if (!success)
                {
                    MessageBox.Show("That SKU already exists in the database.", "Add Product", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ResetEditor();
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save product.\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSuppliers()
        {
            _suppliers = SupplierRepository.GetSuppliers();
            cmbSupplier.DataSource = _suppliers.ToList();
            cmbSupplier.DisplayMember = nameof(AppSupplier.Name);
            cmbSupplier.ValueMember = nameof(AppSupplier.Id);
            cmbSupplier.SelectedIndex = -1;
        }

        private void EnsureProductListHost()
        {
            if (_productListHost != null)
            {
                return;
            }

            panelRow1.Visible = false;
            guna2Panel1.Visible = false;
            guna2Panel2.Visible = false;

            _productListHost = new FlowLayoutPanel
            {
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                Name = "flowProductsDb",
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 60, 20, 20),
                WrapContents = false
            };

            _productListHost.ControlAdded += (_, __) => LayoutProductManagement();
            _productListHost.ControlRemoved += (_, __) => LayoutProductManagement();

            pnlProductList.Controls.Add(_productListHost);
            _productListHost.BringToFront();
        }

        private void LoadProducts()
        {
            try
            {
                var products = ProductRepository.GetProducts();
                _productListHost.Controls.Clear();

                foreach (var product in products)
                {
                    _productListHost.Controls.Add(CreateProductRow(product));
                }

                LayoutProductManagement();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load products.\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetEditor()
        {
            _editingProductId = null;
            txtProductName.Clear();
            txtSKU.Clear();
            txtPrice.Clear();
            txtStock.Clear();
            cmbSupplier.SelectedIndex = -1;
            btnAddProduct.Text = "+ Add Product";
        }

        private Guna.UI2.WinForms.Guna2Panel CreateProductRow(AppProduct product)
        {
            var panel = new Guna.UI2.WinForms.Guna2Panel
            {
                BorderRadius = 10,
                BorderThickness = 1,
                Size = new Size(831, 70),
                Margin = new Padding(0, 0, 0, 12)
            };

            var nameLabel = new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = Color.Black,
                Location = new Point(20, 10),
                Text = product.Name
            };

            var detailsLabel = new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.Gray,
                Location = new Point(20, 35),
                Text = $"{product.Sku} • {product.SupplierName} • ${product.Price:0.00} • Stock: {product.Stock}"
            };

            var editButton = new Guna.UI2.WinForms.Guna2Button
            {
                FillColor = Color.White,
                ForeColor = Color.DimGray,
                Location = new Point(661, 20),
                Size = new Size(35, 30),
                Text = "✎"
            };

            editButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            var deleteButton = new Guna.UI2.WinForms.Guna2Button
            {
                FillColor = Color.White,
                ForeColor = Color.DimGray,
                Location = new Point(738, 20),
                Size = new Size(35, 30),
                Text = "🗑"
            };

            deleteButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            editButton.Click += (sender, e) =>
            {
                _editingProductId = product.Id;
                txtProductName.Text = product.Name;
                txtSKU.Text = product.Sku;
                txtPrice.Text = product.Price.ToString("0.00", CultureInfo.InvariantCulture);
                txtStock.Text = product.Stock.ToString(CultureInfo.InvariantCulture);
                cmbSupplier.SelectedValue = product.SupplierId;
                btnAddProduct.Text = "Save Product";
            };

            deleteButton.Click += (sender, e) =>
            {
                if (MessageBox.Show($"Delete '{product.Name}'?", "Delete Product", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                try
                {
                    ProductRepository.DeleteProduct(product.Id);
                    if (_editingProductId == product.Id)
                    {
                        ResetEditor();
                    }

                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to delete product.\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            panel.Controls.Add(deleteButton);
            panel.Controls.Add(editButton);
            panel.Controls.Add(detailsLabel);
            panel.Controls.Add(nameLabel);
            return panel;
        }

        private void LayoutProductManagement()
        {
            if (_productListHost == null)
            {
                return;
            }

            var hostWidth = Math.Max(300, pnlProductList.ClientSize.Width - 40);
            var hostHeight = Math.Max(120, pnlProductList.ClientSize.Height - 80);
            if (_productListHost.Dock == DockStyle.None)
            {
                _productListHost.Size = new Size(hostWidth, hostHeight);
            }

            foreach (Control control in _productListHost.Controls)
            {
                if (control is not Guna.UI2.WinForms.Guna2Panel row)
                {
                    continue;
                }

                row.Size = new Size(Math.Max(200, _productListHost.ClientSize.Width - 40), row.Height);

                if (row.Controls.Count >= 4)
                {
                    var deleteButton = row.Controls[0];
                    var editButton = row.Controls[1];
                    deleteButton.Location = new Point(row.Width - deleteButton.Width - 20, deleteButton.Location.Y);
                    editButton.Location = new Point(deleteButton.Left - editButton.Width - 16, editButton.Location.Y);
                }
            }
        }

        private void pnlProductList_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panelRow1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
