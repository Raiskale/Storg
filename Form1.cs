using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Svg;
using System.Security.Cryptography;

namespace Salasanakone
{
    public partial class Storg : Form
    {
        // Enryptio
        public static byte[] aesKey;

        public Storg()
        {
            InitializeComponent();

            if (!AskMasterKey())
            {
                MessageBox.Show("Application will close because no valid master key was entered.");
                Application.Exit();
            }
        }

        private bool AskMasterKey()
        {
            string masterKeyFile = Path.Combine(Application.StartupPath, "masterkey.txt");

            if (!File.Exists(masterKeyFile))
            {
                // Asking user to set masterkey
                for (int i = 0; i < 3; i++)
                {
                    string masterPassword = Prompt.ShowDialog("Create master password:", "Master Password");
                    if (!string.IsNullOrEmpty(masterPassword))
                    {
                        using (SHA256 sha = SHA256.Create())
                        {
                            aesKey = sha.ComputeHash(Encoding.UTF8.GetBytes(masterPassword));
                            File.WriteAllText(masterKeyFile, Convert.ToBase64String(aesKey));
                        }
                        return true;
                    }
                    MessageBox.Show("Master password cannot be empty!");
                }
                return false; // 3 empty inputs
            }
            else
            {
                // if masterkey set, try to verify it 
                string storedHash = File.ReadAllText(masterKeyFile);
                for (int attempts = 0; attempts < 3; attempts++)
                {
                    string currentMasterPassword = Prompt.ShowDialog("Enter Master key:", "Master Password");

                    if (string.IsNullOrEmpty(currentMasterPassword))
                    {
                        MessageBox.Show("Master key cannot be empty!");
                        continue;
                    }

                    using (SHA256 sha = SHA256.Create())
                    {
                        byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(currentMasterPassword));
                        string hash = Convert.ToBase64String(hashBytes);

                        if (hash == storedHash)
                        {
                            aesKey = hashBytes;
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Incorrect master key!");
                        }
                    }
                }
                return false; // 3 wrong input
            }
        }



        private void LoadSvgToPictureBox(string svgPath, PictureBox pb)
        {
            SvgDocument svgDoc = SvgDocument.Open(svgPath);
            Bitmap bmp = svgDoc.Draw();
            pb.Image = bmp;
        }

        public class PasswordInfo
        {
            public string Site { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Category { get; set; }
        }

        private List<PasswordInfo> passwords = new List<PasswordInfo>();
        // passwordien json
        private string passwordFile = Path.Combine(Application.StartupPath, "passwords.json");
        
        private void LoadPasswords()
        {
            if (File.Exists(passwordFile))
            {
                string json = File.ReadAllText(passwordFile);
                passwords = JsonSerializer.Deserialize<List<PasswordInfo>>(json);

                foreach (var p in passwords)
                    if (!string.IsNullOrEmpty(p.Password))
                        p.Password = DecryptString(p.Password, aesKey);
            }

            listBox1.Items.Clear();
            foreach (var p in passwords)
                listBox1.Items.Add($"{p.Site} - {p.Username}");
        }
        // saving
        private void SavePasswords()
        {
            var encryptedPasswords = passwords.Select(p => new PasswordInfo
            {
                Site = p.Site,
                Username = p.Username,
                Password = string.IsNullOrEmpty(p.Password) ? "" : EncryptString(p.Password, aesKey),
                Category = p.Category
            }).ToList();

            string json = JsonSerializer.Serialize(encryptedPasswords, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(passwordFile, json);
        }

        private string EncryptString(string plainText, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(bytes, 0, bytes.Length);
                        cs.FlushFinalBlock();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private string DecryptString(string cipherText, byte[] key)
        {
            try
            {
                if (string.IsNullOrEmpty(cipherText))
                    return null;

                byte[] fullCipher = Convert.FromBase64String(cipherText);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    byte[] iv = new byte[16];
                    Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    using (MemoryStream ms = new MemoryStream())
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(fullCipher, iv.Length, fullCipher.Length - iv.Length);
                        cs.FlushFinalBlock();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private bool passwordVisible = false;

        private void Storg_Load(object sender, EventArgs e)
        {
            LoadPasswords();

            if (passwords.Count > 0)
            {
                listBox1.SelectedIndex = 0;
                var first = passwords[0];
                label4.Text = first.Site;
                label6.Text = first.Username;
                label8.Text = string.IsNullOrEmpty(first.Password) ? "****" : new string('*', first.Password.Length);
                label10.Text = first.Category;
                passwordVisible = false;
            }
            else
            {
                label4.Text = "-";
                label6.Text = "-";
                label8.Text = "-";
                label10.Text = "-";
            }
        }

        // empty events
        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e) { }
        private void pictureBox5_Click(object sender, EventArgs e) { }
        private void pictureBox3_Click_1(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void pictureBox4_Click(object sender, EventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }
        private void label8_Click(object sender, EventArgs e) { }
        private void label10_Click(object sender, EventArgs e) { }

        private void button1_Click(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index >= 0 && index < passwords.Count)
            {
                var selected = passwords[index];
                if (passwordVisible)
                {
                    label8.Text = new string('*', selected.Password.Length);
                    passwordVisible = false;
                }
                else
                {
                    label8.Text = selected.Password;
                    passwordVisible = true;
                }
            }
        }
        // + button to add passowrds
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Form inputForm = new Form()
            {
                Width = 300,
                Height = 260,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Add a password.",
                StartPosition = FormStartPosition.CenterParent
            };

            Label lblSite = new Label() { Left = 10, Top = 20, Text = "Site/App:" };
            TextBox txtSite = new TextBox() { Left = 10, Top = 40, Width = 260 };
            Label lblUsername = new Label() { Left = 10, Top = 70, Text = "Username/Gmail:" };
            TextBox txtUsername = new TextBox() { Left = 10, Top = 90, Width = 260 };
            Label lblPassword = new Label() { Left = 10, Top = 120, Text = "Password:" };
            TextBox txtPassword = new TextBox() { Left = 10, Top = 140, Width = 260 };
            Label lblCategory = new Label() { Left = 10, Top = 170, Text = "Category:" };
            TextBox txtCategory = new TextBox() { Left = 10, Top = 190, Width = 260 };
            Button btnOK = new Button() { Text = "OK", Left = 50, Width = 80, Top = 220, DialogResult = DialogResult.OK };
            Button btnCancel = new Button() { Text = "Cancel", Left = 150, Width = 80, Top = 220, DialogResult = DialogResult.Cancel };

            inputForm.Controls.AddRange(new Control[] { lblSite, txtSite, lblUsername, txtUsername, lblPassword, txtPassword, lblCategory, txtCategory, btnOK, btnCancel });
            inputForm.AcceptButton = btnOK;
            inputForm.CancelButton = btnCancel;

            if (inputForm.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(txtSite.Text) && !string.IsNullOrEmpty(txtUsername.Text))
                {
                    PasswordInfo newItem = new PasswordInfo
                    {
                        Site = txtSite.Text,
                        Username = txtUsername.Text,
                        Password = txtPassword.Text,
                        Category = string.IsNullOrEmpty(txtCategory.Text) ? "Else" : txtCategory.Text
                    };

                    passwords.Add(newItem);
                    SavePasswords();
                    listBox1.Items.Add($"{newItem.Site} - {newItem.Username}");
                    MessageBox.Show("Password saved!");
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index >= 0 && index < passwords.Count)
            {
                var selected = passwords[index];
                label4.Text = selected.Site;
                label6.Text = selected.Username;
                label8.Text = string.IsNullOrEmpty(selected.Password) ? "****" : new string('*', selected.Password.Length);
                label10.Text = selected.Category;
                passwordVisible = false;
            }
        }
        // copying to clipboard
        private void button2_Click(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index >= 0 && index < passwords.Count)
            {
                string pw = passwords[index].Password;
                if (!string.IsNullOrEmpty(pw))
                {
                    Clipboard.SetText(pw);
                    MessageBox.Show("Password copied to clipboard!");
                }
                else
                    MessageBox.Show("It doesn't have a password yet.");
            }
            else
                MessageBox.Show("Select a password first!.");
        }
        // settings
        private void pictureBox7_Click(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0 || index >= passwords.Count)
            {
                MessageBox.Show("Choose a password first.");
                return;
            }

            var selected = passwords[index];

            Form settingsForm = new Form()
            {
                Width = 250,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Settings",
                StartPosition = FormStartPosition.CenterParent
            };

            Button btnEdit = new Button() { Text = "Edit", Left = 50, Width = 120, Top = 20, DialogResult = DialogResult.OK };
            Button btnDelete = new Button() { Text = "Delete", Left = 50, Width = 120, Top = 60 };
            Button btnCancel = new Button() { Text = "Cancel", Left = 50, Width = 120, Top = 100, DialogResult = DialogResult.Cancel };
            settingsForm.Controls.AddRange(new Control[] { btnEdit, btnDelete, btnCancel });

            btnDelete.Click += (s, ev) =>
            {
                var result = MessageBox.Show("Are you sure u want to delete this password?", "Im Sure.", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    passwords.RemoveAt(index);
                    SavePasswords();
                    LoadPasswords();
                    settingsForm.Close();
                }
            };

            btnEdit.Click += (s, ev) =>
            {
                Form editForm = new Form()
                {
                    Width = 300,
                    Height = 260,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = "Edit Password",
                    StartPosition = FormStartPosition.CenterParent
                };

                Label lblSite = new Label() { Left = 10, Top = 20, Text = "Site/App:" };
                TextBox txtSite = new TextBox() { Left = 10, Top = 40, Width = 260, Text = selected.Site };
                Label lblUsername = new Label() { Left = 10, Top = 70, Text = "Username/Gmail:" };
                TextBox txtUsername = new TextBox() { Left = 10, Top = 90, Width = 260, Text = selected.Username };
                Label lblPassword = new Label() { Left = 10, Top = 120, Text = "Password:" };
                TextBox txtPassword = new TextBox() { Left = 10, Top = 140, Width = 260, Text = selected.Password };
                Label lblCategory = new Label() { Left = 10, Top = 170, Text = "Category:" };
                TextBox txtCategory = new TextBox() { Left = 10, Top = 190, Width = 260, Text = selected.Category };
                Button btnOK = new Button() { Text = "Save", Left = 50, Width = 80, Top = 220, DialogResult = DialogResult.OK };
                Button btnCancelEdit = new Button() { Text = "Cancel", Left = 150, Width = 80, Top = 220, DialogResult = DialogResult.Cancel };

                editForm.Controls.AddRange(new Control[] { lblSite, txtSite, lblUsername, txtUsername, lblPassword, txtPassword, lblCategory, txtCategory, btnOK, btnCancelEdit });
                editForm.AcceptButton = btnOK;
                editForm.CancelButton = btnCancelEdit;

                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    selected.Site = txtSite.Text;
                    selected.Username = txtUsername.Text;
                    selected.Password = txtPassword.Text;
                    selected.Category = txtCategory.Text;

                    SavePasswords();
                    LoadPasswords();
                    listBox1.SelectedIndex = index;
                }
            };

            settingsForm.ShowDialog();
        }

        private void label8_Click_1(object sender, EventArgs e) { }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 10, Top = 10, Text = text, Width = 260 };
            TextBox textBox = new TextBox() { Left = 10, Top = 40, Width = 260 };
            Button confirmation = new Button() { Text = "OK", Left = 100, Width = 80, Top = 70, DialogResult = DialogResult.OK };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
