using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    //Checking if restart was clicked
    public void OnRestartClicked()
    {
        if (GameState.I != null) GameState.I.Restart();
    }
    
    //checking if quit was clicked
    public void OnQuitClicked()
    {
        if (GameState.I != null) GameState.I.QuitToDesktop();
    }
}
