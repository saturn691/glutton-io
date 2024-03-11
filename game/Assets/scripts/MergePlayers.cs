using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MergePlayers : MonoBehaviour
{
    public string playerName; // Unique ID for each player
    public string blobID;
    public static bool canMerge = true; // Flag to indicate if merging is allowed
    // private Transform textTransform; // Reference to the Text GameObject's Transform

    // public Text textComponent; // Reference to the Text component


    private void Start()
    {
        // Initialize playerName for each player object
        playerName = "name"; //
        blobID = System.Guid.NewGuid().ToString(); //unique id for each blob per player
        // textComponent = textTransform.GetComponent<Text>();
        // textComponent.text = playerName;
    }

    private void OnTriggerEnter2D(Collider2D remaining)
    {
        Debug.Log("Triggered");
        // Check if the colliding GameObject has the "Player" tag
        if (remaining.CompareTag("Player"))
        {
            MergePlayers remainingMergePlayers = remaining.GetComponent<MergePlayers>();

            // Merge players only if they have different IDs and merging is allowed
            if (remainingMergePlayers.playerName == playerName && MergePlayers.canMerge && canMerge)
            {
                // Merge the players
                Merge(gameObject, remaining.gameObject);

                // Set both players' canMerge to false
                canMerge = false;
                remaining.GetComponent<Collider2D>().isTrigger = false;
                MergePlayers.canMerge = false;

                // // Start the merge cooldown timer for both players
                // StartCoroutine(MergeCooldown());
                // Debug.Log("2nd coroutine");
                // StartCoroutine(remainingMergePlayers.MergeCooldown());
                // Debug.Log("Merged");
            }
        }
    }

    void Merge(GameObject player2, GameObject player1)
    {
        // Calculate the new scale for the merged player
        float newScale = player1.transform.localScale.x + player2.transform.localScale.x;
        newScale = newScale / 1.1f;
        // Set the new scale for the merged player
        player1.transform.localScale = new Vector3(newScale, newScale, newScale);

        Camera.main.GetComponent<CamerFollow>().RemovePlayerFromTrack(transform);
        // MassSpawner.ins.GetComponent<MassSpawner>().RemovePlayer(player2);

        // Destroy the remaining player
        Destroy(player2);
    }

    // IEnumerator MergeCooldown()
    // {
    //     Debug.Log("Merge cooldown started");
    //     // Wait for 10 seconds
    //     yield return new WaitForSecondsRealtime(1);
    //     Debug.Log("Merge cooldown ended");
    //     // Enable merging again after cooldown
    //     canMerge = true;
    // } 

}