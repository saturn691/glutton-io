using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Allows the player to eat mass objects by checking for collisions with them.
/// </summary>
public class PlayerEatMass : MonoBehaviour
{
    //=========================================================================
    // Fields
    //=========================================================================

    public GameObject[] Mass;
    MassSpawner ms;

    //=========================================================================
    // Public methods
    //=========================================================================

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
        List<GameObject> MassList = new List<GameObject>();

        for (int i = 0; i < Mass.Length; i++)
        {
            MassList.Add(Mass[i]);
        }
        MassList.Add(MassObject);

        Mass = MassList.ToArray();
    }

    //=========================================================================
    // Private methods
    //=========================================================================

    private void UpdateMass()
    {
        Mass = GameObject.FindGameObjectsWithTag("Mass");
    }


    private void PlayerEat()
    {
        //Debug.Log("mass EATED added");

        // Calculate new radius of the player.

        transform.localScale += new Vector3(0.08f, 0.08f, 0.08f);
        MergePlayers.canMerge = true;
        GetComponent<Collider2D>().isTrigger = true;
    }


    /// <summary>
    /// Method to be called every (few) frame(s) to check if the player has
    /// eaten a mass object.
    /// </summary>
    private void Check()
    {
        // Update the mass array
        UpdateMass();

        // Check if the player has collided with any mass object
        for (int i = 0; i < Mass.Length; i++)
        {
            Transform m = Mass[i].transform;

            if (Vector2.Distance(transform.position, m.position) 
                <= transform.localScale.x / 2
            ) {
                RemoveMass(m.gameObject);
                Debug.Log("mass object removed");
                PlayerEat();

                if (m != null && m.gameObject != null)
                {
                    ms.RemoveMass(m.gameObject);
                    Destroy(m.gameObject);
                }
            }
        }

        return;
    }


    // Start is called before the first frame update
    private void Start()
    {
        UpdateMass();
        InvokeRepeating("Check", 0, 0.1f);
        ms = MassSpawner.ins;
    }
}
