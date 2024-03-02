using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actions : MonoBehaviour
{
    public GameObject Mass;
    public Transform MassPosition;

    public float Percentage = 0.01f;


    public void ThrowMass()
    {
        if(transform.localScale .x < 1)
        {
            return;
        }
        // rotate 
        Vector2 Direction = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float Z_Rotation = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0, 0, Z_Rotation);

        // instantiate mass 
        GameObject b = Instantiate(Mass, MassPosition.position, Quaternion.identity);

        // apply force
        b.GetComponent<MassForce>().ApplyForce = true;


        // add mass to the player
        ms.AddMass(b);


        // lose mass
        transform.localScale -= new Vector3(0.05f, 0.05f, 0.05f);
    }

    public void Split()
    {
        // jump action is here
        if(transform.localScale.x < 2)
        {
            // return if the player size was low
            return;
        }

        GameObject b = Instantiate(gameObject, transform.position, Quaternion.identity);

        // apply force

        b.GetComponent<SplitForce>().enabled = true;
        b.GetComponent<SplitForce>().SplitForceMethod();
    }



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
}
