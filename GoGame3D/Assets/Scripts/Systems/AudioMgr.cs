using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AudioMgr : Singleton<AudioMgr>
{
    public enum Volume
    {
        MASTER, MUSIC, SFX
    }

    private static bool isInitialized = false;
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private AudioSource SFX;
    [SerializeField] private AudioSource additionalSFX;
    public AudioClip putAudioClip;
    public AudioClip dropAudioClip;
    private AudioClip currentClip;
    private Queue<AudioClip> audioQueue = new Queue<AudioClip>();

    public override void Awake()
    {
        base.Awake();
        if (isInitialized)
        {
            Destroy(gameObject);
            return;
        }

        isInitialized = true;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!SFX.isPlaying && audioQueue.Count > 0)
        {
            currentClip = audioQueue.Dequeue();
            SFX.clip = currentClip;
            SFX.Play();
        }
        else if (!additionalSFX.isPlaying && audioQueue.Count > 0)
        {
            currentClip = audioQueue.Dequeue();
            additionalSFX.clip = currentClip;
            additionalSFX.Play();
        }
    }

    private void Start()
    {
        InitializeAudioMixer();
    }

    private void InitializeAudioMixer()
    {
        SetAudioVolume(Volume.MASTER.ToString(), GetValue(Volume.MASTER.ToString()));
        SetAudioVolume(Volume.MUSIC.ToString(), GetValue(Volume.MUSIC.ToString()));
        SetAudioVolume(Volume.SFX.ToString(), GetValue(Volume.SFX.ToString()));
    }

    public float GetValue(string name)
    {
        return PlayerPrefs.GetFloat(name, 1f);
    }
    
    public void SetAudioVolume(string name, float volume)
    {
        _audioMixer.SetFloat(name,  volume == 0 ? -80f :  Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat(name, volume);
        PlayerPrefs.Save();
    }

    public void PlayAudio(AudioClip clip)
    {
        if (!SFX.isPlaying)
        {
            currentClip = clip;
            SFX.clip = currentClip;
            SFX.Play();
        }
        // else if (!additionalSFX.isPlaying)
        // {
        //     currentClip = clip;
        //     additionalSFX.clip = currentClip;
        //     additionalSFX.Play();
        // }
        else
        {
            if (audioQueue.Count < 1f)
            {
                audioQueue.Enqueue(clip);
            }
        }
    }
}
