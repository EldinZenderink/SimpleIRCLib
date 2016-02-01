using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleIRCLib;
using System.Threading;

namespace IrcLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //setup vars
            string ip;
            int port;
            string username;
            string password;
            string channel;

            //setup screen:
            Console.WriteLine("Server IP(default is : 54.229.0.87(irc.rizon.net)) = ");
            if ((ip = Console.ReadLine()) == "")
            {
                ip = "54.229.0.87";
            }

            Console.WriteLine("Server Port(default is : 6667) = ");
            if (Console.ReadLine() != "")
            {
                port = Convert.ToInt32(Console.ReadLine());
            }
            else
            {
                port = 6667;
            }

            Console.WriteLine("Username(default is : RareIRC_Client) = ");
            if ((username = Console.ReadLine()) == "")
            {
                username = "RareIRC_Client";
            }

            Console.WriteLine("Password(not working yet, default is : ) = ");
            if ((password = Console.ReadLine()) == "")
            {
                password = "";
            }

            Console.WriteLine("Channel(default is : #RareIRC) = ");
            if ((channel = Console.ReadLine()) == "")
            {
                channel = "#horriblesubs";
            }

            SimpleIRC irc = new SimpleIRC();
            irc.setupIrc(ip, port, username, password, channel, chatOutputCallback);
            irc.setDebugCallback(debugOutputCallback);
            irc.startClient();

            string prevProg = "";
            while (true)
            {
                if(irc.getDownloadProgress()[0] != "NULL")
                {
                    string currentProg = irc.getDownloadProgress()[7];

                    if(currentProg != prevProg)
                    {
                        Console.WriteLine("DOWNLOAD PROGRESS: " + currentProg + "%");
                    }
                    prevProg = currentProg;
                    Thread.Sleep(1);
                } else
                {
                    string Input = Console.ReadLine();
                    if (Input != null || Input != "" || Input != String.Empty)
                    {
                        irc.sendMessage(Input);
                    }
                }
               
            }
        }

        public static void chatOutputCallback(string chat)
        {
            Console.WriteLine(chat);
        }

        public static void debugOutputCallback(string debug)
        {
            Console.WriteLine("===============DEBUG MESSAGE===============");
            Console.WriteLine(debug);
            Console.WriteLine("===============END DEBUG MESSAGE===============");
        }
    }
}
