using UnityEngine;

public class BeamKill : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            GameState.I?.GameOver("abducted");
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            GameState.I?.GameOver("abducted");
    }
}