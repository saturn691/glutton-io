using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    #region Instance

    public static Map ins;

    private void Awake()
    {
        if (ins == null)
        {
            ins = this;
        }
    }
    #endregion

    // The limits of the map
    // e.g. (100, 200) creates a map of -50 to 50 on the x axis and -100 to 100 
    // on the y axis
    public Vector2 MapLimits;
    
    public Color MapLimits_Color;

    private void OnDrawGizmos()
    {
        Gizmos.color = MapLimits_Color;
        Gizmos.DrawWireCube(transform.position, MapLimits); 
    }
}
