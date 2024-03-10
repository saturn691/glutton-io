using System;
using System.Collections;
using System.Collections.Generic;
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
public class Blob
{
    public string id;
    public Position position;
    public int size;

    public Blob(string id, Position position, int size)
    {
        this.id = id;
        this.position = position;
        this.size = size;
    }
}


[Serializable]
public class Player
{
    public string socketId;
    public int color;
    public bool gameOver;
    public Blob blob;

    public GameObject gameObject;


    public Player(
        string socketId,
        int color,
        bool gameOver, 
        Blob blob,
        GameObject gameObject
    ) {
        this.socketId = socketId;
        this.color = color;
        this.gameOver = gameOver;
        this.blob = blob;
        this.gameObject = gameObject;
    }
}