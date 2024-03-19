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
    public string selfSocketId;
    public PlayerMovement playerMovement;

    private void Start()
    {
        playerMovement = PlayerMovement.instance;
    }


    /// <summary>
    /// Initialize the players manager with the players data
    /// The schema must match EXACTLY the server's schema or an exception will
    /// be thrown
    /// </summary>
    /// <param name="msgData">The JSON data from the server</param>
    public void Init(object msgData)
    {

        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(msgData.ToString());

        selfSocketId = (string)data["socketId"];    // handle self-identification

        var players = JsonConvert.DeserializeObject<Dictionary<string, object>>(data["players"].ToString());
        
        foreach (KeyValuePair<string, object> kvp in players) {

            // Player deserialised, ensure class matches the server
            
            Player player = JsonConvert.DeserializeObject<Player>(kvp.Value.ToString());
            AddPlayer(player);  
        }

        Debug.Log($"Initialised PlayersManager!");

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
            Color playerColor = new Color(
                (float)((player.color >> 16) & 0xFF) / (float) 255, 
                (float)((player.color >> 8) & 0xFF) / (float) 255, 
                (float)((player.color) & 0xFF) / (float) 255
            );

            Debug.Log("Player color:" + playerColor);

            sr.color = playerColor;
        }

        player.blob.gameObject = p;

        // Set size of gameObject
        float r = Blob.GetRadius(player.blob.size);
        p.transform.localScale = new Vector3(r, r, r);

        // Add the player to the dictionary
        PlayersDict.Add(
            player.socketId, 
            player
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

    /// <summary>
    /// TODO_COMMENT
    /// </summary>
    public void RemovePlayerById (string socketId)
    {
        Debug.Log("Removing player: " + socketId);
        Destroy(PlayersDict[socketId].blob.gameObject);
        PlayersDict.Remove(socketId);
    }
    
    /// <summary>
    /// TODO COMMENT Method to be called when the player has eaten a food object.
    /// Update new player's blob object size and rendered size
    /// </summary>
    public void UpdateSelfSize(int newSize) {
        // playerMovement.blob.size = newSize;
        playerMovement.blob.Resize(newSize);
        float newRadius = Blob.GetRadius(playerMovement.blob.size);
        transform.localScale = new Vector3(newRadius, newRadius, newRadius);

        // Play sound effect
        // soundEffectsPlayer.PlayFood();
    }
}
