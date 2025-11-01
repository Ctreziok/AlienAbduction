using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxFollow : MonoBehaviour
{
    public Transform target;                  //usually Camera.main.transform
    [Range(0f,1f)] public float parallax = 0.3f;
    public bool lockY = true;

    [Tooltip("World-space Y where this layer should live (used when lockY = true).")]
    public float baseY = 0f;

    Vector3 startPos;
    Vector3 targetStart;
    bool anchored;

    void OnEnable()
    {
        if (!target) target = Camera.main ? Camera.main.transform : null;
        AnchorNow();
    }

    [ContextMenu("Anchor to current transform")]
    public void AnchorNow()
    {
        if (!target) return;
        startPos    = transform.position;     //x/z from current transform
        targetStart = target.position;
        //capture the current Y as the baseline if locked
        if (lockY) baseY = transform.position.y;
        anchored = true;
    }

    void LateUpdate()
    {
        if (!Application.isPlaying || !target || !anchored) return;

        var delta = target.position - targetStart;

        float x = startPos.x + delta.x * parallax;
        float y = lockY ? baseY : startPos.y + delta.y * parallax;

        transform.position = new Vector3(x, y, transform.position.z);
    }
}
