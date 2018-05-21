using System;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;

namespace SimpleIRCLib
{
    /// <summary>
    /// Class for downloading files using the DCC protocol on a sperarate thread from the main IRC Client thread.
    /// </summary>
    public class DCCClient
    {

        /// <summary>
        /// For firing update event using DCCEventArgs from DCCEventArgs.cs
        /// </summary>
        public event EventHandler<DCCEventArgs> OnDccEvent;
        /// <summary>
        /// For firing debug event using DCCDebugMessageArgs from DCCEventArgs.cs
        /// </summary>
        public event EventHandler<DCCDebugMessageArgs> OnDccDebugMessage;

        /// <summary>
        /// Raw DCC String used for getting the file location (server) and some basic file information
        /// </summary>
        public string NewDccString { get; set; }
        /// <summary>
        /// File name of the file being downloaded
        /// </summary>
        public string NewFileName { get; set; }
        /// <summary>
        /// Pack ID of file on bot where file resides
        /// </summary>
        public int NewPortNum { get; set; }
        /// <summary>
        /// FileSize of the file being downloaded
        /// </summary>
        public Int64 NewFileSize { get; set; }
        /// <summary>
        /// Port of server of file location
        /// </summary>
        public string NewIp { get; set; }
        /// <summary>
        /// Progress from 0-100 (%)
        /// </summary>
        public int Progress { get; set; }
        /// <summary>
        /// Download status, such as: WAITING,DOWNLOADING,FAILED:[ERROR],ABORTED
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Download speed in: KB/s
        /// </summary>
        public Int64 BytesPerSecond { get; set; }
        /// <summary>
        /// Download speed in: MB/s
        /// </summary>
        public int KBytesPerSecond { get; set; }
        /// <summary>
        /// Download status, such as: WAITING,DOWNLOADING,FAILED:[ERROR],ABORTED
        /// </summary>
        public int MBytesPerSecond { get; set; }
        /// <summary>
        /// Bot name where file resides
        /// </summary>
        public string BotName { get; set; }
        /// <summary>
        /// Pack ID of file on bot where file resides
        /// </summary>
        public string PackNum { get; set; }
        /// <summary>
        /// Check for status of DCCClient
        /// </summary>
        public bool IsDownloading { get; set; }
        /// <summary>
        /// Path to the file that is being downloaded, or has been downloaded
        /// </summary>
        public string CurrentFilePath { get; set; }

        /// <summary>
        /// Local bool to tell the while loop within the download thread to stop.
        /// </summary>
        private bool _shouldAbort = false;

        /// <summary>
        /// Client that is currently running, used for sending abort messages when a download fails, or a dcc string fails to parse.
        /// </summary>
        private IrcClient _ircClient;

        /// <summary>
        /// Current download directory that will be used when starting a download
        /// </summary>
        private string _curDownloadDir;

        /// <summary>
        /// Thread where download logic is running
        /// </summary>
        private Thread _downloader;

        /// <summary>
        /// Initial constructor.
        /// </summary>
        public DCCClient()
        {
            IsDownloading = false;
        }

        /// <summary>
        /// Starts a downloader by parsing the received message from the irc server on information
        /// </summary>
        /// <param name="dccString">message from irc server</param>
        /// <param name="downloaddir">download directory</param>
        /// <param name="bot">bot where the file came from</param>
        /// <param name="pack">pack on bot where the file came from</param>
        /// <param name="client">irc client used the moment it received the dcc message, used for sending abort messages when download fails unexpectedly</param>
        public void StartDownloader(string dccString, string downloaddir, string bot, string pack, IrcClient client)
        {
            if ((dccString ?? downloaddir ?? bot ?? pack) != null && dccString.Contains("SEND") && !IsDownloading)
            {
                NewDccString = dccString;
                _curDownloadDir = downloaddir;
                BotName = bot;
                PackNum = pack;
                _ircClient = client;

                //parsing the data for downloader thread

                UpdateStatus("PARSING");
                bool isParsed = ParseData(dccString);

                //try to set the necesary information for the downloader
                if (isParsed)
                {
                    _shouldAbort = false;
                   //start the downloader thread
                    _downloader = new Thread(new ThreadStart(this.Downloader));
                    _downloader.IsBackground = true;
                    _downloader.Start();
                }
                else
                {
                    OnDccDebugMessage?.Invoke(this,
                        new DCCDebugMessageArgs(
                            "Can't parse dcc string and start downloader, failed to parse data, removing from que\n", "DCC STARTER"));
                    _ircClient.SendMessageToAll("/msg " + BotName + " xdcc remove " + PackNum);
                    _ircClient.SendMessageToAll("/msg " + BotName + " xdcc cancel");
                }
            }
            else
            {
                if (IsDownloading)
                {
                    OnDccDebugMessage?.Invoke(this,
                        new DCCDebugMessageArgs("You are already downloading! Ignore SEND request\n", "DCC STARTER"));
                }
                else
                {
                    OnDccDebugMessage?.Invoke(this,
                        new DCCDebugMessageArgs("DCC String does not contain SEND and/or invalid values for parsing! Ignore SEND request\n", "DCC STARTER"));
                }
            }
        }

        /// <summary>
        /// Parses the received DCC string
        /// </summary>
        /// <param name="dccString">dcc string</param>
        /// <returns>returns true if parsing was succesfull, false if failed</returns>
        private bool ParseData(string dccString)
        {
            /*
           * :_bot PRIVMSG nickname :DCC SEND \"filename\" ip_networkbyteorder port filesize
           *AnimeDispenser!~desktop@Rizon-6AA4F43F.ip-37-187-118.eu PRIVMSG WeebIRCDev :DCC SEND "[LNS] Death Parade - 02 [BD 720p] [7287AE5C].mkv" 633042523 59538 258271780  
           *HelloKitty!~nyaa@ny.aa.ny.aa PRIVMSG WeebIRCDev :DCC SEND [Coalgirls]_Spirited_Away_(1280x692_Blu-ray_FLAC)_[5805EE6B].mkv 3281692293 35567 10393049211
           :[EWG]-bOnez!EWG@CRiTEN-BB8A59E9.ip-158-69-126.net PRIVMSG LittleWeeb_jtokck :DCC SEND The.Good.Doctor.S01E13.Seven.Reasons.1080p.AMZN.WEB-DL.DD+5.1.H.264-QOQ.mkv 2655354388 55000 1821620363
           *Ginpa2:DCC SEND "[HorribleSubs] Dies Irae - 05 [480p].mkv" 84036312 35016 153772128 
             */

            dccString = RemoveSpecialCharacters(dccString).Substring(1);
            OnDccDebugMessage?.Invoke(this,
                new DCCDebugMessageArgs("DCCClient: DCC STRING: " + dccString, "DCC PARSER"));


            if (!dccString.Contains(" :DCC"))
            {
                BotName = dccString.Split(':')[0];
                if (dccString.Contains("\""))
                {
                    NewFileName = dccString.Split('"')[1];

                    OnDccDebugMessage?.Invoke(this,
                        new DCCDebugMessageArgs("DCCClient1: FILENAME PARSED: " + NewFileName, "DCC PARSER"));
                    string[] thaimportantnumbers = dccString.Split('"')[2].Trim().Split(' ');
                    if (thaimportantnumbers[0].Contains(":"))
                    {
                        NewIp = thaimportantnumbers[0];
                    }
                    else
                    {
                        try
                        {

                            OnDccDebugMessage?.Invoke(this,
                                new DCCDebugMessageArgs("DCCClient1: PARSING FOLLOWING IPBYTES: " + thaimportantnumbers[0], "DCC PARSER"));
                            string ipAddress = UInt64ToIPAddress(Int64.Parse(thaimportantnumbers[0]));
                            NewIp = ipAddress;
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    OnDccDebugMessage?.Invoke(this,
                        new DCCDebugMessageArgs("DCCClient1: IP PARSED: " + NewIp, "DCC PARSER"));
                    NewPortNum = int.Parse(thaimportantnumbers[1]);
                    NewFileSize = Int64.Parse(thaimportantnumbers[2]);

                    return true;
                }
                else
                {
                    NewFileName = dccString.Split(' ')[2];


                    OnDccDebugMessage?.Invoke(this,
                        new DCCDebugMessageArgs("DCCClient2: FILENAME PARSED: " + NewFileName, "DCC PARSER"));

                    if (dccString.Split(' ')[3].Contains(":"))
                    {
                        NewIp = dccString.Split(' ')[3];
                    }
                    else
                    {
                        try
                        {


                            OnDccDebugMessage?.Invoke(this,
                                new DCCDebugMessageArgs("DCCClient2: PARSING FOLLOWING IPBYTES DIRECTLY: " + dccString.Split(' ')[3], "DCC PARSER"));
                            string ipAddress = UInt64ToIPAddress(Int64.Parse(dccString.Split(' ')[3]));
                            NewIp = ipAddress;
                        }
                        catch
                        {

                            return false;
                        }
                    }
                    OnDccDebugMessage?.Invoke(this,
                        new DCCDebugMessageArgs("DCCClient2: IP PARSED: " + NewIp, "DCC PARSER"));
                    NewPortNum = int.Parse(dccString.Split(' ')[4]);
                    NewFileSize = Int64.Parse(dccString.Split(' ')[5]);
                    return true;
                }
            } else
            {
                BotName = dccString.Split('!')[0];
                if (dccString.Contains("\""))
                {
                    NewFileName = dccString.Split('"')[1];

                    OnDccDebugMessage?.Invoke(this,
                        new DCCDebugMessageArgs("DCCClient3: FILENAME PARSED: " + NewFileName, "DCC PARSER"));
                    string[] thaimportantnumbers = dccString.Split('"')[2].Trim().Split(' ');

                    if (thaimportantnumbers[0].Contains(":"))
                    {
                        NewIp = thaimportantnumbers[0];
                    }
                    else
                    {
                        try
                        {

                            OnDccDebugMessage?.Invoke(this,
                                new DCCDebugMessageArgs("DCCClient3: PARSING FOLLOWING IPBYTES DIRECTLY: " + thaimportantnumbers[0], "DCC PARSER"));
                            string ipAddress = UInt64ToIPAddress(Int64.Parse(thaimportantnumbers[0]));
                            NewIp = ipAddress;
                        }
                        catch
                        {
                            return false;
                        }
                    }


                    OnDccDebugMessage?.Invoke(this,
                        new DCCDebugMessageArgs("DCCClient3: IP PARSED: " + NewIp, "DCC PARSER"));
                    NewPortNum = int.Parse(thaimportantnumbers[1]);
                    NewFileSize = Int64.Parse(thaimportantnumbers[2]);
                    return true;
                } else
                {
                    NewFileName = dccString.Split(' ')[5];

                    OnDccDebugMessage?.Invoke(this,
                        new DCCDebugMessageArgs("DCCClient4: FILENAME PARSED: " + NewFileName, "DCC PARSER"));

                    if (dccString.Split(' ')[6].Contains(":"))
                    {
                        NewIp = dccString.Split(' ')[6];
                    } else
                    {
                        try
                        {

                            OnDccDebugMessage?.Invoke(this,
                                new DCCDebugMessageArgs("DCCClient4: PARSING FOLLOWING IPBYTES DIRECTLY: " + dccString.Split(' ')[6], "DCC PARSER"));
                            string ipAddress = UInt64ToIPAddress(Int64.Parse(dccString.Split(' ')[6]));
                            NewIp = ipAddress;
                        }
                        catch
                        {
                            return false;
                        }

                    }

                    OnDccDebugMessage?.Invoke(this,
                        new DCCDebugMessageArgs("DCCClient4: IP PARSED: " + NewIp, "DCC PARSER"));
                    NewPortNum = int.Parse(dccString.Split(' ')[7]);
                    NewFileSize = Int64.Parse(dccString.Split(' ')[8]);
                    return true;

                }
                

            }

        }

        /// <summary>
        /// Method ran within downloader thread, starts a connection to the file server, and receives the file accordingly, sends updates using event handler during the download.
        /// </summary>
        public void Downloader()
        {

            UpdateStatus("WAITING");

            //combining download directory path with filename

            if (_curDownloadDir != null)
            {
                if (_curDownloadDir != string.Empty)
                {
                    if (_curDownloadDir.Length > 0)
                    {
                        if (!Directory.Exists(_curDownloadDir))
                        {
                            Directory.CreateDirectory(_curDownloadDir);
                        }
                    }
                    else
                    {
                        _curDownloadDir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Downloads");
                        if (!Directory.Exists(_curDownloadDir))
                        {
                            Directory.CreateDirectory(_curDownloadDir);
                        }
                    }
                }
                else
                {
                    _curDownloadDir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Downloads");
                    if (!Directory.Exists(_curDownloadDir))
                    {
                        Directory.CreateDirectory(_curDownloadDir);
                    }
                }
            }
            else
            {
                _curDownloadDir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Downloads");
                if (!Directory.Exists(_curDownloadDir))
                {
                    Directory.CreateDirectory(_curDownloadDir);
                }
            }
        
            string dlDirAndFileName = Path.Combine(_curDownloadDir, NewFileName);
            CurrentFilePath = dlDirAndFileName;
            try
            {
                if (!File.Exists(dlDirAndFileName))
                {
                    OnDccDebugMessage?.Invoke(this,
                        new DCCDebugMessageArgs("File does not exist yet, start connection \n ", "DCC DOWNLOADER"));

                    //start connection with tcp server
                    using (TcpClient dltcp = new TcpClient(NewIp, NewPortNum))
                    {
                        using (NetworkStream dlstream = dltcp.GetStream())
                        {
                            //succesfully connected to tcp server, status is downloading
                            UpdateStatus("DOWNLOADING");

                            //values to keep track of progress
                            Int64  bytesReceived = 0;
                            Int64  oldBytesReceived = 0;
                            Int64  oneprocent = NewFileSize / 100;
                            DateTime start = DateTime.Now;
                            bool timedOut = false;

                            //values to keep track of download position
                            int count;

                            //to me this buffer size seemed to be the most efficient.
                            byte[] buffer;
                            if (NewFileSize > 1048576)
                            {
                                OnDccDebugMessage?.Invoke(this,
                                    new DCCDebugMessageArgs("Big file, big buffer (1 mb) \n ", "DCC DOWNLOADER"));
                                buffer = new byte[16384];
                            } else if(NewFileSize < 1048576 && NewFileSize > 2048)
                            {
                                OnDccDebugMessage?.Invoke(this,
                                    new DCCDebugMessageArgs("Smaller file (< 1 mb), smaller buffer (2 kb) \n ", "DCC DOWNLOADER"));
                                buffer = new byte[2048];
                            } else if (NewFileSize < 2048 && NewFileSize > 128)
                            {
                                OnDccDebugMessage?.Invoke(this,
                                    new DCCDebugMessageArgs("Small file (< 2kb mb), small buffer (128 b) \n ", "DCC DOWNLOADER"));
                                buffer = new byte[128];
                            } else
                            {
                                OnDccDebugMessage?.Invoke(this,
                                    new DCCDebugMessageArgs("Tiny file (< 128 b), tiny buffer (2 b) \n ", "DCC DOWNLOADER"));
                                buffer = new byte[2];
                            }
                                

                            //create file to write to
                            using (FileStream writeStream = new FileStream(dlDirAndFileName, FileMode.Append, FileAccess.Write, FileShare.Read))
                            {
                                writeStream.SetLength(NewFileSize);
                                IsDownloading = true;
                                //download while connected and filesize is not reached
                                while (dltcp.Connected && bytesReceived < NewFileSize && !_shouldAbort)
                                {
                                    if (_shouldAbort)
                                    {
                                        dltcp.Close();
                                        dlstream.Dispose();
                                        writeStream.Close();
                                    }
                                    //keep track of progress
                                    DateTime end = DateTime.Now;
                                    if (end.Second !=  start.Second)
                                    {

                                        BytesPerSecond = bytesReceived - oldBytesReceived;
                                        KBytesPerSecond = (int)(BytesPerSecond / 1024);
                                        MBytesPerSecond = (KBytesPerSecond / 1024);
                                        oldBytesReceived = bytesReceived;
                                        start = DateTime.Now;
                                        UpdateStatus("DOWNLOADING");
                                    }

                                    //count bytes received
                                    count = dlstream.Read(buffer, 0, buffer.Length);

                                    //write to file
                                    writeStream.Write(buffer, 0, count);

                                    //count bytes received
                                    bytesReceived += count;

                                    Progress = (int)(bytesReceived / oneprocent);
                                }

                                //close all connections and streams (just to be save)
                                dltcp.Close();
                                dlstream.Dispose();
                                writeStream.Close();

                                IsDownloading = false;

                                if (_shouldAbort)
                                {
                                    try
                                    {
                                        OnDccDebugMessage?.Invoke(this,
                                            new DCCDebugMessageArgs("Downloader Stopped", "DCC DOWNLOADER"));
                                        OnDccDebugMessage?.Invoke(this,
                                            new DCCDebugMessageArgs("File " + CurrentFilePath + " will be deleted due to aborting", "DCC DOWNLOADER"));
                                        File.Delete(CurrentFilePath);

                                    }
                                    catch (Exception e)
                                    {
                                        OnDccDebugMessage?.Invoke(this,
                                            new DCCDebugMessageArgs("File " + CurrentFilePath + " probably doesn't exist :X", "DCC DOWNLOADER"));
                                        OnDccDebugMessage?.Invoke(this,
                                            new DCCDebugMessageArgs(e.ToString(), "DCC DOWNLOADER"));
                                    }

                                    UpdateStatus("ABORTED");
                                } else
                                {

                                    //consider 95% downloaded as done, files are sequentually downloaded, sometimes download stops early, but the file still is usable
                                    if (Progress < 95 && !_shouldAbort)
                                    {
                                        UpdateStatus("FAILED");
                                        OnDccDebugMessage?.Invoke(this,
                                            new DCCDebugMessageArgs("Download stopped at < 95 % finished, deleting file: " + NewFileName + " \n", "DCC DOWNLOADER"));
                                        OnDccDebugMessage?.Invoke(this,
                                            new DCCDebugMessageArgs("Download stopped at : " + bytesReceived + " bytes, a total of " + Progress + "%", "DCC DOWNLOADER"));
                                        File.Delete(dlDirAndFileName);
                                        timedOut = false;


                                    }
                                    else if (timedOut && Progress < 95 && !_shouldAbort)
                                    {
                                        UpdateStatus("FAILED: TIMED OUT");
                                        OnDccDebugMessage?.Invoke(this,
                                            new DCCDebugMessageArgs("Download timed out at < 95 % finished, deleting file: " + NewFileName + " \n", "DCC DOWNLOADER"));
                                        OnDccDebugMessage?.Invoke(this,
                                            new DCCDebugMessageArgs("Download stopped at : " + bytesReceived + " bytes, a total of " + Progress + "%", "DCC DOWNLOADER"));
                                        File.Delete(dlDirAndFileName);
                                        timedOut = false;
                                    }
                                    else if (!_shouldAbort)
                                    {
                                        //make sure that in the event something happens and the downloader calls delete after finishing, the file will remain where it is.
                                        dlDirAndFileName = "";
                                        UpdateStatus("COMPLETED");
                                    }

                                }
                                _shouldAbort = false;

                            }
                        }
                    }
                }
                else
                {
                    OnDccDebugMessage?.Invoke(this,
                        new DCCDebugMessageArgs("File already exists, removing from xdcc que!\n", "DCC DOWNLOADER"));
                    _ircClient.SendMessageToAll("/msg " + BotName + " xdcc remove " + PackNum);
                    _ircClient.SendMessageToAll("/msg " + BotName + " xdcc cancel");
                    UpdateStatus("FAILED: ALREADY EXISTS");
                }
                OnDccDebugMessage?.Invoke(this,
                    new DCCDebugMessageArgs("File has been downloaded! \n File Location:" + CurrentFilePath , "DCC DOWNLOADER"));

            }
            catch (SocketException e)
            {
                OnDccDebugMessage?.Invoke(this,
                    new DCCDebugMessageArgs("Something went wrong while downloading the file! Removing from xdcc que to be sure! Error:\n" + e.ToString(), "DCC DOWNLOADER"));
                _ircClient.SendMessageToAll("/msg " + BotName + " xdcc remove " + PackNum);
                _ircClient.SendMessageToAll("/msg " + BotName + " xdcc cancel");
                UpdateStatus("FAILED: CONNECTING");
            }
            catch (Exception ex)
            {
                OnDccDebugMessage?.Invoke(this,
                    new DCCDebugMessageArgs("Something went wrong while downloading the file! Removing from xdcc que to be sure! Error:\n" + ex.ToString(), "DCC DOWNLOADER"));
                _ircClient.SendMessageToAll("/msg " + BotName + " xdcc remove " + PackNum);
                _ircClient.SendMessageToAll("/msg " + BotName + " xdcc cancel");
                UpdateStatus("FAILED: UNKNOWN");
            }
            IsDownloading = false;
        }        

        /// <summary>
        /// Fires the event with the update about the download currently running
        /// </summary>
        /// <param name="statusin">the current status of the download</param>
        private void UpdateStatus(string statusin)
        {
            Status = statusin;
            OnDccEvent?.Invoke(this, new DCCEventArgs(this));
        }

        /// <summary>
        /// Stops a download if one is running, checks if the donwnloader thread actually stops.
        /// </summary>
        /// <returns>true if stopped succesfully</returns>
        public bool AbortDownloader(int timeOut)
        {
            if (CheckIfDownloading())
            {
                OnDccDebugMessage?.Invoke(this,
                    new DCCDebugMessageArgs("File " + CurrentFilePath + " will be deleted after aborting.", "DCC DOWNLOADER"));

                _shouldAbort = true;

                int timeout = 0;
                while (CheckIfDownloading())
                {
                    if (timeout > timeOut)
                    {
                        return false;
                    }
                    timeout++;
                    Thread.Sleep(1);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if download is still running.
        /// </summary>
        /// <returns>true if still downloading</returns>
        public bool CheckIfDownloading()
        {
            return IsDownloading;
        }

        /// <summary>
        /// Removes special characters from  a string (used for filenames)
        /// </summary>
        /// <param name="str">string to parse</param>
        /// <returns>string wihtout special chars</returns>
        private string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if (c > 31 && c < 219)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Reverses IP from little endian to big endian or vice versa depending on what succeeds.
        /// </summary>
        /// <param name="ip">ip string</param>
        /// <returns>reversed ip string</returns>
        private string ReverseIp(string ip)
        {
            string[] parts = ip.Trim().Split('.');
            if(parts.Length >= 3)
            {
                OnDccDebugMessage?.Invoke(this,
                    new DCCDebugMessageArgs("DCCClient: converting ip: " + ip, "DCC IP PARSER"));
                string newip = parts[3] + "." + parts[2] + "." + parts[1] + "." + parts[0];
                OnDccDebugMessage?.Invoke(this,
                    new DCCDebugMessageArgs("DCCClient: to: " + newip, "DCC IP PARSER"));

                return newip;
            } else
            {
                OnDccDebugMessage?.Invoke(this,
                    new DCCDebugMessageArgs("DCCClient: converting ip: " + ip, "DCC IP PARSER"));
                OnDccDebugMessage?.Invoke(this,
                    new DCCDebugMessageArgs("DCCClient: amount of parts: " + parts.Length, "DCC IP PARSER"));
                return "0.0.0.0";
            }
        }

        /// <summary>
        /// Converts a long/int64 to a ip string.
        /// </summary>
        /// <param name="address">int64 numbers representing IP address</param>
        /// <returns>string with ip</returns>
        private string UInt64ToIPAddress(Int64 address)
        {
            string ip = string.Empty;
            for (int i = 0; i < 4; i++)
            {
                int num = (int)(address / Math.Pow(256, (3 - i)));
                address = address - (long)(num * Math.Pow(256, (3 - i)));
                if (i == 0)
                {
                    ip = num.ToString();
                }
                else
                {
                    ip = ip + "." + num.ToString();
                }
            }
            return ip;
        }
    }
}
