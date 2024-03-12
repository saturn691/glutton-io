/// <summary>
/// Must agree with the server's schema.
/// </summary>

public enum ClientMsgType
{
    Join,
    UpdatePosition,
    PlayerEatenFood,
    PlayerEatenEnemy
    
}

public enum ServerMsgType {
    InitSocketId, // 0
    PlayerJoined, // 1
    PlayerLeft, // 2
    UpdatePlayersPosition, // 3
    FoodAdded, // 4
    PlayerAteFood, // 5
    PlayerAteEnemy
}

// Join message data
public class JoinMsgData
{
    public string playerId;

    public JoinMsgData(string playerId)
    {
        this.playerId = playerId;
    }
}