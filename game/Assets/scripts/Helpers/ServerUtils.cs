using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[Serializable]
public class ServerUtils
{
    public static void HandlePlayerJoined(PlayersManager pmInst, object msgData) {
        Debug.Log("Handling player joined: " + msgData);
        var player = JsonConvert.DeserializeObject<Player>(msgData.ToString());

        pmInst.AddPlayer(player);
    }

    public static void HandleUpdatePlayersPosition(PlayersManager pmInst, object msgData)
    {
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(msgData.ToString());
        foreach (var player in data)
        {
            if ((string)player["socketId"] == pmInst.selfSocketId) continue;
            
            
            Position pos = JsonConvert.DeserializeObject<Position>(player["position"].ToString());
            pmInst.UpdatePlayerPosition(
                (string)player["socketId"],
                pos.x,
                pos.y
            );
        }
    }

    public static void HandleFoodAdded(MassSpawner msInst, object msgData)
    {        
        var data = JsonConvert.DeserializeObject<Blob>(msgData.ToString());
        msInst.AddFood(data);
    }


    public static void HandlePlayerAteFood(PlayersManager pmInst, MassSpawner msInst, object msgData)
    {        
        var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(msgData.ToString());
        
        string foodBlobId = (string)data["foodId"];
        string playerId = (string)data["playerId"];
        

        if (pmInst.selfSocketId != playerId)
        {
            // Update other player's size
            int newSize = pmInst.PlayersDict[playerId].blob.size + Blob.DefaultFoodSize;
            pmInst.UpdatePlayerSize(playerId, newSize);
            msInst.RemoveFoodBlobById(foodBlobId);
            Debug.Log("Other player ate food");
        }

    }
}