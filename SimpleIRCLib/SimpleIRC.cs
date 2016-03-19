using System;
using System.IO;
namespace SimpleIRCLib
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class SimpleIRC
    {
        //sets the method to be called when there is a debug message
        public Action<string> DebugCallBack;

        //sets the method to be called when there is a change in the status of the current download
        public Action downloadStatusChange;

        //sets the method to be called when there is a chat message to be shown
        public Action<string, string> chatOutput;

        //sets the download folder for the current download and following downloads (can be changed when instance is running)
        public string downloadDir { get; set; }

        //public bool which should stop every running tasks and threads
        public bool shouldClientStop = false;

        //public available information

        public string newIP { get; set; }
        public int newPort { get; set; }
        public string newUsername { get; set; }
        public string newChannel { get; set; }
        public string newPassword { get; set; }

        //private variables
        private IrcConnect con;

        //Constructor
        public SimpleIRC()
        {
            newIP = "";
            newPort = 0;
            newUsername = "";
            newPassword = "";
            newChannel = "";
            downloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }

        //setup connection to irc server and channels
        public void setupIrc(string IP, int Port, string Username, string Password, string Channel, Action<string, string> chatoutput)
        {
            newIP = IP;
            newPort = Port;
            newUsername = Username;
            newPassword = Password;
            newChannel = Channel;
            chatOutput = chatoutput;
            DebugCallBack = null;
            downloadStatusChange = null;
            downloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }

        //for custom download directory
        public void setCustomDownloadDir(string downloaddir)
        {
            downloadDir = downloaddir;
        }

        //set debug callback
        public void setDebugCallback(Action<string> callback)
        {
            DebugCallBack = callback;
        }

        public void setDownloadStatusChangeCallback(Action callback)
        {
            downloadStatusChange = callback;
        }

        //starts the irc client
        public void startClient()
        {

            con = new IrcConnect(newIP, newPort, newUsername, newPassword, newChannel, this);
            if (con != null)
            {
                if (!con.isConnectionEstablised)
                {
                    con.Connect();
                }
                else
                {
                    DebugCallBack("You are already connected to a server, disconnect first! \n");
                }
            } else
            {
                if (DebugCallBack != null)
                {          
                    DebugCallBack("You are already connected to a server, disconnect first! \n");
                }
            }
        }

        public bool isClientRunning()
        {
            return con.isConnectionEstablised;
        }

        public void stopClient()
        {
            //execute quit stuff
            if (con.isConnectionEstablised)
            {
                shouldClientStop = true;
            }
        }

        //gets the download details by defining which detail you want
        public object getDownloadProgress(string whichdownloaddetail)
        {
            object[] dlDetails = con.passDownloadDetails();
            if(whichdownloaddetail == "mbps")
            {
                return dlDetails[9];
            }
            else if (whichdownloaddetail == "kbps")
            {
                return dlDetails[8];
            }
            else if(whichdownloaddetail == "bps")
            {
                return dlDetails[7];
            }
            else if (whichdownloaddetail == "filename")
            {
                return dlDetails[1];
            }
            else if (whichdownloaddetail == "bot")
            {
                return dlDetails[6];
            }
            else if (whichdownloaddetail == "pack")
            {
                return dlDetails[5];
            }
            else if (whichdownloaddetail == "dccstring")
            {
                return dlDetails[0];
            }
            else if (whichdownloaddetail == "ip")
            {
                return dlDetails[3];
            }
            else if (whichdownloaddetail == "port")
            {
                return dlDetails[4];
            }
            else if (whichdownloaddetail == "progress")
            {
                return dlDetails[10];
            }
            else if (whichdownloaddetail == "status")
            {
                return dlDetails[11];
            } else
            {
                return null;
            }

        }
        //send message
        public void sendMessage(string message)
        {
            if (con.isConnectionEstablised) {
                con.sendMsg(message);
            }            
        }

    }
}
