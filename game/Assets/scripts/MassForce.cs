using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassForce : MonoBehaviour
{
    public bool ApplyForce = false;
    public float Speed = 50f;
    public float LoseSpeed = 100f;
    public float RandomRotation = 10f;
    public float RandomeForce = 5f;
    public Vector3 Direction;
    ServerConnect serverConnect;
    private int moveCount = 0;
    Vector2 startPos;
    Vector2 finalPos;

    string blobId;
    MassSpawner massSpawner;

    // public int moveCount = 100;

    // Start is called before the first frame update
    void Start()
    {
        serverConnect = ServerConnect.instance;
        massSpawner = MassSpawner.ins;
        if (ApplyForce == false)
        {
            enabled = false;
            return;
        }
    }

    public void Init(float x, float y, string id) {
        // Rotate
        float zr = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg + 90f;
        zr += Random.Range(-RandomRotation, RandomRotation);
        transform.rotation = Quaternion.Euler(0, 0, zr);
        Speed += Random.Range(-RandomeForce, RandomeForce);
        startPos = new Vector2(x, y);
        Direction = transform.up;
        blobId = id;
    
        moveCount = 0;
    }

    public void InitParams(float speed, Vector3 direction, Vector2 start, string id) {
        Speed = speed;
        Direction = direction;
        startPos = new Vector2(start.x, start.y);
        moveCount = 0;
        blobId = id;
    }

    public Position GetFinalPos() {
        return new Position(finalPos.x, finalPos.y);
    }

    public Dictionary<string, object> MassInfoToDict(Vector2 finalPos) {
        
        return new Dictionary<string, object> {
            {"startPos", new Dictionary<string, float>() {
                {"x", startPos.x},
                {"y", startPos.y}
            }},
            {"endPos", new Dictionary<string, float>() {
                {"x", finalPos.x},
                {"y", finalPos.y}
            }},
            {"direction", new Dictionary<string, float>() {
                {"x", Direction.x},
                {"y", Direction.y}
            }},
            {"initialSpeed", Speed},
        };
    }

    public Dictionary<string, object>  GetFinalPosition()
    {
        
        // Dtrection
        float speed = Speed;
        float multiplier = 0.001f;
        finalPos = new Vector2(startPos.x, startPos.y);

        int numMoves = 0;
        while (speed > 0)
        {
            finalPos.x += (Direction.normalized.x * speed * multiplier);
            finalPos.y += (Direction.normalized.y * speed * multiplier);
            speed -= LoseSpeed * multiplier;
            numMoves++;
        }

        return MassInfoToDict(finalPos);
    }

    // Update is called once per frame
    void Update()
    {

        float multiplier = 0.001f;
        moveCount++;

        float newX = startPos.x + (Direction.normalized.x * Speed * multiplier);
        float newY = startPos.y + (Direction.normalized.y * Speed * multiplier);

        transform.position = new Vector3(newX, newY, 0);
        startPos = new Vector2(newX, newY);
        moveCount++;

        Speed -= LoseSpeed * multiplier;
        
        if (Speed <= 0) {
            if (massSpawner.FoodDict.ContainsKey(blobId)) {
                massSpawner.FoodDict[blobId].position = new Position(finalPos.x, finalPos.y);
            } else {
                massSpawner.FoodDict.Add(blobId, new Blob(blobId, 1, new Position(finalPos.x, finalPos.y), this.gameObject));
            }
            enabled = false;
        }

    }
}