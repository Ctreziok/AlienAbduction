using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    public void OnRestartClicked() => GameState.I?.Restart();
    public void OnQuitClicked(){
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;   // stops Play Mode in Editor
        #else
            Application.Quit();                                // quits in a build
        #endif
    }
}
