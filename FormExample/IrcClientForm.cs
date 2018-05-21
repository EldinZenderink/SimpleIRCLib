using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
//the library
using SimpleIRCLib;
using System.Collections.Generic;

namespace FormExample
{
    /// <summary>
    /// This class is meant as example on how to use SimpleIRCLib, this does not mean that this is the correct way to program!
    /// It's meant to showcase a few of the available methods within SimpleIRCLib, you should figure out on your own how to implement it to suit your needs!
    /// It lacks a few options, such as leaving a specific channel, which will be implemented in the future. If your knowledged, you could send a raw message
    /// to the server containing commands to PART from a channel and use the OnRawMessageReceived event to check the server response.
    /// </summary>
    public partial class IrcClientForm : Form
    {
        //initiate irc client
        private readonly SimpleIRC _irc;

        //initiate debugform
        private readonly DebugForm _debugForm;

        private string _defaultDownloadDirectory;

        /// <summary>
        /// Form Contstructor, initializes SimpleIRC Library and registers event handlers.
        /// </summary>
        public IrcClientForm()
        {
            _irc = new SimpleIRC();
            _irc.IrcClient.OnMessageReceived += OnMessagesReceived;
            _irc.IrcClient.OnUserListReceived += OnUserListReceived;
            _irc.DccClient.OnDccEvent += OnDccEvent;
            _debugForm = new DebugForm(_irc);
            _defaultDownloadDirectory = Directory.GetCurrentDirectory();

            InitializeComponent();
        }

        /// <summary>
        /// Gets the values from the input fields and starts the client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ConnectButton_Click(object sender, EventArgs e)
        {
            if (ServerInput.Text != "" && PortInput.Text != "" && UsernameInput.Text != "" && ChannelsInput.Text != "" && _irc.IsClientRunning() == false)
            {
                int port = -1;
                if ((port = int.Parse(PortInput.Text)) != -1)
                {
                    //parameters as follows: ip or address to irc server, username, password(not functional), channels, and method to execute when message is received (see line 103)
                    _irc.SetupIrc(ServerInput.Text, UsernameInput.Text,  ChannelsInput.Text, int.Parse(PortInput.Text));

                    //Sets event handlers for all the possible events (!IMPORTANT: do this after intializing irc.SetupIRC !!!)
                    

                    _irc.StartClient();
                }
            } else
            {
                 MessageBox.Show("You need to fill in all the information fields!");
            }
        }

        /// <summary>
        /// Disconnects the irc client, closes all open tabs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DisconnectButton_Click(object sender, EventArgs e)
        {
            if (_irc.StopClient())
            {
                ircChatTabs.TabPages.Clear();
                userListTabs.TabPages.Clear();
            }
        }
        
        /// <summary>
        /// Sends if enter is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                if (MessageInput.Text != "" && _irc.IsClientRunning())
                {
                    _irc.SendMessageToAll(MessageInput.Text);
                }
            }
            
        }

        /// <summary>
        /// Event handler for receiving messages from the Irc Client.
        /// </summary>
        /// <param name="sender">Values of the class that fired the event</param>
        /// <param name="args">IrcReceivedEventArgs containing the message information</param>
        public void OnMessagesReceived(object sender, IrcReceivedEventArgs args)
        {
            OnMessagesReceivedLocal(args.Channel, args.User, args.Message);
        }

        /// <summary>
        /// Method that gets invoked on the main thread, adds a message to the richtextbox within a the correct channel tab.
        /// </summary>
        /// <param name="channel">channel messaged was received on</param>
        /// <param name="user">user that send the message to the channel</param>
        /// <param name="message">message that the user had send on the channel</param>
        public void OnMessagesReceivedLocal(string channel, string user, string message)
        {
            if (this.ircChatTabs.InvokeRequired)
            {
                this.ircChatTabs.Invoke(new MethodInvoker(() => OnMessagesReceivedLocal(channel, user, message)));
            }
            else
            {
                //searches for the tab for the correct channel
                bool found = false;
                int index = 0;
                foreach (TabPage tab in ircChatTabs.TabPages)
                {
                    if (channel.Contains(tab.Name))
                    {
                        found = true;
                        break;
                    }

                    index++;
                }

                //if the tab has been found, add a message to the richtextbox within that tab.
                if (found)
                {
                    if (ircChatTabs.TabPages[index].Controls.ContainsKey(channel))
                    {
                        RichTextBox selectedRtb = (RichTextBox)ircChatTabs.TabPages[index].Controls[channel];
                        selectedRtb.AppendText(user + " : " + message + Environment.NewLine);
                        selectedRtb.ScrollToCaret();
                    }
                }
               
            }
        }

        /// <summary>
        /// Event that gets fired when a user list has been received.
        /// </summary>
        /// <param name="sender">Values of the class that fired the event</param>
        /// <param name="args">IrcUserListReceivedEventArgs contains the Dictionary where the key is the channel and the list contains the usernames</param>
        public void OnUserListReceived(object sender, IrcUserListReceivedEventArgs args )
        {
            OnUserListReceivedLocal(args.UsersPerChannel);
        }

        /// <summary>
        /// Method to invoke on the main thread, checks if a tab for the chat exists with name of the channel, if not, it creates it, same goes for the tab with the user name list.
        /// </summary>
        /// <param name="userList">Dictionary where the key is the channel and the list contains the usernames </param>
        public void OnUserListReceivedLocal(Dictionary<string, List<string>> userList)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => OnUserListReceivedLocal(userList)));
            }
            else
            {
                //iterate through each channel
                foreach (KeyValuePair<string, List<string>> channelsAndUsers in userList)
                {
                    //search for a tab with the same channel name within the chatTabs
                    bool foundChatTab = false;
                    int indexChatTab = 0;
                    foreach (TabPage tab in ircChatTabs.TabPages)
                    {
                        if (channelsAndUsers.Key.Equals(tab.Name))
                        {
                            Debug.WriteLine("FOUND TAB: " + tab.Name);
                            foundChatTab = true;
                            break;
                        }

                        indexChatTab++;
                    }

                    if (!foundChatTab)
                    {
                        TabPage newTab = new TabPage(channelsAndUsers.Key);
                        newTab.Name = channelsAndUsers.Key;
                        RichTextBox rtb = new RichTextBox();
                        rtb.Dock = DockStyle.Fill;
                        rtb.BorderStyle = BorderStyle.None;
                        rtb.Name = channelsAndUsers.Key;
                        newTab.Controls.Add(rtb);
                        ircChatTabs.TabPages.Add(newTab);
                    }

                    //search for a tab with the same channel name within the userListTabs 
                    bool foundUserListTab = false;
                    int userListTabIndex = 0;
                    foreach (TabPage tab in userListTabs.TabPages)
                    {
                        if (channelsAndUsers.Key.Equals(tab.Name))
                        {
                            foundUserListTab = true;
                            break;
                        }

                        userListTabIndex++;
                    }

                    if (!foundUserListTab)
                    {
                        TabPage newTab = new TabPage(channelsAndUsers.Key);
                        newTab.Name = channelsAndUsers.Key;
                        RichTextBox rtb = new RichTextBox();
                        rtb.Dock = DockStyle.Fill;
                        rtb.BorderStyle = BorderStyle.None;
                        rtb.Name = channelsAndUsers.Key;
                        foreach (string user in channelsAndUsers.Value)
                        {
                            rtb.AppendText(user + Environment.NewLine);
                        }
                        rtb.ScrollToCaret();
                        newTab.Controls.Add(rtb);
                        userListTabs.TabPages.Add(newTab);
                    }
                    else
                    {

                        if (userListTabs.TabPages[userListTabIndex].Controls.ContainsKey(channelsAndUsers.Key))
                        {
                            RichTextBox selectedRtb = (RichTextBox)userListTabs.TabPages[userListTabIndex].Controls[channelsAndUsers.Key];
                            foreach (string user in channelsAndUsers.Value)
                            {
                                selectedRtb.AppendText(user + Environment.NewLine);
                            }
                            selectedRtb.ScrollToCaret();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Event that fires when DCCClient starts downloading.
        /// </summary>
        /// <param name="sender">Values of the class that fired the event</param>
        /// <param name="args">DCCEventArgs contains all the information about the download update</param>
        public void OnDccEvent(object sender, DCCEventArgs args)
        {
            string fileName = args.FileName;
            int downloadProgress = args.Progress;
            string downloadSpeed = args.KBytesPerSecond.ToString();
            string status = args.Status;

            string fullDownloadInformation = fileName + "   |   " + status + "  |   " + downloadSpeed + " kb/s   |   " + _defaultDownloadDirectory;
            
            UpdateDownloadList(fullDownloadInformation, fileName);
          
            UpdateProgressBar(downloadProgress);

            if (status.Contains("COMPLETED"))
            {
                UpdateProgressBar(100);
            }

        }

        /// <summary>
        /// Updates the DownloadList object on the main form while the download is going, invoke is necesary because
        /// method is being called from a different Thread!
        /// </summary>
        /// <param name="toUpdate">string that needs to be added/updated</param>
        /// <param name="fileName">the filename which is used for searching the item in the download list for updating</param>
        public void UpdateDownloadList(string toUpdate, string fileName)
        {
            if (this.DownloadsList.InvokeRequired)
            {
                this.DownloadsList.Invoke(new MethodInvoker(() => UpdateDownloadList(toUpdate, fileName)));
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
        public void UpdateProgressBar(int progress)
        {
            if (this.DownloadProgressBar.InvokeRequired)
            {
                this.DownloadProgressBar.Invoke(new MethodInvoker(() => UpdateProgressBar(progress)));
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
        public void ShowDebugButton_Click(object sender, EventArgs e)
        {
            try
            {
                _debugForm.Show();
            } catch
            {

            }
        }

        /// <summary>
        /// Opens the folder where the selected file is being downloaded to
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DownloadsList_MouseDoubleClick(object sender, MouseEventArgs e)
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
        public void SetDownloadFolderButton_Click(object sender, EventArgs e)
        {
            DialogResult result = OpenFolderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string newDownloadDirectory = OpenFolderDialog.SelectedPath;
                _defaultDownloadDirectory = newDownloadDirectory;
                _irc.SetCustomDownloadDir(newDownloadDirectory);
            }
        }

        /// <summary>
        /// Stops the irc client on form close, otherwise it would keep running in the background!!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_irc.IsClientRunning())
            {
                _irc.StopClient();
            }
        }

        /// <summary>
        /// Gets the users in the current channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateUserList_Click(object sender, EventArgs e)
        {
            RichTextBox selectedRtb = (RichTextBox)userListTabs.SelectedTab.Controls[0];
            selectedRtb.Clear();
            if (_irc.IsClientRunning())
            {
                _irc.GetUsersInDifferentChannel(userListTabs.SelectedTab.Name);
            }
        }

        /// <summary>
        /// Sends a message to a specific channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SendToChannel_Click(object sender, EventArgs e)
        {
            string channel = ircChatTabs.SelectedTab.Name;
            _irc.SendMessageToChannel(MessageInput.Text, channel);
        }

        /// <summary>
        /// Sends a message to all channels.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SendToAll_Click(object sender, EventArgs e)
        {
            _irc.SendMessageToAll(MessageInput.Text);
        }

    }
}
