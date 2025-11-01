using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }
    public AudioMixer mixer;
    public AudioSource musicSource; //route to Music group
    public AudioSource sfxSource;   //route to SFX group

    void Awake() { if (I != null) { Destroy(gameObject); return; } I = this; DontDestroyOnLoad(gameObject); }
    public void PlayMusic(AudioClip clip, float volume=0.5f, bool loop=true) { if (!clip) return; musicSource.clip=clip; musicSource.loop=loop; musicSource.volume=volume; musicSource.Play();}
    public void StopMusic() => musicSource.Stop();
    public void PlaySFX(AudioClip clip, float volume=1f) { if (!clip) return; sfxSource.PlayOneShot(clip, volume); }
    public void SetMusicDb(float db) => mixer.SetFloat("MusicVol", db);
    public void SetSfxDb(float db) => mixer.SetFloat("SFXVol", db);
}
