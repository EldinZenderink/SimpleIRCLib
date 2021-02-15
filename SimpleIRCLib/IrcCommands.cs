using System;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;

namespace SimpleIRCLib
{
    /// <summary>
    /// Class used for sending specific commands to the IRC server, and waiting for responses from the irc server before continueing
    /// </summary>
    class IrcCommands
    {
        /// <summary>
        /// reader to read from the irc server stream
        /// </summary>
        private readonly StreamReader _reader;

        /// <summary>
        /// reader to write to the irc server stream
        /// </summary>
        private readonly StreamWriter _writer;

        /// <summary>
        /// global error message string, used for defining the error that might occur with a readable string
        /// </summary>
        private string _errorMessage;

        /// <summary>
        /// global error integer, used for getting the specific error id specified within the IRC specs RFC 2812
        /// </summary>
        private int _responseNumber;

        /// <summary>
        /// user name that has been registered
        /// </summary>
        private string _username;

        /// <summary>
        /// channel that hs been joined
        /// </summary>
        private string _channels;

        /// <summary>
        /// Constructor, that requires the stream reader and writer set before initializing
        /// </summary>
        /// <param name="stream">Stream to read/write from/to</param>
        public IrcCommands(NetworkStream stream)
        {
            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream);
        }

        /// <summary>
        /// Constructor, that requires the stream reader and writer set before initializing
        /// </summary>
        /// <param name="stream">Stream to read/write from/to</param>
        public IrcCommands(SslStream stream)
        {
            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream);
        }

        /// <summary>
        /// Get the error message that probably has occured when calling this method
        /// </summary>
        /// <returns>returns error message</returns>
        public string GetErrorMessage()
        {
            return _errorMessage;
        }

        /// <summary>
        /// Get the error number that probably had occured when calling this method (RFC 2812)
        /// </summary>
        /// <returns></returns>
        public int GetErrorNumber()
        {
            return _responseNumber;
        }

        /// <summary>
        /// Sets the password for a connection and waits if there is any response, if there is, it may continue, unless the reponse contains an error message
        /// </summary>
        /// <param name="password">password to set</param>
        /// <returns>true/false depending if error occured</returns>
        public bool SetPassWord(string password)
        {
            _writer.WriteLine("PASS " + password + Environment.NewLine);
            _writer.Flush();

            while (true)
            {
                string ircData = _reader.ReadLine();
                if (ircData.Contains("462"))
                {
                    _responseNumber = 462;
                    _errorMessage = "PASSWORD ALREADY REGISTERED";
                    return false;
                }
                else if (ircData.Contains("461"))
                {
                    _responseNumber = 461;
                    _errorMessage = "PASSWORD COMMAND NEEDS MORE PARAMETERS";
                    return false;
                }
                else if (ircData.Contains("004"))
                {
                    return true;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Sends the user and nick command to the irc server, checks for error messages (it waits for a reply to come through first, before deciding what to do). 
        /// </summary>
        /// <param name="user">Username</param>
        /// <param name="channel"></param>
        /// <returns>True/False, depending if error occured or not</returns>
        public bool JoinNetwork(string user, string channels)
        {
            _username = user;
            _channels = channels;
            _writer.WriteLine("NICK " + user + Environment.NewLine);
            _writer.Flush();
            _writer.WriteLine("USER " + user + " 8 * :" + user + "_SimpleIRCLib" + Environment.NewLine);
            _writer.Flush();

            while (true)
            {
                try
                {
                    string ircData = _reader.ReadLine();
                    if (ircData != null)
                    {
                        if (ircData.Contains("PING"))
                        {
                            string pingID = ircData.Split(':')[1];
                            _writer.WriteLine("PONG :" + pingID);
                            _writer.Flush();
                        }

                        if (CheckMessageForError(ircData))
                        {
                            if (_responseNumber == 266)
                            {
                                return JoinChannel(channels, user);
                            }
                        }
                    }
                    Thread.Sleep(1);
                }
                catch (Exception e)
                {

                    Debug.WriteLine("RECEIVED: " + e.ToString());
                    return false;
                }
            }
        }

        /// <summary>
        /// Sends a join request to the irc server, then waits for a response before continueing
        /// </summary>
        /// <param name="channels">Channels to join</param>
        /// <param name="username">Username that joins</param>
        /// <returns>True on sucess, false on error</returns>
        public bool JoinChannel(string channels, string username)
        {
            _channels = channels;

            _writer.WriteLine("JOIN " + channels + Environment.NewLine);
            _writer.Flush();
            while (true)
            {

                string ircData = _reader.ReadLine();
                if (ircData != null)
                {
                    if (ircData.Contains("PING"))
                    {
                        string pingID = ircData.Split(':')[1];
                        _writer.WriteLine("PONG :" + pingID);
                        _writer.Flush();
                    }

                    if (ircData.Contains(username) && ircData.Contains("JOIN"))
                    {
                        return true;
                    }
                }
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Sends the set nickname request to the server, waits until a response is given from the server before deciding to continue
        /// </summary>
        /// <param name="nickname">Nickname</param>
        /// <returns>True on success, false on error.</returns>
        public bool SetNickName(string nickname)
        {
            _writer.WriteLine("NICK " + nickname + Environment.NewLine);
            _writer.Flush();

            while (true)
            {

                string ircData = _reader.ReadLine();

                return CheckMessageForError(ircData);
            }
        }



        public bool CheckMessageForError(string message)
        {
            string codeString = message.Split(' ')[1].Trim();
            if (int.TryParse(codeString, out _responseNumber))
            {
                foreach (string errorMessage in RFC1459Codes.ListWithErrors)
                {
                    if (errorMessage.Contains(codeString))
                    {
                        _errorMessage = errorMessage;
                        return false;
                    }
                }

                _errorMessage = "Message does not contain Error Code!";
                return true;
            }
            else
            {
                Debug.WriteLine("Could not parse number");
                _responseNumber = 0;
                _errorMessage = "Message does not contain Error Code, could not parse number!";
                return true;
            }


        }
    }
}
