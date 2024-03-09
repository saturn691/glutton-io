using System.Collections.Generic;
using UnityEngine;
// using ServerConnect;

public class PlayerMovement : MonoBehaviour
{
<<<<<<< HEAD
    //StreamReader sr;
    //string rawInput;    

=======
>>>>>>> main
    Actions actions;

    public bool LockActions = false;
    public float Speed = 10f;

    Map map;
    
    ServerConnect server;

    private FpgaController fpgaController;

    int msgCount = 0;

    MassSpawner massSpawner;

    PlayersManager playersManager;

    void Awake()
    {
        fpgaController = new FpgaController();
    }

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
<<<<<<< HEAD
        fpgaController.UpdateData();

        float accel_x = fpgaController.GetReadingX();
        float accel_y = fpgaController.GetReadingY();
        int switches = fpgaController.GetSwitchValue();
        int throwMass = fpgaController.GetKey0();
        int split = fpgaController.GetKey1();

        Debug.Log("Input " + accel_x + " " + accel_y + " " + switches + " " + throwMass + " " + split);
    
        Vector3 Direction = new Vector3(accel_x, accel_y, 0);
=======
        float Speed_ = Speed / transform.localScale.x;
        Vector2 Direction = Camera.main.ScreenToWorldPoint(Input.mousePosition);
>>>>>>> main

        // The magnitude of the direction vector does not affect the speed in
        // the MoveTowards function, so we have to calculate the speed manually
        float Speed_ = Speed * Direction.magnitude / transform.localScale.x;

        // Update the position of the player
        transform.position += Direction * Speed_ * Time.deltaTime;

        // Clamp the player position to the map limits
        transform.position = new Vector3(
            Mathf.Clamp(
                transform.position.x, 
                map.MapLimits.x * -1 / 2, 
                map.MapLimits.x / 2
            ),
            Mathf.Clamp(
                transform.position.y, 
                map.MapLimits.y * -1 / 2, 
                map.MapLimits.y / 2
            ),
            transform.position.z
        );

        // Send message to the server
        if (msgCount % 2000 == 0)
        {
            Dictionary<string, object> updatePlayerPosMsg = new Dictionary<string, object> {
                {"x", transform.position.x},
                {"y", transform.position.y}
            };

            var UpdatePosMsg = new ClientMessage(ClientMsgType.UpdatePosition, updatePlayerPosMsg);
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

        if (Input.GetKeyDown(KeyCode.W) || split == 1)
        {
            actions.ThrowMass();
        }
        if (Input.GetKeyDown(KeyCode.Space) || throwMass == 1)
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

