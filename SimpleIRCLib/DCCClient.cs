using System;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;

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
        public string newIp2 { get; set; }
        public int Progress { get; set; }
        public string Status { get; set; }
        public Int64 Bytes_Seconds { get; set; }
        public int KBytes_Seconds { get; set; }
        public int MBytes_Seconds { get; set; }
        public string botName { get; set; }
        public string packNum { get; set; }
        public bool isDownloading { get; set; }
        public string currentFilePath { get; set; }


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

                updateStatus("PARSING");
                bool isParsed = parseData(dccString);

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
            }
            else
            {
                if (isDownloading)
                {
                    simpleirc.DebugCallBack("You are already downloading! Ignore SEND request\n");
                }
                else
                {
                    simpleirc.DebugCallBack("DCC String does not contain SEND and/or invalid values for parsing! Ignore SEND request\n");
                }
            }
        }

        //parses data received by the dcc strings, necesary for details to connect to the right tcp server where file is located
        private bool parseData(string dccString)
        {
            /*
           * :bot PRIVMSG nickname :DCC SEND \"filename\" ip_networkbyteorder port filesize
           *AnimeDispenser!~desktop@Rizon-6AA4F43F.ip-37-187-118.eu PRIVMSG WeebIRCDev :DCC SEND "[LNS] Death Parade - 02 [BD 720p] [7287AE5C].mkv" 633042523 59538 258271780  
           *HelloKitty!~nyaa@ny.aa.ny.aa PRIVMSG WeebIRCDev :DCC SEND [Coalgirls]_Spirited_Away_(1280x692_Blu-ray_FLAC)_[5805EE6B].mkv 3281692293 35567 10393049211
           :[EWG]-bOnez!EWG@CRiTEN-BB8A59E9.ip-158-69-126.net PRIVMSG LittleWeeb_jtokck :DCC SEND The.Good.Doctor.S01E13.Seven.Reasons.1080p.AMZN.WEB-DL.DD+5.1.H.264-QOQ.mkv 2655354388 55000 1821620363
           *Ginpa2:DCC SEND "[HorribleSubs] Dies Irae - 05 [480p].mkv" 84036312 35016 153772128 
             */

            //bot
            //file
            //size
            //ip
            //port
            dccString = RemoveSpecialCharacters(dccString).Substring(1);
            simpleirc.DebugCallBack("DCCClient: DCC STRING: " + dccString);


            if (!dccString.Contains(" :DCC"))
            {
                botName = dccString.Split(':')[0];
                if (dccString.Contains("\""))
                {
                    newFileName = dccString.Split('"')[1];

                    simpleirc.DebugCallBack("DCCClient1: FILENAME PARSED: " + newFileName);
                    string[] thaimportantnumbers = dccString.Split('"')[2].Trim().Split(' ');
                    if (thaimportantnumbers[0].Contains(":"))
                    {
                        newIp = thaimportantnumbers[0];
                    }
                    else
                    {
                        try
                        {

                            simpleirc.DebugCallBack("DCCClient1: PARSING FOLLOWING IPBYTES USING NTOH: " + thaimportantnumbers[0]);
                            Int64 hostmode = (Int64)IPAddress.NetworkToHostOrder(Int64.Parse(thaimportantnumbers[0]));
                            string ipAddress = UInt64ToIPAddress(hostmode);
                            newIp = ipAddress;
                        }
                        catch
                        {
                            simpleirc.DebugCallBack("DCCClient1: PARSING FOLLOWING IPBYTES: " + thaimportantnumbers[0]);
                            string ipAddress = UInt64ToIPAddress(Int64.Parse(thaimportantnumbers[0]));
                            newIp = ipAddress;
                        }
                    }

                    simpleirc.DebugCallBack("DCCClient1: IP PARSED: " + newIp);
                    newPortNum = int.Parse(thaimportantnumbers[1]);
                    newFileSize = Int64.Parse(thaimportantnumbers[2]);

                    return true;
                }
                else
                {
                    newFileName = dccString.Split(' ')[2];

                    simpleirc.DebugCallBack("DCCClient2: FILENAME PARSED: " + newFileName);

                    if (dccString.Split(' ')[3].Contains(":"))
                    {
                        newIp = dccString.Split(' ')[3];
                    }
                    else
                    {
                        try
                        {

                            simpleirc.DebugCallBack("DCCClient2: PARSING FOLLOWING IPBYTES USING NTOH: " + dccString.Split(' ')[3]);
                            Int64 hostmode = (Int64)IPAddress.NetworkToHostOrder(Int64.Parse(dccString.Split(' ')[3]));
                            string ipAddress = UInt64ToIPAddress(hostmode);
                            newIp = ipAddress;
                        }
                        catch
                        {

                            simpleirc.DebugCallBack("DCCClient2: PARSING FOLLOWING IPBYTES DIRECTLY: " + dccString.Split(' ')[3]);
                            string ipAddress = UInt64ToIPAddress(Int64.Parse(dccString.Split(' ')[3]));
                            newIp = ipAddress;
                        }
                    }
                    simpleirc.DebugCallBack("DCCClient2: IP PARSED: " + newIp);
                    newPortNum = int.Parse(dccString.Split(' ')[4]);
                    newFileSize = Int64.Parse(dccString.Split(' ')[5]);
                    return true;
                }
            } else
            {
                botName = dccString.Split('!')[0];
                if (dccString.Contains("\""))
                {
                    newFileName = dccString.Split('"')[1];

                    simpleirc.DebugCallBack("DCCClient3: FILENAME PARSED: " + newFileName);
                    string[] thaimportantnumbers = dccString.Split('"')[2].Trim().Split(' ');

                    if (thaimportantnumbers[0].Contains(":"))
                    {
                        newIp = thaimportantnumbers[0];
                    }
                    else
                    {
                        try
                        {
                            simpleirc.DebugCallBack("DCCClient3: PARSING FOLLOWING IPBYTES USING NTOH: " + thaimportantnumbers[0]);
                            Int64 hostmode = (Int64)IPAddress.NetworkToHostOrder(Int64.Parse(thaimportantnumbers[0]));
                            string ipAddress = UInt64ToIPAddress(hostmode);
                            newIp = ipAddress;
                        }
                        catch
                        {
                            simpleirc.DebugCallBack("DCCClient3: PARSING FOLLOWING IPBYTES DIRECTLY: " + thaimportantnumbers[0]);
                            string ipAddress = UInt64ToIPAddress(Int64.Parse(thaimportantnumbers[0]));
                            newIp = ipAddress;
                        }
                    }
                  
                    simpleirc.DebugCallBack("DCCClient3: IP PARSED: " + newIp);
                    newPortNum = int.Parse(thaimportantnumbers[1]);
                    newFileSize = Int64.Parse(thaimportantnumbers[2]);
                    return true;
                } else
                {
                    newFileName = dccString.Split(' ')[5];

                    simpleirc.DebugCallBack("DCCClient4: FILENAME PARSED: " + newFileName);
                    
                    if(dccString.Split(' ')[6].Contains(":"))
                    {
                        newIp = dccString.Split(' ')[6];
                    } else
                    {
                        try
                        {
                            simpleirc.DebugCallBack("DCCClient4: PARSING FOLLOWING IPBYTES USING NTOH: " + dccString.Split(' ')[6]);
                            Int64 hostmode = Int64.Parse(dccString.Split(' ')[6]);
                            string ipAddress = UInt64ToIPAddress(hostmode).ToString();
                            newIp = ipAddress;
                        }
                        catch
                        {
                            simpleirc.DebugCallBack("DCCClient4: PARSING FOLLOWING IPBYTES DIRECTLY: " + dccString.Split(' ')[6]);
                            string ipAddress = UInt64ToIPAddress(Int64.Parse(dccString.Split(' ')[6]));
                            newIp = ipAddress;
                        }

                    }

                    simpleirc.DebugCallBack("DCCClient4: IP PARSED: " + newIp);
                    newPortNum = int.Parse(dccString.Split(' ')[7]);
                    newFileSize = Int64.Parse(dccString.Split(' ')[8]);
                    return true;

                }
                

            }

            return false;
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

                            simpleirc.downloadStatusChange();
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
                                    if (end.Second !=  start.Second)
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
            simpleirc.downloadStatusChange();
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
                    simpleirc.DebugCallBack(e.ToString());
                }
            }

            updateStatus("ABORTED");

            simpleirc.downloadStatusChange();
        }

        public bool checkIfDownloading()
        {
            return isDownloading;
        }

        private int lastIndexOfFileName(string str)
        {
            int strlen = str.Length;
            int i;
            for (i = strlen - 1; i > 0; i--)
            {
                if(str[i] != ' ')
                {

                    int value = 0;
                    int.TryParse(str[i].ToString(), out value);
                    if(value < 0)
                    {
                        return i;
                    }

                }

            }
            return -1;
        }

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

        private string reverseIp(string ip)
        {
            string[] parts = ip.Trim().Split('.');
            if(parts.Length >= 3)
            {
                simpleirc.DebugCallBack("DCCClient: converting ip: " + ip);
                string newip = parts[3] + "." + parts[2] + "." + parts[1] + "." + parts[0];
                simpleirc.DebugCallBack("DCCClient: to: " + newip);

                return newip;
            } else
            {
                simpleirc.DebugCallBack("DCCClient: converting ip: " + ip);
                simpleirc.DebugCallBack("DCCClient: amount of parts: " + parts.Length);
                return "0.0.0.0";
            }
        }

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
