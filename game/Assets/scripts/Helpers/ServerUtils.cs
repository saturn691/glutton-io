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
    public static void HandlePlayerJoined(PlayersManager pmInst, PlayerScore psInst, object msgData) {
        var player = JsonConvert.DeserializeObject<Player>(msgData.ToString());
        psInst.UpdateLeaderboards(player.socketId, player.blob.size);
        pmInst.AddPlayer(player);
    }

    public static void HandlePlayerLeft(PlayersManager pmInst, PlayerScore psInst, object msgData) {
        var playerId = msgData.ToString();
        psInst.RemoveFromLeaderboard(playerId);
        pmInst.RemovePlayerById(playerId);
    }

    public static void HandleFoodAdded(MassSpawner msInst, object msgData)
    {        
        var data = JsonConvert.DeserializeObject<Blob>(msgData.ToString());
        msInst.AddFood(data);
    }

    public static void HandleUpdatePlayersPosition(PlayersManager pmInst, object msgData)
    {
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(msgData.ToString());
        foreach (var player in data)
        {
            string otherPlayerId = (string)player["socketId"];
            
            // If just eaten player, skip
            if (!pmInst.PlayersDict.ContainsKey(otherPlayerId)) continue;

            
            if (otherPlayerId== pmInst.selfSocketId) continue;
            
            
            Position pos = JsonConvert.DeserializeObject<Position>(player["position"].ToString());
            pmInst.UpdatePlayerPosition(
                otherPlayerId,
                pos.x,
                pos.y
            );
        }
    }

    // TODO ADD COMMENT
    public static void HandlePlayerAteFood(PlayersManager pmInst, MassSpawner msInst, PlayerScore playerScore, object msgData)
    {        
        var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(msgData.ToString());
        
        string foodBlobId = (string)data["foodId"];
        string playerId = (string)data["playerId"];
        
        int newSize = 0;
        if (pmInst.selfSocketId == playerId) {
            if (pmInst.playerMovement.ChangesOccurLocally) return;

            newSize = pmInst.playerMovement.blob.size + Blob.DefaultFoodSize;
            pmInst.UpdateSelfSize(newSize);
            msInst.RemoveFoodBlobById(foodBlobId);
            playerScore.UpdateLeaderboards(playerId, newSize);
        } else {
            newSize = pmInst.PlayersDict[playerId].blob.size + Blob.DefaultFoodSize;
            pmInst.UpdatePlayerSize(playerId, newSize);
            msInst.RemoveFoodBlobById(foodBlobId);
            playerScore.UpdateLeaderboards(playerId, newSize);
        }

    }

    // TODO ADD COMMENT
    public static void HandlePlayerAteEnemy(PlayersManager pmInst, PlayerMovement playerMovement, PlayerScore playerScore, object msgData)
    {
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(msgData.ToString());
        
        string playerWhoAteId = data["playerWhoAte"].ToString();
        int newSize = JsonConvert.DeserializeObject<int>(data["newSize"].ToString());
        string playerEatenId = data["playerEaten"].ToString();


        if (pmInst.selfSocketId == playerEatenId) {
            Debug.Log("You were eaten! Game over!");
            playerMovement.Died = true;
            playerMovement.DestroySelf();
            return;
        }

        playerScore.UpdateLeaderboards(playerWhoAteId, newSize);
        
        if (pmInst.selfSocketId == playerWhoAteId) {
            if (pmInst.playerMovement.ChangesOccurLocally) return; 
            
            pmInst.UpdateSelfSize(newSize);
            playerScore.RemoveFromLeaderboard(playerEatenId);
            pmInst.RemovePlayerById(playerEatenId);

        } else {
            pmInst.UpdatePlayerSize(playerWhoAteId, newSize);
            playerScore.RemoveFromLeaderboard(playerEatenId);
            pmInst.RemovePlayerById(playerEatenId);
        }
        
        

    }

    // <summary>
    // Handle player threw mass
    // 1. Reduce player's size
    // 2. Handle mass -> instantiate and trigger force, then add to dict
    // </summary>
    public static void HandlePlayerThrewMass(
        PlayersManager pmInst, 
        MassSpawner msInst, 
        PlayerScore playerScoreInst,
        PlayerMovement playerMovementInst,
        object msgData
    ) {
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(msgData.ToString());
        string playerId = data["playerId"].ToString();
        
        
        if (pmInst.selfSocketId == playerId) {
            playerScoreInst.UpdateLeaderboards(playerId, playerMovementInst.blob.size - 1);
            return;
        };

        // 1. Reduce player's size
        pmInst.UpdatePlayerSize(playerId, pmInst.PlayersDict[playerId].blob.size - 1);
        playerScoreInst.UpdateLeaderboards(playerId, pmInst.PlayersDict[playerId].blob.size - 1);

        // 2. Throw mass
        string blobId = data["blobId"].ToString();
        float speed = JsonConvert.DeserializeObject<float>(data["initialSpeed"].ToString());
        Position startPos = JsonConvert.DeserializeObject<Position>(data["startPos"].ToString());
        Position endPos = JsonConvert.DeserializeObject<Position>(data["endPos"].ToString());
        Position dir = JsonConvert.DeserializeObject<Position>(data["direction"].ToString());

        msInst.AddThrownMass(
            blobId,
            speed, 
            new Vector3(dir.x, dir.y, 0), 
            new Vector2(startPos.x, startPos.y), 
            endPos
        );

    }
}