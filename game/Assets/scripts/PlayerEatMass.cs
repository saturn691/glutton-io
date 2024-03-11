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
    PlayersManager playersManager;
    ServerConnect server;
    PlayerMovement playerMovement;


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

    private void Awake()
    {
        massSpawner = MassSpawner.ins;
        playersManager = PlayersManager.instance;
        server = ServerConnect.instance;
        playerMovement = PlayerMovement.instance;
    }

    private void UpdateMass()
    {
        Mass = GameObject.FindGameObjectsWithTag("Mass");
    }


    /// <summary>
    /// Method to be called when the player has eaten a food object.
    /// Update new player's blob object size and rendered size
    /// </summary>
    private void PlayerEatFood()
    {
        // Calculate new radius of the player.
        playerMovement.blob.size += Blob.DefaultFoodSize;
        float newRadius = Blob.GetRadius(playerMovement.blob.size);
        transform.localScale = new Vector3(newRadius, newRadius, newRadius);

        // MergePlayers.canMerge = true;
        // GetComponent<Collider2D>().isTrigger = true;
    }

    /// <summary>
    /// Method to be called when the player has eaten a food object.
    /// Update new player's blob object size and rendered size
    /// </summary>
    private void PlayerEatEnemy(Blob otherBlob)
    {
        // Calculate new radius of the player.
        playerMovement.blob.size += otherBlob.size;
        float newRadius = Blob.GetRadius(playerMovement.blob.size);
        transform.localScale = new Vector3(newRadius, newRadius, newRadius);

        // MergePlayers.canMerge = true;
        // GetComponent<Collider2D>().isTrigger = true;
    }


    /// <summary>
    /// Method to be called every (few) frame(s) to check if the player has
    /// eaten a mass object.
    /// </summary>
    private async void Check()
    {
        var playersDictCopy = new Dictionary<string, Player>(playersManager.PlayersDict);
        foreach (KeyValuePair<string, Player> kvp in playersDictCopy)
        {
            string otherPlayerId = kvp.Key;
            Player otherPlayer = kvp.Value;
            if (otherPlayerId == playersManager.selfSocketId) continue;
            
            Blob thisBlob = playerMovement.blob;
            Blob otherBlob = otherPlayer.blob;

            // If ate other blob
            if (thisBlob.LargerThan(otherBlob) && thisBlob.Encountered(otherBlob)) {
                PlayerEatEnemy(otherBlob);

                Debug.Log("Sending ws message: PlayerEatenEnemy");

                playersManager.RemovePlayerById(otherPlayerId);

                Player otherPlayerWithoutGameObject = new Player(
                    otherPlayer.socketId,
                    new Blob(otherPlayer.blob.id, otherPlayer.blob.size, otherPlayer.blob.position, null)
                );
                server.SendWsMessage(new ClientMessage(
                    ClientMsgType.PlayerEatenEnemy, 
                    otherPlayerWithoutGameObject  
                ));
                
            } 
            
            else if (otherBlob.LargerThan(thisBlob) && otherBlob.Encountered(thisBlob)) {
                Debug.Log("Died");
            }
        }

        // 2. Check if player has eaten a food object
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


    void Update()
    {
        Check();
    }


    // Start is called before the first frame update
    void Start()
    {
        playerMovement = PlayerMovement.instance;
        server = ServerConnect.instance;
        massSpawner = MassSpawner.ins;
        playersManager = PlayersManager.instance;
    }
}
