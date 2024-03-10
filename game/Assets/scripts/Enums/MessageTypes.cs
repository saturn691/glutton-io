/// <summary>
/// Must agree with the server's schema.
/// </summary>

public enum ClientMsgType
{
    Join,
    UpdatePosition
    
}

public enum ServerMsgType {
    InitSocketId,
    PlayerJoined,
    PlayerLeft,
    UpdatePlayersPosition,
    BlobEats,
    BlobEaten
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