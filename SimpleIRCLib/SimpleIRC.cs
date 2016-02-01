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

        //private variables
        private string newIP;
        private int newPort;
        private string newUsername;
        private string newPassword;
        private string newChannel;
        private bool conCheck;
        private DCCClient dcc;
        private string downloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        private Action<string> chatOutput;

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
        public void setupIrc(string IP, int Port, string Username, string Password, string Channel, Action<string> chatoutput)
        {
            newIP = IP;
            newPort = Port;
            newUsername = Username;
            newPassword = Password;
            newChannel = Channel;
            chatOutput = chatoutput;
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

        public void setIP(string IP)
        {
            newIP = IP;
        }

        public void setPort(int Port)
        {
            newPort = Port;
        }

        public void setUsername(string Username)
        {
            newUsername = Username;
        }

        public void setPassword(string Password)
        {
            newPassword = Password;
        }

        public void setChannel(string Channel)
        {
            newChannel = Channel;
        }

        //get current connection details
        public string getIP()
        {
            return newIP;
        }

        public int getPort()
        {
            return newPort;
        }

        public string getUsername()
        {
            return newUsername;
        }

        public string getPassword()
        {
            return newPassword;
        }

        public string getChannel()
        {
            return newChannel;
        }

        //starts the irc client
        public void startClient()
        {
            try
            {
                DebugCallBack("Start server!");
            } catch (NullReferenceException e)
            {
                DebugCallBack = debugVoid;
            }

            try
            {
                downloadStatusChange();
            }
            catch (NullReferenceException e)
            {
                downloadStatusChange = downloadStatusVoid;
            }

            IrcConnect con = new IrcConnect(newIP, newPort, newUsername, newPassword, newChannel);
            conCheck = con.Connect();

            if (conCheck)
            {
                DebugCallBack("succesful connected to the irc server!");
                string ircData = "";
                var worker = new Thread(() =>
                {
                    while ((ircData = IrcConnect.reader.ReadLine()) != null)
                    {
                        if (ircData.Contains("PONG"))
                        {

                        }
                        else
                        {
                            chatOutput(ircData);
                        }

                        if (ircData.Contains("DCC SEND") && ircData.Contains(newUsername))
                        {
                            dcc = new DCCClient(ircData, downloadDir, IrcSend.bot, IrcSend.packNumber);
                            DebugCallBack("\n DCC SERVER REPLY: " + dcc.downloadDetails()[0]);
                            DebugCallBack("\n FILENAME: " + dcc.downloadDetails()[1]);
                            DebugCallBack("\n FILESIZE: " + dcc.downloadDetails()[2]);
                            DebugCallBack("\n IP: " + dcc.downloadDetails()[3]);
                            DebugCallBack("\n PORT: " + dcc.downloadDetails()[4]);
                            DebugCallBack("\n PACK: " + dcc.downloadDetails()[5]);
                            DebugCallBack("\n BOT: " + dcc.downloadDetails()[6]);
                        }
                        Thread.Sleep(1);
                    }
                });

                worker.Start();
            }
            else
            {
                DebugCallBack("Something went wrong while connecting, try again!");
            }
        }

        //send message
        public void sendMessage(string message)
        {
            try
            {
                IrcSend.sendMsg(message);
            } catch (Exception e)
            {
                DebugCallBack("Could not send message, irc client is not running :X, error: \n" + e.ToString());
            }
            
        }

        public string[] getDownloadProgress()
        {
            if (DCCClient.Downloading) {
                return dcc.downloadDetails();
            } else
            {
                return new string[]{"NULL","","","","","","","","","","",""};
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
