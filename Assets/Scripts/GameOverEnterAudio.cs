using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameOverEnterAudio : MonoBehaviour
{
    public AudioClip gameOverSfx;
    void Start() { if (gameOverSfx) AudioManager.I?.PlaySFX(gameOverSfx, 1f); }
}
