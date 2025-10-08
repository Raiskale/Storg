using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Salasanakone
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // verifying masterkey before opening anything
            byte[] aesKey;
            if (!AskOrSetKey(out aesKey))
            {
                MessageBox.Show("Application will close because no valid master key was entered.");
                return; // not opening since invalid key
            }

            // setting master key
            Storg.aesKey = aesKey;

            // opening the application
            Application.Run(new Storg());
        }

        private static bool AskOrSetKey(out byte[] key)
        {
            key = null;
            string masterKeyFile = Path.Combine(Application.StartupPath, "masterkey.txt");

            if (!File.Exists(masterKeyFile))
            {
                // creating new master key
                string masterPassword = Prompt.ShowDialog("Set a master password:", "Master Password");
                if (string.IsNullOrEmpty(masterPassword))
                    return false; // no accept empty passowrd

                using (SHA256 sha = SHA256.Create())
                {
                    key = sha.ComputeHash(Encoding.UTF8.GetBytes(masterPassword));
                    File.WriteAllText(masterKeyFile, Convert.ToBase64String(key));
                }

                return true;
            }
            else
            {
                // asking for master key max 3 times.
                int attempts = 0;
                while (attempts < 3)
                {
                    string currentMasterPassword = Prompt.ShowDialog("Enter Master key:", "Master Password");
                    if (string.IsNullOrEmpty(currentMasterPassword))
                    {
                        attempts++;
                        continue;
                    }

                    using (SHA256 sha = SHA256.Create())
                    {
                        byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(currentMasterPassword));
                        string hash = Convert.ToBase64String(hashBytes);
                        string storedHash = File.ReadAllText(masterKeyFile);

                        if (hash == storedHash)
                        {
                            key = hashBytes;
                            return true;
                        }
                    }

                    MessageBox.Show("Wrong master key!");
                    attempts++;
                }

                return false; // three fails
            }
        }
    }
}
