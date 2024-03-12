using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class Menu : MonoBehaviour
{
    public void OnConnectButton ()
    {
        SceneManager.LoadScene("WaitingForPlayers");
    }

    public void OnQuitButton ()
    {
        Application.Quit();
    }
}
