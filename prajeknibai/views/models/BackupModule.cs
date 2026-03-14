using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using prajeknibai.controller;

namespace prajeknibai.views.models
{
    public class BackupModule : UserControl
    {
        private readonly Guna2HtmlLabel _statusValueLabel;
        private readonly Guna2HtmlLabel _lastBackupValueLabel;
        private readonly FlowLayoutPanel _historyPanel;

        public BackupModule()
        {
            BackColor = Color.PaleTurquoise;
            Dock = DockStyle.Fill;

            var headerPanel = CreateHeaderPanel();
            var contentPanel = CreateContentPanel(out _statusValueLabel, out _lastBackupValueLabel, out _historyPanel);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);

            RefreshHistory();
        }

        private static Guna2Panel CreateHeaderPanel()
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
                Text = "Backup"
            };

            var subtitle = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.DimGray,
                Location = new Point(26, 52),
                Text = "Create a database backup for the whole system"
            };

            panel.Controls.Add(subtitle);
            panel.Controls.Add(title);
            return panel;
        }

        private Guna2Panel CreateContentPanel(out Guna2HtmlLabel statusValueLabel, out Guna2HtmlLabel lastBackupValueLabel, out FlowLayoutPanel historyPanel)
        {
            var panel = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                FillColor = Color.Transparent,
                Padding = new Padding(20)
            };

            var summaryCard = new Guna2Panel
            {
                BorderRadius = 16,
                FillColor = Color.White,
                Location = new Point(20, 20),
                Size = new Size(900, 180),
                ShadowDecoration = { Enabled = true }
            };

            var titleLabel = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 15F, FontStyle.Regular),
                Location = new Point(24, 22),
                Text = "Database Backup Center"
            };

            var descriptionLabel = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.DimGray,
                Location = new Point(24, 58),
                Text = "Save a .bak file of `prajeknibai_db` to keep a secure copy of your system data."
            };

            var createBackupButton = new Guna2Button
            {
                AutoRoundedCorners = true,
                BorderRadius = 21,
                FillColor = Color.MediumTurquoise,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.Black,
                Location = new Point(24, 115),
                Size = new Size(210, 45),
                Text = "Create Backup"
            };
            createBackupButton.Click += CreateBackupButton_Click;

            var statusTitleLabel = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Location = new Point(310, 109),
                Text = "Status"
            };

            statusValueLabel = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.DimGray,
                Location = new Point(310, 136),
                Text = "No backup created yet"
            };

            var lastBackupTitleLabel = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Location = new Point(590, 109),
                Text = "Last File"
            };

            lastBackupValueLabel = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.DimGray,
                Location = new Point(590, 136),
                Text = "No file yet"
            };

            summaryCard.Controls.Add(lastBackupValueLabel);
            summaryCard.Controls.Add(lastBackupTitleLabel);
            summaryCard.Controls.Add(statusValueLabel);
            summaryCard.Controls.Add(statusTitleLabel);
            summaryCard.Controls.Add(createBackupButton);
            summaryCard.Controls.Add(descriptionLabel);
            summaryCard.Controls.Add(titleLabel);

            var infoCard = new Guna2Panel
            {
                BorderRadius = 16,
                FillColor = Color.White,
                Location = new Point(20, 220),
                Size = new Size(900, 330),
                ShadowDecoration = { Enabled = true }
            };

            var infoTitle = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 15F, FontStyle.Regular),
                Location = new Point(24, 22),
                Text = "Backup Notes"
            };

            var infoText = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.DimGray,
                Location = new Point(24, 62),
                Text = "• Use this before major system changes.\n• Save backups in a secure folder or external drive.\n• The backup file can be restored later from SQL Server tools."
            };

            var historyTitle = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 13F, FontStyle.Regular),
                Location = new Point(24, 135),
                Text = "Backup History"
            };

            historyPanel = new FlowLayoutPanel
            {
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                Location = new Point(24, 170),
                Name = "historyPanel",
                Size = new Size(852, 138),
                WrapContents = false
            };

            infoCard.Controls.Add(historyPanel);
            infoCard.Controls.Add(historyTitle);
            infoCard.Controls.Add(infoText);
            infoCard.Controls.Add(infoTitle);

            panel.Controls.Add(infoCard);
            panel.Controls.Add(summaryCard);

            return panel;
        }

        private void CreateBackupButton_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Backup Files (*.bak)|*.bak",
                FileName = $"prajeknibai_db_{DateTime.Now:yyyyMMdd_HHmmss}.bak"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                AppDatabase.BackupDatabase(dialog.FileName);
                var fileName = Path.GetFileName(dialog.FileName);
                var createdAt = DateTime.Now;

                BackupHistoryRepository.AddBackupHistory(fileName, dialog.FileName, createdAt);
                _statusValueLabel.Text = "Backup created successfully";
                _statusValueLabel.ForeColor = Color.SeaGreen;
                _lastBackupValueLabel.Text = fileName;
                _lastBackupValueLabel.ForeColor = Color.DimGray;
                RefreshHistory();
                MessageBox.Show("Database backup created successfully.", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _statusValueLabel.Text = "Backup failed";
                _statusValueLabel.ForeColor = Color.Firebrick;
                _lastBackupValueLabel.Text = "No file yet";
                MessageBox.Show($"Unable to create backup.\n\n{ex.Message}", "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshHistory()
        {
            _historyPanel.Controls.Clear();

            IReadOnlyList<BackupHistoryEntry> history;
            try
            {
                history = BackupHistoryRepository.GetBackupHistory();
            }
            catch (Exception ex)
            {
                _statusValueLabel.Text = "Unable to load history";
                _statusValueLabel.ForeColor = Color.Firebrick;
                _historyPanel.Controls.Add(new Guna2HtmlLabel
                {
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                    ForeColor = Color.Firebrick,
                    Margin = new Padding(3, 3, 3, 10),
                    Text = $"Unable to load backup history. {ex.Message}"
                });
                return;
            }

            if (history.Count > 0)
            {
                _lastBackupValueLabel.Text = history[0].FileName;
                _lastBackupValueLabel.ForeColor = Color.DimGray;
            }

            if (history.Count == 0)
            {
                _historyPanel.Controls.Add(new Guna2HtmlLabel
                {
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                    ForeColor = Color.DimGray,
                    Margin = new Padding(3, 3, 3, 10),
                    Text = "No backup history yet."
                });
                return;
            }

            foreach (var entry in history)
            {
                _historyPanel.Controls.Add(CreateHistoryRow(entry));
            }
        }

        private static Guna2Panel CreateHistoryRow(BackupHistoryEntry entry)
        {
            var row = new Guna2Panel
            {
                BorderRadius = 12,
                FillColor = Color.WhiteSmoke,
                Margin = new Padding(0, 0, 0, 8),
                Size = new Size(828, 54)
            };

            var fileNameLabel = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Location = new Point(16, 8),
                Text = entry.FileName
            };

            var detailsLabel = new Guna2HtmlLabel
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.DimGray,
                Location = new Point(16, 28),
                Text = $"{entry.CreatedAt:g} • {entry.FullPath}"
            };

            row.Controls.Add(detailsLabel);
            row.Controls.Add(fileNameLabel);
            return row;
        }

    }
}
