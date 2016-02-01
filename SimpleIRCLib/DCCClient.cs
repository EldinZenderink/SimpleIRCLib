using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;


namespace SimpleIRCLib
{
    class DCCClient : SimpleIRC
    {

        //Member vars
        private string newDccString;
        private string newFileName;
        private long newIpAddress;
        private int newPortNum;
        private int newFileSize;
        private string newIp;
        private string downloadDir;
        private string[] dccparts;

        private Int64 Progress;
        private Int64 Bytes_Seconds;
        private Int64 KBytes_Seconds;
        private Int64 MBytes_Seconds;

        private string botName;
        private string packNum;

        //public vars
        public static bool Downloading;

        //overload constructor
        public DCCClient(string dccString, string downloaddir, string bot, string pack)
        {
            newDccString = dccString;
            downloadDir = downloaddir;
            botName = bot;
            packNum = pack;

            /*
             * :bot PRIVMSG nickname :DCC SEND \"filename\" ip_networkbyteorder port filesize
            */

            //currently accepted file extensions
            string[] fileExtensions = { ".mkv", ".mp4", ".avi", ".mp3", ".pdf", ".jpg" };

            //remove everything till and including "SEND"
            int posStart = newDccString.IndexOf("SEND");
            string dccstringp1 = newDccString.Substring(posStart + 5);


            //getting position of filename by checking file extension
            int posEnd = 0;
            string extensionUsed = "";

            foreach (string extension in fileExtensions)
            {
                if (dccstringp1.IndexOf(extension) != 0)
                {
                    posEnd = dccstringp1.IndexOf(extension);
                    extensionUsed = extension;
                    break;
                }
            }

            //set filename to download
            newFileName = dccstringp1.Substring(0, posEnd + 4);

            //remove chars not part of filename
            if (newFileName.Contains("\""))
            {
                newFileName = newFileName.Substring(1);
            }

            //get part from dcc send string which contains the connection details (ip, port and filesize)
            string dccstring = Regex.Split(dccstringp1, extensionUsed)[1].Substring(2);

            //split them into seperate pieces
            dccparts = dccstring.Split(' ');

            //most occurances of dcc send string will start at index 1 in the array
            if (dccparts[0] != "")
            {
                string[] tempArray = { "", dccparts[0], dccparts[1], dccparts[2] };
                dccparts = tempArray;
            }

            //try to set the necesary information for the downloader
            try
            {
                //convert text to integers
                newIpAddress = Convert.ToInt64(dccparts[1]);
                newPortNum = Convert.ToInt32(dccparts[2]);

                //create IPEndPoint (convert integers to actual ip data)
                IPEndPoint hostIPEndPoint = new IPEndPoint(newIpAddress, newPortNum);
                string[] ipadressinfoparts = hostIPEndPoint.ToString().Split(':');
                string[] ipnumbers = ipadressinfoparts[0].Split('.');
                string ip = ipnumbers[3] + "." + ipnumbers[2] + "." + ipnumbers[1] + "." + ipnumbers[0];
                newIp = ip;

                //set filesize to compare for calculating progress while downloading (most of the time there is a weird char at the end of the filesize)
                int charlength = dccparts[3].Length;
                string filesizes = dccparts[3].Substring(0, charlength - 1);
                newFileSize = Convert.ToInt32(filesizes);

                Thread downloader = new Thread(new ThreadStart(this.Downloader));
                downloader.Start();
            }
            catch (Exception e)
            {
                DebugCallBack("Can't parse dcc string and start downloader: \n" + e.ToString());
            }

        }

        public string[] downloadDetails()
        {
            return new string[] { newDccString, newFileName, newFileSize.ToString(), newIp, newPortNum.ToString(), packNum, botName, Progress.ToString(), Bytes_Seconds.ToString(), KBytes_Seconds.ToString(), MBytes_Seconds.ToString() };
        }       
        
        //creates a tcp socket connection for the retrieved ip/port from the dcc ctcp by the dcc bot/server
        public void Downloader()
        {
            DebugCallBack("Start Downloader \n ");
            string dlDirAndFileName = downloadDir + "\\" + newFileName;
            try
            {
                if (!File.Exists(dlDirAndFileName))
                {

                    DebugCallBack("File does not exist yet, start connection \n ");
                    
                    //start connection with tcp server
                    TcpClient dltcp = new TcpClient(newIp, newPortNum);
                    NetworkStream dlstream = dltcp.GetStream();

                    //values to keep track of progress
                    Int64 bytesReceived = 0;
                    Int64 oldBytesReceived = 0;
                    Int64 oneprocent = newFileSize / 100;
                    DateTime start = DateTime.Now;
                    bool timedOut = false;

                    //values to keep track of download position
                    int count;
                    byte[] buffer = new byte[1048576];

                    //create file to write to
                    FileStream writeStream = new FileStream(dlDirAndFileName, FileMode.Append, FileAccess.Write);

                    //download while connected and filesize is not reached
                    while (dltcp.Connected && bytesReceived < newFileSize)
                    {
                        //keep track of progress
                        DateTime end = DateTime.Now;
                        if (start.Second != end.Second)
                        {

                            Bytes_Seconds = bytesReceived - oldBytesReceived;
                            Progress = bytesReceived / oneprocent;
                            KBytes_Seconds = Bytes_Seconds / 1024;
                            MBytes_Seconds = KBytes_Seconds / 1024;
                            oldBytesReceived = bytesReceived;
                            start = DateTime.Now;
                            downloadStatusChange();
                        }

                        //global checker to keep track if download is bussy
                        Downloading = true;

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

                    //consider 95% downloaded as done
                    if (Progress < 95)
                    {
                        DebugCallBack("Download stopped at < 95 % finished, deleting file: " + newFileName + " \n");
                        File.Delete(dlDirAndFileName);
                    }
                    else if (timedOut && Progress < 95)
                    {
                        DebugCallBack("Download timed out at < 95 % finished, deleting file: " + newFileName + " \n");
                        File.Delete(dlDirAndFileName);
                    }
                }
                else
                {
                    DebugCallBack("File already exists, removing from xdcc que!\n");
                    IrcSend.sendMsg("/msg " + botName + " xdcc remove " + packNum);
                    IrcSend.sendMsg("/msg " + botName + " xdcc cancel");
                }
                DebugCallBack("File has been downloaded! \n File Location:" + dlDirAndFileName);
            }
            catch (Exception e)
            {
                DebugCallBack("Something went wrong while downloading the file! Removing from xdcc que to be sure! Error:\n" + e.ToString());
                IrcSend.sendMsg("/msg " + botName + " xdcc remove " + packNum);
                IrcSend.sendMsg("/msg " + botName + " xdcc cancel");
            }

            Downloading = false;
        }        
    }
}
