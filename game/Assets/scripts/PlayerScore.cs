using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NewBehaviourScript : MonoBehaviour
{
    public TMP_Text Score;

    void UpdateScore()
    {
        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");

        float _score = 0f;

        for (int i = 0; i < player.Length; i ++)
        {
            _score += player[i].transform.localScale.x;
        }

        Score.text = _score.ToString("f2");
    }

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("UpdateScore", 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
