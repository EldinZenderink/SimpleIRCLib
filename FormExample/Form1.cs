using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
//the library
using SimpleIRCLib;
using System.Collections.Generic;

namespace FormExample
{
    public partial class Form1 : Form
    {
        //initiate irc client
        public SimpleIRC irc = new SimpleIRC();

        //initiate debugform
        public DebugForm debugForm = new DebugForm();
       


        public string defaultDownloadDirectory = "";

        public Form1()
        {
            InitializeComponent();
            defaultDownloadDirectory = Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Gets the values from the input fields and starts the client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (ServerInput.Text != "" && PortInput.Text != "" && UsernameInput.Text != "" && ChannelsInput.Text != "" && irc.isClientRunning() == false)
            {
                int port = -1;
                if ((port = int.Parse(PortInput.Text)) != -1)
                {
                    //parameters as follows: ip or address to irc server, username, password(not functional), channels, and method to execute when message is received (see line 103)
                    irc.setupIrc(ServerInput.Text, port, UsernameInput.Text, "", ChannelsInput.Text, AppendChatMessageToChatOutput);

                    //sets the method for appending text to a debug form, see Class "DebugForm.cs" line: 27 
                    irc.setDebugCallback(debugForm.AppendToDebugOutput);

                    //sets method for updating download information while downloading, see line: 119
                    irc.setDownloadStatusChangeCallback(OnDownloadEvent);

                    //sets the download dir to where the application runs
                    irc.setCustomDownloadDir(defaultDownloadDirectory);

                    //set callback when the list with users arrives
                    irc.setUserListReceivedCallback(UserListReceived);

                    //Start client
                    irc.startClient();
                }
            } else
            {
                 MessageBox.Show("You need to fill in all the information fields!");
            }
        }

        /// <summary>
        /// Disconnects the irc client, if connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            if (irc.isClientRunning())
            {
                irc.stopClient();
            }
        }

        /// <summary>
        /// Sends a message to the irc server on button click, if connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendMessageButton_Click(object sender, EventArgs e)
        {
            if (MessageInput.Text != "" && irc.isClientRunning())
            {
                irc.sendMessage(MessageInput.Text);
            }
        }

        /// <summary>
        /// Sends a message to the irc server on enter press, if connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                if (MessageInput.Text != "" && irc.isClientRunning())
                {
                    irc.sendMessage(MessageInput.Text);
                }
            }
            
        }

        /// <summary>
        /// Appends chat message received from irc server to richtextbox output box. 
        /// Invoke is needed because this method executes on a different thread!
        /// </summary>
        /// <param name="user">The user where the messsage came from</param>
        /// <param name="message">The actual message</param>
        private void AppendChatMessageToChatOutput(string user, string message)
        {
            if (this.ChatOutput.InvokeRequired)
            {
                this.ChatOutput.Invoke(new MethodInvoker(() => AppendChatMessageToChatOutput(user, message)));
            } else
            {
                this.ChatOutput.AppendText(user + " : " + message + "\n");
            }
        }

        /// <summary>
        ///  Adds all users from channel to the list
        /// </summary>
        /// <param name="list"></param>
        private void UserListReceived(string[] list)
        {
            if (this.ChatOutput.InvokeRequired)
            {
                this.ChatOutput.Invoke(new MethodInvoker(() => UserListReceived(list)));
            }
            else
            {
                this.UserList.Items.Clear();
                foreach (string user in list)
                {
                    this.UserList.Items.Add(user);
                }               
            }
        }

        /// <summary>
        /// Gets the information about the download
        /// </summary>
        private void OnDownloadEvent()
        {
            string fileName = irc.getDownloadProgress("filename").ToString();
            int downloadProgress = (int)irc.getDownloadProgress("progress");
            string downloadSpeed = irc.getDownloadProgress("kbps").ToString();
            string status = irc.getDownloadProgress("status").ToString();

            string fullDownloadInformation = fileName + "   |   " + status + "  |   " + downloadSpeed + " kb/s   |   " + defaultDownloadDirectory;

            //see line 144
            updateDownloadList(fullDownloadInformation, fileName);
            //see line 183
            updateProgressBar(downloadProgress);

            if (status.Contains("COMPLETED"))
            {
                updateProgressBar(100);
            }

        }

        /// <summary>
        /// Updates the DownloadList object on the main form while the download is going, invoke is necesary because
        /// method is being called from a different Thread!
        /// </summary>
        /// <param name="toUpdate">string that needs to be added/updated</param>
        /// <param name="fileName">the filename which is used for searching the item in the download list for updating</param>
        private void updateDownloadList(string toUpdate, string fileName)
        {
            if (this.DownloadsList.InvokeRequired)
            {
                this.DownloadsList.Invoke(new MethodInvoker(() => updateDownloadList(toUpdate, fileName)));
            }
            else
            {
                int indexOfDownloadItem = 0;
                for (int i = indexOfDownloadItem; i < DownloadsList.Items.Count; ++i)
                {
                    string lbString = DownloadsList.Items[i].ToString();
                    if (lbString.Contains(fileName))
                    {
                        indexOfDownloadItem = i;
                        break;
                    }
                }

                try
                {
                    DownloadsList.Items.RemoveAt(indexOfDownloadItem);
                    DownloadsList.Items.Insert(indexOfDownloadItem, toUpdate);
                    DownloadsList.SelectedIndex = indexOfDownloadItem;
                }
                catch
                {
                    DownloadsList.Items.Add(toUpdate);
                }
            }
        }

        /// <summary>
        /// Updates the progress bar
        /// </summary>
        /// <param name="progress">defines the current progress</param>
        private void updateProgressBar(int progress)
        {
            if (this.DownloadProgressBar.InvokeRequired)
            {
                this.DownloadProgressBar.Invoke(new MethodInvoker(() => updateProgressBar(progress)));
            } else
            {
                this.DownloadProgressBar.Value = progress;
            }
        }

        /// <summary>
        /// Opens the debug form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowDebugButton_Click(object sender, EventArgs e)
        {
            try
            {
                debugForm.Show();
            } catch
            {

            }
        }

        /// <summary>
        /// Opens the folder where the selected file is being downloaded to
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadsList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int currentlySelected = DownloadsList.SelectedIndex;
            try
            {
                string currentItem = DownloadsList.Items[currentlySelected].ToString();

            if (currentItem.Contains("|"))
            {
                string fileLocation = currentItem.Split('|')[currentItem.Split('|').Length - 1].Trim();
                Process.Start(fileLocation);
            }
            }
            catch
            {

            }            
        }

        /// <summary>
        /// Set download  directory to a custom directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetDownloadFolderButton_Click(object sender, EventArgs e)
        {
            DialogResult result = OpenFolderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string newDownloadDirectory = OpenFolderDialog.SelectedPath;
                defaultDownloadDirectory = newDownloadDirectory;
                irc.setCustomDownloadDir(newDownloadDirectory);
            }
        }

        /// <summary>
        /// Stops the irc client on form close, otherwise it would keep running in the background!!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (irc.isClientRunning())
            {
                irc.stopClient();
            }
        }

        /// <summary>
        /// Gets the users in the current channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateUserList_Click(object sender, EventArgs e)
        {
            this.UserList.Items.Clear();
            if (irc.isClientRunning())
            {
                irc.getUsersInCurrentChannel();
            }
        }
    }
}
