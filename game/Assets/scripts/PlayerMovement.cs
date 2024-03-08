using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using ServerConnect;
using System;
using System.Text;
using System.IO;
using System.Linq;

public class PlayerMovements : MonoBehaviour
{
     StreamReader sr = new StreamReader(@"C:\Users\kr1pt0\glutton-io\game\Assets\scripts\input.txt");
    

    Actions actions;

    public bool LockActions = false;
    public float Speed = 5f;

    Map map;

    ServerConnect server;

    int msgCount = 0;

    MassSpawner massSpawner;

    PlayersManager playersManager;

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
        //var fs = new FileStream(@"C:\Users\kr1pt0\glutton-io\game\Assets\scripts\input.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite); 

        //using (var sr = new StreamReader(fs))
        //{
            var rawInput = sr.ReadLine();
            Debug.Log("Input " + sr.ReadLine());
            Dictionary<string, string> input = rawInput.Trim().Substring(1, rawInput.Length - 2).Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(part => part.Split(": "))
                                                .ToDictionary(split => split[0], split => split[1]);
            int accel_x = Int32.Parse(input["'accel_x'"]);
            int accel_y = Int32.Parse(input["\'accel_y\'"]);
            int switches = Int32.Parse(input["'switch'"]);
            int throwMass = Int32.Parse(input["'key0'"]);
            int split = Int32.Parse(input["'key1'"]);
            

            float Speed_ = Speed / transform.localScale.x;
            Vector2 Direction = new Vector2(accel_x / 10, accel_y/10);

            Direction.x = Mathf.Clamp(Direction.x, map.MapLimits.x * -1 / 2, map.MapLimits.x / 2);
            Direction.y = Mathf.Clamp(Direction.y, map.MapLimits.y * -1 / 2, map.MapLimits.y / 2);
            transform.position = Vector2.MoveTowards(transform.position, Direction, Speed_ * Time.deltaTime);
        //}
        

        // Send message


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
}

