using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinalScoreText : MonoBehaviour
{
    public TextMeshProUGUI finalText;

    void Start()
    {
        float last = PlayerPrefs.GetFloat("LastScore", 0f);
        float best = PlayerPrefs.GetFloat("BestScore", 0f);
        if (finalText)
            finalText.text = $"SCORE: {Mathf.FloorToInt(last)}\nBEST: {Mathf.FloorToInt(best)}";
    }
}
