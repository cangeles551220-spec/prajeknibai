using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace prajeknibai.views.models
{
    public class SystemSalesModule : UserControl
    {
        private readonly Guna2Panel _headerPanel;
        private readonly Guna2Button _viewAllButton;
        private readonly FlowLayoutPanel _planCardsPanel;
        private readonly Guna2Panel _recentSalesContainer;
        private readonly FlowLayoutPanel _recentSalesPanel;
        private readonly Guna2Panel _bottomPanel;
        private readonly Guna2Button _manageSalesButton;

        public SystemSalesModule()
        {
            BackColor = Color.PaleTurquoise;
            Dock = DockStyle.Fill;

            _headerPanel = CreateHeaderPanel();
            _viewAllButton = CreateViewAllButton();
            _planCardsPanel = CreatePlanCardsPanel();
            _recentSalesContainer = CreateRecentSalesContainer();
            _recentSalesPanel = CreateRecentSalesPanel();
            _bottomPanel = CreateBottomPanel();
            _manageSalesButton = CreateManageSalesButton();

            _headerPanel.Controls.Add(_viewAllButton);
            _recentSalesContainer.Controls.Add(_recentSalesPanel);
            _bottomPanel.Controls.Add(_manageSalesButton);
            Controls.Add(_bottomPanel);
            Controls.Add(_recentSalesContainer);
            Controls.Add(_planCardsPanel);
            Controls.Add(_headerPanel);

            BuildPlanCards();
            BuildRecentSalesRows();
        }

        private Guna2Panel CreateHeaderPanel()
        {
            var panel = new Guna2Panel
            {
                BorderColor = Color.Black,
                BorderRadius = 12,
                BorderThickness = 1,
                Dock = DockStyle.Top,
                FillColor = Color.White,
                Height = 85,
                Padding = new Padding(24, 20, 24, 20)
            };

            var title = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 18F, FontStyle.Regular),
                Location = new Point(24, 20),
                Text = "System Sales"
            };

            panel.Controls.Add(title);
            return panel;
        }

        private Guna2Button CreateViewAllButton()
        {
            var button = new Guna2Button
            {
                AutoRoundedCorners = true,
                BorderRadius = 18,
                FillColor = Color.PaleTurquoise,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.Black,
                Location = new Point(760, 18),
                Size = new Size(120, 38),
                Text = "View All"
            };

            button.Click += ManageSalesButton_Click;

            return button;
        }

        private FlowLayoutPanel CreatePlanCardsPanel()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 190,
                Padding = new Padding(16, 18, 16, 8),
                WrapContents = false,
                BackColor = Color.Transparent
            };
        }

        private Guna2Panel CreateRecentSalesContainer()
        {
            return new Guna2Panel
            {
                Dock = DockStyle.Fill,
                FillColor = Color.Transparent,
                AutoScroll = true,
                Padding = new Padding(0)
            };
        }

        private FlowLayoutPanel CreateRecentSalesPanel()
        {
            return new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(16, 10, 16, 10),
                WrapContents = false,
                BackColor = Color.Transparent
            };
        }

        private Guna2Panel CreateBottomPanel()
        {
            return new Guna2Panel
            {
                Dock = DockStyle.Bottom,
                Height = 78,
                FillColor = Color.Transparent,
                Padding = new Padding(16, 10, 16, 12)
            };
        }

        private Guna2Button CreateManageSalesButton()
        {
            var button = new Guna2Button
            {
                AutoRoundedCorners = true,
                BorderRadius = 24,
                Dock = DockStyle.Fill,
                FillColor = Color.MediumTurquoise,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                ForeColor = Color.Black,
                Margin = new Padding(0),
                Text = "Manage Sales"
            };

            button.Click += ManageSalesButton_Click;
            return button;
        }

        private void BuildPlanCards()
        {
            _planCardsPanel.Controls.Add(CreatePlanCard("Basic Plan", "$500"));
            _planCardsPanel.Controls.Add(CreatePlanCard("Pro Plan", "$2,000"));
            _planCardsPanel.Controls.Add(CreatePlanCard("Enterprise Plan", "$5,000"));
        }

        private void BuildRecentSalesRows()
        {
            _recentSalesPanel.Controls.Add(CreateSectionTitle("Recent Sales"));
            _recentSalesPanel.Controls.Add(CreateSaleRow("Tech Corp", "Enterprise Plan", "$5000", true));
            _recentSalesPanel.Controls.Add(CreateSaleRow("StartUp Inc", "Professional Plan", "$2000", true));
            _recentSalesPanel.Controls.Add(CreateSaleRow("Small Biz", "Basic Plan", "$500", false));
        }

        private Control CreateSectionTitle(string text)
        {
            return new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 16F, FontStyle.Regular),
                Margin = new Padding(8, 0, 3, 10),
                Text = text,
                AutoSize = true
            };
        }

        private Guna2Panel CreatePlanCard(string planName, string price)
        {
            var card = new Guna2Panel
            {
                BorderRadius = 18,
                FillColor = Color.White,
                Size = new Size(275, 130),
                Margin = new Padding(8, 0, 8, 0),
                ShadowDecoration = { Enabled = true }
            };

            var title = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 14F, FontStyle.Regular),
                Location = new Point(20, 24),
                Text = planName
            };

            var amount = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 22F, FontStyle.Regular),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(20, 62),
                Text = price
            };

            card.Controls.Add(amount);
            card.Controls.Add(title);
            return card;
        }

        private Guna2Panel CreateSaleRow(string company, string planType, string amount, bool success)
        {
            var row = new Guna2Panel
            {
                BorderRadius = 14,
                FillColor = Color.White,
                Margin = new Padding(8, 0, 8, 10),
                Size = new Size(850, 82),
                ShadowDecoration = { Enabled = true }
            };

            var companyLabel = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                Location = new Point(20, 16),
                Text = company
            };

            var planLabel = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.DimGray,
                Location = new Point(20, 46),
                Text = planType
            };

            var amountLabel = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                Location = new Point(560, 26),
                Text = amount
            };

            var statusBadge = new Guna2Button
            {
                AutoRoundedCorners = true,
                BorderRadius = 16,
                Enabled = false,
                FillColor = success ? Color.Honeydew : Color.LemonChiffon,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = success ? Color.SeaGreen : Color.DarkGoldenrod,
                Location = new Point(710, 22),
                Size = new Size(110, 34),
                Text = success ? "success" : "pending"
            };

            row.Controls.Add(statusBadge);
            row.Controls.Add(amountLabel);
            row.Controls.Add(planLabel);
            row.Controls.Add(companyLabel);
            return row;
        }

        private void ManageSalesButton_Click(object sender, EventArgs e)
        {
            using var manager = new SystemSalesManager();
            manager.ShowDialog(FindForm());
        }
    }
}
