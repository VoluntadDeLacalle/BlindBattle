﻿using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectsManager : SingletonMonoBehaviour<SoundEffectsManager>
{
    public AudioSource audioSource2D;

    [SerializeField] private ThreeDimensionalSFX SFX3DPrefab;
    public List<AudioClip> soundEffects;

    private readonly Dictionary<string, AudioClip> soundEffectsDict = new Dictionary<string, AudioClip>();

    new void Awake()
    {
        base.Awake();

        soundEffectsDict.Clear();
        foreach (var clip in soundEffects)
        {
            soundEffectsDict[clip.name] = clip;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Play(string key)
    {
        if (soundEffectsDict.ContainsKey(key))
        {
            Play(soundEffectsDict[key]);
        }
    }

    //public void PlayAt(string key, Vector3 position)
    //{
    //    if (soundEffectsDict.ContainsKey(key))
    //    {
    //        PlayAt(soundEffectsDict[key], position);
    //    }
    //}


    public void Play(AudioClip audioClip)
    {
        if (audioClip == null)
        {
            return;
        }

        audioSource2D.spatialBlend = 0f;
        audioSource2D.PlayOneShot(audioClip);
    }

    public void PlayAt(AudioClip audioClip, Vector3 position, float maxDistance)
    {
        var o = Instantiate(SFX3DPrefab, position, Quaternion.identity);
        o.GetComponent<ThreeDimensionalSFX>().Init(audioClip, maxDistance);
    }

    public AudioClip GetClip(string key)
    {
        if (soundEffectsDict.ContainsKey(key))
        {
            return soundEffectsDict[key];
        }

        return null;
    }
}