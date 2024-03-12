using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// It is VERY IMPORTANT that the schema of the classes in this file match
/// EXACTLY the schema of the classes in the server's Player.ts file. 
/// 
/// This is required for the JSON deserialization to work properly.
/// </summary>

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
    public Blob blob;
    public int color;

    // public GameObject gameObject;
    public Player(string socketId, Blob blob)
    {
        this.socketId = socketId;
        this.blob = blob;
    }
}