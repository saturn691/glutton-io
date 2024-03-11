using System.Collections.Generic;
using UnityEngine;
// using ServerConnect;

public class PlayerMovement : MonoBehaviour
{
    //==========================================================================
    // Fields
    //==========================================================================
    
    const int MsgInterval = 20;
    const int StartingSize = 30;

    public Blob blob;
    private Actions actions;
    private Map map;
    private ServerConnect server;
    private MassSpawner massSpawner;
    private GameObject[] Mass;
    private FpgaController fpgaController;
    private PlayersManager playersManager;
    private int msgCount = 0;

    public bool LockActions = false;
    public float Speed = 10f;
    public Vector3 Direction;

    #region Instance
    public static PlayerMovement instance { get; private set; } // Singleton instance


    //==========================================================================
    // Methods
    //==========================================================================

    void Awake()
    {
        fpgaController = new FpgaController();
        if (instance == null)
        {
            instance = this;
        }
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        blob = new Blob("0", StartingSize, new Position(0, 0), gameObject);
        float r = Blob.GetRadius(StartingSize);
        transform.localScale = new Vector3(r, r, r);

        map = Map.ins;
        server = ServerConnect.instance;
        actions = GetComponent<Actions>();
        massSpawner = MassSpawner.ins;
    }

    // Update is called once per frame
    void Update()
    {
        fpgaController.UpdateData();

        float accel_x = fpgaController.GetReadingX();
        float accel_y = fpgaController.GetReadingY();
        int switches = fpgaController.GetSwitchValue();
        int throwMass = fpgaController.GetKey0();
        int split = fpgaController.GetKey1();

       // Debug.Log("Input " + accel_x + " " + accel_y + " " + switches + " " + throwMass + " " + split);
    
        Direction = new Vector3(accel_x, accel_y, 0);

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

        if (Input.GetKeyDown(KeyCode.W) || split == 1)
        {
            actions.ThrowMass(Direction);
        }
        if (Input.GetKeyDown(KeyCode.Space) || throwMass == 1)
        {
            // split
            if (MassSpawner.ins.Players.Count >= MassSpawner.ins.MaxPlayers)
            {
                return;
            }
            actions.Split(Direction);
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
        // MassSpawner.ins.RemovePlayer(gameObject);
    }

    void OnDestroy()
    {
        // Camera.main.GetComponent<CamerFollow>().RemovePlayerFromTrack(transform);
    }
}

