using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Security;

namespace SimpleIRCLib
{
    /// <summary>
    /// For running IRC Client logic, here is where all the magic happens
    /// </summary>
    public class IrcClient
    {

        /// <summary>
        /// Event Handler for firing when a (parsed) message is received on a channel from other users, event uses IrcReceivedEventArgs from IrcEventArgs.cs
        /// </summary>
        public event EventHandler<IrcReceivedEventArgs> OnMessageReceived;

        /// <summary>
        /// Event Handler for firing when a raw message is received from the irc server, event uses IrcRawReceivedEventArgs from IrcEventArgs.cs
        /// </summary>
        public event EventHandler<IrcRawReceivedEventArgs> OnRawMessageReceived;

        /// <summary>
        /// Event Handler for firing when a list with users per channel is received, event uses IrcUserListReceivedEventArgs from IrcEventArgs.cs
        /// </summary>
        public event EventHandler<IrcUserListReceivedEventArgs> OnUserListReceived;

        /// <summary>
        /// Event Handler for firing when this client wants to send a debug message, event uses IrcDebugMessageEventArgs from IrcEventArgs.cs
        /// </summary>
        public event EventHandler<IrcDebugMessageEventArgs> OnDebugMessage;

        /// <summary>
        /// Port of irc server to connect to
        /// </summary>
        private int _newPort;
        /// <summary>
        /// Username to register on irc server
        /// </summary>
        private string _newUsername;
        /// <summary>
        /// Password to connect to a secured irc server
        /// </summary>
        private string _newPassword;
        /// <summary>
        /// Ip address of irc server
        /// </summary>
        private string _newIp;
        /// <summary>
        /// List with channels seperated with ',' to join when connecting to IRC server
        /// </summary>
        private string _newChannels;
        /// <summary>
        /// timeout before throwing timeout errors (using OnDebugMessage)
        /// </summary>
        private int _timeOut;
        /// <summary>
        /// download directory used for DCCClient.cs
        /// </summary>
        private string _downloadDirectory;
        /// <summary>
        /// for enabling tls/ssl secured connection to the irc server
        /// </summary>
        private bool _enableSSL;
        /// <summary>
        /// for checking if a connection is succesfully established
        /// </summary>
        private bool _isConnectionEstablised;
        /// <summary>
        /// for checking if client has started listening to the server
        /// </summary>
        private bool _isClientRunning;
        /// <summary>
        /// packnumber used to initialize dccclient downloader
        /// </summary>
        private string _packNumber;
        /// <summary>
        /// bot used to initialzie dccclient downloader
        /// </summary>
        private string _bot;
        /// <summary>
        /// DCCClient to be used for downloading
        /// </summary>
        private DCCClient _dccClient;
        /// <summary>
        /// TcpClient used for connecting to the irc server
        /// </summary>
        private TcpClient _tcpClient;
        /// <summary>
        /// IRC commands
        /// </summary>
        private IrcCommands _ircCommands;
        /// <summary>
        /// unsecure network stream for listening and reading to the irc server
        /// </summary>
        private NetworkStream _networkStream;
        /// <summary>
        /// secure network stream for listening and reading to the irc server
        /// </summary>
        private SslStream _networkSStream;
        /// <summary>
        /// used for writing to the irc server
        /// </summary>
        private StreamWriter _streamWriter;
        /// <summary>
        /// used for reading from the irc server
        /// </summary>
        private StreamReader _streamReader;
        /// <summary>
        /// receiver task
        /// </summary>
        private Task _receiverTask = null;
        /// <summary>
        /// used for stopping the receiver task 
        /// </summary>
        private bool _stopTask = false;

        /// <summary>
        /// Default constructor, needed so that the client can register events before starting the connection.
        /// </summary>
        public IrcClient()
        {
            _isConnectionEstablised = false;
            _isClientRunning = false;
        }

        /// <summary>
        /// Sets up the information needed for the client to start a connection to the irc server. Sends a warning to the debug message event if ports are out of the standard specified ports for IRC.
        /// </summary>
        /// <param name="ip">Server address, possibly works with dns addresses (irc.xxx.x), but ip addresses works just fine (of type string)</param>
        /// <param name="username">Username the client wants to use, of type string</param>
        /// <param name="channels">Channel(s) the client wants to connect to, possible to connect to multiple channels at once by seperating each channel with a ',' (Example: #chan1,#chan2), of type string</param>
        /// <param name="dccClient">DCC Client, for downloading using the DCC protocol</param>
        /// <param name="downloadDirectory">Download Directory, used by the DCC CLient to store files in the specified directory, if left empty, it will create a "Download" directory within the same folder where this library resides</param>
        /// <param name="port">Port, optional parameter, where default = 0 (Automatic port selection), is port of the server you want to connect to, of type int</param>
        /// <param name="password">Password, optional parameter, where default value is "", can be used to connect to a password protected server.</param>
        /// <param name="timeout">Timeout, optional parameter, where default value is 3000 milliseconds, the maximum time before a server needs to answer, otherwise errors are thrown.</param>
        /// <param name="enableSSL">Timeout, optional parameter, where default value is 3000 milliseconds, the maximum time before a server needs to answer, otherwise errors are thrown.</param>
        public void SetConnectionInformation(string ip, string username, string channels,
            DCCClient dccClient, string downloadDirectory, int port = 0, string password = "", int timeout = 3000, bool enableSSL = true)
        {
            _newIp = ip;
            _newPort = port;
            _newUsername = username;
            _newPassword = password;
            _newChannels = channels;
            _isConnectionEstablised = false;
            _isClientRunning = false;
            _timeOut = timeout;
            _downloadDirectory = downloadDirectory;
            _dccClient = dccClient;
            _enableSSL = enableSSL;

            if (_enableSSL)
            {
                if (port == 0)
                {
                    port = 6697;
                } else if (port != 6697)
                {
                    OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("PORT: " + port.ToString() + " IS NOT COMMONLY USED FOR TLS/SSL CONNECTIONS, PREFER TO USE 6697 FOR SSL!", "SETUP WARNING"));
                }
            }
            else
            {
                if (port == 0)
                {
                    port = 6667;
                }
                else if (port < 6665 && port > 6669)
                {
                    OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("PORT: " + port.ToString() + " IS NOT COMMONLY USED FOR NON TLS/SSL CONNECTIONS, PREFER TO USE PORTS BETWEEN 6665 & 6669!", "SETUP WARNING"));
                }
            }
        }

        /// <summary>
        /// Changes the download directory, will apply to the next instantiated download.
        /// </summary>
        /// <param name="downloadDirectory">Path to directory (creates it if it does not exist)</param>
        public void SetDownloadDirectory(string downloadDirectory)
        {
            _downloadDirectory = downloadDirectory;
        }

        /// <summary>
        /// Starts the connection to the irc server, sends the register user command and register nick name command.
        /// </summary>
        /// <returns>true/false depending if error coccured</returns>
        public bool Connect()
        {
            try
            {
                _isConnectionEstablised = false;
                _receiverTask = new Task(StartReceivingChat);
                _receiverTask.Start();

                int timeout = 0;
                while (!_isConnectionEstablised)
                {
                    Thread.Sleep(1);
                    if (timeout > _timeOut)
                    {
                        return false;
                    }
                    timeout++;
                }

                OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("IRC CLIENT SUCCESFULLY RUNNING", "IRC SETUP"));
                return true;
            }
            catch(Exception e)
            {
                OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs(e.ToString(), "SETUP ERROR"));
                return false;
            }
        }

        /// <summary>
        /// Starts quiting procedure, sends QUIT message to server and waits until server closes connection with the client, after that, it shuts down every reader and stream, and stops the receiver task.
        /// </summary>
        /// <returns></returns>
        public bool QuitConnect()
        {
            OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("STARTING IRC CLIENT SHUTDOWN PROCEDURE", "QUIT"));
            //send quit to server
            if (WriteIrc("QUIT"))
            {
                int timeout = 0;
                while (_tcpClient.Connected)
                {
                    Thread.Sleep(1);
                    if (timeout >= _timeOut)
                    {
                        return false;
                    }
                    timeout++;
                }

                _stopTask = true;
                Thread.Sleep(200);
                //stop everything in right order
                _receiverTask.Dispose();
                _streamReader.Dispose();
                _networkStream.Close();
                _streamWriter.Close();
                _tcpClient.Close();

                _isConnectionEstablised = false;

                OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("FINISHED SHUTDOWN PROCEDURE", "QUIT"));
                return true;

            }
            else
            {
                OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("COULD NOT WRITE QUIT COMMAND TO SERVER", "QUIT"));
                return true;
            }
        }

        /// <summary>
        /// Starts the receiver task.
        /// </summary>
        public void StartReceivingChat()
        {

            _tcpClient = new TcpClient(_newIp, _newPort);

            int timeout = 0;
            while (!_tcpClient.Connected)
            {
                Thread.Sleep(1);

                if (timeout >= _timeOut)
                {
                    OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("TIMEOUT, COULD NOT CONNECT TO TCP SOCKET", "IRC SETUP"));
                }
                timeout++;
            }


            if (!_enableSSL)
            {
                _networkStream = _tcpClient.GetStream(); Thread.Sleep(500);
                _streamReader = new StreamReader(_networkStream);
                _streamWriter = new StreamWriter(_networkStream);
                _ircCommands = new IrcCommands(_networkStream);
            }
            else
            {
                _networkSStream = new SslStream(_tcpClient.GetStream());
                _networkSStream.AuthenticateAsClient(_newIp);
                _streamReader = new StreamReader(_networkSStream);
                _streamWriter = new StreamWriter(_networkSStream);
                _ircCommands = new IrcCommands(_networkSStream);

            }


            _isConnectionEstablised = true;
            OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("CONNECTED TO TCP SOCKET", "IRC SETUP"));



            if (_newPassword.Length > 0)
            {
                if (!_ircCommands.SetPassWord(_newPassword))
                {
                    OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs(_ircCommands.GetErrorMessage(), "IRC SETUP ERROR"));
                    _isConnectionEstablised = false;
                }
            }
            Debug.WriteLine("Joining channels: " + _newChannels);
            if (!_ircCommands.JoinNetwork(_newUsername, _newChannels))
            {
                OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs(_ircCommands.GetErrorMessage(), "IRC SETUP ERROR"));
                _isConnectionEstablised = false;
            }

            if (_isConnectionEstablised)
            {
                OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("CONNECTED TO IRC SERVER", "IRC SETUP"));

                Task.Run(() => ReceiveChat());
            }
        }

        /// <summary>
        /// Receiver task, receives messages from server, handles joining intial join to channels, if the server responses with a 004 (which is a welcome message, meaning it has succesfully connected)
        /// Handles PRIVMSG messages
        /// Handles PING messages
        /// Handles JOIN messages
        /// Handles QUIT messages (though it's not yet possible to determine which channels these users have left
        /// Handles 353 messages (Users list per channel)
        /// Handles 366 messages (Finished user list per channel)
        /// Handles DCC SEND messages
        /// </summary>
        private void ReceiveChat()
        {
            try
            {
                OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("STARTING LISTENER!", "IRC RECEIVER"));
              

                Dictionary<string, List<string>> usersPerChannelDictionary = new Dictionary<string, List<string>>();

                _isClientRunning = true;
                _isConnectionEstablised = true;
                while (!_stopTask)
                {

                    string ircData = _streamReader.ReadLine();

                    OnRawMessageReceived?.Invoke(this, new IrcRawReceivedEventArgs(ircData));

                    

                    if (ircData.Contains("PING"))
                    {
                        string pingID = ircData.Split(':')[1];
                        WriteIrc("PONG :" + pingID);
                    }
                    if ( ircData.Contains("PRIVMSG"))
                    {


                        //:MrRareie!~MrRareie_@Rizon-AC4B78B2.cm-3-2a.dynamic.ziggo.nl PRIVMSG #RareIRC :wassup


                        
                        try
                        {
                            string messageAndChannel = ircData.Split(new string[] { "PRIVMSG" }, StringSplitOptions.None)[1];
                            string message = messageAndChannel.Split(':')[1].Trim();
                            string channel = messageAndChannel.Split(':')[0].Trim();
                            string user = ircData.Split(new string[] { "PRIVMSG" }, StringSplitOptions.None)[0].Split('!')[0].Substring(1);

                            channel = channel.Replace("=", string.Empty);

                            OnMessageReceived?.Invoke(this, new IrcReceivedEventArgs(message, user, channel));
                        }
                        catch(Exception ex)
                        {
                            OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs(ex.ToString(), "MESSAGE RECEIVED ERROR (PRIVMSG)"));
                        }



                    }
                    else if (ircData.Contains("JOIN"))
                    {
                        //RAW: :napiz!~napiz@Rizon-BF96A69D.dyn.optonline.net JOIN :#NIBL


                        try
                        {
                            string channel = ircData.Split(new string[] { "JOIN" }, StringSplitOptions.None)[1].Split(':')[1];
                            string userThatJoined = ircData.Split(new string[] { "JOIN" }, StringSplitOptions.None)[0].Split(':')[1].Split('!')[0];

                            OnMessageReceived?.Invoke(this, new IrcReceivedEventArgs("User Joined", userThatJoined, channel));
                        }
                        catch (Exception ex)
                        {
                            OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs(ex.ToString(), "MESSAGE RECEIVED ERROR (JOIN)"));
                        }

                    }
                    else if (ircData.Contains("QUIT"))
                    {

                        //RAW: :MrRareie!~MrRareie_@Rizon-AC4B78B2.cm-3-2a.dynamic.ziggo.nl QUIT 
                        try
                        {
                            string user = ircData.Split(new string[] { "QUIT" }, StringSplitOptions.None)[0].Split('!')[0].Substring(1);

                            OnMessageReceived?.Invoke(this, new IrcReceivedEventArgs("User Left", user, "unknown"));
                        }
                        catch (Exception ex)
                        {
                            OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs(ex.ToString(), "MESSAGE RECEIVED ERROR (JOIN)"));
                        };
                    }

                    if (ircData.Contains("DCC SEND") && ircData.Contains(_newUsername))
                    {
                        _dccClient.StartDownloader(ircData, _downloadDirectory, _bot, _packNumber, this);
                    }

                    //RareIRC_Client = #weebirc :RareIRC_Client
                    if (ircData.Contains(" 353 "))
                    {
                        //:irc.x2x.cc 353 RoflHerp = #RareIRC :RoflHerp @MrRareie
                        try
                        {
                            string channel = ircData.Split(new[] { " " + _newUsername + " ="}, StringSplitOptions.None)[1].Split(':')[0].Replace(" ", string.Empty); 
                            string userListFullString = ircData.Split(new[] { " " + _newUsername + " =" }, StringSplitOptions.None)[1].Split(':')[1];
                           

                            if (!channel.Contains(_newUsername) && !channel.Contains("="))
                            {
                                string[] users = userListFullString.Split(' ');
                                if (usersPerChannelDictionary.ContainsKey(channel))
                                {
                                    usersPerChannelDictionary.TryGetValue(channel, out var currentUsers);


                                    foreach (string name in users)
                                    {
                                        if (!name.Contains(_newUsername))
                                        {
                                            currentUsers.Add(name);
                                        }
                                    }
                                    usersPerChannelDictionary[channel.Trim()] = currentUsers;
                                }
                                else
                                {
                                    List<string> currentUsers = new List<string>();
                                    foreach (string name in users)
                                    {
                                        currentUsers.Add(name);
                                    }
                                    usersPerChannelDictionary.Add(channel.Trim(), currentUsers);
                                }
                            }
                          

                        } catch (Exception ex)
                        {
                            OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs(ex.ToString(), "MESSAGE RECEIVED ERROR (USERLIST)"));
                        }
                                     
                        
                    }

                    if(ircData.ToLower().Contains(" 366 "))
                    {
                        OnUserListReceived?.Invoke(this, new IrcUserListReceivedEventArgs(usersPerChannelDictionary));
                        usersPerChannelDictionary.Clear();
                    }
                    Thread.Sleep(1);
                }

                OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("RECEIVER HAS STOPPED RUNNING", "MESSAGE RECEIVER"));

                QuitConnect();
                _stopTask = false;
            } catch (Exception ioex)
            {
                OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("LOST CONNECTION: " + ioex.ToString(), "MESSAGE RECEIVER"));
                if (_isConnectionEstablised)
                {
                    _stopTask = false;
                    QuitConnect();
                }
            }
            _isClientRunning = false;
        }   

       
        /// <summary>
        /// Sends message to all channels, if message is not one of the following:
        /// /msg [bot] xdcc send [pack]
        /// /msg [bot] xdcc cancel
        /// /msg [bot] xdcc remove [pack]
        /// /names [channels] (can be empty)
        /// /join [channels]
        /// /quit
        /// /msg [channel/user] [message]
        /// </summary>
        /// <param name="input">String to send</param>
        /// <returns>true / false depending if it could send the message to the server</returns>
        public bool SendMessageToAll(string input)
        {

            input = input.Trim();
            Regex regex1 = new Regex(@"^(?=.*(?<botname>(?<=/msg)(.*)(?=xdcc)))(?=.*(?<packnum>(?<=(send)+(\s))(.*)))");
            Match matches1 = regex1.Match(input.ToLower());
            Regex regex2 = new Regex(@"^(?=.*(?<botname>(?<=/msg)(.*)(?=xdcc)))(?=.*(?<packnum>(?<=(cancel))(.*)))");
            Match matches2 = regex2.Match(input.ToLower());
            Regex regex3 = new Regex(@"^(?=.*(?<botname>(?<=/msg)(.*)(?=xdcc)))(?=.*(?<packnum>(?<=(remove)+(\s))(.*)))");
            Match matches3 = regex3.Match(input.ToLower());

            if (matches1.Success)
            {
                _bot = matches1.Groups["botname"].Value.Trim(); 
                _packNumber = matches1.Groups["packnum"].Value.Trim(); 
                string xdccdl = "PRIVMSG " + _bot + " :XDCC SEND " + _packNumber;
                return WriteIrc(xdccdl);
            }
            else if (matches2.Success)
            {
                _bot = matches2.Groups["botname"].Value;
                string xdcccl = "PRIVMSG " + _bot + " :XDCC CANCEL";
                return WriteIrc(xdcccl);
            }
            else if (matches3.Success)
            {
                _bot = matches3.Groups["botname"].Value;
                _packNumber = matches3.Groups["packnum"].Value;
                string xdccdl = "PRIVMSG " + _bot + " :XDCC REMOVE " + _packNumber;
                return WriteIrc(xdccdl);
            } else if (input.ToLower().Contains("/names"))
            {
                if (input.Contains("#"))
                {
                    string channelList = input.Split(' ')[1];
                    return GetUsersInChannel(channelList);
                } else
                {
                    return GetUsersInChannel("");
                }
            }
            else if (input.ToLower().Contains("/join"))
            {

                if (input.Split(' ').Length > 0)
                {
                    string channels = input.Split(' ')[1];
                    _newChannels += channels;
                    return WriteIrc("JOIN " + channels);
                }
                else
                {
                    return false;
                }
            }
            else if(input.ToLower().Contains("/quit"))
            {
                return QuitConnect();
            }
            else if (input.ToLower().Contains("/msg"))
            {
                return WriteIrc(input.Replace("/msg", "PRIVMSG"));
            }
            else
            {
                return WriteIrc("PRIVMSG " + _newChannels + " :" + input);
            }

          
        }


        /// <summary>
        /// Sends message to specific channels, if message is not one of the following:
        /// /msg [bot] xdcc send [pack]
        /// /msg [bot] xdcc cancel
        /// /msg [bot] xdcc remove [pack]
        /// /names [channels] (can be empty)
        /// /join [channels]
        /// /quit
        /// /msg [channel/user] [message]
        /// </summary>
        /// <param name="input">String to send</param>
        /// <param name="channel">Channel(s) to send to</param>
        /// <returns>true / false depending if it could send the message to the server</returns>
        public bool SendMessageToChannel(string input, string channel)
        {

            input = input.Trim();
            Regex regex1 = new Regex(@"^(?=.*(?<botname>(?<=/msg)(.*)(?=xdcc)))(?=.*(?<packnum>(?<=(send)+(\s))(.*)))");
            Match matches1 = regex1.Match(input.ToLower());
            Regex regex2 = new Regex(@"^(?=.*(?<botname>(?<=/msg)(.*)(?=xdcc)))(?=.*(?<packnum>(?<=(cancel))(.*)))");
            Match matches2 = regex2.Match(input.ToLower());
            Regex regex3 = new Regex(@"^(?=.*(?<botname>(?<=/msg)(.*)(?=xdcc)))(?=.*(?<packnum>(?<=(remove)+(\s))(.*)))");
            Match matches3 = regex3.Match(input.ToLower());

            if (matches1.Success)
            {
                _bot = matches1.Groups["botname"].Value.Trim();
                _packNumber = matches1.Groups["packnum"].Value.Trim();
                string xdccdl = "PRIVMSG " + _bot + " :XDCC SEND " + _packNumber;
                return WriteIrc(xdccdl);
            }
            else if (matches2.Success)
            {
                _bot = matches2.Groups["botname"].Value;
                string xdcccl = "PRIVMSG " + _bot + " :XDCC CANCEL";
                return WriteIrc(xdcccl);
            }
            else if (matches3.Success)
            {
                _bot = matches3.Groups["botname"].Value;
                _packNumber = matches3.Groups["packnum"].Value;
                string xdccdl = "PRIVMSG " + _bot + " :XDCC REMOVE " + _packNumber;
                return WriteIrc(xdccdl);
            }
            else if (input.ToLower().Contains("/names"))
            {
                if (input.Contains("#"))
                {
                    string channelList = input.Split(' ')[1];
                    return GetUsersInChannel(channelList);
                }
                else
                {
                    return GetUsersInChannel(channel);
                }
            }
            else if (input.ToLower().Contains("/join"))
            {

                if (input.Split(' ').Length > 0)
                {
                    string channels = input.Split(' ')[1];
                    _newChannels += channels;
                    return WriteIrc("JOIN " + channels);
                }
                else
                {
                    return false;
                }
            }
            else if (input.ToLower().Contains("/quit"))
            {
                return QuitConnect();
            }
            else if (input.ToLower().Contains("/msg"))
            {
                return WriteIrc(input.Replace("/msg", "PRIVMSG"));
            }
            else
            {
                return WriteIrc("PRIVMSG " + channel + " :" + input);
            }


        }

        /// <summary>
        /// Sends a raw message to the irc server, without any parsing applied
        /// </summary>
        /// <param name="msg"> message to send</param>
        /// <returns>true/false depending if it could write to the irc server</returns>
        public bool SendRawMsg(string msg)
        {
            return WriteIrc(msg);
        }

        /// <summary>
        /// Sends the get names for specific channels or for all channels.
        /// </summary>
        /// <param name="channel">channel, optional, default = "" = all channels,</param>
        /// <returns>true/false depending if it could write to the server</returns>
        public bool GetUsersInChannel(string channel = "")
        {
            
            if (channel != "")
            {
                return WriteIrc("NAMES " + channel);
            } else
            {
                return WriteIrc("NAMES " + _newChannels);
            }
        }

        /// <summary>
        /// Writes a message to the irc server.
        /// </summary>
        /// <param name="input">Message to send</param>
        /// <returns>true/false depending if it could write to the irc server</returns>
        public bool WriteIrc(string input)
        {
            try
            {
                _streamWriter.Write(input + Environment.NewLine);
                _streamWriter.Flush();
                return true;
            } catch(Exception e)
            {
                OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("Could not send message" + input + ", _tcpClient client is not running :X, error : " + e.ToString(), "MESSAGE SENDER")); ;
                return false;
            }

        }

        /// <summary>
        /// Stops a download if a download is running.
        /// </summary>
        /// <returns>true/false depending if an error occured or not</returns>
        public bool StopXDCCDownload()
        {
            try
            {
                return !_dccClient.AbortDownloader(_timeOut);
            } catch (Exception e)
            {
                OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("Could not stop XDCC Download, error: " + e.ToString(), "IRC CLIENT XDCC STOP"));
                return true;
            }
        }
        
        /// <summary>
        /// Checks if a download is running or not.
        /// </summary>
        /// <returns>true/false depending if a download is running, or if an error occured</returns>
        public bool CheckIfDownloading()
        {
            try
            {
                return _dccClient.CheckIfDownloading();
            }
            catch (Exception e)
            {
                OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("Could not check if download has started, error: " + e.ToString(), "IRC CLIENT XDCC CHECK"));
                return false;
            }
        }

        /// <summary>
        /// Stops the client.
        /// </summary>
        public bool StopClient()
        {
            _stopTask = true;
            OnDebugMessage?.Invoke(this, new IrcDebugMessageEventArgs("CLOSING CLIENT", "CLOSE"));
            return QuitConnect();
        }

        /// <summary>
        /// Gets the connection status
        /// </summary>
        /// <returns>true/false depending on the connection status</returns>
        public bool IsConnectionEstablished()
        {
            return _isConnectionEstablised;
        }

        /// <summary>
        /// Gets the client status
        /// </summary>
        /// <returns>true or false depending if the client is running or not</returns>
        public bool IsClientRunning()
        {
            return _isClientRunning;
        }
    }

    
}
