using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerFollow : MonoBehaviour
{
    //cam issue
    Camera cam;
    public Vector3 Offset;
    public Vector3 Change;
    public float Speed = 0.4f;
    //zoming variables
    public float MaxZoom = 100f;
    public float MinZoom = 5f;
    public float ZoomSpeed = 1f;
    //debugging tool
    public float ZoomController = 1f;

    
    
    private void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Zoom();
    }

    public void Zoom()
    {
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, GetZoom(), ZoomSpeed);

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, MinZoom, MaxZoom);
    }

    public float GetZoom()
    {
        MassSpawner ms = MassSpawner.ins;

        Bounds bounds = new Bounds(ms.Players[0].transform.position, Vector3.zero);
        for (int i = 0; i < ms.Players.Count; i++)
        {
            bounds.Encapsulate(ms.Players[i].transform.position);
        }

        return (bounds.size.x + bounds.size.y) / ZoomController;
    }

    public void Move()
    {
        Vector3 position = GetCenter() + Offset;

        transform.position = Vector3.SmoothDamp(transform.position, position, ref Change, Speed);
    }

    Vector3 GetCenter()
    {
        MassSpawner ms = MassSpawner.ins;

        Bounds bounds = new Bounds(ms.Players[0].transform.position, Vector3.zero);
        for (int i = 0; i < ms.Players.Count; i++)
        {
            bounds.Encapsulate(ms.Players[i].transform.position);
        }

        return bounds.center;
    }
}
