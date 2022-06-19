using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DevRoundManagerConfig
{
    public HostOrJoinDevUI HostOrJoinDevUIPrefab;
}

public class RoundManager : SingletonMonoBehaviour<RoundManager>
{
    public DevRoundManagerConfig devRoundManagerConfig;

    private new void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager.Instance.networkRunner)
        {
            StartRound();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartRound()
    {

    }
}
