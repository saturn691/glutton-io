using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class WaitingForPlayers : MonoBehaviour
{
    public void OnPlayButton ()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
