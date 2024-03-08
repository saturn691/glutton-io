using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class ServerMessage
{
    public ServerMsgType type;
    public object data;

    public ServerMessage(ServerMsgType type, object data)
    {
        this.type = type;
        this.data = data;
    }
}