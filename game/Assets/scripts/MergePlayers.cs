using UnityEngine;
using System.Collections;

public class MergePlayers : MonoBehaviour
{
    public int playerId; // Unique ID for each player
    public static bool canMerge = true; // Flag to indicate if merging is allowed

    private void Start()
    {
        // Initialize playerId for each player object
        playerId = GetHashCode(); // Using GetHashCode() to generate a unique ID
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding GameObject has the "Player" tag
        if (other.CompareTag("Player"))
        {
            MergePlayers otherMergePlayers = other.GetComponent<MergePlayers>();

            // Merge players only if they have different IDs and merging is allowed
            if (otherMergePlayers.playerId != playerId && MergePlayers.canMerge)
            {
                // Merge the players
                Merge(gameObject, other.gameObject);

                // Set both players' canMerge to false
                canMerge = false;
                MergePlayers.canMerge = false;

                // // Start the merge cooldown timer for both players
                // StartCoroutine(MergeCooldown());
                // Debug.Log("2nd coroutine");
                // StartCoroutine(otherMergePlayers.MergeCooldown());
                // Debug.Log("Merged");
            }
        }
    }

    void Merge(GameObject player2, GameObject player1)
    {
        // Calculate the new scale for the merged player
        float newScale = player1.transform.localScale.x + player2.transform.localScale.x;

        // Set the new scale for the merged player
        player1.transform.localScale = new Vector3(newScale, newScale, newScale);

        Camera.main.GetComponent<CamerFollow>().RemovePlayerFromTrack(transform);
        MassSpawner.ins.GetComponent<MassSpawner>().RemovePlayer(player2);

        // Destroy the other player
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