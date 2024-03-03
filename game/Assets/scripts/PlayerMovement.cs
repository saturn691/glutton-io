using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{

    Actions actions;

    public bool LockActions = false;
    public float Speed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        actions = GetComponent<Actions>();
    }

    // Update is called once per frame
    void Update()
    {
        float Speed_ = Speed / transform.localScale.x;
        Vector2 Direction = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = Vector2.MoveTowards(transform.position, Direction, Speed_ * Time.deltaTime);


        if (LockActions)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            actions.ThrowMass();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // split
            if(MassSpawner.ins.Players.Count >= MassSpawner.ins.MaxPlayers)
            {
                return;
            }
            actions.Split();
        }
    }

    public void OnEnable()
    {
        if(MassSpawner.ins.Players.Count > MassSpawner.ins.MaxPlayers)
        {
            Destroy(gameObject);
            return;
        }
        MassSpawner.ins.AddPlayer(gameObject);
    }

    public void OnDisable()
    {
        MassSpawner.ins.RemovePlayer(gameObject);
    }
}

