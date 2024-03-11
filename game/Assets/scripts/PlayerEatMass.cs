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
    MassSpawner massSpawner;
    ServerConnect server;

    //=========================================================================
    // Public methods
    //=========================================================================



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


    // TO UPDATE: Player's size updates based on mass it eats
    private void PlayerEatFood()
    {

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

        var foodDictCopy = new Dictionary<string, Blob>(massSpawner.FoodDict);
        foreach (KeyValuePair<string, Blob> kvp in foodDictCopy)
        {
            string blobId = kvp.Key;
            Blob foodBlob = kvp.Value;

            GameObject foodGameObject = foodBlob.gameObject;

            if (Vector2.Distance(transform.position, foodGameObject.transform.position) 
                <= transform.localScale.x / 2
            ) {

                massSpawner.RemoveFoodBlobById(foodBlob.id);

                // Pre render here, but if not verified by server, then render again
                // Maybe set timeout of 10s for server to verify
                PlayerEatFood();

                // Send server PlayerEatenFoodMsg
                server.SendWsMessage(new ClientMessage(ClientMsgType.PlayerEatenFood, foodBlob.id));
            }
        }
    

        return;
    }


    // Start is called before the first frame update
    void Start()
    {

        server = ServerConnect.instance;
        massSpawner = MassSpawner.ins;
        InvokeRepeating("Check", 0, 0.1f);
        
    }
}
