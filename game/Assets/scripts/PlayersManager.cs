using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class PlayersManager : MonoBehaviour
{

    #region instance 
    public static PlayersManager instance;
    private void Awake()
    {
        if( instance == null) {
            instance = this;
        }
    }
    #endregion

    public GameObject PlayerMass;
    // public List<GameObject> Players = new List<GameObject>();
    public Dictionary<string, Player> PlayersDict = new Dictionary<string, Player>();
    public int MaxPlayers = 10;

    private Vector2 oldPosition; // Store the old position

    public string selfSocketId;

    private void Start() {
    }

    public void Init(object msgData) {

        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(msgData.ToString());
        
        selfSocketId = (string)data["socketId"];

        var players = JsonConvert.DeserializeObject<Dictionary<string, object>>(data["players"].ToString());
        
        foreach (KeyValuePair<string, object> kvp in players) {
            Player player = JsonConvert.DeserializeObject<Player>(kvp.Value.ToString());
            AddPlayer(player.socketId, player.position);
        }
    }

    public void AddPlayer(string socketId, Position position) {
        
        Vector2 Position = new Vector2(position.x, position.y);
        GameObject p =  Instantiate(PlayerMass, Position, Quaternion.identity);
        PlayersDict.Add(socketId, new Player(socketId, position, p));
    }

    public void UpdatePlayerPosition(string socketId, float x, float y) {
        var playerObj = PlayersDict[socketId].gameObject;
        playerObj.transform.position = new Vector2(x, y);
    }

    public void Update() {
        // Get old position
        // oldPosition = PlayersDict["player1"].transform.position;
        // // Update the position
        // PlayersDict["player1"].transform.position = new Vector2(oldPosition.x + 0.001f, oldPosition.y);
    }

}
