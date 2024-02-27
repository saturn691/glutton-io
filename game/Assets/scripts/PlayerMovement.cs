using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Extra packages
// using System.ArraySegment;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


public class PlayerMovements : MonoBehaviour
{

    public float Speed = 5f;
    // websocket  connection

    async void InitWsConnection() {
        var serverUri = new Uri("ws://localhost:8080");
        Debug.Log("Connecting to " + serverUri + "...");
        using (var client = new ClientWebSocket())
        {
            try
            {

                // 
                // Connect to the WebSocket server
                await client.ConnectAsync(serverUri, CancellationToken.None);
                Debug.Log("Connected!");

                // Create JSON formatted data
                var msg = new WsMessage("message", "Hello from Unity!");
                var jsonData = JsonUtility.ToJson(msg);

                // Convert JSON data to string
                var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonData));

                // Send the message to the server
                await client.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                Debug.Log("First message sent to server");

                await ReceiveMessages(client);
            }
            catch (WebSocketException e)
            {
                Debug.Log("Exception: " + e.Message);
            }
        }
    }

    async Task ReceiveMessages(ClientWebSocket client)
    {
        var buffer = new byte[1024 * 4];
        try
        {
            while (client.State == WebSocketState.Open)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close) {
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                } else {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.Log("Message received from server: " + message);

                    // Process the message as needed
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving: " + e.Message);
        }
    }

    // Start is called before the first frame update
    void Start() {
        Debug.Log("PlayerMovement script start!");
        InitWsConnection();
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("Updating player movement");
        float Speed_ = Speed / transform.localScale.x;
        Vector2 Direction = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = Vector2.MoveTowards(transform.position, Direction, Speed_ * Time.deltaTime);
    }
}
