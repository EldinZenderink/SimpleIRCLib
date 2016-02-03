using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace SimpleIRCLib
{
    public class IrcConnect : SimpleIRC
    {

        //Member Variables

        private int newPort;
        private string newUsername;
        private string newPassword;
        private Pinger ping;
        private DCCClient dcc;
        private Thread worker;

        private bool conCheck = false;
        //Accessable stuff
        private static StreamReader reader;
        private static StreamWriter writer;
        private static NetworkStream stream;
        private static TcpClient irc;


        public static string newIP;
        public static string newChannel;
        
        //Overload Constructor - safe way to get variables
        public IrcConnect(string IP, int Port, string Username, string Password, string Channel)
        {
            newIP = IP;
            newPort = Port;
            newUsername = Username;
            newPassword = Password;
            newChannel = Channel;
        }

        //connects to irc server, gives a boolean back on succesfull connect etc
        public bool Connect()
        {
            try
            {
                irc = new TcpClient(newIP, newPort);
                stream = irc.GetStream();

                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                ping = new Pinger();
                ping.Start();

                writeIrc("USER " + newUsername + " 8 * : Testing RareAMVS C# irc client");
                writeIrc("NICK " + newUsername);
                writeIrc("JOIN " + newChannel);

                DebugCallBack("succesful connected to the irc server!");
                string ircData = "";
                worker = new Thread(() =>
                {
                    while ((ircData = reader.ReadLine()) != null)
                    {
                        conCheck = true;
                        if (ircData.Contains("PRIVMSG"))
                        {
                            string userName = ircData.Split('~')[0].Split('!')[0];
                            string messageFromUser;
                            if (!userName.Contains(" "))
                            {
                                try
                                {
                                    messageFromUser = ircData.Substring(ircData.IndexOf(newChannel)).Split(':')[1];
                                }
                                catch 
                                {
                                    userName = "SERVER: ";
                                    messageFromUser = ircData;
                                }
                                chatOutput(userName, messageFromUser);
                            }
                            
                        }
                        else if (ircData.Contains("JOIN"))
                        {
                            string userName = ircData.Split('~')[0].Split('!')[0].Substring(1);
                            chatOutput(userName, "JOINED");
                        }
                        else if (ircData.Contains("QUIT"))
                        {
                            string userName = ircData.Split('~')[0].Split('!')[0].Substring(1);
                            chatOutput(userName, "QUITED");
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

                    conCheck = false;
                });

                worker.Start();

                return true;
            }
            catch(Exception e)
            {
                DebugCallBack("Error Connecting to IRC Server: \n " + e.ToString());
                return false;
            }
        }


        public void quitConnect()
        {
            //send quit to server
            writeIrc("QUIT");

            //stop download if downloading
            if (DCCClient.Downloading)
            {
                dcc.abortDownloader();
            }

            //stop everything in right order
            ping.Stop();
            worker.Abort();
            reader.Close();
            writer.Close();
            irc.Close();
            conCheck = false;
        }

        public bool clientStatus()
        {
            return conCheck;
        }

        public string[] passDownloadProgress()
        {
            if (DCCClient.Downloading)
            {
                return dcc.downloadDetails();
            }
            else
            {
                return new string[] { "NULL", "", "", "", "", "", "", "", "", "", "", "" };
            }
        }

        //function to write to the irc server, bit easier to use and better looking
        public static void writeIrc(string input)
        {
            try
            {
                writer.WriteLine(input);
                writer.Flush();
            }
            catch (NullReferenceException e)
            {
                DebugCallBack("Could not send message, irc client is not running :X, error: \n" + e.ToString());
            }
        }
    }
}
