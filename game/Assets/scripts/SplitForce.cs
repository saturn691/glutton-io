using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitForce : MonoBehaviour
{
    public float Speed;
    public float loseSpeed;
    public float DifaultSpeed;

    public bool ApplyForce = false;


    public void SplitForceMethod()
    {
        GetComponent<CircleCollider2D>().enabled = false; 
        GetComponent<PlayerMovement>().LockActions = true;


        Vector2 Dir = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float rot = Mathf.Atan2(Dir.y, Dir.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0, 0, rot);


        Speed = DifaultSpeed;
        ApplyForce = true;
    }


    private void Update()
    {
        if(ApplyForce == false)
        {
            enabled = false;
            return;
        }

        transform.Translate(Vector2.up * Speed * Time.deltaTime);
        Speed -= loseSpeed * Time.deltaTime/1.5f;

        if (Speed <= 0)
        {
            GetComponent<CircleCollider2D>().enabled = true;
            GetComponent<PlayerMovement>().LockActions = false;

            enabled = false;
        }
    }
}
