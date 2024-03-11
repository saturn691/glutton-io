using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
// Extra packages
// using System.ArraySegment;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ServerConnect : MonoBehaviour
{
    #region Instance

    public static ServerConnect instance { get; private set; } // Singleton instance
    private ClientWebSocket client; // Keep the client accessible
    public PlayersManager playersManager;
    public MassSpawner massSpawner;

    public PlayerMovements playerMovements;
    
    public async Task SendWsMessage(ClientMessage msg)
    {
        // Debug.Log("Sending new ws message!");
        if (client != null && client.State == WebSocketState.Open)
        {
            string jsonData = JsonConvert.SerializeObject(msg);

            // Convert JSON data to string
            var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonData));

            // Send the message to the server
            await client.SendAsync(
                bytesToSend, 
                WebSocketMessageType.Text, 
                true, 
                CancellationToken.None
            );
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    #endregion

    async Task InitWsConnection()
    {  
        string url = "ws://3.10.169.198:8080";
        // string url = "ws://localhost:8080";
        var serverUri = new Uri(url);
        Debug.Log("Connecting to " + serverUri + "...");
        using (client = new ClientWebSocket())
        {
            try
            {
                // Connect to the WebSocket server
                await client.ConnectAsync(serverUri, CancellationToken.None);
                Debug.Log("Connected!");

                // Create JSON formatted data
                await SendWsMessage(new ClientMessage(
                    ClientMsgType.Join, playerMovements.blob
                ));

                // new JoinMsgData("player1") // TODO: Change to unique blobId

                await ReceiveMessages();
            }
            catch (WebSocketException e)
            {
                Debug.Log("Exception: " + e.Message);
            }
        }
    }

    async Task HandleServerMessage(ServerMessage msg)
    {
        switch (msg.type)
        {
            case ServerMsgType.InitSocketId:
                if (playersManager == null)
                {
                    playersManager = PlayersManager.instance;
                    Debug.Log("Players manager is null");
                }
                playersManager.Init(msg.data);
                // Init blobs too
                break;
            case ServerMsgType.PlayerJoined:
                Debug.Log("Received player joined msg");
                ServerUtils.HandlePlayerJoined(playersManager, msg.data);
                break;
            case ServerMsgType.PlayerLeft:
                // Debug.Log("Player left: " + msg.data);
                break;
            case ServerMsgType.UpdatePlayersPosition:
                // Debug.Log("Update players position: " + msg.data);
                ServerUtils.HandleUpdatePlayersPosition(playersManager, msg.data);
                break;
            case ServerMsgType.FoodAdded:
                ServerUtils.HandleFoodAdded(massSpawner, msg.data);
                break;
            case ServerMsgType.PlayerAteFood:
                ServerUtils.HandlePlayerAteFood(playersManager, massSpawner, msg.data);
                break;
            default:
                Debug.LogWarning("Unknown message type received: " + msg.type);
                break;
        }
    }

    async Task ReceiveMessages()
    {
        var buffer = new byte[1024 * 4];
        
        while (client.State == WebSocketState.Open)
        {
            var result = await client.ReceiveAsync(
                new ArraySegment<byte>(buffer), 
                CancellationToken.None
            );

            if (client.State == WebSocketState.CloseReceived)
            {
                // If the server has initiated a close, respond with a close as well
                await client.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, 
                    string.Empty, 
                    CancellationToken.None
                );
            }
            else if (result != null && 
                    result.MessageType == WebSocketMessageType.Close
            ) {
                // If a close message is received, initiate the close 
                // handshake if it hasn't been done yet
                await client.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, 
                    string.Empty, 
                    CancellationToken.None
                );
            }

            try 
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ServerMessage msg = JsonConvert.DeserializeObject<ServerMessage>(message);
                // Debug.Log("Received: " + message);
                if (msg != null)
                {
                    // Debug.Log("Handling message...");
                    await HandleServerMessage(msg);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving message: " + e.Message);
            }
        }
    }


    // Start is called before the first frame update
    public async void Start()
    {
        playersManager = PlayersManager.instance;
        massSpawner = MassSpawner.ins;
        playerMovements = PlayerMovements.instance;
        InitWsConnection();
        // ReceiveMessages();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private async Task CloseWebSocketAsync()
    {
        if (client != null && (client.State == WebSocketState.Open || client.State == WebSocketState.Connecting))
        {
            try
            {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.LogError($"WebSocket closing error: {e.Message}");
            }
        }
    }

    void OnDestroy()
    {
        // Because OnDestroy cannot be async, we start the close operation without awaiting it.
        var _ = CloseWebSocketAsync(); // Fire-and-forget (not ideal, but necessary under these circumstances)
    }

    void OnApplicationQuit()
    {
        // Same approach for application quitting scenario
        var _ = CloseWebSocketAsync(); // Fire-and-forget
    }

}
