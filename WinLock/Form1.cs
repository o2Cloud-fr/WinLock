using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Net;
using FluentEmail.Core;
using FluentEmail.Smtp;
using System.Net.Mail;

namespace WinLock
{
    public partial class Form1 : Form
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const string correctPassword = "0000";

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        private TextBox textBoxPassword;
        private Button buttonSubmit;
        private Label labelInstruction;
        private Label labelStatus; // Added label for status message

        public Form1()
        {
            InitializeComponents();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.Opacity = 0.9; // Increase transparency for modern feel
            this.BackColor = System.Drawing.Color.FromArgb(34, 34, 34); // Dark background color

            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private void InitializeComponents()
        {
            labelInstruction = new Label();
            labelInstruction.Text = "Enter password to unlock:";
            labelInstruction.Location = new System.Drawing.Point(300, 170);
            labelInstruction.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Regular); // Modern font
            labelInstruction.ForeColor = System.Drawing.Color.White; // White text color
            labelInstruction.AutoSize = true;

            textBoxPassword = new TextBox();
            buttonSubmit = new Button();
            labelStatus = new Label(); // Initialize the status label

            textBoxPassword.Location = new System.Drawing.Point(300, 200);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.PasswordChar = '*';
            textBoxPassword.Size = new System.Drawing.Size(250, 30); // Adjusted size
            textBoxPassword.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Regular); // Modern font
            textBoxPassword.TabIndex = 0;
            textBoxPassword.ForeColor = System.Drawing.Color.White;
            textBoxPassword.BackColor = System.Drawing.Color.FromArgb(50, 50, 50); // Dark background for the textbox

            buttonSubmit.Location = new System.Drawing.Point(570, 200);
            buttonSubmit.Name = "buttonSubmit";
            buttonSubmit.Size = new System.Drawing.Size(100, 35); // Adjusted button size
            buttonSubmit.TabIndex = 1;
            buttonSubmit.Text = "Submit";
            buttonSubmit.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Regular); // Modern font
            buttonSubmit.ForeColor = System.Drawing.Color.White;
            buttonSubmit.BackColor = System.Drawing.Color.FromArgb(75, 75, 75); // Dark background for button
            buttonSubmit.FlatStyle = FlatStyle.Flat;
            buttonSubmit.FlatAppearance.BorderSize = 0; // Remove button border for sleek look
            buttonSubmit.UseVisualStyleBackColor = true;
            buttonSubmit.Click += new System.EventHandler(this.buttonSubmit_Click);

            // Set up status label
            labelStatus.Location = new System.Drawing.Point(300, 240); // Positioned below the password box
            labelStatus.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Italic); // Modern font, italic style
            labelStatus.ForeColor = System.Drawing.Color.White; // White text for status message
            labelStatus.AutoSize = true;
            labelStatus.Visible = false; // Initially hidden

            this.Controls.Add(labelInstruction);
            this.Controls.Add(textBoxPassword);
            this.Controls.Add(buttonSubmit);
            this.Controls.Add(labelStatus); // Add the status label to the form
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if ((vkCode >= (int)Keys.D0 && vkCode <= (int)Keys.D9) ||
                    (vkCode >= (int)Keys.NumPad0 && vkCode <= (int)Keys.NumPad9))
                {
                    return CallNextHookEx(_hookID, nCode, wParam, lParam);
                }
                else
                {
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }

        private async void buttonSubmit_Click(object sender, EventArgs e)
        {
            // Désactive le bouton pour éviter plusieurs clics pendant l'envoi de l'email
            buttonSubmit.Enabled = false;

            // Affiche le message "Please wait while the notification is being sent to the administrator"
            labelStatus.Text = "Please wait while the notification is being sent to the administrator...";
            labelStatus.Visible = true;

            if (textBoxPassword.Text == correctPassword)
            {
                UnhookWindowsHookEx(_hookID);
                this.Close();
            }
            else
            {
                MessageBox.Show("Incorrect Password. Try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try
                {
                    Email.DefaultSender = new SmtpSender(new SmtpClient("smtp.server.com")
                    {
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential("email@server.com", "P@ssw0rd"),
                        EnableSsl = true
                    });

                    var email = Email
                        .From("email@server.com")
                        .To("email@server.com")
                        .Subject("WinLock - o2Cloud")
                        .Body($"The incorrect password was entered during the unlock attempt.");

                    var sendResult = await email.SendAsync();

                    if (sendResult.Successful)
                    {
                        MessageBox.Show("Email sent successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to send email. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error sending email: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                textBoxPassword.Clear();
                textBoxPassword.Focus();
            }

            // Réactive le bouton après l'envoi de l'email
            buttonSubmit.Enabled = true;

            // Cache le message une fois l'envoi terminé
            labelStatus.Visible = false;
        }
    }
}
