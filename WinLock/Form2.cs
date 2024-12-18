﻿using System;
using System.Windows.Forms;

namespace WinLock
{
    public partial class Form2 : Form
    {
        private const string password = "0000"; // Mot de passe à définir

        public Form2()
        {
            InitializeComponent();
        }

        private void btnUnlock_Click(object sender, EventArgs e)
        {
            if (txtPassword.Text == password)
            {
                this.Close(); // Ferme la fenêtre si le mot de passe est correct
            }
            else
            {
                MessageBox.Show("Mot de passe incorrect", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnUnlock = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.txtPassword.Location = new System.Drawing.Point(12, 12);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(260, 20);
            this.txtPassword.TabIndex = 0;
            this.txtPassword.UseSystemPasswordChar = true;

            this.btnUnlock.Location = new System.Drawing.Point(197, 38);
            this.btnUnlock.Name = "btnUnlock";
            this.btnUnlock.Size = new System.Drawing.Size(75, 23);
            this.btnUnlock.TabIndex = 1;
            this.btnUnlock.Text = "Déverrouiller";
            this.btnUnlock.UseVisualStyleBackColor = true;
            this.btnUnlock.Click += new System.EventHandler(this.btnUnlock_Click);

            this.ClientSize = new System.Drawing.Size(284, 71);
            this.Controls.Add(this.btnUnlock);
            this.Controls.Add(this.txtPassword);
            this.Name = "Form2";
            this.Text = "Déverrouillage";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnUnlock;
    }
}
