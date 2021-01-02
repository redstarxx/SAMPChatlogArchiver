using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using IWshRuntimeLibrary;

namespace SAMPChatlogArchiver
{
    public partial class ControlWindow : Form
    {
        private static string archiveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            + @"\GTA San Andreas User Files\SAMP\archived chatlogs\";
        bool directoryExist = Directory.Exists(archiveDirectory);

        private static string logDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) 
            + @"\GTA San Andreas User Files\SAMP";
        bool logExist = Directory.Exists(logDirectory);

        string previousLogContents = string.Empty;

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
                    DialogResult result = MessageBox.Show("SAMP directory does not exist. Please make sure SAMP folder exists " +
                        @"in your Documents\GTA San Andreas User Files folder. Application will exit to prevent unwanted errors.", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (result == DialogResult.OK)
                    {
                        Application.Exit();
                    }

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

            bool startupShortcut = System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) 
                + @"\SAMPChatlogArchiver.lnk");

            switch (startupShortcut)
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

        private void ControlWindow_Shown(object sender, EventArgs e)
        {
            switch (Program.launchOnStartup)
            {
                case true:
                    this.Hide();
                    this.Visible = false;
                    break;
                case false:
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

                // Prevents duplication
                if (previousLogContents == toWrite)
                {
                    return;
                }

                else
                {
                    //DateTime constructors
                    string currentDateTime = DateTime.Now.ToString("ddMMMyyyy hhmmtt");
                    currentDateTime = currentDateTime.ToUpperInvariant();

                    string chatlogFileName = currentDateTime + ".txt";
                    StreamWriter chatlogWriter = new StreamWriter(archiveDirectory + chatlogFileName);
                    chatlogWriter.Write(toWrite);
                    previousLogContents = toWrite;

                    LastSaveNameButton.Text = chatlogFileName;
                    LastSaveNameButton.Enabled = true;

                    chatlogReader.Dispose();
                    chatlogWriter.Dispose();
                }              
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error occured. Your chatlog.txt may have been moved to another location." + Environment.NewLine
                    + "Exception details:\n" + Environment.NewLine + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LastSaveNameButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(archiveDirectory + LastSaveNameButton.Text);
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
            MessageBox.Show("• Anti-duplication archiving code now should work. \n• Upon selecting the \"Run at startup\" " +
                "option in Settings, a shortcut would be created in Startup folder instead of creating a new registry key. " +
                "\n• The app will now immediately minimizes to tray upon starting on startup."
                + "\n 01/JAN/2021 - RedStar", "Changelog - v1.0.1", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            string startupFolderDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

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
                    MessageBox.Show("You must re-run this program as administrator to complete this.", "Error"
                        , MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                else if (isElevated == true)
                {                   
                    var shortcutShell = new WshShell();

                    var shortcut = shortcutShell.CreateShortcut(startupFolderDirectory + "\\" +
                        Application.ProductName + ".lnk") as IWshShortcut;
                    shortcut.TargetPath = Application.ExecutablePath;
                    shortcut.WorkingDirectory = Application.StartupPath;
                    shortcut.Arguments = "startup";
                    shortcut.Save();

                    MessageBox.Show("Successfully added SA:MP Chatlog Archiver to startup list.", 
                        "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    runAtStartupToolStripMenuItem.Checked = true;
                }
            }

            else if (runAtStartupToolStripMenuItem.Checked == true)
            {
                System.IO.File.Delete(startupFolderDirectory + @"\SAMPChatlogArchiver.lnk");

                MessageBox.Show("Successfully removed SA:MP Chatlog Archiver from startup list.", 
                    "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                runAtStartupToolStripMenuItem.Checked = false;
            }
        }
    }
}
