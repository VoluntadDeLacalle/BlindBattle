using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeDimensionalSFX : NetworkBehaviour
{
    private AudioSource audioSource3D;
    private AudioClip audioClip;

    private void Awake()
    {
        audioSource3D = GetComponent<AudioSource>();
    }

    [Networked] private TickTimer SFXLife { get; set; }

    public void Init(AudioClip audioClipToPlay, float maxDistance)
    {
        audioSource3D.clip = audioClip;
        audioSource3D.maxDistance = maxDistance;
        audioSource3D.PlayOneShot(audioClipToPlay);
        SFXLife = TickTimer.CreateFromSeconds(Runner, audioClipToPlay.length);
    }

    public override void FixedUpdateNetwork()
    {
        if (SFXLife.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }
}
