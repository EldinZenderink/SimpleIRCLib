## `DCCClient`

```csharp
public class SimpleIRCLib.DCCClient

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | BotName |  | 
| `Int64` | BytesPerSecond |  | 
| `String` | CurrentFilePath |  | 
| `Boolean` | IsDownloading |  | 
| `Int32` | KBytesPerSecond |  | 
| `Int32` | MBytesPerSecond |  | 
| `String` | NewDccString |  | 
| `String` | NewFileName |  | 
| `Int64` | NewFileSize |  | 
| `String` | NewIp |  | 
| `String` | NewIp2 |  | 
| `Int32` | NewPortNum |  | 
| `String` | PackNum |  | 
| `Int32` | Progress |  | 
| `String` | Status |  | 


Events

| Type | Name | Summary | 
| --- | --- | --- | 
| `EventHandler<DCCDebugMessageArgs>` | OnDccDebugMessage |  | 
| `EventHandler<DCCEventArgs>` | OnDccEvent |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | AbortDownloader(`Int32` timeOut) |  | 
| `Boolean` | CheckIfDownloading() |  | 
| `void` | Downloader() |  | 
| `void` | StartDownloader(`String` dccString, `String` downloaddir, `String` bot, `String` pack, `IrcClient` client) |  | 


## `DCCDebugMessageArgs`

```csharp
public class SimpleIRCLib.DCCDebugMessageArgs

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | Message |  | 
| `String` | Type |  | 


## `DCCEventArgs`

```csharp
public class SimpleIRCLib.DCCEventArgs

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | Bot |  | 
| `Int64` | BytesPerSecond |  | 
| `String` | DccString |  | 
| `String` | FileName |  | 
| `Int64` | FileSize |  | 
| `String` | Ip |  | 
| `Int32` | KBytesPerSecond |  | 
| `Int32` | MBytesPerSecond |  | 
| `String` | Pack |  | 
| `Int32` | Port |  | 
| `Int32` | Progress |  | 
| `String` | Status |  | 


## `IrcClient`

```csharp
public class SimpleIRCLib.IrcClient

```

Events

| Type | Name | Summary | 
| --- | --- | --- | 
| `EventHandler<IrcDebugMessageEventArgs>` | OnDebugMessageReceived |  | 
| `EventHandler<IrcReceivedEventArgs>` | OnMessageReceived |  | 
| `EventHandler<IrcRawReceivedEventArgs>` | OnRawMessageReceived |  | 
| `EventHandler<IrcUserListReceivedEventArgs>` | OnUserListReceived |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | CheckIfDownloading() |  | 
| `Boolean` | Connect() |  | 
| `Boolean` | GetUsersInChannel(`String` channel = ) |  | 
| `Boolean` | IsClientRunning() |  | 
| `Boolean` | IsConnectionEstablished() |  | 
| `Boolean` | QuitConnect() |  | 
| `Boolean` | SendMessageToAll(`String` input) |  | 
| `Boolean` | SendMessageToChannel(`String` input, `String` channel) |  | 
| `Boolean` | SendRawMsg(`String` msg) |  | 
| `void` | SetConnectionInformation(`String` ip, `String` username, `String` channel, `DCCClient` dccClient, `String` downloadDirectory, `Int32` port = 0, `String` password = , `Int32` timeout = 3000, `Boolean` enableSSL = True) |  | 
| `void` | SetDownloadDirectory(`String` downloadDirectory) |  | 
| `void` | StartReceivingChat() |  | 
| `void` | StopClient() |  | 
| `Boolean` | StopXDCCDownload() |  | 
| `Boolean` | WriteIrc(`String` input) |  | 


## `IrcDebugMessageEventArgs`

```csharp
public class SimpleIRCLib.IrcDebugMessageEventArgs

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | Message |  | 
| `String` | Type |  | 


## `IrcRawReceivedEventArgs`

```csharp
public class SimpleIRCLib.IrcRawReceivedEventArgs

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | Message |  | 


## `IrcReceivedEventArgs`

```csharp
public class SimpleIRCLib.IrcReceivedEventArgs

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | Channel |  | 
| `String` | Message |  | 
| `String` | User |  | 


## `IrcUserListReceivedEventArgs`

```csharp
public class SimpleIRCLib.IrcUserListReceivedEventArgs

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Dictionary<String, List<String>>` | UsersPerChannel |  | 


## `SimpleIRC`

```csharp
public class SimpleIRCLib.SimpleIRC

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `DCCClient` | DccClient |  | 
| `IrcClient` | IrcClient |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | CheckIfDownload() |  | 
| `void` | GetUsersInCurrentChannel() |  | 
| `void` | GetUsersInDifferentChannel(`String` channel) |  | 
| `Boolean` | IsClientRunning() |  | 
| `Boolean` | SendMessageToAll(`String` message) |  | 
| `Boolean` | SendMessageToChannel(`String` message, `String` channel) |  | 
| `Boolean` | SendRawMessage(`String` message) |  | 
| `void` | SetCustomDownloadDir(`String` downloaddir) |  | 
| `void` | SetupIrc(`String` ip, `String` username, `String` channel, `Int32` port = 0, `String` password = , `Int32` timeout = 3000, `Boolean` enableSSL = True) |  | 
| `Boolean` | StartClient() |  | 
| `Boolean` | StopClient() |  | 
| `Boolean` | StopXDCCDownload() |  | 


