using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace prajeknibai.views.models
{
    public partial class ucpayroll : UserControl
    {
        public ucpayroll()
        {
            InitializeComponent();
            Load += ucpayroll_Load;
            Resize += ucpayroll_Resize;
        }

        private void ucpayroll_Load(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutPayroll));
        }

        private void ucpayroll_Resize(object sender, EventArgs e)
        {
            BeginInvoke(new Action(LayoutPayroll));
        }

        private void pnlEmployeesList_Paint(object sender, PaintEventArgs e)
        {

        }

        private void LayoutPayroll()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            SuspendLayout();

            const int outerMargin = 20;
            const int topGap = 18;
            const int cardGap = 18;

            var topCardWidth = (ClientSize.Width - (outerMargin * 2) - cardGap) / 2;
            guna2Panel1.Size = new Size(topCardWidth, guna2Panel1.Height);
            guna2Panel2.Location = new Point(guna2Panel1.Right + cardGap, guna2Panel2.Location.Y);
            guna2Panel2.Size = new Size(topCardWidth, guna2Panel2.Height);

            guna2PictureBox1.Location = new Point(guna2Panel1.Width - guna2PictureBox1.Width - 30, guna2PictureBox1.Location.Y);
            pnlMonthlyPayroll.Location = new Point(guna2Panel2.Width - pnlMonthlyPayroll.Width - 30, pnlMonthlyPayroll.Location.Y);

            pnlEmployeesList.Location = new Point(outerMargin, guna2Panel1.Bottom + topGap);
            pnlEmployeesList.Size = new Size(ClientSize.Width - (outerMargin * 2), ClientSize.Height - pnlEmployeesList.Top - outerMargin);

            LayoutPayrollEmployeeRow(guna2HtmlLabel9, guna2HtmlLabel10);
            LayoutPayrollEmployeeRow(guna2HtmlLabel13, guna2HtmlLabel14);
            LayoutPayrollEmployeeRow(guna2HtmlLabel17, guna2HtmlLabel18);

            ResumeLayout();
        }

        private void LayoutPayrollEmployeeRow(Control amountLabel, Control suffixLabel)
        {
            var rightEdge = pnlEmployeesList.ClientSize.Width - 22;
            suffixLabel.Location = new Point(rightEdge - suffixLabel.Width, suffixLabel.Location.Y);
            amountLabel.Location = new Point(rightEdge - amountLabel.Width, amountLabel.Location.Y);
        }
    }
}
