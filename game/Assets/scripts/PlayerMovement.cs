using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using ServerConnect;

public class PlayerMovements : MonoBehaviour
{
    //==========================================================================
    // Fields
    //==========================================================================
    
    const int MsgInterval = 20;
    
    private Actions actions;
    private Map map;
    private ServerConnect server;
    private MassSpawner massSpawner;
    private PlayersManager playersManager;
    private int msgCount = 0;

    public bool LockActions = false;
    public float Speed = 5f;


    //==========================================================================
    // Methods
    //==========================================================================

    // Start is called before the first frame update
    void Start()
    {
        map = Map.ins;
        server = ServerConnect.instance;
        actions = GetComponent<Actions>();
        massSpawner = MassSpawner.ins;
    }

    // Update is called once per frame
    void Update()
    {
        float Speed_ = Speed / transform.localScale.x;
        Vector2 Direction = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Direction.x = Mathf.Clamp(Direction.x, map.MapLimits.x * -1 / 2, map.MapLimits.x / 2);
        Direction.y = Mathf.Clamp(Direction.y, map.MapLimits.y * -1 / 2, map.MapLimits.y / 2);
        transform.position = Vector2.MoveTowards(transform.position, Direction, Speed_ * Time.deltaTime);

        // Send message to server
        if (msgCount % MsgInterval == 0)
        {
            Dictionary<string, object> updatePlayerPosMsg = new Dictionary<string, object> {
                {"x", transform.position.x},
                {"y", transform.position.y}
            };

            var UpdatePosMsg = new ClientMessage(
                ClientMsgType.UpdatePosition, 
                updatePlayerPosMsg
            );

            server.SendWsMessage(UpdatePosMsg).ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    Debug.LogError($"Error sending message: {task.Exception}");
                }
            });
        }
        
        msgCount++;

        // serverconnec
        // serverconnect.Instance.SendMessage(yourMessage).ContinueWith(task => 
        // {
        //     if (task.Exception != null)
        //     {
        //         Debug.LogError($"Error sending message: {task.Exception}");
        //     }
        // });

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
            if (MassSpawner.ins.Players.Count >= MassSpawner.ins.MaxPlayers)
            {
                return;
            }
            actions.Split();
        }
    }

    public void OnEnable()
    {
        if (MassSpawner.ins.Players.Count > MassSpawner.ins.MaxPlayers)
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

    void OnDestroy()
    {
        Camera.main.GetComponent<CamerFollow>().RemovePlayerFromTrack(transform);
    }
}

