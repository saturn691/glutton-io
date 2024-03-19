using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;


public class PlayerScore : MonoBehaviour
{
    #region Instance

    public static PlayerScore instance { get; private set; } // Singleton instance

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    #endregion

    public Text Score;
    public List<LeaderboardItem> Leaderboards;
    public PlayersManager playersManager;
    public PlayerMovement playerMovement;

    public void Init(object msgData) {
        Leaderboards = new List<LeaderboardItem>();
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(msgData.ToString());
        var leaderboards = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(data["leaderboards"].ToString());

        foreach (Dictionary<string, object> item in leaderboards)
        {
            string socketId = item["socketId"].ToString();
            int size = Convert.ToInt32(item["size"]);
            Leaderboards.Add(new LeaderboardItem(socketId, size));
        }

        if (Leaderboards.Count < 5) {
            Leaderboards.Add(new LeaderboardItem(playersManager.selfSocketId, playerMovement.blob.size));
            Leaderboards.Sort((x, y) => y.size.CompareTo(x.size));
        }

        UpdateLeaderboardText();
        // Debug.Log("Initialised leaderboards: " + Leaderboards);
    }

    public void UpdateLeaderboards(string socketId, int newSize) {
        bool found = false;
        for (int i = 0; i < Leaderboards.Count; i++)
        {
            if (Leaderboards[i].socketId == socketId){
                Leaderboards[i].size = newSize;
                found = true;
                break;
            }
        }

        if (!found) {
            Leaderboards.Add(new LeaderboardItem(socketId, newSize));
        }
        
        Leaderboards.Sort((x, y) => y.size.CompareTo(x.size));
        
        if (!found) {
            Leaderboards.RemoveAt(Leaderboards.Count - 1);
        }
        
        UpdateLeaderboardText();
    }

    public void RemoveFromLeaderboard(string socketId) {
        for (int i = 0; i < Leaderboards.Count; i++)
        {
            if (Leaderboards[i].socketId == socketId){
                Leaderboards.RemoveAt(i);
                Debug.Log("Removed " + socketId + " from leaderboard");

                // Get next best from PlayerDict
                int nextBestSize = Leaderboards[Leaderboards.Count - 1].size;

                int maxSoFar = 0;
                string maxSocketId = "";
                foreach (KeyValuePair<string, Player> player in playersManager.PlayersDict)
                {
                    if (player.Key != socketId && player.Value.blob.size > maxSoFar && player.Value.blob.size < nextBestSize) {
                        maxSoFar = player.Value.blob.size;
                        maxSocketId = player.Key;
                    }
                }

                if (maxSocketId != "") {
                    Leaderboards.Add(new LeaderboardItem(maxSocketId, maxSoFar));
                }

                break;
            }
        }
        

        UpdateLeaderboardText();
    }
    void UpdateLeaderboardText()
    {
        //TODO: update leaderboard
        string leaderboardText = "";
        int i = 1;
        foreach (LeaderboardItem player in Leaderboards)
        {   
            string socketId = player.socketId;
            if (player.socketId == playersManager.selfSocketId) {
                socketId = "You";
            } else {
                socketId = player.socketId.Substring(0, 6);
            }

            leaderboardText += $"{i}. {socketId} - {player.size}\n";
            i++;
        }
        Score.text = leaderboardText;
        // Score.text = "1. Player1 - 50\n2. Player2 - 40\n3. Player3 - 15";
    }

    void Start()
    {
        playersManager = PlayersManager.instance;
        playerMovement = PlayerMovement.instance;
        // InvokeRepeating("UpdateScore", 1, 1);
    }

    void Update()
    {
        
    }
}
