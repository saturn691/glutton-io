using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actions : MonoBehaviour
{
    public GameObject Mass;
    public Transform MassPosition;
    public float Percentage = 0.01f;


    // Start is called before the first frame update

    PlayerEatMass mass_script;
    MassSpawner ms;
    void Start()
    {
        mass_script = GetComponent<PlayerEatMass>();
        ms = MassSpawner.ins;
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

    public void ThrowMass(Vector3 direction)
    {
        if(transform.localScale.x < 1f)
        {
            return;
        }

        // rotate 
        Vector2 Direction = direction;
        float Z_Rotation = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0, 0, Z_Rotation);

        // instantiate mass 
        GameObject b = Instantiate(Mass, MassPosition.position, Quaternion.identity);

        // apply force
        b.GetComponent<MassForce>().ApplyForce = true;
        b.GetComponent<MassForce>().Direction = -direction;

        // add mass to the player
        ms.AddMass(b);

        // lose mass
        transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
    }

    public void Split(Vector3 direction)
    {
        if(transform.localScale.x <= 2)
        {
            // Return if the player size is low
            return;
        }

        // Lose mass
        transform.localScale /= 4;
        Vector3 newMass = 3*transform.localScale;
        gameObject.GetComponent<Collider2D>().isTrigger = false;

        // Instantiate a new player object using the assigned prefab
        GameObject newPlayer = Instantiate(gameObject, transform.position, Quaternion.identity);
        transform.localScale = newMass;

        // Apply any additional setup for the new player object
        newPlayer.GetComponent<Collider2D>().isTrigger = false;
        newPlayer.GetComponent<SplitForce>().enabled = true;
        newPlayer.GetComponent<SplitForce>().SplitForceMethod(direction);
    }


}
