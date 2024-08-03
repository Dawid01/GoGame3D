using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicMgr : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] musicClips;
    private int _musicIndex = 0;
    void Start()
    {
        StartCoroutine(PlayMusic());
    }

    private IEnumerator PlayMusic()
    {
        while (true)
        {
            audioSource.clip = musicClips[_musicIndex % musicClips.Length];
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length + 2f);
            _musicIndex++;
        }
    }

    void Update()
    {
        
    }
}
