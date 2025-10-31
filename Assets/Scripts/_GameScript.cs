using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
    public static GameState I { get; private set; }
    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    //Loading GameOver Scene
    public void GameOver(string reason = "")
    {
        SceneManager.LoadScene("GameOver", LoadSceneMode.Single);
    }

    //Restarting Game Scene
    public void Restart()
    {
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    //Quit
    public void QuitToDesktop()
    {
        Application.Quit();
    }
}
