# Game Docs

## Scripts relevant to client-server interaction

### Classes

1. **ClientMessage / ServerMessage**

| param | type | description |
| --- | --- | --- |
| type  | ClientMsgType / ServerMsgType | Identifier for type of message being sent / received |
| data | object | General object representing data being sent / received by WS message |
1. **Player**

Contains info & methods regarding players in the game. Similar to **Player** class on server. 

| param | type | description |
| --- | --- | --- |
| socketId | string | Unique identifier for each player in the game, used as the key in the players state |
| position | Position | Current position of this player in x, y coordinates |
| gameObject | GameObject | GameObject for the player, used to update their movement / appearance in the actual game |

Note: On the client side, as of now, only players, not including the client, are represented by this class. 

---

### **Enums/MessageTypes.cs**

The enums / classes in this file are the same as that of **MessageTypes.ts** on the server. 

**IMPORTANT:** The order of enums in **ClientMsgType** & **ServerMsgType** must match those on the server because these are essentially int values 

---

### PlayersManager.cs

This script manages the state of the players in the game, such as their movement, size, etc. 

**Parameters**

| param | type | description |
| --- | --- | --- |
| PlayersDict | Dictionary<string, Player> | Contains information about each of the players currently in the game, such as position, size, etc. socketId is used to identify players |
| selfSocketId | string | socketId of the current client |

**Methods**

| method | description |
| --- | --- |
| Init | Called when the client first connects to the server. Initialises selfSocketId which is generated server side and also existing PlayersDict state for current players in the game |
| AddPlayer | Called when ServerMsgType.PlayerJoined is received. Adds the new player to the PlayersDict state |
| UpdatePlayerPosition | Used to update a particular player’s position on the game. Responsible for actual movement of player’s blobs in the game |

---

### ServerConnect.cs

This script manages the web socket connection state

**To use the instance of this class in other scripts**

```csharp
ServerConnect server;

void Start() {
	server = ServerConnect.instance;
}
```

**Parameters**

| param | type | description |
| --- | --- | --- |
| instance | ServerConnect | Used to allow other classes to access a single instance of this class. For e.g. to call SendWsMessage from other classes |
| client | ClientWebSocket | WebSocket client used to make the connection & send / receive messages |

**Methods**

| method | description |
| --- | --- |
| SendWsMessage | Used to send a ClientMessage to the server.  |
| InitWsConnection | Method to connect to server called once on script Start() |
| HandleServerMessage | Top level function which handles messages received from the server and calls the appropriate functions to handle each ServerMsgType. Actual functions should usually be written in ServerUtils class so as not to bloat the file |

---

### ServerUtils.cs

Helper class for to write handler functions for messages received from server  

**Functions**

| method | description |
| --- | --- |
| HandlePlayerJoined | Handles ServerMsgType.PlayerJoined message. Used to add the new player to the PlayersDict state |
| HandleUpdatePlayersPosition | Handles ServerMsgType.UpdatePlayersPosition message. Takes in a list of Dictionary<string, Position> indicating new positions for each socketId, and then updates each socketId correspondingly in the PlayersDict state |

Note: ServerUtils doesn’t have access to the PlayerManagers state, and therefore functions inside of it are usually marked **static** and must be passed in its arguments any state instances to manipulate (e.g. PlayersManager instance as pmInst)