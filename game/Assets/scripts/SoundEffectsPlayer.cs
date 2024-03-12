using UnityEngine;

public class SoundEffectsPlayer : MonoBehaviour
{
    //==========================================================================
    // Fields
    //==========================================================================

    public AudioSource audioSource;
    public AudioClip food, lose, player;

    #region Instance

    public static SoundEffectsPlayer instance { get; private set; } // Singleton instance

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    #endregion


    //==========================================================================
    // Methods
    //==========================================================================

    /// <summary>
    /// High note (E and C)
    /// </summary>
    public void PlayFood()
    {
        audioSource.clip = food;
        audioSource.Play();
    }


    /// <summary>
    /// F major chord
    /// </summary>
    public void PlayLose()
    {
        audioSource.clip = lose;
        audioSource.Play();
    }


    /// <summary>
    /// C2 and C3 
    /// </summary>
    public void PlayPlayer()
    {
        audioSource.clip = player;
        audioSource.Play();
    }
}


