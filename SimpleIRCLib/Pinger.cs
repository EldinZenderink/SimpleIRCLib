using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SimpleIRCLib
{
    class Pinger : SimpleIRC
    {
        //vars needed to ping pong with the server
        private string ping = "PING :";
        private Thread pingSender;

        //creates a thread for the pinger
        public Pinger()
        {
            pingSender = new Thread(new ThreadStart(this.Run));
        }
        //starts the ping thread
        public void Start()
        {
            pingSender.Start();
        }
        //function that runs in the ping thread, used to keep the connection with the irc server alive
        public void Run()
        {
            while (true)
            {
                IrcConnect.writeIrc(ping + IrcConnect.newIP);
                Thread.Sleep(15000);
            }
        }
    }
}
