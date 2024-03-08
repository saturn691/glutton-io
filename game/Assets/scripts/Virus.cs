using UnityEngine;
using System.Collections.Generic;

public class Virus : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {

            GameObject Player = collision.gameObject;
            Player.transform.localScale += new Vector3(5f, 5f, 5f);

            int BurstNumber = (int)Player.transform.localScale.x;

            Player.transform.localScale /= Player.transform.localScale.x;

            List<GameObject> instantiatedBalls = new List<GameObject>();
            instantiatedBalls.Add(Player);
            for (int i = 0; i < BurstNumber; i++)
            {
                GameObject ball = Instantiate(Player, Player.transform.position, Quaternion.identity);
                instantiatedBalls.Add(ball);
            }

            float rotation = 360 / BurstNumber;
            int CurrentRotation = 0;

            for (int i = 0; i< instantiatedBalls.Count; i++)
            {
                GameObject b = instantiatedBalls[i];
                CurrentRotation += 1;
                b.transform.rotation = Quaternion.Euler(0, 0, rotation * CurrentRotation);

                b.GetComponent<CircleCollider2D>().enabled = false;
                b.GetComponent<PlayerMovement>().LockActions = true;

                b.GetComponent<SplitForce>().Speed = b.GetComponent<SplitForce>().DifaultSpeed;
                b.GetComponent<SplitForce>().ApplyForce = true;

                b.GetComponent<SplitForce>().enabled = true;

            }


            //destroy virus
            Destroy(gameObject);
        }
    }
}
