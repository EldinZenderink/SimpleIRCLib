## `DebugForm`

```csharp
public class FormExample.DebugForm
    : Form, IComponent, IDisposable, IOleControl, IOleObject, IOleInPlaceObject, IOleInPlaceActiveObject, IOleWindow, IViewObject, IViewObject2, IPersist, IPersistStreamInit, IPersistPropertyBag, IPersistStorage, IQuickActivate, ISupportOleDropSource, IDropTarget, ISynchronizeInvoke, IWin32Window, IArrangedElement, IBindableComponent, IContainerControl

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | ClearButton_Click(`Object` sender, `EventArgs` e) | Clears a specific rich textbox | 
| `void` | DebugForm_FormClosing(`Object` sender, `FormClosingEventArgs` e) | Makes sure that the form doesn't actually close, but hides instead, so debug messages can still be appended! | 
| `void` | DebugForm_Load(`Object` sender, `EventArgs` e) | Register event handlers for the IrcClient and DCCClient when the form loads. | 
| `void` | Dispose(`Boolean` disposing) | Clean up any resources being used. | 
| `void` | OnDccDebugMessage(`Object` source, `DCCDebugMessageArgs` args) | Event for receiving debug messages from the DccClient | 
| `void` | OnDccDebugMessageLocal(`String` type, `String` message) | For appending the debug message on the main thread using invoke required. | 
| `void` | OnIrcDebugMessage(`Object` source, `IrcDebugMessageEventArgs` args) | Event for receiving debug messages from the IrcClient | 
| `void` | OnIrcDebugMessageLocal(`String` type, `String` message) | For appending the debug message on the main thread using invoke required. | 
| `void` | OnRawMessageReceived(`Object` source, `IrcRawReceivedEventArgs` args) | Event for receiving raw messages from the irc server. | 
| `void` | OnRawMessageReceivedLocal(`String` message) | For appending the rawmessage on the main thread using invoke required. | 


## `IrcClientForm`

This class is meant as example on how to use SimpleIRCLib, this does not mean that this is the correct way to program!  It's meant to showcase a few of the available methods within SimpleIRCLib, you should figure out on your own how to implement it to suit your needs!  It lacks a few options, such as leaving a specific channel, which will be implemented in the future. If your knowledged, you could send a raw message  to the server containing commands to PART from a channel and use the OnRawMessageReceived event to check the server response.
```csharp
public class FormExample.IrcClientForm
    : Form, IComponent, IDisposable, IOleControl, IOleObject, IOleInPlaceObject, IOleInPlaceActiveObject, IOleWindow, IViewObject, IViewObject2, IPersist, IPersistStreamInit, IPersistPropertyBag, IPersistStorage, IQuickActivate, ISupportOleDropSource, IDropTarget, ISynchronizeInvoke, IWin32Window, IArrangedElement, IBindableComponent, IContainerControl

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | ConnectButton_Click(`Object` sender, `EventArgs` e) | Gets the values from the input fields and starts the client | 
| `void` | DisconnectButton_Click(`Object` sender, `EventArgs` e) | Disconnects the irc client, closes all open tabs. | 
| `void` | Dispose(`Boolean` disposing) | Clean up any resources being used. | 
| `void` | DownloadsList_MouseDoubleClick(`Object` sender, `MouseEventArgs` e) | Opens the folder where the selected file is being downloaded to | 
| `void` | Form1_FormClosing(`Object` sender, `FormClosingEventArgs` e) | Stops the irc client on form close, otherwise it would keep running in the background!!! | 
| `void` | MessageInput_KeyDown(`Object` sender, `KeyEventArgs` e) | Sends if enter is pressed. | 
| `void` | OnDccEvent(`Object` sender, `DCCEventArgs` args) | Event that fires when DCCClient starts downloading. | 
| `void` | OnMessagesReceived(`Object` sender, `IrcReceivedEventArgs` args) | Event handler for receiving messages from the Irc Client. | 
| `void` | OnMessagesReceivedLocal(`String` channel, `String` user, `String` message) | Method that gets invoked on the main thread, adds a message to the richtextbox within a the correct channel tab. | 
| `void` | OnUserListReceived(`Object` sender, `IrcUserListReceivedEventArgs` args) | Event that gets fired when a user list has been received. | 
| `void` | OnUserListReceivedLocal(`Dictionary<String, List<String>>` userList) | Method to invoke on the main thread, checks if a tab for the chat exists with name of the channel, if not, it creates it, same goes for the tab with the user name list. | 
| `void` | SendToAll_Click(`Object` sender, `EventArgs` e) | Sends a message to all channels. | 
| `void` | SendToChannel_Click(`Object` sender, `EventArgs` e) | Sends a message to a specific channel. | 
| `void` | SetDownloadFolderButton_Click(`Object` sender, `EventArgs` e) | Set download  directory to a custom directory | 
| `void` | ShowDebugButton_Click(`Object` sender, `EventArgs` e) | Opens the debug form | 
| `void` | UpdateDownloadList(`String` toUpdate, `String` fileName) | Updates the DownloadList object on the main form while the download is going, invoke is necesary because  method is being called from a different Thread! | 
| `void` | UpdateProgressBar(`Int32` progress) | Updates the progress bar | 
| `void` | UpdateUserList_Click(`Object` sender, `EventArgs` e) | Gets the users in the current channel. | 


