using System;

[Serializable]
public class WsMessage
{
    public string type;
    public string message;

    public WsMessage(string type, string message)
    {
        this.type = type;
        this.message = message;
    }
}