using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class ClientMessage
{
    public ClientMsgType type;
    public object data;

    public ClientMessage(ClientMsgType type, object data)
    {
        this.type = type;
        this.data = data;
    }
}