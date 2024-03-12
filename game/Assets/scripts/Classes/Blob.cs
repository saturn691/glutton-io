using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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
    public GameObject gameObject;

    public static int DefaultFoodSize = 1;

    public static double MassMultiplier = 1.1;

    public static double DistanceMultiplier = 1.75;

    
    //==========================================================================
    // Methods
    //==========================================================================


    public Blob(string id, int size, Position position, GameObject gameObject)
    {
        this.id = id;
        this.position = position;
        this.size = size;
        this.gameObject = gameObject;
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
    public static float GetRadius(int size)
    {
        return (float)Math.Sqrt(size / Math.PI);
    }


    /// <summary>
    ///  Check if this blob meets with other blob
    /// </summary>
    public bool Encountered(Blob other)
    {  


        var deltaX = this.gameObject.transform.position.x - other.gameObject.transform.position.x;
        var deltaY = this.gameObject.transform.position.y - other.gameObject.transform.position.y;

        var distBetweenCenters = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        var thisRadius = Math.Sqrt(this.size / Math.PI);
        return thisRadius > distBetweenCenters * DistanceMultiplier;

        // // distance between the two blobs
        // float blobDistance = Vector2.Distance(
        //     this.gameObject.transform.position, 
        //     blob.gameObject.transform.position
        // );

        // return blobDistance <= this.gameObject.transform.localScale.x / 2;
    }
    
    /// <summary>
    ///  Check if this blob is larger than other blob
    /// </summary>
    public bool LargerThan(Blob other)
    {
        return this.size > other.size * MassMultiplier;
    }
}
