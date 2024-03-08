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
    List<GameObject> MassList = new List<GameObject>(Mass);

    // Remove the MassObject from the list
    MassList.Remove(MassObject);

    // Update the Mass array
    Mass = MassList.ToArray();
}

    public void AddMass(GameObject MassObject)
    {
        Debug.Log("mass object added");
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
            Transform m = Mass[i].transform;

            if(Mass[i] == null)
            {
                Debug.Log("mass object is null");
                ms.RemoveMass(m.gameObject);
                Destroy(m.gameObject);
                UpdateMass();
                return;
            }



            if (Vector2.Distance(transform.position, m.position) <= transform.localScale.x / 2)
            {
                RemoveMass(m.gameObject);
                Debug.Log("mass object removed");
                // eat 
                PlayerEat();
                if (m != null && m.gameObject != null){

                // destroy
                ms.RemoveMass(m.gameObject);
                Destroy(m.gameObject);
                }
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
                Debug.Log("mass EATED added");

        transform.localScale += new Vector3(0.08f, 0.08f, 0.08f);
        MergePlayers.canMerge = true;
    }

}
