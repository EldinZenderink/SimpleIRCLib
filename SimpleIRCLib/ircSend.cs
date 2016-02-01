using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIRCLib
{
    class IrcSend : SimpleIRC
    {

        //public accessible variables
        public static string packNumber;
        public static string bot;

        //this is where different kind of messages can be send to the server
        public static void sendMsg(string Input)
        {
            //if its a xdcc snend message, it will make sure that values for packNumber and bot are both actual packnumber and bot to eliminate 
            //errors where somehow another part of the string would be seen as a packNumber or bot
            if (Input.ToLower().Contains("xdcc send"))
            {
                string[] xdccparts = Input.Split(' ');
                int bCounter = 0;
                while (xdccparts[bCounter].Contains("xdcc") || xdccparts[bCounter].Contains("send") || xdccparts[bCounter].Contains("/msg") || xdccparts[bCounter].Contains("#"))
                {
                    bCounter++;
                    if (bCounter == 10)
                    {
                        break;
                    }
                }

                int pCounter = 0;
                while (!xdccparts[pCounter].Contains("#"))
                {
                    pCounter++;
                    if (pCounter == 10)
                    {
                        break;
                    }
                }
                bot = xdccparts[bCounter];
                packNumber = xdccparts[pCounter];

                string xdccdl = "PRIVMSG " + bot + " :XDCC SEND " + packNumber;
                IrcConnect.writeIrc(xdccdl);
            }
            //to stop a xdcc transfer
            else if (Input.ToLower().Contains("xdcc cancel"))
            {
                string[] xdccparts = Input.Split(' ');
                string xdcccl = "PRIVMSG " + xdccparts[1] + " :XDCC CANCEL";
                IrcConnect.writeIrc(xdcccl);
            }
            //to remove from que
            else if (Input.ToLower().Contains("xdcc remove"))
            {
                string[] xdccparts = Input.Split(' ');
                string xdccdl = "PRIVMSG " + xdccparts[1] + " :XDCC REMOVE " + xdccparts[4];
                IrcConnect.writeIrc(xdccdl);
            }
            //to entirely quit the irc server
            else if (Input.Contains("/quit"))
            {
                IrcConnect.writeIrc("QUIT");
            }
            //for sending normal chat messages to the server
            else
            {
                IrcConnect.writeIrc("PRIVMSG " + IrcConnect.newChannel + " :" + Input);
            }

        }

        //actually sends it to the server with a function defined in IrcConnect
        public static void sendInput(string Input)
        {
            IrcConnect.writeIrc(Input);
        }

    }
}
