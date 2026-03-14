namespace prajeknibai
{
    partial class SystemSalesManager
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            panelHeader = new Guna.UI2.WinForms.Guna2Panel();
            btnClose = new Guna.UI2.WinForms.Guna2Button();
            lblTitle = new Guna.UI2.WinForms.Guna2HtmlLabel();
            panelContent = new Guna.UI2.WinForms.Guna2Panel();
            lblSubtitle = new Guna.UI2.WinForms.Guna2HtmlLabel();
            panelHeader.SuspendLayout();
            panelContent.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BorderColor = System.Drawing.Color.Black;
            panelHeader.BorderRadius = 12;
            panelHeader.BorderThickness = 1;
            panelHeader.Controls.Add(btnClose);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.CustomizableEdges = customizableEdges1;
            panelHeader.Dock = DockStyle.Top;
            panelHeader.FillColor = System.Drawing.Color.White;
            panelHeader.Location = new System.Drawing.Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.ShadowDecoration.CustomizableEdges = customizableEdges2;
            panelHeader.Size = new System.Drawing.Size(900, 80);
            panelHeader.TabIndex = 0;
            // 
            // btnClose
            // 
            btnClose.AutoRoundedCorners = true;
            btnClose.BorderRadius = 21;
            btnClose.CustomizableEdges = customizableEdges3;
            btnClose.FillColor = System.Drawing.Color.PaleTurquoise;
            btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnClose.ForeColor = System.Drawing.Color.Black;
            btnClose.Location = new System.Drawing.Point(746, 19);
            btnClose.Name = "btnClose";
            btnClose.ShadowDecoration.CustomizableEdges = customizableEdges4;
            btnClose.Size = new System.Drawing.Size(130, 45);
            btnClose.TabIndex = 1;
            btnClose.Text = "Close";
            btnClose.Click += btnClose_Click;
            // 
            // lblTitle
            // 
            lblTitle.BackColor = System.Drawing.Color.Transparent;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            lblTitle.Location = new System.Drawing.Point(24, 18);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(254, 43);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "System Sales Manager";
            // 
            // panelContent
            // 
            panelContent.BorderRadius = 12;
            panelContent.Controls.Add(lblSubtitle);
            panelContent.CustomizableEdges = customizableEdges5;
            panelContent.FillColor = System.Drawing.Color.White;
            panelContent.Location = new System.Drawing.Point(24, 103);
            panelContent.Name = "panelContent";
            panelContent.ShadowDecoration.CustomizableEdges = customizableEdges6;
            panelContent.Size = new System.Drawing.Size(852, 365);
            panelContent.TabIndex = 1;
            // 
            // lblSubtitle
            // 
            lblSubtitle.BackColor = System.Drawing.Color.Transparent;
            lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblSubtitle.Location = new System.Drawing.Point(28, 31);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new System.Drawing.Size(452, 30);
            lblSubtitle.TabIndex = 0;
            lblSubtitle.Text = "Manage sales plans, clients, and transactions from here.";
            // 
            // SystemSalesManager
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = System.Drawing.Color.PaleTurquoise;
            ClientSize = new System.Drawing.Size(900, 500);
            Controls.Add(panelContent);
            Controls.Add(panelHeader);
            Name = "SystemSalesManager";
            StartPosition = FormStartPosition.CenterParent;
            Text = "SystemSalesManager";
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelContent.ResumeLayout(false);
            panelContent.PerformLayout();
            ResumeLayout(false);
        }

        private Guna.UI2.WinForms.Guna2Panel panelHeader;
        private Guna.UI2.WinForms.Guna2Button btnClose;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblTitle;
        private Guna.UI2.WinForms.Guna2Panel panelContent;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblSubtitle;
    }
}
