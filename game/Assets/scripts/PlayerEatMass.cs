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
    SoundEffectsPlayer soundEffectsPlayer;


    //=========================================================================
    // Public methods
    //=========================================================================



    //=========================================================================
    // Private methods
    //=========================================================================

    private void Awake()
    {
        massSpawner = MassSpawner.ins;
        playersManager = PlayersManager.instance;
        server = ServerConnect.instance;
        playerMovement = PlayerMovement.instance;
        soundEffectsPlayer = SoundEffectsPlayer.instance;
    }

    private void UpdateMass()
    {
        Mass = GameObject.FindGameObjectsWithTag("Mass");
    }

    /// <summary>
    /// Method to be called every (few) frame(s) to check if the player has
    /// eaten a mass object.
    /// </summary>
    private async void Check()
    {
        // 1. Check if player has eaten another player
        var playersDictCopy = new Dictionary<string, Player>(playersManager.PlayersDict);
        foreach (KeyValuePair<string, Player> kvp in playersDictCopy)
        {
            string otherPlayerId = kvp.Key;
            Player otherPlayer = kvp.Value;
            if (otherPlayerId == playersManager.selfSocketId) continue;
            
            Blob thisBlob = playerMovement.blob;
            Blob otherBlob = otherPlayer.blob;

            // If ate other blob
            if (!otherPlayer.IsEaten() && thisBlob.LargerThan(otherBlob) && thisBlob.Encountered(otherBlob)) {
                Debug.Log("Sending ws message: PlayerEatenEnemy");
                otherPlayer.SetEaten(true);

                // For quick local change
                if (playerMovement.ChangesOccurLocally) {
                    int newSize = playerMovement.blob.size + otherBlob.size;
                    playersManager.UpdateSelfSize(newSize);
                    playerScore.UpdateLeaderboards(playersManager.selfSocketId, newSize);
                    playersManager.RemovePlayerById(otherPlayerId);
                }

                await server.SendWsMessage(new ClientMessage(
                    ClientMsgType.PlayerEatenEnemy, 
                    otherPlayer.WithoutGameObject()
                ));   
            }
        }

        // 2. Check if player has eaten a food object
        var foodDictCopy = new Dictionary<string, Blob>(massSpawner.FoodDict);
        foreach (KeyValuePair<string, Blob> kvp in foodDictCopy)
        {
            string blobId = kvp.Key;
            Blob foodBlob = kvp.Value;

            GameObject foodGameObject = foodBlob.gameObject;

            if (!foodBlob.eaten && Vector2.Distance(transform.position, foodGameObject.transform.position) 
                <= transform.localScale.x / 2
            ) {
                foodBlob.SetEaten(true);

                // For quick local change
                if (playerMovement.ChangesOccurLocally) {
                    int newSize = playerMovement.blob.size + Blob.DefaultFoodSize;
                    playersManager.UpdateSelfSize(newSize);
                    massSpawner.RemoveFoodBlobById(foodBlob.id);
                    playerScore.UpdateLeaderboards(playersManager.selfSocketId, newSize);
                }
                
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
