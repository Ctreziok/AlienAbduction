using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class UfoController : MonoBehaviour
{
    [Header("Timing")]
    public float warnTime = 1.0f;           //telegraph duration
    public float beamTime = 1.2f;           //active damage window
    public float cooldown = 28f;            //base cooldown
    public float cooldownJitter = 6f;       //+/- randomness

    [Header("Placement")]
    public float yTop = 5.5f;               //UFO hover height
    public float beamWidth = 1.2f;          //world units
    public float viewMargin = 0.8f;         //don't spawn at extreme edges
    public LayerMask playerMask;            //set to Player layer
    public LayerMask groundMask;            //set to Ground layer

    [Header("Refs")]
    public Transform body;                  //child "Body"
    public Transform beam;                  //child "Beam"
    public SpriteRenderer beamSR;           //SR on Beam
    public BoxCollider2D beamCol;           //collider on Beam
    public AudioSource audioSource;
    public AudioClip warnSfx;
    public AudioClip beamLoopSfx;

    Transform player;
    float nextTimer;

    void Awake()
    {
        if (!beamSR && beam) beamSR = beam.GetComponent<SpriteRenderer>();
        if (!beamCol && beam) beamCol = beam.GetComponent<BoxCollider2D>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO) player = playerGO.transform;

        //ensure initial hidden state
        SetBeamActive(false, 0f);
    }

    void OnEnable()
    {
        //Start a little after game starts
        nextTimer = 2.0f;
        StartCoroutine(MainLoop());
    }

    IEnumerator MainLoop()
    {
        //run forever
        while (true)
        {
            //cooldown countdown
            while (nextTimer > 0f)
            {
                nextTimer -= Time.deltaTime;
                Hover();
                yield return null;
            }

            //Position UFO + telegraph
            float x = PickSafeAwareX();
            ShowUfoAt(x);
            transform.position = new Vector3(x, yTop, 0f);
            body.localPosition = Vector3.zero;

            //Telegraph (beam faint, collider disabled)
            SetBeamActive(false, 0.35f);
            PlayOneShot(warnSfx);
            yield return new WaitForSeconds(warnTime);

            //Active beam
            AudioManager.I?.SetMusicDb(-8f);
            SetBeamActive(true, 1f);
            StartLoop(beamLoopSfx);
            float t = beamTime;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                //if player overlaps beam, end game
                if (beamCol && player)
                {
                    //lightweight overlap check
                    Collider2D hit = Physics2D.OverlapBox(beamCol.bounds.center, beamCol.bounds.size, 0f, playerMask);
                    if (hit != null)
                    {
                        if (GameState.I != null) GameState.I.GameOver("abducted");
                        else Debug.Log("Game Over: abducted");
                        break;
                    }
                }
                Hover();
                yield return null;
            }
            StopLoop();
            SetBeamActive(false, 0f);
            AudioManager.I?.SetMusicDb(0f);
            HideUfoBetweenAttacks();
            
            //schedule next
            nextTimer = cooldown + Random.Range(-cooldownJitter, cooldownJitter);
        }
    }

    //Pick an X within camera view while leaving room on both sides
    float PickSafeAwareX()
    {
        var cam = Camera.main;
        float half = cam.orthographicSize * cam.aspect;

        //keep inside margins so we never cover entire width
        float left = cam.transform.position.x - half * (1f - 0.05f);
        float right = cam.transform.position.x + half * (1f - 0.05f);
        float inner = Mathf.Clamp01(1f - viewMargin);
        float minX = Mathf.Lerp(cam.transform.position.x - half, left, inner);
        float maxX = Mathf.Lerp(cam.transform.position.x + half, right, inner);

        //Avoid being right on top of player
        float best = Random.Range(minX, maxX);
        if (player)
        {
            for (int i = 0; i < 12; i++)
            {
                float candidate = Random.Range(minX, maxX);
                if (Mathf.Abs(candidate - player.position.x) > beamWidth * 0.75f)
                {
                    best = candidate;
                    break;
                }
            }
        }
        return best;
    }

    void SetBeamActive(bool active, float alpha)
    {
        if (!beam) return;

        var cam = Camera.main;
        float camHeight    = cam.orthographicSize * 2f;
        float visualHeight = camHeight + 4f; //a little extra so it always spans
        
        if (beamCol)
        {
            beamCol.isTrigger = true;
            beamCol.enabled   = active;
            beamCol.offset    = Vector2.zero;
            beamCol.size      = new Vector2(beamWidth, visualHeight);
        }

        //size the sprite directly (keep transform scale at 1,1,1)
        if (beamSR)
        {
            beamSR.drawMode = SpriteDrawMode.Sliced;
            beamSR.size     = new Vector2(beamWidth, visualHeight);

            var c = beamSR.color;
            c.a   = Mathf.Clamp01(alpha);
            beamSR.color = c;
        }

        //Keep transform scale neutral to avoid double-scaling
        beam.localScale = Vector3.one;

        //Put the beam down from the UFO
        beam.position = new Vector3(
            transform.position.x,
            transform.position.y - (visualHeight * 0.5f),
            0f
        );
    }



    void Hover()
    {
        if (!body) return;
        // simple sine hover
        float hover = Mathf.Sin(Time.time * 2.2f) * 0.05f;
        body.localPosition = new Vector3(0f, hover, 0f);
    }

    void PlayOneShot(AudioClip clip)
    {
        if (!audioSource || clip == null) return;
        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.Play();
    }

    void StartLoop(AudioClip clip)
    {
        if (!audioSource || clip == null) return;
        // just play once (no loop)
        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.Play();
    }

    void StopLoop()
    {
        if (!audioSource) return;
        audioSource.Stop();
        audioSource.clip = null;
    }


    //shows beam width
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.2f, 1f, 0.4f);
        Gizmos.DrawCube(new Vector3(transform.position.x, 0f, 0f), new Vector3(beamWidth, 100f, 0.1f));
    }
    
    void HideUfoBetweenAttacks()
    {
        //lift it way above the camera
        transform.position = new Vector3(transform.position.x, yTop + 20f, 0f);
        if (body) body.gameObject.SetActive(false);
    }

    void ShowUfoAt(float x)
    {
        transform.position = new Vector3(x, yTop, 0f);
        if (body) body.gameObject.SetActive(true);
    }

}
