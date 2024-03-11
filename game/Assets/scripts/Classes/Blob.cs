using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Blob
{  
    public string id;
    public Position position;
    public float size;

    public GameObject gameObject;


    public Blob(string id, float size, Position position, GameObject gameObject)
    {
        this.id = id;
        this.size = size;
        this.position = position;
        this.gameObject = gameObject;
    }
}