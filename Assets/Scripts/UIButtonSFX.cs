using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSfx : MonoBehaviour
{
    public AudioClip click;
    void Awake() => GetComponent<Button>().onClick.AddListener(() => {
        if (click) AudioManager.I?.PlaySFX(click, 0.9f);
    });
}
