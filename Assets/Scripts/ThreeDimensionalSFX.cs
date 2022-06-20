using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeDimensionalSFX : MonoBehaviour
{
    public AudioSource audioSource3D;

    private void Awake()
    {

    }
    

    public void Init(AudioClip audioClipToPlay, float maxDistance)
    {
        audioSource3D.maxDistance = maxDistance;
        audioSource3D.PlayOneShot(audioClipToPlay);

        this.WaitAndExecute(() =>
        {
            Destroy(gameObject);
        }, audioClipToPlay.length, true);
    }
}
