# SimpleIRCLib for Csharp
**THIS LIBRARY IS STILL IN DEVELOPMENT**

This library is designed to make communication through IRC easier to implement in your application. In comparison to other C# IRC libraries, this library also enables you to download using the DCC (XDCC) protocol, used by IRC. 

It's main features are:

  - Is simple to use.
  - Is simple to implement
  - It's lightweight
  - Multiple channel support
  - DCC Download support

### NuGet
[NuGet Package](https://www.nuget.org/packages/SimpleIRCLib)

### Direct Download DLL
[DLL](https://github.com/EldinZenderink/SimpleIRCLib/raw/master/Library/SimpleIRCLib.dll)

### Version
1.0.0:
- First release version

1.0.1:
- Fixed bug where leaving out debug callback would cause nullreferenceexception. 


1.1.1:
- Added split in username | message
- Added quit/disconnect method
- removed usesless getters and setters
- fixed a few more less significant bugs

1.1.2:
- Removed getDownloadProgress, now download information can be retreived within the  downloadStatusChangedCallback.
- Fixed error when you try to check if client is running before connecting to a server!

1.1.3:
- Code mostly reworked by following feedback from this post: [reddit](https://www.reddit.com/r/csharp/comments/452i80/simple_irc_library_with_dcc_download_option_d/), such as:
- removed static fields
- removed inheriting classes
- text now being parsed through regex
- for receiving messages: a async task instead of a thread
- getDownloadProgress/downloadStatusChangedCallback has different way of operation (see below)
- addes status attribute for downloadstatus/progress

1.1.3 -> 1.1.7:
- stability issues fixed

1.1.8:
- bugfixes, see commit description

1.1.9:
- bugfixes, see commit description

1.2.0
- Functionality update: Added method to retreive all the users in the current channel, as well as in other channels (but most of them will be hidden, so not very usefull ^^).

 
### Usage - Console Application

*TIP: If you do not want a seperate DLL file with your program you can either copy the .cs files to your solution/project and manually change the Namespace, or you can use a program called [ILMerge](https://www.microsoft.com/en-us/download/details.aspx?id=17630) to combine a exe and dll together(not tested)!*

This is a list with the most important methods available to you:

    (void)        setupIrc(ip, port, username, password, channel, chatOutputCallback);
    (void)        setDebugCallback(debugOutputCallback);
    (string)      downloadDir;            //is now string, can be changed while running instance 
    (void)        setDownloadStatus(downloadStatusCallback);
    (void)        startClient();
    (void)        stopClient();
    (bool)        isClientRunning();
    (bool)        stopXDCCDownload(); 
    (object)      getDownloadProgress(string whichdownloaddetail) //see below
    (void)        getUsersInCurrentChannel();
    (void)        getUsersInDifferentChannel(string channel)
    (void)        sendMessage(message); 
    (string)      newUsername;            //is now string field instead of method
    (string)      newChannel;             //is now string field instead of method


Before you start programming, you need to get the package on the NuGet page for this library, or you need to download the dll file manually and reference it in your c# solution/project. Afterwards, you need to do the following:

`using SimpleIrcLib;`

After doing that, add the following code to start your irc client:

    SimpleIRC irc = new SimpleIRC();
    irc.setupIrc(ip, port, username, password, channel, chatOutputCallback);
    irc.setDebugCallback(debugOutputCallback);
    irc.setDownloadStatusChangeCallback(downloadStatusCallback);
    irc.setUserListReceivedCallback(userListReceivedCallback);
    irc.startClient();
    
Your callbacks should/could look like this:

**chatOutputCallback:**

    void chatOutputCallback(string user, string message)
    {
        Console.WriteLine(user + ":" + message);
    }


**debugOutputCallback:**

    void debugOutputCallback(string debug)
    {
        Console.WriteLine("===============DEBUG MESSAGE===============");
        Console.WriteLine(debug);
        Console.WriteLine("===============END DEBUG MESSAGE===============");
    }
    
**downloadStatusCallback:**

    void downloadStatusCallback() //see below for definition of each index in this array
    {
         Object information = irc.getDownloadProgress("progress");
         Object speedkbps = irc.getDownloadProgress("kbps");
         Object status = irc.getDownloadProgress("status");
         Object filename = irc.getDownloadProgress("filename");
    }

**userListReceivedCallback**

    void downloadStatusCallback(string[] users) //see below for definition of each index in this array
    {
         foreach(string user in users){
            Console.WriteLine(user);            
         }
    }
    

And here is a bit of gibrish code for sending messages to the irc server:
    
    while (true)  //irc output and such are handled in different threads
    {
        string Input = Console.ReadLine();
        if (Input != null || Input != "" || Input != String.Empty)
        {
            irc.sendMessage(Input);
        }
    }


For getting information about the download in progress (in `downloadStatusChangeCallback()`), you can use this function:

`getDownloadProgress(string whichdownloaddetail)`

This will return an array of strings when a download is running, but if there is no download while you are requesting information, it will return a empty array, except for the first index which will contain the string "NULL".

Array that will be returned when downloading:

| Whichdownloaddetail  | What it is    | Explanation |
| ------------- |:-------------:| ----- |
| dccstring     | DCC receive string     | The bot sends you a string with connection details.|
| filename      | Filename               | Filename of the file that you currently are downloading. |
| size          | Filesize               | Size of the file that you currently  are downloading. (In bytes)|
| ip            | Server IP              | IP from the file server to which you have connected. |
| port          | Server Port            | Port from the file server to which you have connected. |
| pack          | Pack Number            | Packnumer corresponding to the file you have requested through XDCC.|
| bot           | Bot Name               | Bot from which you requested the file. |
| progress      | Progress               | Progress of completion is %.  |
| status        | status of download/connection| Gives status about download and connection, such as, waiting, downloading, failed | etc
| bps           | Bytes Per Second       | |
| kbps          | KBytes Per Second      | |
| mbps          | MBytes Per Second      | |



### Full Example 
An (quick and dirty) example can be found here: 
[Example](https://github.com/EldinZenderink/SimpleIRCLib/blob/master/IrcLibTest/Program.cs)

A winform example including video tutorial (v1.1.2): 
[YouTube](https://www.youtube.com/watch?v=Y5JPdwFwoSI)

### Development
I will try to fix (significant) bugs as quick as possible, but due to my study taking a rollercoaster dive in a few days it might take a while before an actual update will appear. It is very barebone and does need some refinement. So, progress in development will come down to how much free time I have and how much of it I want to spend working on this library.

### Todos

- MONO :D
- Some DCC fixes, most things seem to work, but there are some odd cases where it might not work.
- More readable code (getting better)
- Renaming some stupidly named names 


### Disclaimer
This library is still in alpha stadium, many things might go wrong and therefore I am not 
responsible for whatever happens while you use this application.

License
----

MIT


**Free Software, Hell Yeah!**

