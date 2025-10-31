using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureBootFirst()
    {
        //The game will only start from boot
    }

    void Start()
    {
        StartCoroutine(LoadGameNextFrame());
    }

    System.Collections.IEnumerator LoadGameNextFrame()
    {
        yield return null; //waiting one frame
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }
}

