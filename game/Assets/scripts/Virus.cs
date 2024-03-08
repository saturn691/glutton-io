using UnityEngine;
using System.Collections.Generic;

public class Virus : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject player = other.gameObject;
            player.transform.localScale += new Vector3(5f, 5f, 5f);

            int burstNumber = (int)player.transform.localScale.x;
            player.transform.localScale /= player.transform.localScale.x;

            List<GameObject> instantiatedBalls = new List<GameObject>();
            instantiatedBalls.Add(player);

            for (int i = 0; i < burstNumber; i++)
            {
                GameObject ball = Instantiate(player, player.transform.position, Quaternion.identity);
                instantiatedBalls.Add(ball);
            }

            float rotation = 360f / burstNumber;
            int currentRotation = 0;

            foreach (GameObject b in instantiatedBalls)
            {
                currentRotation += 1;
                b.transform.rotation = Quaternion.Euler(0, 0, rotation * currentRotation);
                b.GetComponent<Collider2D>().isTrigger = false;
                b.GetComponent<CircleCollider2D>().enabled = false;
                b.GetComponent<PlayerMovement>().LockActions = true;
                b.GetComponent<SplitForce>().Speed = b.GetComponent<SplitForce>().DifaultSpeed;
                b.GetComponent<SplitForce>().ApplyForce = true;
                b.GetComponent<SplitForce>().enabled = true;
            }

            // Destroy the virus
            Destroy(gameObject);
        }
    }
}
