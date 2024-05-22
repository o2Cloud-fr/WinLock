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
        private const int WH_KEYBOARD_LL = 13; // Ajout de la constante
        private const int WM_KEYDOWN = 0x0100; // Correction de la constante
        private const int WM_SYSKEYDOWN = 0x0104; // Correction de la constante
        private const string correctPassword = "0000"; // Change to your desired password

        // Import necessary functions from user32.dll
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
        private Label labelInstruction; // Added label for instruction

        public Form1()
        {
            InitializeComponent();
            InitializeComponents();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.Opacity = 0.8; // Set the transparency to 80%

            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private void InitializeComponents()
        {
            labelInstruction = new Label(); // Initialize label for instruction
            labelInstruction.Text = "Enter password to unlock:"; // Set instruction text
            labelInstruction.Location = new System.Drawing.Point(300, 170); // Adjust position as needed
            labelInstruction.AutoSize = true;

            textBoxPassword = new TextBox();
            buttonSubmit = new Button();

            textBoxPassword.Location = new System.Drawing.Point(300, 200); // Adjust position as needed
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.PasswordChar = '*';
            textBoxPassword.Size = new System.Drawing.Size(200, 20);
            textBoxPassword.TabIndex = 0;

            buttonSubmit.Location = new System.Drawing.Point(510, 200); // Adjust position as needed
            buttonSubmit.Name = "buttonSubmit";
            buttonSubmit.Size = new System.Drawing.Size(75, 23);
            buttonSubmit.TabIndex = 1;
            buttonSubmit.Text = "Submit";
            buttonSubmit.UseVisualStyleBackColor = true;
            buttonSubmit.Click += new System.EventHandler(this.buttonSubmit_Click);

            this.Controls.Add(labelInstruction); // Add label to form
            this.Controls.Add(textBoxPassword);
            this.Controls.Add(buttonSubmit);
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

                // Check if the key is a numeric key or a numpad key
                if ((vkCode >= (int)Keys.D0 && vkCode <= (int)Keys.D9) || // Numeric keys
                    (vkCode >= (int)Keys.NumPad0 && vkCode <= (int)Keys.NumPad9)) // Numpad keys
                {
                    // Allow numeric and numpad keys
                    return CallNextHookEx(_hookID, nCode, wParam, lParam);
                }
                else
                {
                    // Block all other keys
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.SuspendLayout();
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            // Optionally, initialize additional components here
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }

        private async void buttonSubmit_Click(object sender, EventArgs e)
        {
            if (textBoxPassword.Text == correctPassword)
            {
                UnhookWindowsHookEx(_hookID);
                this.Close(); // Close the form if the password is correct
            }
            else
            {
                MessageBox.Show("Incorrect Password. Try again.");

                try
                {
                    // Configure FluentEmail pour utiliser SMTP avec authentification
                    Email.DefaultSender = new SmtpSender(new SmtpClient("smtp.server.com")
                    {
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential("email@erver.com", "P@ssw0rd"),
                        EnableSsl = true // Activer SSL si nécessaire
                    });

                    // Créez le message e-mail
                    var email = Email
                        .From("email@erver.com")
                        .To("email@erver.com")
                        .Subject("Subject")
                        .Body($"Le mot de passe incorrect a été saisi lors de la tentative de déverrouillage du système.");

                    // Envoyez l'e-mail
                    await email.SendAsync();

                   //MessageBox.Show("L'e-mail a été envoyé avec succès !", "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show($"Une erreur s'est produite lors de l'envoi de l'e-mail : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                textBoxPassword.Clear();
                textBoxPassword.Focus();
            }
        }
    }
}
