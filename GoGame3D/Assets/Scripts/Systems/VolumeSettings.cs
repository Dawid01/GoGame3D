using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
struct AudioSlider
{
    public Slider slider;
    public AudioMgr.Volume volume;
}

public class VolumeSettings : MonoBehaviour
{

    [SerializeField] private AudioSlider[] _audioSliders;
    
    void Start()
    {
        AudioMgr audioMgr = AudioMgr.Instance;
        for (int i = 0; i < _audioSliders.Length; i++)
        {
            AudioSlider audioSlider = _audioSliders[i];
            string name = audioSlider.volume.ToString();
            audioSlider.slider.value = audioMgr.GetValue(name);
            audioSlider.slider.onValueChanged.AddListener(value =>
            {
                audioMgr.SetAudioVolume(name, value);
            });
        }
    }
    
}
