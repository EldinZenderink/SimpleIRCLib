using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;


namespace SimpleIRCLib
{
    public class IrcConnect : SimpleIRC
    {

        //Member Variables

        private int newPort;
        private string newUsername;
        private string newPassword;


        //Accessable stuff
        public static StreamReader reader;
        public static StreamWriter writer;
        public static NetworkStream stream;
        public static TcpClient irc;

        public static string newIP;
        public static string newChannel;

        //Default Constructor - null state
        public IrcConnect()
        {
            newIP = "";
            newPort = 0;
            newUsername = "";
            newPassword = "";
            newChannel = "";
        }

        //Overload Constructor - safe way to get variables
        public IrcConnect(string IP, int Port, string Username, string Password, string Channel)
        {
            newIP = IP;
            newPort = Port;
            newUsername = Username;
            newPassword = Password;
            newChannel = Channel;
        }

        //Accessor Functions
        public string getConIP()
        {
            return newIP;
        }

        public int getConPort()
        {
            return newPort;
        }

        public string getConUsername()
        {
            return newUsername;
        }

        public string getConPassword()
        {
            return newPassword;
        }

        public string getConChannel()
        {
            return newChannel;
        }

        //Mutator Functions
        public void setConIP(string IP)
        {
            newIP = IP;
        }

        public void setConPort(int Port)
        {
            newPort = Port;
        }

        public void setConUsername(string Username)
        {
            newUsername = Username;
        }

        public void setConPassword(string Password)
        {
            newPassword = Password;
        }

        public void setConChannel(string Channel)
        {
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

                Pinger ping = new Pinger();
                ping.Start();

                writeIrc("USER " + newUsername + " 8 * : Testing RareAMVS C# irc client");
                writeIrc("NICK " + newUsername);
                writeIrc("JOIN " + newChannel);

                return true;
            }
            catch(Exception e)
            {
                DebugCallBack("Error Connecting to IRC Server: \n " + e.ToString());
                return false;
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
