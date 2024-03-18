using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;

public class PlayerScore : MonoBehaviour
{
    public Text Score;

    void UpdateScore()
    {
        //TODO: update leaderboard

        Score.text = "1. Player1 - 50\n2. Player2 - 40\n3. Player3 - 15";
    }

    void Start()
    {
        InvokeRepeating("UpdateScore", 1, 1);
    }

    void Update()
    {
        
    }
}
