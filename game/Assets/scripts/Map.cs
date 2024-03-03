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

    public Vector2 MapLimits;
    public Color MapLimits_Color;

    private void OnDrawGizmos()
    {
        Gizmos.color = MapLimits_Color;
        Gizmos.DrawWireCube(transform.position, MapLimits); 
    }
}
