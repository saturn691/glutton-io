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
public class Blob
{
    //==========================================================================
    // Fields
    //==========================================================================
    
    private const float BaseSpeed = 5f;
    public string id;
    public Position position;
    public int size;
    
    //==========================================================================
    // Methods
    //==========================================================================


    public Blob(string id, Position position, int size)
    {
        this.id = id;
        this.position = position;
        this.size = size;
    }

    /// <summary
    /// Must agree with the server's speed calculation.
    /// Otherwise, the movement of the bots will be incorrect.
    /// </summary>
    public double GetSpeed()
    {
        return BaseSpeed / Math.Log(size);
    }

    /// <summary>
    ///  Must agree with the server's mass object radius.
    ///  Otherwise, the rendering of the mass object will be incorrect.
    /// </summary>
    public static double GetRadius(int size)
    {
        return Math.Sqrt(size / Math.PI);
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