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
[DLL](https://github.com/EldinZenderink/SimpleIRCLib/raw/master/SimpleIRCLib/bin/Debug/SimpleIRCLib.dll)

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

### Usage - Console Application

*TIP: If you do not want a seperate DLL file with your program you can either copy the .cs files to your solution/project and manually change the Namespace, or you can use a program called [ILMerge](https://www.microsoft.com/en-us/download/details.aspx?id=17630) to combine a exe and dll together(not tested)!*

This is a list with the most important methods available to you:

    (void)        setupIrc(ip, port, username, password, channel, chatOutputCallback);
    (void)        setDebugCallback(debugOutputCallback);
    (void)        setCustomDownloadDir(downloaddir);  
    (void)        setDownloadStatus(downloadStatusCallback);
    (void)        startClient();
    (void)        stopClient();
    (bool)        isClientRunning();
    (void)        sendMessage(message);
    (string)      getUsername();
    (string)      getChannel();


Before you start programming, you need to get the package on the NuGet page for this library, or you need to download the dll file manually and reference it in your c# solution/project. Afterwards, you need to do the following:

`using SimpleIrcLib;`

After doing that, add the following code to start your irc client:

    SimpleIRC irc = new SimpleIRC();
    irc.setupIrc(ip, port, username, password, channel, chatOutputCallback);
    irc.setDebugCallback(debugOutputCallback);
    irc.setDownloadStatusChangedCallback(downloadStatusCallback);
    irc.startClient();
    
Your callbacks should/could look like this:

**chatOutputCallback:**

    void chatOutputCallback(string chat)
    {
        Console.WriteLine(chat);
    }


**debugOutputCallback:**

    void debugOutputCallback(string debug)
    {
        Console.WriteLine("===============DEBUG MESSAGE===============");
        Console.WriteLine(debug);
        Console.WriteLine("===============END DEBUG MESSAGE===============");
    }
    
**downloadStatusCallback:**

    void downloadStatusCallback(string[] downloadStatus) //see below for definition of each index in this array
    {
         Console.WriteLine("File: " + downloadStatus[1] + " \n Progress: " + downloadStatus[7] + "% \n Speed: " + downloadStatus[9] + "kb/s \n");
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


For getting information about the download in progress, you can use this function:

`getDownloadProgress()`

This will return an array of strings when a download is running, but if there is no download while you are requesting information, it will return a empty array, except for the first index which will contain the string "NULL".

Array that will be returned when downloading:

| Array Index   | What it is    | Explanation |
| ------------- |:-------------:| ----- |
| 0             | DCC receive string     | The bot sends you a string with connection details.|
| 1             | Filename               | Filename of the file that you currently are downloading. |
| 2             | Filesize               | Size of the file that you currently  are downloading.|
| 3             | Server IP              | IP from the file server to which you have connected. |
| 4             | Server Port            | Port from the file server to which you have connected. |
| 5             | Pack Number            | Packnumer corresponding to the file you have requested through XDCC.|
| 6             | Bot Name               | Bot from which you requested the file. |
| 7             | Progress               | Progress of completion is %.  |
| 8             | Bytes Per Second       | |
| 9             | KBytes Per Second      | |
| 10            | MBytes Per Second      | |



### Full Example
An (quick and dirty) example can be found here: 
[Example](https://github.com/EldinZenderink/SimpleIRCLib/blob/master/IrcLibTest/Program.cs)

A winform example including video tutorial: 
[YouTube](https://www.youtube.com/watch?v=Y5JPdwFwoSI)

### Development
I will try to fix (significant) bugs as quick as possible, but due to my study taking a rollercoaster dive in a few days it might take a while before an actual update will appear. It is very barebone and does need some refinement. So, progress in development will come down to how much free time I have and how much of it I want to spend working on this library.

### Todos

- Some DCC fixes, most things seem to work, but there are some odd cases where it might not work.
- More readable code
- Maybe, if requested, DCC upload. 


### Disclaimer
This library is still in alpha stadium, many things might go wrong and therefore I am not 
responsible for whatever happens while you use this application.

License
----

MIT


**Free Software, Hell Yeah!**

