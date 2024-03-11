/// <summary>
/// Must agree with the server's schema.
/// </summary>

public enum ClientMsgType
{
    Join,
    UpdatePosition,
    PlayerEatenFood
    
}

public enum ServerMsgType {
    InitSocketId,
    PlayerJoined,
    PlayerLeft,
    UpdatePlayersPosition,
    FoodAdded,
    PlayerAteFood
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