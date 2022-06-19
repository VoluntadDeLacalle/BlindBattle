using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMusic : SingletonMonoBehaviour<BGMusic>
{
    [HideInInspector]
    public AudioSource audioSource;

    private new void Awake()
    {
        base.Awake();
        
        if (Instance != this)
        {
            return;
        }

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
