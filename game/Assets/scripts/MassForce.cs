using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassForce : MonoBehaviour
{

    public bool ApplyForce = false;

    public float Speed = 30f;
    public float LoseSpeed = 140f;


    public float RandomRotation = 10f;
    public float RandomeForce = 5f;


    // Start is called before the first frame update
    void Start()
    {
        if (ApplyForce == false)
        {
            enabled = false;
            return;
        }

        // calculate and apply rotation
        Vector2 Direction = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float zr = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg + 90f;

        zr += Random.Range(-RandomRotation, RandomRotation);

        transform.rotation = Quaternion.Euler(0, 0, zr);


        Speed += Random.Range(-RandomeForce, RandomeForce);

    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.up * Speed * Time.deltaTime);
        Speed -= LoseSpeed * Time.deltaTime;

        if(Speed <= 0)
        {
            enabled = false;
        }
    }
}
