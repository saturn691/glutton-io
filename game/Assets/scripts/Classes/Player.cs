using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Position
{
    public float x;
    public float y;

    public Position(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}


[Serializable]
public class Player
{
    public string socketId;
    public Position position;

    public GameObject gameObject;


    public Player(string socketId, Position position, GameObject gameObject)
    {
        this.socketId = socketId;
        this.position = position;
        this.gameObject = gameObject;
    }
}