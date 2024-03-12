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

    public PlayerMovement playerMovement;
    
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
        // string url = "ws://3.10.169.198:8080";
        string url = "ws://localhost:8080";
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
                Blob playerBlob = playerMovement.blob;
                Blob blobWithoutGameObject = new Blob(playerBlob.id, playerBlob.size, playerBlob.position, null);
                await SendWsMessage(new ClientMessage(
                    ClientMsgType.Join,
                    blobWithoutGameObject 
                ));

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
                break;
            case ServerMsgType.UpdatePlayersPosition:
                ServerUtils.HandleUpdatePlayersPosition(playersManager, msg.data);
                break;
            case ServerMsgType.FoodAdded:
                ServerUtils.HandleFoodAdded(massSpawner, msg.data);
                break;
            case ServerMsgType.PlayerAteFood:
                ServerUtils.HandlePlayerAteFood(playersManager, massSpawner, msg.data);
                break;

            case ServerMsgType.PlayerAteEnemy:
                ServerUtils.HandlePlayerAteEnemy(playersManager, msg.data);
                break;

            default:
                Debug.LogWarning("Unknown message type received: " + msg.type);
                break;
        }
    }

    async Task ReceiveMessages()
    {
        var buffer = new byte[1024 * 4];
        
        while (client != null && client.State == WebSocketState.Open)
        {
            if (client == null) break;
            var result = await client.ReceiveAsync(
                new ArraySegment<byte>(buffer), 
                CancellationToken.None
            );

            // TODO: Handle close properly. What does a close message look like...?
            if (client.State == WebSocketState.CloseReceived)
            {
                Debug.Log("Close received");

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
                    await HandleServerMessage(msg);
                }
            }
            catch (Exception e)
            {
                // Log, but don't close the script by logging an error
                Debug.Log("Error receiving message: " + e.Message);
            }
        }
    }


    // Start is called before the first frame update
    public async void Start()
    {
        playersManager = PlayersManager.instance;
        massSpawner = MassSpawner.ins;
        playerMovement = PlayerMovement.instance;
        InitWsConnection();
        // ReceiveMessages();
    }


    private async Task CloseWebSocketAsync()
    {
        if (client != null && (client.State == WebSocketState.Open || client.State == WebSocketState.Connecting))
        {
            try
            {
                Debug.Log("Closing WebSocket...");
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                client = null;
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
        // Fire-and-forget (not ideal, but necessary under these circumstances)
        Debug.Log("OnDestroy");
        var _ = CloseWebSocketAsync(); 
    }

    void OnApplicationQuit()
    {
        // Same approach for application quitting scenario
        var _ = CloseWebSocketAsync(); // Fire-and-forget
    }

}
