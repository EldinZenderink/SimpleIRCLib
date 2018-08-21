# SimpleIRCLib for Csharp
**THIS LIBRARY IS STILL IN DEVELOPMENT**
**STARTING AT V2.0.0 THIS LIBRARY IS NOT BACK-WARDS COMPATIBLE WITH PREVIOUS VERSIONS!**

This library is designed to make communication through IRC easier to implement in your application. In comparison to other C# IRC libraries, this library also enables you to download using the DCC (XDCC) protocol, used by IRC. 

It's main features are:

  - Is simple to use.
  - Is simple to implement
  - It's lightweight
  - Multiple channel support
  - DCC Download support

### NuGet
[NuGet Package](https://www.nuget.org/packages/SimpleIRCLib)

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

1.2.1
- Functionality update: Send Raw Messages (such as NICK, PRIVMSG etc.)
- Functionality update: Retreive Raw Data
- Functionality update: Send /msg using normal sendMessage() function
- Functionality update: Receive Notice message just like PRIVMSG (end user sees the source of the notice, for example: NickServ: bla bla bla)
- Code update: changed parsing messages a bit.

1.2.2
- Fixed issue with downloading filesizes larger than 4Gb.

1.2.3
- Abort download only deletes file if it was actually downloading the file.(had issues where it would still delete even if the file was already done downloading)
 
1.2.4
- Bug where some DCC SEND messages from bots were not detected & parsed properly. 
- Should support IPv6 now. (Not really tested).
- Added function to check if download thread is still running.
- Added some debug messages for debugging purposes.
- Default download directory is now set to the same directory where the libary resides.
- Added flag to check if an error occured of any kind within the library (doesn't tell what kind of error yet).

2.0.0
- Rewritten IrcConnect class (now called IrcClient) to prevent Race conditions!
- Added timeout warnings.
- Added support for TLS/SSL
- Added better error handeling using the error codes from RFC 1459 IRC Protocol
- Added full support for receiving and sending to seperate channels
- Added comments and changed names to fit C# code convention
- Under the hood DCC fixes for more stability and error handeling when downloads go wrong
- **Changed Action based callback methods to Event handlers**

2.1.0
- Support for multiple .NET FrameWorks such as: .NET 4.5, .NET 4.6, .NET 4.7 & .NET CORE 2.0 & .NET CORE 2.1
- Renamed a few properties.

2.1.1 & 2.1.2
- DCC Fixes

2.2.0
- Moved from multiple target framework to single framework: NETStandard2.0. 

### Wiki
To get a better picture of the available methods and properties, go to this wiki:
[SimpleIRCLib Wiki](https://github.com/EldinZenderink/SimpleIRCLib/wiki/SimpleIRCLib-Methods-Wiki#simpleirc)

For a full WinForms example, go to:
[WinForm Example](https://github.com/EldinZenderink/SimpleIRCLib/tree/master/FormExample)

For a simplified console example:
[Console Example](https://github.com/EldinZenderink/SimpleIRCLib/tree/master/IrcLibTest)

### Tutorial
A winform example including video tutorial (v1.1.2) **OLD - NOT SUPPORTED FOR v2.0.0**: 
[YouTube](https://www.youtube.com/watch?v=Y5JPdwFwoSI)

-New v2.0.0 and up tutorial comming soon!

### Development
I will try to fix (significant) bugs as quick as possible, but due to my study taking a rollercoaster dive in a few days it might take a while before an actual update will appear. It is very barebone and does need some refinement. So, progress in development will come down to how much free time I have and how much of it I want to spend working on this library.

### Todos

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

