using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEatMass : MonoBehaviour
{

    public GameObject[] Mass;
    MassSpawner ms;



    public void UpdateMass()
    {
        Mass = GameObject.FindGameObjectsWithTag("Mass");
    }

    public void RemoveMass(GameObject MassObject)
    {
        List<GameObject> MassList = new List<GameObject>();

        for (int i = 0; i < Mass.Length; i++)
        {
            MassList.Add(Mass[i]);
        }
        MassList.Remove(MassObject);

        Mass = MassList.ToArray();
    }
    public void AddMass(GameObject MassObject)
    {
        List<GameObject> MassList = new List<GameObject>();

        for (int i = 0; i < Mass.Length; i++)
        {
            MassList.Add(Mass[i]);
        }
        MassList.Add(MassObject);

        Mass = MassList.ToArray();
    }


    public void Check()
    {


        for (int i = 0; i < Mass.Length; i++)
        {
            if(Mass[i] == null)
            {
                UpdateMass();
                return;
            }


            Transform m = Mass[i].transform;

            if (Vector2.Distance(transform.position, m.position) <= transform.localScale.x / 2)
            {
                RemoveMass(m.gameObject);
                // eat 
                PlayerEat();

                // destroy
                ms.RemoveMass(m.gameObject);
                Destroy(m.gameObject);
            }
        }
    }

    
    // Start is called before the first frame update
    void Start()
    {
        UpdateMass();
        InvokeRepeating("Check", 0, 0.1f);
        ms = MassSpawner.ins;
    }

    void PlayerEat()
    {
        transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
    }

}
