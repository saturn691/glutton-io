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
    public string playerTag;

    // public GameObject gameObject;
    public Player(string socketId, Blob blob)
    {
        this.playerTag = "Player";
        this.socketId = socketId;
        this.blob = blob;
    }

    public void SetEaten(bool eaten) {
        this.blob.eaten = eaten;
    }

    public bool IsEaten() {
        return this.blob.eaten;
    }

    public Player WithoutGameObject() {
        return new Player(this.socketId, this.blob.WithoutGameObject());
    }
}

public class LeaderboardItem
{
    public string socketId;
    public int size;

    public LeaderboardItem(string socketId, int size)
    {
        this.socketId = socketId;
        this.size = size;
    }
}