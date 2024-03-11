using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class PlayersManager : MonoBehaviour
{
    #region instance 
    public static PlayersManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #endregion

    public GameObject PlayerMass;

    public Dictionary<string, Player> PlayersDict = new Dictionary<string, Player>();
    public int MaxPlayers = 10;

    private Vector2 oldPosition; // Store the old position

    public string selfSocketId;

    private void Start()
    {
    }


    /// <summary>
    /// Initialize the players manager with the players data
    /// The schema must match EXACTLY the server's schema or an exception will
    /// be thrown
    /// </summary>
    /// <param name="msgData">The JSON data from the server</param>
    public void Init(object msgData)
    {
        Debug.Log("Initializing players manager!");
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(msgData.ToString());

        selfSocketId = (string)data["socketId"];    // handle self-identification

        var players = JsonConvert.DeserializeObject<Dictionary<string, object>>(data["players"].ToString());
        
        foreach (KeyValuePair<string, object> kvp in players) {

            // Player deserialised, ensure class matches the server
            
            Player player = JsonConvert.DeserializeObject<Player>(kvp.Value.ToString());
            Debug.Log("Existing player: " + player.socketId + " Position: " + player.blob.position.x + " " + player.blob.position.y);
            AddPlayer(player);  
        }

        Debug.Log($"Player {selfSocketId} initialized!");

        return;
    }

    public void AddPlayer(Player player)
    {

        Vector2 Position = new Vector2(
            player.blob.position.x, 
            player.blob.position.y
        );

        // TODO change from PlayerMass to an ACTUAL player (in the inspector)
        GameObject p = Instantiate(PlayerMass, Position, Quaternion.identity);
        
        // Set the color of the player
        SpriteRenderer sr = p.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Color is a 24-bit integer
            sr.color = new Color(
                (player.color >> 16) & 0xFF, 
                (player.color >> 8) & 0xFF, 
                player.color & 0xFF
            );
        }

        player.blob.gameObject = p;

        // Set size of gameObject
        float r = Blob.GetRadius(player.blob.size);
        p.transform.localScale = new Vector3(r, r, r);

        // Add the player to the dictionary
        PlayersDict.Add(
            player.socketId, 
            player
            // new Player(
            //     player.socketId, 
            //     player.color, 
            //     false, 
            //     new Blob(player.socketId, player.blob.position, player.blob.size), 
            //     p
            // )
        );
    }

    public void UpdatePlayerPosition(string socketId, float x, float y)
    {
        // TODO | At the moment the player is a mass, so this will throw an 
        // TODO | exception when it is eaten. The collision must be done 
        // TODO | server side.
        
        var playerObj = PlayersDict[socketId].blob.gameObject;
        PlayersDict[socketId].blob.position = new Position(x, y);
        playerObj.transform.position = new Vector2(x, y);
    }

    /// <summary>
    /// TODO_COMMENT
    /// </summary>
    public void UpdatePlayerSize(string socketId, int newSize)
    {
        PlayersDict[socketId].blob.size = newSize;
        float r = Blob.GetRadius(newSize);
        PlayersDict[socketId].blob.gameObject.transform.localScale = new Vector3(r, r, r);
    }

    public void Update()
    {
        // Get old position
        // oldPosition = PlayersDict["player1"].transform.position;
        // // Update the position
        // PlayersDict["player1"].transform.position = new Vector2(oldPosition.x + 0.001f, oldPosition.y);
    }

}
