using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    static int score = 0;
    private bool gameOverWin = false;

    public TextMeshProUGUI scoreText;

    public GameObject partyBoy;

    private void Update()
    {
        scoreText.text = "score : " + score;
    }

    public bool GameOverWin
    {
        get => gameOverWin;
        set
        {
            if(!gameOverWin && value)
            {
                gameOverWin = value;
                partyBoy.SetActive(true);
            }
        }
    }

    public static void ResetScore()
    {
        score = 0;
    }


    private void CallPartyBoy()
    {

    }
    public bool IsGameOverWin()
    {
        GameOverWin = score == 6;

        return GameOverWin;
    }
    public static int IncreaseScore()
    {
        score++;
        return score;
    }
}
