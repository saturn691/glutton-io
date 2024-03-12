using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerFollow : MonoBehaviour
{
    Camera cam;
    public Vector3 Offset;
    public Vector3 Change;
    public float Speed = 0.4f;
    public float MaxZoom = 100f;
    public float MinZoom = 5f;
    public float ZoomSpeed = 1f;
    public float ZoomController = 1f;

    List<Transform> trackedPlayers = new List<Transform>();
    PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = PlayerMovement.instance;
        cam = Camera.main;
        AddPlayersToTrack();
    }

    void Update()
    {
        // Move();
        // Zoom();
    }

    void Zoom()
    {
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, GetZoom(), ZoomSpeed);
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, MinZoom, MaxZoom);
    }

    float GetZoom()
    {
        Bounds bounds = new Bounds(trackedPlayers[0].position, Vector3.zero);
        foreach (var player in trackedPlayers)
        {
            bounds.Encapsulate(player.position);
        }
        return (bounds.size.x + bounds.size.y) / ZoomController;
    }

    void Move()
    {
        Vector3 position = GetCenter() + Offset;
        transform.position = Vector3.SmoothDamp(transform.position, position, ref Change, Speed);
    }

    Vector3 GetCenter()
    {
        if (playerMovement == null) return Vector3.zero;

        else {
            Debug.Log("GetCenter" + trackedPlayers[0]);
            Bounds bounds = new Bounds(trackedPlayers[0].position, Vector3.zero);
            return bounds.center;
            foreach (var player in trackedPlayers)
            {
                bounds.Encapsulate(player.position);
            }
            return bounds.center;
        }
        
    }

    void AddPlayersToTrack()
    {
        // if (playerMovement == null) {
        //     playerMovement = PlayerMovement.instance;
        //     trackedPlayers.Add(playerMovement.transform);
        // }
        
        // MassSpawner ms = MassSpawner.ins;
        // foreach (var player in ms.Players)
        // {
        //     if (player != null)
        //         trackedPlayers.Add(player.transform);
        // }
    }

    public void RemovePlayerFromTrack(Transform playerTransform)
    {
    
        if (trackedPlayers.Contains(playerTransform))
        {
            Debug.Log("RemovePlayerFromTrack" + playerTransform.name);
            trackedPlayers.Remove(playerTransform);
        }
    }
}
