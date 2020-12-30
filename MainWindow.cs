using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using Microsoft.Win32;

namespace SAMPChatlogArchiver
{
    public partial class ControlWindow : Form
    {
        private static string archiveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            + @"\GTA San Andreas User Files\SAMP\archived chatlogs";
        bool directoryExist = Directory.Exists(archiveDirectory);

        string previousLogPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\chatlog.txt";

        private static string logDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) 
            + @"\GTA San Andreas User Files\SAMP";
        //private static string logDirectory = @"C:\Users\5novk\Documents\GTA San Andreas User Files\SAMP";
        bool logExist = Directory.Exists(logDirectory);

        RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        public ControlWindow()
        {            
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            progressBar1.MarqueeAnimationSpeed = 50;
            gameStateLabel.Text = "Not running.";
            gameStateLabel.ForeColor = Color.OrangeRed;
            archiveCheckBox.Checked = true;
        }

        private void ControlWindow_Load(object sender, EventArgs e)
        {
            switch (logExist)
            {
                case true:
                    break;
                case false:
                    MessageBox.Show("SAMP directory does not exist. Please make sure the SAMP folder exists " +
                        @"in your Documents\GTA San Andreas User Files folder.", "Error", MessageBoxButtons.OK
                        , MessageBoxIcon.Information);
                    Application.Exit();
                    break;
                default:
                    break;
            }

            switch (directoryExist)
            {
                case true:
                    break;
                case false:
                    Directory.CreateDirectory(logDirectory + @"\archived chatlogs");
                    break;
                default:
                    break;
            }

            bool startupRegistry = rk.GetValueNames().Contains("SA:MP Chatlog Archiver");

            switch (startupRegistry)
            {
                case true:
                    runAtStartupToolStripMenuItem.Checked = true;
                    break;
                case false:
                    runAtStartupToolStripMenuItem.Checked = false;
                    break;
                default:
                    break;
            }
        }

        bool enableArchiving = false;

        private void archiveCheckBoxTimer_Tick(object sender, EventArgs e)
        {
            if (archiveCheckBox.Checked == true)
            {
                progressBar1.Style = ProgressBarStyle.Marquee;
                archiveStateLabel.Text = "Enabled.";
                archiveStateLabel.ForeColor = Color.Green;

                enableArchiving = true;
            }

            else if (archiveCheckBox.Checked == false)
            {
                progressBar1.Style = ProgressBarStyle.Continuous;
                archiveStateLabel.Text = "Disabled.";
                archiveStateLabel.ForeColor = Color.OrangeRed;

                enableArchiving = false;
            }
        }

        bool detectGame = false;
        int tickGameProcess = 0;
        int tickGameNotActive = 0;

        private void gameProcessTimer_Tick(object sender, EventArgs e)
        {
            if (detectGame == true)
            {
                gameStateLabel.Text = "Running.";
                gameStateLabel.ForeColor = Color.Green;
            }

            else if (detectGame == false)
            {
                gameStateLabel.Text = "Not running.";
                gameStateLabel.ForeColor = Color.OrangeRed;
            }

            Process[] gameProcess = Process.GetProcessesByName("gta_sa");

            if (gameProcess.Length > 0)
            {
                detectGame = true;
                tickGameProcess = 1;
            }

            else
            {
                detectGame = false;
                tickGameNotActive = 1;
            }

            if (tickGameNotActive == 1 && tickGameProcess == 1)
            {
                if (gameProcess.Length > 0 == false && enableArchiving == true)
                {
                    tickGameNotActive = 0;
                    tickGameProcess = 0;

                    WriteChatlog();
                }
            }
        }

        private void WriteChatlog()
        {
            try
            {
                StreamReader chatlogReader = new StreamReader(logDirectory + @"\chatlog.txt");
                string toWrite = chatlogReader.ReadToEnd();

                bool previousLogs = File.Exists(previousLogPath);
                string previousLogContents;

                switch (previousLogs)
                {
                    case true:
                        previousLogContents = File.ReadAllText(previousLogPath);
                        if (previousLogContents == toWrite)
                        {
                            return;
                        }

                        else
                        {
                            File.WriteAllText(previousLogPath, toWrite);
                        }

                        break;
                    case false:
                        File.WriteAllText(previousLogPath, toWrite);
                        break;
                    default:
                        break;
                }

                //DateTime constructors
                string currentDateTime = DateTime.Now.ToString("ddMMMyyyy hhmmtt");
                currentDateTime = currentDateTime.ToUpperInvariant();

                string chatlogFileName = currentDateTime + ".txt";
                StreamWriter chatlogWriter = new StreamWriter(logDirectory + @"\archived chatlogs\" + chatlogFileName);
                chatlogWriter.Write(toWrite);

                LastSaveNameButton.Text = chatlogFileName;
                LastSaveNameButton.Enabled = true;

                chatlogReader.Dispose();
                chatlogWriter.Dispose();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error occured. Your chatlog.txt may not be inside SAMP folder." + Environment.NewLine
                    + "Exception details:\n" + Environment.NewLine + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LastSaveNameButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(archiveDirectory + @"\" + LastSaveNameButton.Text.ToString());
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error occured. Your saved chatlog may have been moved into another directory."
                    + Environment.NewLine + "Exception details:\n" + Environment.NewLine + ex, "Error", MessageBoxButtons.OK
                    , MessageBoxIcon.Error);
            }
        }

        private void ControlWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            notifyIcon1.Visible = true;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            notifyIcon1.Visible = true;
        }

        private void goToArchiveFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(logDirectory + @"\archived chatlogs");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("• Basic chatlog archiving system. \n• Added option to register app to startup registry key. \n• Added archive folder shortcut tab. " +
                "\n• Added button to access recent saved logs faster."
                + "\n 30/DEC/2020 - RedStar", "Changelog - v1.0", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void archiveFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(archiveDirectory);
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void runAtStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (runAtStartupToolStripMenuItem.Checked == false)
            {
                bool isElevated;
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
                }

                if (isElevated == false)
                {
                    MessageBox.Show("You must run this program as administrator to add it to the startup list.", "Error"
                        , MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                else if (isElevated == true)
                {
                    RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    rk.SetValue("SA:MP Chatlog Archiver", Application.ExecutablePath);

                    MessageBox.Show("Successfully added SA:MP Chatlog Archiver into startup registry key. " +
                        "Make sure you have placed this app on a permanent location for it to work.", 
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    runAtStartupToolStripMenuItem.Checked = true;
                }
            }

            else if (runAtStartupToolStripMenuItem.Checked == true)
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rk.DeleteValue("SA:MP Chatlog Archiver", false);

                MessageBox.Show("Successfully removed SA:MP Chatlog Archiver from startup registry list.", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                runAtStartupToolStripMenuItem.Checked = false;
            }
        }
    }
}
