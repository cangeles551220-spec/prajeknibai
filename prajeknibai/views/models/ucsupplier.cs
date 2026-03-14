using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using prajeknibai.controller;

namespace prajeknibai.views.models
{
    public partial class ucsupplier : UserControl
    {
        private bool _suppliersLoaded;

        public ucsupplier()
        {
            InitializeComponent();
            guna2Button1.Click += guna2Button1_Click;
            Load += ucsupplier_Load;
            Resize += ucsupplier_Resize;
        }

        private void ucsupplier_Load(object sender, EventArgs e)
        {
            if (_suppliersLoaded)
            {
                return;
            }

            _suppliersLoaded = true;
            LoadSuppliers();
            BeginInvoke(new Action(LayoutSuppliers));
        }

        private void ucsupplier_Resize(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutSuppliers));
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            var supplierName = guna2TextBox1.Text.Trim();
            var contactNumber = guna2TextBox2.Text.Trim();

            if (string.IsNullOrWhiteSpace(supplierName) || string.IsNullOrWhiteSpace(contactNumber))
            {
                MessageBox.Show("Please enter supplier name and contact number.", "Add Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!SupplierRepository.AddSupplier(supplierName, contactNumber))
                {
                    MessageBox.Show("That supplier already exists in the database.", "Add Supplier", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                guna2TextBox1.Clear();
                guna2TextBox2.Clear();
                LoadSuppliers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save supplier.\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                var suppliers = SupplierRepository.GetSuppliers();
                flowLayoutPanel1.Controls.Clear();

                foreach (var supplier in suppliers)
                {
                    flowLayoutPanel1.Controls.Add(CreateSupplierPanel(supplier.Name, supplier.ContactNumber));
                }

                LayoutSuppliers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load suppliers.\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Guna.UI2.WinForms.Guna2Panel CreateSupplierPanel(string supplierName, string contactNumber)
        {
            var panel = new Guna.UI2.WinForms.Guna2Panel
            {
                Size = new Size(880, 70)
            };

            var nameLabel = new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0),
                Location = new Point(15, 12),
                Text = supplierName
            };

            var contactLabel = new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(110, 120, 135),
                Location = new Point(15, 38),
                Text = contactNumber
            };

            var detailsLink = new LinkLabel
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0),
                Location = new Point(760, 22),
                Text = "View Details",
                TabStop = true,
                LinkBehavior = LinkBehavior.HoverUnderline,
                LinkColor = Color.FromArgb(37, 99, 235),
                ActiveLinkColor = Color.FromArgb(29, 78, 216),
                VisitedLinkColor = Color.FromArgb(37, 99, 235)
            };
            detailsLink.LinkClicked += (sender, e) =>
                MessageBox.Show($"Supplier: {supplierName}\nContact: {contactNumber}", "Supplier Details", MessageBoxButtons.OK, MessageBoxIcon.Information);

            panel.Controls.Add(detailsLink);
            panel.Controls.Add(contactLabel);
            panel.Controls.Add(nameLabel);

            detailsLink.BringToFront();

            return panel;
        }

        private void LayoutSuppliers()
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
                    if (child is LinkLabel link)
                    {
                        link.Location = new Point(panel.Width - link.Width - 20, link.Location.Y);
                    }
                }
            }
        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
