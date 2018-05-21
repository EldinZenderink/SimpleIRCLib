using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIRCLib
{
    /// <summary>
    /// Event Class for containing eventhandler for Download Updates from DCCClient.cs
    /// </summary>
    public class DCCEventArgs
    {
        /// <summary>
        /// Event for download updates, containging information about the following
        /// DccString : unparsed dccstring received from server, handy for debugging purposes
        /// FileName : file that is currently being downloaded
        /// FileSize : size of file that is currently being downloaded
        /// Ip : server address where file originates from
        /// Port : port of server where file originates from
        /// Pack : original pack that the user requested
        /// Bot : original bot where the user requested a pack (file) from
        /// BytesPerSecond : current download speed in bytes p/s 
        /// KBytesPerSecond : current download speed in kbytes p/s
        /// MBytesPerSecond : current download speed in mbytes p/s
        /// Status : current download status, for example: (WAITING, DOWNLOADING, FAILED, ABORTED, etc)
        /// Progress : percentage downloaded (0-100%) (is int!)
        /// </summary>
        /// <param name="currentClient"></param>
        public DCCEventArgs(DCCClient currentClient)
        {
            DccString = currentClient.NewDccString;
            FileName = currentClient.NewFileName;
            FileSize = currentClient.NewFileSize;
            Ip = currentClient.NewIp;
            Port = currentClient.NewPortNum;
            Pack = currentClient.PackNum;
            Bot = currentClient.BotName;
            BytesPerSecond = currentClient.BytesPerSecond;
            KBytesPerSecond = currentClient.KBytesPerSecond;
            MBytesPerSecond = currentClient.MBytesPerSecond;
            Status = currentClient.Status;
            Progress = currentClient.Progress;
            FilePath = currentClient.CurrentFilePath;
        }

        /// <summary>
        /// Raw DCC String used for getting the file location (server) and some basic file information
        /// </summary>
        public string DccString { get; }
        /// <summary>
        /// File name of the file being downloaded
        /// </summary>
        public string FileName { get; }
        /// <summary>
        /// FileSize of the file being downloaded
        /// </summary>
        public Int64 FileSize { get; }
        /// <summary>
        /// Server address of file location
        /// </summary>
        public string Ip { get; }
        /// <summary>
        /// Port of server of file location
        /// </summary>
        public int Port { get; }
        /// <summary>
        /// Pack ID of file on bot where file resides
        /// </summary>
        public string Pack { get; }
        /// <summary>
        /// Bot name where file resides
        /// </summary>
        public string Bot { get; }
        /// <summary>
        /// Download speed in: B/s
        /// </summary>
        public long BytesPerSecond { get; }
        /// <summary>
        /// Download speed in: KB/s
        /// </summary>
        public int KBytesPerSecond { get; }
        /// <summary>
        /// Download speed in: MB/s
        /// </summary>
        public int MBytesPerSecond { get; }
        /// <summary>
        /// Download status, such as: WAITING,DOWNLOADING,FAILED:[ERROR],ABORTED
        /// </summary>
        public string Status { get; }
        /// <summary>
        /// Progress from 0-100 (%)
        /// </summary>
        public int Progress { get; }
        /// <summary>
        /// Path to file that is being downloaded
        /// </summary>
        public string FilePath { get; }
    }

    /// <summary>
    /// Event Class for handeling debug events fired within DCCClient.cs
    /// </summary>
    public class DCCDebugMessageArgs
    {
        /// <summary>
        /// Event for debug messages specific to the DCC Client
        /// </summary>
        /// <param name="message">debug message</param>
        /// <param name="type">type of debug message, handy for determing where message occured</param>
        public DCCDebugMessageArgs(string message, string type)
        {
            Message = message;
            Type = type;
        }

        /// <summary>
        /// Containing debug message
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// Containing debug type
        /// </summary>
        public string Type { get; }
    }
}
