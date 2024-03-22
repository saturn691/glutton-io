# Server Docs

### Start

```bash
# switch to server directory
cd server
cd server-side

# install necessary dependencies
npm i

# run game server with nodemon
npm run dev2
```

---

### index.ts

**handleWsMessage():** top level function which handles client sent messages

**main():** initialises the WebSocket server and GameState class

```bash
const ws = new WebSocketServer({ port: 8080 });
const game = new GameState(1, ws);
```

---

### MessageType.ts

| Name | Description |
| --- | --- |
| ClientMsgType (enum) | Identifiers for different types of messages sent from client to server |
| ServerMsgType (enum) | Identifiers for different types of messages sent from server to client |
| JoinMessageData (type) | Fields sent from client to server when message type is ClientMsgType.Join |

**Note:** 

- JoinMessageData is an example given that allows you to specify the fields that are contained in [msg.data](http://msg.data) when client sends a message to server
- In general, a new type doesn’t have to be created for every message type, but rather, if the message data being sent contains many fields, it would be helpful to create a type for it to ensure no errors

---

### **GameState (Game.ts)**

This class is extremely important to the server. It essentially contains all information regarding state of the game

**Parameters**

| param | type | description |
| --- | --- | --- |
| id | number | Id of particular game. Useful if we want server to handle multiple game rooms, but for now irrelevant |
| players | Dictionary from socketId to Player | Contains information about each of the players currently in the game, such as position, size, etc. socketId is used to identify players |
| numPlayers | number | Total number of players in game |
| updatedPositions | Array of recently updated positions | Helper state used to keep track of recently updated positions. Should contain 1 per player in game. Used when sending batch update to clients’ |
|  |  |  |

**Methods**

| method | description |
| --- | --- |
| InitPlayerJoined | Called when a client first connects to the server. Sends generated socket id for client & existing players state to client that connected |
| AddPlayer | Called when ClientMsgType.Join message is received. Essentially adds new player to the players state and broadcasts ServerMsgType.PlayerJoined |
| RemovePlayer | Called when client disconnects or leaves. Removes player from players state and broadcasts ServerMsgType.PlayerLeft |
| UpdatePlayerPosition | Called when ClientMsgType.UpdatePosition is received. Updates new position for client in players state. Once it has received an update from all different players, it will broadcast ServerMsgType.UpdatePlayersPosition so that clients can update their local state |
| broadcast | Helper function to send a message to all players in game. Allows for exclusion of message to 1 player which is useful when an update is triggered by a particular client. For instance, if client C joins the game, client A & B must be updated, but not client C |

---

### Player

This class contains all information related to a particular player.

**Parameters**

| param | type | description |
| --- | --- | --- |
| socketId | string | UUID which identifies the player. Is used as the key for each player in the GameState.players object. Super important and overall identifier for player both on server and on client |
| id | string | Player identifier which will be used as a display. Not currently in use |
| color | PlayerColor | Color of player’s blobs on the game |
| socket | WebSocket | Object which contains the WebSocket connection to client. Used to send messages to this player |
| position | Position | Current position of player in x, y coordinates. Will probably need to be refactored when allowing multiple blobs per player |

**Methods**

| method | description |
| --- | --- |
| UpdatePosition | Used to update player’s position in game state |