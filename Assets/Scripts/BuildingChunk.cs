using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BuildingChunk : MonoBehaviour
{
    [Tooltip("Local Y of the roof top marker used for alignment.")]
    public float topLocalY = 0f;
    public float width = 6f; //keep consistent across prefabs

    void Reset()
    {
        //infering width from sprite bounds
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr) width = sr.bounds.size.x;
    }
}
