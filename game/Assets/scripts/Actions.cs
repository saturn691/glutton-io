using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

public class Actions : MonoBehaviour
{

    public GameObject Mass;
    public Transform MassPosition;
    public float Percentage = 0.01f;


    // Start is called before the first frame update

    PlayerEatMass mass_script;
    MassSpawner massSpawner;
    PlayerMovement playerMovement;
    ServerConnect serverConnect;

    void Start()
    {
        serverConnect = ServerConnect.instance;
        playerMovement = PlayerMovement.instance;
        mass_script = GetComponent<PlayerEatMass>();
        massSpawner = MassSpawner.ins;
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.localScale.x < 1)
        {
            return;
        }
        transform.localScale -= new Vector3(Percentage, Percentage, Percentage) * Time.deltaTime;
    }

    public async void ThrowMass(Vector3 direction)
    {
        if(transform.localScale.x < 1f)
        {
            return;
        }

        // rotate 
        Vector2 Direction = direction;
        float Z_Rotation = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0, 0, Z_Rotation);

        // 1. Instantiate food blob
        GameObject b = Instantiate(Mass, playerMovement.transform.position, Quaternion.identity);
        float r = Blob.GetRadius(1);
        b.transform.localScale = new Vector3(r, r, r);
        

        // 2. Simulate food movement
        string blobId = Guid.NewGuid().ToString();
        b.GetComponent<MassForce>().ApplyForce = true;
        b.GetComponent<MassForce>().Init(b.transform.position.x, b.transform.position.y, blobId);
        Dictionary<string, object> res = b.GetComponent<MassForce>().GetFinalPosition();

        // New size: 
        playerMovement.blob.Resize(playerMovement.blob.size - 1);

        // Send message to the server
        res["blobId"] = blobId;
        await serverConnect.SendWsMessage(new ClientMessage(ClientMsgType.PlayerThrewMass, res));
    
    }

}
