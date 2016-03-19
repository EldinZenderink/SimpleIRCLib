using System;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;


namespace SimpleIRCLib
{
    class DCCClient
    {
        //public available information for checking download Status and information
        public string newDccString { get; set; }
        public string newFileName { get; set; }
        public int newPortNum { get; set; }
        public int newFileSize { get; set; }
        public string newIp { get; set; }
        public int Progress { get; set; }
        public string Status { get; set; }
        public long Bytes_Seconds { get; set; }
        public int KBytes_Seconds { get; set; }
        public int MBytes_Seconds { get; set; }
        public string botName { get; set; }
        public string packNum { get; set; }
        public bool isDownloading { get; set; }

        //class linking of some sort
        private SimpleIRC simpleirc;
        private IrcConnect ircConnect;

        //contains the downloaddir
        private string curDownloadDir;
        //async thread for downloading
        private Thread downloader;

        //overload constructor
        public DCCClient(SimpleIRC sirc, IrcConnect ircCon)
        {
            simpleirc = sirc;
            ircConnect = ircCon;
        }

        //parses data from dcc string and start the downloader thread
        public void startDownloader(string dccString, string downloaddir, string bot, string pack)
        {
            if ((dccString ?? downloaddir ?? bot ?? pack) != null && dccString.Contains("SEND") && !isDownloading)
            {
                newDccString = dccString;
                curDownloadDir = downloaddir;
                botName = bot;
                packNum = pack;

                //parsing the data for downloader thread

                updateStatus("Parsing");
                bool isParsed = parseData();

                //try to set the necesary information for the downloader
                if (isParsed)
                {
                    //start the downloader thread
                    downloader = new Thread(new ThreadStart(this.Downloader));
                    downloader.Start();
                }
                else
                {
                    simpleirc.DebugCallBack("Can't parse dcc string and start downloader, failed to parse data, removing from que\n");
                    ircConnect.sendMsg("/msg " + botName + " xdcc remove " + packNum);
                    ircConnect.sendMsg("/msg " + botName + " xdcc cancel");
                }
            }
            else
            {
                if (isDownloading)
                {
                    simpleirc.DebugCallBack("You are already downloading! Removing from que\n");
                }
                else
                {
                    simpleirc.DebugCallBack("DCC String does not contain SEND and/or invalid values for parsing! Removing from que\n");

                }
                ircConnect.sendMsg("/msg " + botName + " xdcc remove " + packNum);
                ircConnect.sendMsg("/msg " + botName + " xdcc cancel");
            }
        }

        //parses data received by the dcc strings, necesary for details to connect to the right tcp server where file is located
        private bool parseData()
        {
            /*
           * :bot PRIVMSG nickname :DCC SEND \"filename\" ip_networkbyteorder port filesize
           *AnimeDispenser!~desktop@Rizon-6AA4F43F.ip-37-187-118.eu PRIVMSG WeebIRCDev :DCC SEND "[LNS] Death Parade - 02 [BD 720p] [7287AE5C].mkv" 633042523 59538 258271780  
           *HelloKitty!~nyaa@ny.aa.ny.aa PRIVMSG WeebIRCDev :DCC SEND [Coalgirls]_Spirited_Away_(1280x692_Blu-ray_FLAC)_[5805EE6B].mkv 3281692293 35567 10393049211
          */
            Regex regex = new Regex(@"^(?=.*(?<filename>(?<=\SEND )(.*?)(?=(\d{9,10}))))(?=.*(?<bitwiseip>((\s)+(\d{9,10}))+(\s)))(?=.*(?<port>((\s)+(\d{4,6})+(\s))))(?=.*(?<size>(\s)+(\d+)(?!.*\d)))");
            Match matches = regex.Match(newDccString.Trim());

            if (matches.Success)
            {
                newFileName = matches.Groups["filename"].Value.Trim();
                char[] invalidFileChars = Path.GetInvalidFileNameChars();
                        
                string pattern = "[\\~#%&*{}/:<>?|\"-]";
                Regex regEx = new Regex(pattern);
                newFileName = Regex.Replace(regEx.Replace(newFileName, ""), @"\s+", " ");
                simpleirc.DebugCallBack("New Filename: " + newFileName + "\n");
                simpleirc.DebugCallBack(" newFileName(without illigal chars): " + newFileName + "\n");


                newFileSize = Convert.ToInt32(matches.Groups["size"].Value.Trim());
                //convert bitwise ip to normal ip
                try
                {
                    int newIpBW = Convert.ToInt32(matches.Groups["bitwiseip"].Value.Trim());
                    IPEndPoint hostIPEndPoint = new IPEndPoint(newIpBW, newPortNum);
                    string[] ipadressinfoparts = hostIPEndPoint.ToString().Split(':');
                    string[] ipnumbers = ipadressinfoparts[0].Split('.');
                    string ip = ipnumbers[3] + "." + ipnumbers[2] + "." + ipnumbers[1] + "." + ipnumbers[0];
                    newIp = ip;
                } catch(Exception e)
                {
                    simpleirc.DebugCallBack("GOT AN ERROR TRYING PARSE IP: " + e.ToString() + "\n");
                }


                newPortNum = Convert.ToInt32(matches.Groups["port"].Value.Trim());
                return true;
            } else
            {
                simpleirc.DebugCallBack("Regex Failed at parsing XDCC Command :X \n");
                return false;
            }
            
        }

        //creates a tcp socket connection for the retrieved ip/port from the dcc tcp by the dcc bot/server
        //and creates a file, to where it writes the incomming data from the tcp connection.
        public void Downloader()
        {
            updateStatus("WAITING");

            //combining download directory path with filename
            string[] pathToCombine = new string[] { curDownloadDir, newFileName };
            string dlDirAndFileName = Path.Combine(pathToCombine);

            try
            {
                if (!File.Exists(dlDirAndFileName))
                {
                    simpleirc.DebugCallBack("File does not exist yet, start connection \n ");

                    //start connection with tcp server
                    using (TcpClient dltcp = new TcpClient(newIp, newPortNum))
                    {
                        using (NetworkStream dlstream = dltcp.GetStream())
                        {
                            //succesfully connected to tcp server, status is downloading
                            updateStatus("DOWNLOADING");

                            //values to keep track of progress
                            long bytesReceived = 0;
                            long oldBytesReceived = 0;
                            long oneprocent = newFileSize / 100;
                            DateTime start = DateTime.Now;
                            bool timedOut = false;

                            //values to keep track of download position
                            int count;

                            //to me this buffer size seemed to be the most efficient.
                            byte[] buffer = new byte[1048576];

                            //create file to write to
                            using (FileStream writeStream = new FileStream(dlDirAndFileName, FileMode.Append, FileAccess.Write))
                            {
                                isDownloading = true;
                                //download while connected and filesize is not reached
                                while (dltcp.Connected && bytesReceived < newFileSize && !simpleirc.shouldClientStop)
                                {
                                    //keep track of progress
                                    DateTime end = DateTime.Now;
                                    if (start.Second != end.Second)
                                    {

                                        Bytes_Seconds = bytesReceived - oldBytesReceived;
                                        Progress = (int)(bytesReceived / oneprocent);
                                        KBytes_Seconds = (int)(Bytes_Seconds / 1024);
                                        MBytes_Seconds = (KBytes_Seconds / 1024);
                                        oldBytesReceived = bytesReceived;
                                        start = DateTime.Now;
                                        simpleirc.downloadStatusChange();
                                    }

                                    //count bytes received
                                    count = dlstream.Read(buffer, 0, buffer.Length);

                                    //write to file
                                    writeStream.Write(buffer, 0, count);

                                    //count bytes received
                                    bytesReceived += count;

                                    //check if data is still available, to avoid stalling of download thread
                                    int timeOut = 0;
                                    while (!dlstream.DataAvailable)
                                    {
                                        if (timeOut == 1000)
                                        {
                                            break;
                                        }
                                        else if (!dltcp.Connected)
                                        {
                                            break;
                                        }
                                        timeOut++;
                                        Thread.Sleep(1);
                                    }

                                    //stop download thread if timeout is reached
                                    if (timeOut == 1000)
                                    {
                                        timedOut = true;
                                        break;
                                    }
                                }

                                //close all connections and streams (just to be save)
                                dltcp.Close();
                                dlstream.Dispose();
                                writeStream.Close();

                                //consider 95% downloaded as done, files are sequentually downloaded, sometimes download stops early, but the file still is usable
                                if (Progress < 95)
                                {
                                    updateStatus("FAILED");
                                    simpleirc.DebugCallBack("Download stopped at < 95 % finished, deleting file: " + newFileName + " \n");
                                    File.Delete(dlDirAndFileName);
                                    timedOut = false;
                                }
                                else if (timedOut && Progress < 95)
                                {
                                    updateStatus("FAILED: TIMED OUT");
                                    simpleirc.DebugCallBack("Download timed out at < 95 % finished, deleting file: " + newFileName + " \n");
                                    File.Delete(dlDirAndFileName);
                                    timedOut = false;
                                } else
                                {
                                    updateStatus("COMPLETED");
                                }
                            }
                        }
                    }
                }
                else
                {
                    simpleirc.DebugCallBack("File already exists, removing from xdcc que!\n");
                    ircConnect.sendMsg("/msg " + botName + " xdcc remove " + packNum);
                    ircConnect.sendMsg("/msg " + botName + " xdcc cancel");
                    updateStatus("ALREADY EXISTS");
                }
                simpleirc.DebugCallBack("File has been downloaded! \n File Location:" + dlDirAndFileName);
            }
            catch (SocketException e)
            {
                simpleirc.DebugCallBack("Something went wrong while downloading the file! Removing from xdcc que to be sure! Error:\n" + e.ToString());
                ircConnect.sendMsg("/msg " + botName + " xdcc remove " + packNum);
                ircConnect.sendMsg("/msg " + botName + " xdcc cancel");
                updateStatus("FAILED: CONNECTING");
            }
            catch (Exception ex)
            {
                simpleirc.DebugCallBack("Something went wrong while downloading the file! Removing from xdcc que to be sure! Error:\n" + ex.ToString());
                ircConnect.sendMsg("/msg " + botName + " xdcc remove " + packNum);
                ircConnect.sendMsg("/msg " + botName + " xdcc cancel");
                updateStatus("FAILED: UNKNOWN");
            }
            isDownloading = false;
        }        

        //updates status about the download besides the progress, such as if it failed etc...
        private void updateStatus(string statusin)
        {
            Status = statusin;
            simpleirc.downloadStatusChange();
        }

        //stops the downloader
        public void abortDownloader()
        {
            simpleirc.DebugCallBack("Downloader Stopped");
            isDownloading = false;
            updateStatus("ABORTED");
            downloader.Abort();
        }
    }
}
