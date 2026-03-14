using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using prajeknibai.controller;

namespace prajeknibai.views.models
{
    public sealed class ucviewstock : UserControl
    {
        private FlowLayoutPanel _listHost;
        private TextBox _txtSearch;
        private Button _btnRefresh;
        private Label _lblTitle;
        private IReadOnlyList<AppProduct> _products = Array.Empty<AppProduct>();

        public ucviewstock()
        {
            BuildUi();
            Load += ucviewstock_Load;
            Resize += ucviewstock_Resize;
        }

        private void ucviewstock_Load(object sender, EventArgs e)
        {
            LoadProducts();
            LayoutUi();
        }

        private void ucviewstock_Resize(object sender, EventArgs e)
        {
            LayoutUi();
        }

        private void BuildUi()
        {
            BackColor = Color.White;

            _lblTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Text = "View Stock"
            };

            _txtSearch = new TextBox
            {
                PlaceholderText = "Search product name or SKU..."
            };
            _txtSearch.TextChanged += (_, __) => ApplyFilter();

            _btnRefresh = new Button
            {
                Text = "Refresh"
            };
            _btnRefresh.Click += (_, __) => LoadProducts();

            _listHost = new FlowLayoutPanel
            {
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            Controls.Add(_lblTitle);
            Controls.Add(_txtSearch);
            Controls.Add(_btnRefresh);
            Controls.Add(_listHost);
        }

        private void LayoutUi()
        {
            const int outer = 16;
            const int gap = 10;

            _lblTitle.Location = new Point(outer, outer);

            var topRowY = _lblTitle.Bottom + gap;
            var refreshWidth = 100;
            _btnRefresh.Size = new Size(refreshWidth, 28);
            _btnRefresh.Location = new Point(Width - outer - refreshWidth, topRowY);

            _txtSearch.Location = new Point(outer, topRowY);
            _txtSearch.Size = new Size(Math.Max(200, _btnRefresh.Left - outer - gap), 28);

            _listHost.Location = new Point(outer, _txtSearch.Bottom + gap);
            _listHost.Size = new Size(Math.Max(200, Width - (outer * 2)), Math.Max(100, Height - _listHost.Top - outer));

            foreach (Control row in _listHost.Controls)
            {
                row.Width = _listHost.ClientSize.Width - 24;
            }
        }

        private void LoadProducts()
        {
            try
            {
                _products = ProductRepository.GetProducts();
                RenderRows(_products);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load stock.\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilter()
        {
            var keyword = _txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                RenderRows(_products);
                return;
            }

            var filtered = _products
                .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                            || p.Sku.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

            RenderRows(filtered);
        }

        private void RenderRows(IReadOnlyList<AppProduct> products)
        {
            _listHost.SuspendLayout();
            _listHost.Controls.Clear();

            foreach (var product in products)
            {
                _listHost.Controls.Add(CreateRow(product));
            }

            _listHost.ResumeLayout(true);
            LayoutUi();
        }

        private Control CreateRow(AppProduct product)
        {
            var row = new Panel
            {
                Height = 56,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var name = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Text = product.Name,
                Location = new Point(12, 8),
                Size = new Size(340, 20)
            };

            var sku = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.DimGray,
                Text = product.Sku,
                Location = new Point(12, 30),
                Size = new Size(220, 18)
            };

            var supplier = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.DimGray,
                Text = string.IsNullOrWhiteSpace(product.SupplierName) ? "" : product.SupplierName,
                Location = new Point(250, 30),
                Size = new Size(220, 18)
            };

            var price = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleRight,
                Text = string.Concat("$", product.Price.ToString("0.00", CultureInfo.InvariantCulture)),
                Location = new Point(500, 10),
                Size = new Size(120, 20)
            };

            var stock = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Text = product.Stock.ToString(CultureInfo.InvariantCulture),
                ForeColor = product.Stock <= 5 ? Color.Crimson : Color.SeaGreen,
                Location = new Point(640, 10),
                Size = new Size(80, 20)
            };

            row.Controls.Add(name);
            row.Controls.Add(sku);
            row.Controls.Add(supplier);
            row.Controls.Add(price);
            row.Controls.Add(stock);

            return row;
        }
    }
}
