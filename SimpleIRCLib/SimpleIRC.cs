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
        public Action<string[]> UsersListReceived;

        //sets the method to be called when there is a chat message to be shown
        public Action<string, string> chatOutput;

        //sets the method to be called when there is a message from the irc server, completely raw, so you can do your own stuff.
        public Action<string> rawOutput;

        //sets the download folder for the current download and following downloads (can be changed when instance is running)
        public string downloadDir { get; set; }

        //public bool which should stop every running tasks and s
        public bool shouldClientStop = false;

        //public available information

        public string newIP { get; set; }
        public int newPort { get; set; }
        public string newUsername { get; set; }
        public string newChannel { get; set; }
        public string newPassword { get; set; }

        //private variables
        private IrcConnect con;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SimpleIRC()
        {
            newIP = "";
            newPort = 0;
            newUsername = "";
            newPassword = "";
            newChannel = "";
            downloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }

        
        /// <summary>
        /// Setup the client to be able to connect to the server
        /// </summary>
        /// <param name="IP">Server address, possibly works with dns addresses (irc.xxx.x), but ip addresses works just fine (of type string)</param>
        /// <param name="Port">Port of the server you want to connect to, of type int</param>
        /// <param name="Username">Username the client wants to use, of type string</param>
        /// <param name="Password">Password, currently not used!, of type string</param>
        /// <param name="Channel">Channel(s) the client wants to connect to, possible to connect to multiple channels at once by seperating each channel with a ',' (Example: #chan1,#chan2), of type string</param>
        /// <param name="chatoutput">callback method when messages are received from the server, method to be used as callback needs two string parameters, one for username, second for the actual message.</param>
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
            shouldClientStop = false;
            downloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }

        /// <summary>
        /// Sets the download directory for dcc downloads.
        /// </summary>
        /// <param name="downloaddir"> Requires a path to a directory of type string as parameter.</param>
        public void setCustomDownloadDir(string downloaddir)
        {
            downloadDir = downloaddir;
        }

        /// <summary>
        ///Set debug callback method. Will be executed when there is a new debug message available
        /// </summary>
        /// <param name="callback">A method which should have a string parameter of its own.</param>
        public void setDebugCallback(Action<string> callback)
        {
            DebugCallBack = callback;
        }

        /// <summary>
        /// Executes a method when there is new information about the current download
        /// </summary>
        /// <param name="callback">takes a method as parameter, which will be executed</param>
        public void setDownloadStatusChangeCallback(Action callback)
        {
            downloadStatusChange = callback;
        }

        /// <summary>
        ///set user list received callback,
        /// </summary>
        /// <param name="callback"> you should provide a method where the method you pass through needs to have a string[] as parameter</param>
        public void setUserListReceivedCallback(Action<string[]> callback)
        {
            UsersListReceived = callback;
        }

        /// <summary>
        /// Sets the download directory for dcc downloads.
        /// </summary>
        /// <param name="downloaddir"> Requires a path to a directory of type string as parameter.</param>
        public void setRawOutput(Action<string> rawoutput)
        {
            rawOutput = rawoutput;
        }

        /// <summary>
        /// Starts the irc client with the given parameters in the constructor
        /// </summary>
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

        /// <summary>
        /// Checks if the client is running.
        /// </summary>
        /// <returns>true or false</returns>
        public bool isClientRunning()
        {
            try
            {
                return con.isConnectionEstablised;
            } catch
            {
                return false;
            }
        }


        /// <summary>
        /// Stops the client
        /// </summary>
        /// <returns>true or false depending on succes</returns>
        public bool stopClient()
        {
            //execute quit stuff
            try
            {
                if (con.isConnectionEstablised)
                {
                    shouldClientStop = true;
                    con.quitConnect();
                    return true;
                } else
                {
                    return false;
                }
            } catch
            {
                return false;
            }
            
        }


        /// <summary>
        ///returns true or false upon calling this method, for telling you if the downlaod has been stopped or not
        /// </summary>
        /// <returns></returns>
        public bool stopXDCCDownload()
        {
           return con.stopXDCCDownload();
        }

        //
        /// <summary>
        /// gets the download details by defining which detail you want
        /// available (use this as the string for the parameter):
        /// </summary>
        /// <param name="whichdownloaddetail">Possible inputs, in order: mbps,kbps,bps,filename,bot,pack,dccstring,ip,port,progress,status,size</param>
        /// <returns>Object of current download, in order: Megabytes Per Second, Kilobytes Per Second, Bytes Per Second, filename, bot (source), pack (unique id for bot), dccstring (raw server return when asked for download), ip (of server where file is located), port (of server where file is located),progress, status of the current download(failed, running etc), size of file</returns>
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
            }
            else if (whichdownloaddetail == "size")
            {
                return dlDetails[2];
            }
            else
            {
                return null;
            }

        }


        /// <summary>
        ///get users in current channel
        /// </summary>
        public void getUsersInCurrentChannel()
        {
            con.getUsersInChannel("");
        }


        /// <summary>
        ///get users in different channel, parameter is the channel name of type string (example: "#yourchannel")
        /// </summary>
        /// <param name="channel"></param>
        public void getUsersInDifferentChannel(string channel)
        {
            con.getUsersInChannel(channel);
        }


        /// <summary>
        ///send message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool sendMessage(string message)
        {
            try
            {
                if (con.isConnectionEstablised)
                {
                    con.sendMsg(message);
                    return true;
                } else
                {
                    return false;
                }
            } catch {
                return false;
            }
                      
        }

        public bool sendRawMessage(string message)
        {
            try
            {
                if (con.isConnectionEstablised)
                {
                    con.sendRawMsg(message);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }

    }
}
