using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace SimpleIRCLib
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class SimpleIRC
    {
        //global static debug function
        public static Action<string> DebugCallBack;

        public static Action downloadStatusChange;

        public static Action<string, string> chatOutput;

        public string downloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        //private variables
        private string newIP;
        private int newPort;
        private string newUsername = "";
        private string newChannel = "";
        private string newPassword;
        private bool conCheck;
        private IrcConnect con;

        //Constructor
        public SimpleIRC()
        {
            newIP = "";
            newPort = 0;
            newUsername = "";
            newPassword = "";
            newChannel = "";
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
            DebugCallBack = debugVoid;
            downloadStatusChange = downloadStatusVoid;
        }

        //return most important information 
        public string getUname() {
            return newUsername;
        }

        public string getChannel()
        {
            return newChannel;
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
           
            con = new IrcConnect(newIP, newPort, newUsername, newPassword, newChannel);
            con.Connect();
           
        }

        public bool isClientRunning()
        {
            return con.clientStatus();
        }

        public void stopClient()
        {
            //execute quit stuff
            if (con.clientStatus())
            {
                con.quitConnect();
            }
        }

        public string[] getDownloadProgress()
        {
            return con.passDownloadProgress();
        }
        //send message
        public void sendMessage(string message)
        {
            try
            {
                IrcSend.sendMsg(message);
            } catch (Exception e) { 

                try
                {
                    DebugCallBack("Could not send message, error :\n" + e.ToString());
                }
                catch
                {
                    //maybe a log file :?
                }
               
            }
            
        }

        public void debugVoid(string input)
        {
            //does nothing
        }

        public void downloadStatusVoid()
        {
            //does nothing
        }

    }
}
