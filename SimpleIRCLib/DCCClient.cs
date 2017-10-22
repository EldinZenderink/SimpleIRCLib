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
        public Int64 newFileSize { get; set; }
        public string newIp { get; set; }
        public int Progress { get; set; }
        public string Status { get; set; }
        public Int64  Bytes_Seconds { get; set; }
        public int KBytes_Seconds { get; set; }
        public int MBytes_Seconds { get; set; }
        public string botName { get; set; }
        public string packNum { get; set; }
        public bool isDownloading { get; set; }
        public string currentFilePath { get; set; }
        public int retries = 0;

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
                    downloader.IsBackground = true;
                    downloader.Start();
                }
                else
                {
                    simpleirc.DebugCallBack("Can't parse dcc string and start downloader, failed to parse data, removing from que\n");
                    ircConnect.sendMsg("/msg " + botName + " xdcc remove " + packNum);
                    ircConnect.sendMsg("/msg " + botName + " xdcc cancel");
                }
                retries = 0;
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


                newFileSize = Convert.ToInt64(matches.Groups["size"].Value.Trim());
                //convert bitwise ip to normal ip
                try
                {
                    newFileName = Regex.Replace(regEx.Replace(newFileName, ""), @"\s+", " ");
                    simpleirc.DebugCallBack("New Filename: " + newFileName + "\n");

                    simpleirc.DebugCallBack(" newIpBW: " + matches.Groups["bitwiseip"].Value.Trim() + "\n");
                    Int64  newIpBW = Convert.ToInt64(matches.Groups["bitwiseip"].Value.Trim());
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

            if (!Directory.Exists(curDownloadDir))
            {
                Directory.CreateDirectory(curDownloadDir);
            } 
            string[] pathToCombine = new string[] { curDownloadDir, newFileName };
            string dlDirAndFileName = Path.Combine(pathToCombine);
            currentFilePath = dlDirAndFileName;
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
                            Int64  bytesReceived = 0;
                            Int64  oldBytesReceived = 0;
                            Int64  oneprocent = newFileSize / 100;
                            DateTime start = DateTime.Now;
                            bool timedOut = false;

                            //values to keep track of download position
                            int count;

                            //to me this buffer size seemed to be the most efficient.
                            byte[] buffer;
                            if (newFileSize > 1048576)
                            {
                                simpleirc.DebugCallBack("DCC Downloader: Big file, big buffer (1 mb) \n ");
                                buffer = new byte[16384];
                            } else if(newFileSize < 1048576 && newFileSize > 2048)
                            {
                                simpleirc.DebugCallBack("DCC Downloader: Smaller file (< 1 mb), smaller buffer (2 kb) \n ");
                                buffer = new byte[2048];
                            } else if (newFileSize < 2048 && newFileSize > 128)
                            {
                                simpleirc.DebugCallBack("DCC Downloader: Small file (< 2kb mb), small buffer (128 b) \n ");
                                buffer = new byte[128];
                            } else
                            {
                                simpleirc.DebugCallBack("DCC Downloader: Tiny file (< 128 b), tiny buffer (2 b) \n ");
                                buffer = new byte[2];
                            }
                                

                            //create file to write to
                            using (FileStream writeStream = new FileStream(dlDirAndFileName, FileMode.Append, FileAccess.Write, FileShare.Read))
                            {
                                writeStream.SetLength(newFileSize);
                                isDownloading = true;
                                //download while connected and filesize is not reached
                                while (dltcp.Connected && bytesReceived < newFileSize && !simpleirc.shouldClientStop)
                                {
                                    //keep track of progress
                                    DateTime end = DateTime.Now;
                                    if (start.Second != end.Second)
                                    {

                                        Bytes_Seconds = bytesReceived - oldBytesReceived;
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

                                    Progress = (int)(bytesReceived / oneprocent);
                                    //check if data is still available, to avoid stalling of download thread
                                    /* int timeOut = 0;
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
                                        Thread.Sleep(4);
                                    }

                                    //stop download thread if timeout is reached
                                    if (timeOut == 1000)
                                    {
                                        timedOut = true;
                                        break;
                                    } */
                                }

                                //close all connections and streams (just to be save)
                                dltcp.Close();
                                dlstream.Dispose();
                                writeStream.Close();

                                //consider 95% downloaded as done, files are sequentually downloaded, sometimes download stops early, but the file still is usable
                                if (Progress < 95 && !simpleirc.shouldClientStop)
                                {
                                    updateStatus("FAILED");
                                    simpleirc.DebugCallBack("Download stopped at < 95 % finished, deleting file: " + newFileName + " \n");
                                    simpleirc.DebugCallBack("Download stopped at : " + bytesReceived + " bytes, a total of " + Progress + "%");
                                    File.Delete(dlDirAndFileName);
                                    timedOut = false;


                                }
                                else if (timedOut && Progress < 95 && !simpleirc.shouldClientStop)
                                {
                                    updateStatus("FAILED: TIMED OUT");
                                    simpleirc.DebugCallBack("Download timed out at < 95 % finished, deleting file: " + newFileName + " \n");
                                    simpleirc.DebugCallBack("Download stopped at : " + bytesReceived + " bytes, a total of " + Progress + "%");
                                    File.Delete(dlDirAndFileName);
                                    timedOut = false;
                                } else if(!simpleirc.shouldClientStop)
                                {
                                    //make sure that in the event something happens and the downloader calls delete after finishing, the file will remain where it is.
                                    dlDirAndFileName = "";
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
                    updateStatus("FAILED: ALREADY EXISTS");
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
            if (isDownloading)
            {

                isDownloading = false;
                downloader.Abort();

                try
                {
                    simpleirc.DebugCallBack("File " + currentFilePath + " will be deleted due to aborting");
                    File.Delete(currentFilePath);
                }
                catch (Exception e)
                {
                    simpleirc.DebugCallBack("File " + currentFilePath + " probably doesn't exist :X");
                }
            }

            updateStatus("ABORTED");
        }
    }
}
