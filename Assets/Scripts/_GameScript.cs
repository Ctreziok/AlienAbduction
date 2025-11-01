using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
    public static GameState I { get; private set; }
    public bool IsPaused  { get; private set; }
    public bool IsOver    { get; private set; }

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable()=> SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scn, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        IsPaused = false;
        IsOver = false;

        //When game scene loads should be new run
        var scorer = FindObjectOfType<ScoreManager>();
        if (scorer) scorer.StartRun();
    }

    public void GameOver(string reason = "")
    {
        if (IsOver) return;
        IsOver = true;

        //stop scoring and save best
        var scorer = FindObjectOfType<ScoreManager>();
        if (scorer) { scorer.StopRun(); }

        //load GameOver scene
        SceneManager.LoadScene("GameOver", LoadSceneMode.Single);
    }

    public void Restart()
    {
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsOver) TogglePause();
    }
}
