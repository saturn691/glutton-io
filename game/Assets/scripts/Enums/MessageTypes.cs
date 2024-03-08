public enum ClientMsgType
{
    Join,
    UpdatePosition
    
}

public enum ServerMsgType {
    InitSocketId,
    PlayerJoined,
    PlayerLeft,
    UpdatePlayersPosition
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