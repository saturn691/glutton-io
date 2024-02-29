using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Extra packages
// using System.ArraySegment;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


public class PlayerMovements : MonoBehaviour
{

    public float Speed = 5f;
    // websocket  connection

    

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("Updating player movement");
        float Speed_ = Speed / transform.localScale.x;
        Vector2 Direction = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = Vector2.MoveTowards(transform.position, Direction, Speed_ * Time.deltaTime);
    }
}
