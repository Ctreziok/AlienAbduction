using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Refs")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestText;

    [Header("Config")]
    public bool useTimeScore = true;           //time survived
    public float pointsPerSecond = 1f;         //1 point/second

    float score;
    bool running;

    public float Score => score;

    void OnEnable() { StartRun(); }

    public void StartRun()
    {
        score = 0f;
        running = true;
        UpdateHUD();
    }

    public void StopRun()
    {
        running = false;
        //Save score and best score
        PlayerPrefs.SetFloat("LastScore", score);
        SaveBest();
    }

    void Update()
    {
        if (!running) return;
        if (useTimeScore) score += pointsPerSecond * Time.deltaTime;
        UpdateHUD();
    }

    void UpdateHUD()
    {
        if (scoreText) scoreText.text = Mathf.FloorToInt(score).ToString();
        if (bestText)  bestText.text  = "BEST: " + Mathf.FloorToInt(PlayerPrefs.GetFloat("BestScore", 0));
    }

    public void SaveBest()
    {
        float best = PlayerPrefs.GetFloat("BestScore", 0);
        if (score > best)
        {
            PlayerPrefs.SetFloat("BestScore", score);
            PlayerPrefs.Save();
        }
        UpdateHUD();
    }
    
}
